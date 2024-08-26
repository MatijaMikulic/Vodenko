using MessageBroker.Common.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using SharedResources;
using SharedResources.Constants;
using MessageModel.Model.Messages;
using MessageModel.Model.DataBlockModel;
using MessageModel.Utilities;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using MathModelOnline.Algorithm;
using MathModelOnline.Model;
using MathModelOnline.Constants;
using MessageManagerService.Constants;
using MathModelOnline.Utilities;
using ClosedXML.Excel;
using System.Timers;
namespace MathModelOnline.Services
{
    public class MathService
    {
        #region Readonly fields
        private readonly IProducerConsumer _producerConsumer;
        private readonly RecursiveLeastSquares _rls;
        private readonly NonLinearModel _nonLinearModel;
        private readonly StateSpaceLinearModel _stateSpaceLinearModel;
        private readonly DCModelCalculations _dcCalculations;
        #endregion

        #region Constants
        private const double SamplingTime = 0.1;
        private const double rlsSample = 20;
        private const int stabilizationCount = 400;


        private const int convergenceCheckInterval = 1;     // Number of iterations to check for convergence
        private const double convergenceThreshold = 1e-4;   // Convergence threshold for theta parameters
        #endregion

        #region Private Attributes
        private bool _isPumpOn;
        private bool _isSSInitialized;
        private int _previousSample;

        private Vector<double> xHatKPrev;
        private Vector<double> xPrev;

        private Queue<double[]> thetaHistory = new Queue<double[]>(); // History of theta parameters
        private Queue<double> h1Stabilization = new Queue<double>();
        private Queue<double> h2Stabilization = new Queue<double>();
        private Queue<double> QuStabilization = new Queue<double>();

        private double prevH1;
        private double prevH2;
        private double prevQu;
        private double prevXv;


        private System.Timers.Timer _sendTimer; // Timer to send model parameters
        private const double SendInterval = 30000; // Interval in milliseconds (30 seconds)
        private double[] _latestTheta = null;

        LowpassFilter filterH1 = new(10, 0.06);
        LowpassFilter filterH2 = new(10, 0.06);
        LowpassFilter filterQu = new(10, 0.06);
        #endregion
        public MathService(IProducerConsumer producerConsumer)
        {
            _producerConsumer = producerConsumer;
            _stateSpaceLinearModel = new StateSpaceLinearModel(ModelParameters.A, ModelParameters.B, ModelParameters.C,
                                                               ModelParameters.variables, ModelParameters.initialValues,
                                                               ModelParameters.inputs, ModelType.CONTINUOUS);

            _dcCalculations = new DCModelCalculations(_stateSpaceLinearModel, SamplingTime);
            _dcCalculations.CalculateDiscreteFormZOH();

            _nonLinearModel = new NonLinearModel();
            _nonLinearModel.SetSampleTime(SamplingTime);

            _rls = InitializeRlsAlgorithm();
            _isPumpOn = true;
            _isSSInitialized = false;
            _previousSample = -1;

            _sendTimer = new System.Timers.Timer(1000) { AutoReset = true };
            _sendTimer.Elapsed += OnSendTimerElapsed;

        }

        private void OnSendTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_latestTheta != null && _latestTheta.Length > 0)
            {
                L2L2_ModelParameters parameters = new L2L2_ModelParameters(_latestTheta.ToList(), 1);

                if (_producerConsumer.IsConnected())
                {
                    _producerConsumer.SendMessage(MessageRouting.SampleDataRoutingKey, parameters);
                    _producerConsumer.SendMessage(MessageRouting.MMServicePrivateRoutingKey, parameters);
                }
            }
        }

        public RecursiveLeastSquares InitializeRlsAlgorithm()
        {
            var config = new RegressionMatrixConfig();
            config.AddRegression("X0", "X1", "0", "0", "U0")
                  .AddRegression("0", "0", "X0", "X1", "0");
            // Theta vector
            var initialTheta = Vector<double>.Build.Dense(new double[] { 0.7946, 0.0007799, 0.9659, 0.03296, 1.375e-05});

            xHatKPrev = Vector<double>.Build.Dense(2); // Initialize previous estimated output
            xPrev = Vector<double>.Build.Dense(2);     // Initialize previous output

            // Initialize windows for moving window approach
            //h1Window = new List<double>();
            //h2Window = new List<double>();
            //quWindow = new List<double>();
            //uWindow  = new List<double>();

            // RLS Algorithm instance
            return new RecursiveLeastSquares(
                initialTheta,
                initialPValue: 10000,
                lambda: 1.0,
                alpha: 0.5,
                alphaWeight: 0.8,
                regressionMatrixFunc: (x, u) => new RegressionMatrixBuilder(config).BuildRegressionMatrix(x, u),
                config: config
            );
        }
        public async Task Start()
        {
            await _producerConsumer.OpenCommunication(MathModelOnlineInfo.ServiceName);
            _producerConsumer.PurgeQueue(MessageRouting.ProcessDataQueue);

            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
            new L2L2_LogMessage(MathModelOnlineInfo.ServiceName,
            "Mathematical Model Online Service has started successfully.",
            Severity.Info, 1));
            _sendTimer.Start();
            await _producerConsumer.ReadMessageFromQueueAsync(MessageRouting.ProcessDataQueue, async (body) =>
            {
                //Console.WriteLine("Receiving new message");
                var message = MessageDeserializationUtilities.DeserializeMessage(body);
                if (message is L2L2_ProcessData data)
                {
                    _isPumpOn = data.ProcessData.IsPumpActive;
                    await ProcessData(data);
                }

            });

        }

        public void Stop()
        {
            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(MathModelOnlineInfo.ServiceName,
                "Mathematical Model Online Service has stopped.",
                Severity.Warning, 1));

            _producerConsumer.Dispose();
            _sendTimer.Stop();
            _sendTimer.Dispose();
        }

        public Task ProcessData(L2L2_ProcessData data)
        {
            double h1Filtered = filterH1.Process(data.ProcessData.WaterLevelTank1);
            double h2Filtered = filterH2.Process(data.ProcessData.WaterLevelTank2);
            double quFiltered = filterQu.Process(data.ProcessData.InletFlow);
            double xvFiltered = data.ProcessData.ValvePositionFeedback;

            if (_previousSample == -1)
            {
                _stateSpaceLinearModel.IsInitialized = false;
                _nonLinearModel.Reinitialize(data.ProcessData.InletFlow,
                                             data.ProcessData.WaterLevelTank1,
                                             data.ProcessData.WaterLevelTank2,
                                             data.ProcessData.ValvePositionFeedback);
            }

            if (_stateSpaceLinearModel.IsInitialized is false)
            {
                ReinitializeModel(h1Filtered, h2Filtered, quFiltered, xvFiltered, data);

                //h1Window.Clear();
                //h2Window.Clear();
                //quWindow.Clear();
                //uWindow. Clear();

                prevH1 = h1Filtered;
                prevH2 = h2Filtered;
                prevQu = quFiltered;
                prevXv = xvFiltered;

            }
            else
            {
                double h1Val = prevH1;
                double h2Val = prevH2;
                double quVal = prevQu;
                double xvVal = prevXv;

                double qi = data.ProcessData.OutletFlow;
                DateTime time = data.ProcessData.GetDateTime();
                bool flag = data.ProcessData.IsPumpActive;

                // Calculate the change in xv using initial value
                if (_isPumpOn is false)
                {
                    xvVal = 0;
                }
                double deltaXv = xvVal - _stateSpaceLinearModel.InitialInputs["xv0"];
                Vector<double> dXv = Vector<double>.Build.Dense(1, deltaXv);

                _dcCalculations.SetCurrentInput(dXv);
                _dcCalculations.SetInletFLowState(quVal - _stateSpaceLinearModel.InitialValues["qu0"]);
                var states = _dcCalculations.CalculateNextState();

                double delta_qu = states[0];
                double delta_h1 = states[1];
                double delta_h2 = states[2];

                double qu_lin = delta_qu + _stateSpaceLinearModel.InitialValues["qu0"];
                double h1_lin = delta_h1 + _stateSpaceLinearModel.InitialValues["h10"];
                double h2_lin = delta_h2 + _stateSpaceLinearModel.InitialValues["h20"];

                // Calculate nonlinear model states based on previous predicted state
                _nonLinearModel.SetInput(xvVal);
                (double qu_nonLin, double h1_nonLin, double h2_nonLin) = _nonLinearModel.CalculateNonLinearModelStates();

                // Convert from cm^3/s to l/min
                double qmn1 = quVal * (60 / 1000.0);
                double qmn2 = qu_lin * (60 / 1000.0);
                double qmn3 = qi * (60 / 1000.0);


                L2L2_DynamicData dynamicData = new L2L2_DynamicData
                   (
                       data.ProcessData.ValvePositionFeedback,
                       (float)(data.ProcessData.InletFlow * (60 / 1000.0f)),
                       (float)data.ProcessData.WaterLevelTank1,
                       (float)data.ProcessData.WaterLevelTank2,
                       (float)(qu_nonLin * (60 / 1000.0f)),
                       (float)h1_nonLin,
                       (float)h2_nonLin,
                       (float)(qu_lin * (60 / 1000.0f)),
                       (float)h1_lin,
                       (float)h2_lin,
                       data.ProcessData.OutletFlow * (60 / 1000.0f),
                       data.ProcessData.GetDateTime(),
                       data.ProcessData.IsPumpActive,
                       data.ProcessData.Sample,
                       data.ProcessData.TargetWaterLevelTank2,
                       1
                   );


                if (_producerConsumer.IsConnected())
                {
                    _producerConsumer.SendMessage(MessageRouting.SampleDataRoutingKey, dynamicData);
                }

                RLSAlgoritm(_previousSample, dXv, h1Val, h2Val, quVal);
                //Console.WriteLine($"{h1_nonLin}, {h1Raw}");
                _previousSample = data.ProcessData.Sample;
                prevH1 = h1Filtered;
                prevH2 = h2Filtered;
                prevQu = quFiltered;
                prevXv = xvFiltered;
                //Console.WriteLine($"{prevH1},{prevH2}, {prevQu}, {deltaXv}");
            }


            _previousSample = data.ProcessData.Sample;
            return Task.CompletedTask;
        }
        public void ReinitializeModel(double h1Filtered, double h2Filtered, double quFiltered, double xvFiltered, L2L2_ProcessData data)
        {
            double h10 = h1Filtered;
            double h20 = h2Filtered;
            double qu0 = quFiltered;
            double xv0 = xvFiltered;

            if (h10 < 0)
            {
                h10 = 0.02;
            }
            if (h20 < 0)
            {
                h20 = 0.01;
            }
            if (h10 <= h20)
            {
                h10 = h20 + 0.1;
            }

            h1Stabilization.Enqueue(h10);
            h2Stabilization.Enqueue(h20);
            QuStabilization.Enqueue(qu0);

            if (h1Stabilization.Count > stabilizationCount) h1Stabilization.Dequeue();
            if (h2Stabilization.Count > stabilizationCount) h2Stabilization.Dequeue();
            if (QuStabilization.Count > stabilizationCount) QuStabilization.Dequeue();

            bool h1Stable = IsStabilized(h1Stabilization, 1.0f);
            bool h2Stable = IsStabilized(h2Stabilization, 1.0f);
            bool quStable = IsStabilized(QuStabilization, 7.0f);

            Console.WriteLine($"{h1Stable}, {h2Stable}, {quStable} ");
            if (h1Stable && h2Stable && quStable)
            {
                Console.WriteLine("Now using Linear Model!");
                Dictionary<string, double> initialValues = new Dictionary<string, double>()
                {
                    { "h20",h20 },
                    { "h10",h10 },
                    { "qu0",qu0 }
                };
                Dictionary<string, double> inputs = new Dictionary<string, double>()
                {
                    { "xv0",xv0}
                };

                _stateSpaceLinearModel.Reinitialize(ModelParameters.variables, initialValues, inputs);
                _dcCalculations.Reinitialize(_stateSpaceLinearModel);
                _dcCalculations.CalculateDiscreteFormZOH();
                var result = DiscreteContinuousConverter.CalculateDiscreteFormZOH(_stateSpaceLinearModel.A, _stateSpaceLinearModel.B, SamplingTime * rlsSample);
                var initialTheta = Vector<double>.Build.Dense(new double[] { 
                    result.Ad[1, 1], result.Ad[1, 2], result.Ad[2, 1], result.Ad[2,2], result.Ad[1,0]
                   });
                _rls.Theta = initialTheta;
            }
            else
            {

                if (_isPumpOn)
                {
                    _nonLinearModel.SetInput(xvFiltered);
                }
                else
                {
                    _nonLinearModel.SetInput(0);
                }

                (double qu_nonLin, double h1_nonLin, double h2_nonLin) = _nonLinearModel.CalculateNonLinearModelStates();

                L2L2_DynamicData dynamicData = new L2L2_DynamicData
                    (
                        data.ProcessData.ValvePositionFeedback,
                       (float)(data.ProcessData.InletFlow * (60 / 1000.0f)),
                       (float)data.ProcessData.WaterLevelTank1,
                       (float)data.ProcessData.WaterLevelTank2,
                       (float)(qu_nonLin * (60 / 1000.0f)),
                       (float)h1_nonLin,
                       (float)h2_nonLin,
                        -1,
                        -1,
                        -1,
                        data.ProcessData.OutletFlow * (60 / 1000.0f),
                        data.ProcessData.GetDateTime(),
                        data.ProcessData.IsPumpActive,
                        data.ProcessData.Sample,
                        data.ProcessData.TargetWaterLevelTank2,
                        1
                    );

                if (_producerConsumer.IsConnected())
                {
                    _producerConsumer.SendMessage(MessageRouting.SampleDataRoutingKey, dynamicData);
                }
            }

        }
        private void RLSAlgoritm(int previousSample, Vector<double> deltaXv, double h1Val, double h2Val, double quVal)
        {
            if (previousSample % rlsSample == 0)
            {
                double h1 = h1Val;
                double h2 = h2Val;
                double qu = quVal;
                double delta_qu = qu - _stateSpaceLinearModel.InitialValues["qu0"];

                Vector<double> dqu = Vector<double>.Build.Dense(1,delta_qu);
                //double xv = xvRaw;
                Vector<double> xK = Vector<double>.Build.DenseOfArray
                    (new double[] {
                     //qu - _stateSpaceLinearModel.InitialValues["qu0"],
                     h1 - _stateSpaceLinearModel.InitialValues["h10"],
                     h2 - _stateSpaceLinearModel.InitialValues["h20"] });
                xHatKPrev = _rls.Update(xK, xHatKPrev, xPrev, dqu);
                // Update previous output
                xPrev = xK;

                thetaHistory.Enqueue(_rls.Theta.ToArray());

                if (thetaHistory.Count > convergenceCheckInterval)
                {
                    thetaHistory.Dequeue(); // Remove the oldest theta parameters
                }

                // Check if the theta parameters have converged
                if (thetaHistory.Count == convergenceCheckInterval && CheckConvergence())
                {


                    var result = _rls.GetEstimatedDiscreteSSModel();
                    // Check A_c1 for NaN values

                    var (A_c1, B_c1) = DiscreteContinuousConverter
                    .CalculateContinuousFormFromDiscrete(result.A_est, result.B_est, SamplingTime * rlsSample);

                    // Check for NaN values in A_c1 and B_c1
                    bool hasNaN = false;
                    // Check A_c1 for NaN values
                    for (int i = 0; i < A_c1.RowCount; i++)
                    {
                        for (int j = 0; j < A_c1.ColumnCount; j++)
                        {
                            if (double.IsNaN(A_c1[i, j]))
                            {
                                hasNaN = true;
                                break;
                            }
                        }
                        if (hasNaN) break;
                    }

                    // Check B_c1 for NaN values if A_c1 is okay
                    if (!hasNaN)
                    {
                        for (int i = 0; i < B_c1.RowCount; i++)
                        {
                            for (int j = 0; j < B_c1.ColumnCount; j++)
                            {
                                if (double.IsNaN(B_c1[i, j]))
                                {
                                    hasNaN = true;
                                    break;
                                }
                            }
                            if (hasNaN) break;
                        }
                    }
                    double[] timeConstants = DiscreteContinuousConverter.CalculateTimeConstants(A_c1);

                    foreach (var t in timeConstants)
                    {
                        if (t < 0.0 || t > 800)
                        {
                            hasNaN = true;
                        }
                    }
                    if (!hasNaN)
                    {
                        _latestTheta = new double[7]
                        {
                            -0.5747,
                            B_c1[0, 0],
                            A_c1[0, 0],
                            A_c1[0, 1],
                            A_c1[1, 0],
                            A_c1[1, 1],
                            1.8689
                        };

                        var A_NEW = Matrix<double>.Build.DenseOfArray(new double[,]
                        {
                            { -0.5747, 0, 0 },
                            { B_c1[0, 0], A_c1[0, 0], A_c1[0, 1] },
                            { B_c1[1, 0],  A_c1[1, 0],  A_c1[1, 1] }
                        });

                        var B_NEW = Matrix<double>.Build.DenseOfArray(new double[,]
                        {
                            {1.8689  },
                            { 0 },
                            { 0 }
                        });

                        _stateSpaceLinearModel.Reinitialize(A_NEW, B_NEW);
                        _dcCalculations.Reinitialize(_stateSpaceLinearModel);
                        _dcCalculations.CalculateDiscreteFormZOH();
                    }
                }
            }
        }
        private bool CheckConvergence()
        {
            if (thetaHistory.Count < convergenceCheckInterval)
            {
                return false;
            }

            double[] initialTheta = thetaHistory.Peek();
            foreach (var theta in thetaHistory)
            {
                for (int i = 0; i < theta.Length; i++)
                {
                    if (Math.Abs(theta[i] - initialTheta[i]) > convergenceThreshold)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsStabilized(Queue<double> values, double tolerance)
        {
            if (values.Count < stabilizationCount)
            {
                return false;
            }
            double maxVal = values.Max();
            double minVal = values.Min();
            return (maxVal - minVal) < tolerance;
        }
    }
}

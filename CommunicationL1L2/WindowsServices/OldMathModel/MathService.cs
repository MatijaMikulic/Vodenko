using MathModelOnline.Utilities;
using MathModelOnline.Utilities.MatrixExponential;
using MathNet.Filtering;
using MathNet.Numerics.LinearAlgebra;
using MessageBroker.Common.Producer;
using MessageManagerService.Constants;
using MessageModel.Model.DataBlockModel;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using SharedResources.Constants;
using System.ComponentModel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
public class MathService
{
    private readonly IProducerConsumer _producerConsumer;
    private StateSpaceModel _stateSpaceModel;

    private int previousSample;
    private double previousXv = 0;
    private double previousQu, previousH1, previousH2;
    private bool isFirstIteration;
    private RLSAlgorithm rlsAlgorithm;
    private Vector<double> yHatKPrev;
    private Vector<double> yPrev;

    private double previousQuPredicted, previousH1Predicted, previousH2Predicted;
    private double previousQuNonLin, previousH1NonLin, previousH2NonLin;

    private List<double> h1Window;
    private List<double> h2Window;
    private List<double> quWindow;
    private List<double> uWindow;
    private int windowSizeH1 = 10;
    private int windowSizeH2 = 10;
    private int windowSizeQu = 10;
    private int windowSizeU = 1;
    private double rlsSample = 1;

    private int convergenceCheckInterval = 1;                    // Number of iterations to check for convergence
    private double convergenceThreshold = 1e-4;                   // Convergence threshold for theta parameters
    private Queue<double[]> thetaHistory = new Queue<double[]>(); // History of theta parameters

    public MathService(IProducerConsumer producerConsumer)
    {
        _producerConsumer = producerConsumer;

        _stateSpaceModel = new StateSpaceModel();
        ModelStatesCalc.SetSampleTime(0.1);
        previousSample = -1;
        isFirstIteration = true; // Flag to skip first iteration

        // Initial RLS algorithm parameters
        //double[] initialTheta = { 0.7502, 0.0009, 0.9604, 0.0375, 0.0375, 0.9382, 0.7622 };
        //double[] initialTheta = new double[7];
        double[] initialTheta = {0.7502, 0.0009, 0.9579, 0.0405, 0.0405, 0.9541, 0.8122};
        //Random rand = new Random();

        //for (int i = 0; i < initialTheta.Length; i++)
        //{
        //    initialTheta[i] = rand.NextDouble();
        //}

        double initialPValue = 1000;
        double lambda = 1.0;
        double alpha = 0.5;
        double alphaWeight = 0.8;

        rlsAlgorithm = new RLSAlgorithm(initialTheta, initialPValue, lambda, alpha, alphaWeight);
        yHatKPrev = Vector<double>.Build.Dense(3); // Initialize previous estimated output
        yPrev = Vector<double>.Build.Dense(3); // Initialize previous output

        // Initialize windows for moving window approach
        h1Window = new List<double>();
        h2Window = new List<double>();
        quWindow = new List<double>();
        uWindow  = new List<double>();
    }

    private void SaveThetaToFile(string filePath, int sampleNum, double[] theta)
    {
        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            string line = sampleNum.ToString() + "," + string.Join(",", theta);
            sw.WriteLine(line);
        }
    }
    public async Task Start()
    {
        //await _producerConsumer.OpenCommunication(MathModelOnlineInfo.ServiceName);
        //_producerConsumer.PurgeQueue(MessageRouting.ProcessDataQueue);

        //_producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
        //    new L2L2_LogMessage(MathModelOnlineInfo.ServiceName,
        //    "Mathematical Model Online Service has started.",
        //    Severity.Info, 1));

        //await _producerConsumer.ReadMessageFromQueueAsync(MessageRouting.ProcessDataQueue, async (body) =>
        //{
        //    Console.WriteLine("Receiving new message");
        //    var message = MessageDeserializationUtilities.DeserializeMessage(body);
        //    if (message is L2L2_ProcessData data)
        //    {
        //        await ProcessData(data);
        //    }

        //});


        string filePath = "C:\\Diplomski\\scripts\\matlab-mathematical_model\\simulation_output.txt"; // Path to your file

        //filePath = "C:\\Diplomski\\scripts\\matlab-mathematical_model\\Copy_of_simulation_output.txt";
        List<DataRecord> dataRecords = ReadDataFile(filePath);
        foreach (DataRecord dataRecord in dataRecords)
        {
            L1L2_ProcessData l1L2_ProcessData = new L1L2_ProcessData();
            l1L2_ProcessData.Sample = dataRecord.SampleNum;
            l1L2_ProcessData.WaterLevelTank1 = (float)dataRecord.H1Lin;
            l1L2_ProcessData.WaterLevelTank2 = (float)dataRecord.H2Lin;
            l1L2_ProcessData.InletFlow = (float)dataRecord.QuLin;
            l1L2_ProcessData.ValvePositionFeedback = (float)dataRecord.XvLin;


            L2L2_ProcessData data = new L2L2_ProcessData(l1L2_ProcessData, 1);
            ProcessData(data);
        }

            //var result1 = rlsAlgorithm.GetEstimatedParameters();
            //double[,] A_c1, B_c1;
            //ModelStatesCalc.CalculateContinuousFormFromDiscrete(out A_c1, out B_c1, result1.A_est, result1.B_est, rlsSample * 0.1);
            //var AcMatrix1 = Matrix<double>.Build.DenseOfArray(A_c1);
            //var timeConstants1 = ModelStatesCalc.CalculateTimeConstants(AcMatrix1);

            //SaveTimeConstantsToFile("timeConstantsFile", dataRecord.SampleNum, timeConstants1);



            //SaveThetaToFile("thetaFilePath", dataRecord.SampleNum, rlsAlgorithm.theta.ToArray());


            var result=rlsAlgorithm.GetEstimatedParameters();


            double[,] A_c, B_c;
            ModelStatesCalc.CalculateContinuousFormFromDiscrete(out A_c, out B_c, result.A_est, result.B_est, rlsSample*0.1);

            var AcMatrix = Matrix<double>.Build.DenseOfArray(A_c);

            var timeConstants = ModelStatesCalc.CalculateTimeConstants(AcMatrix);

            //// Print the results
            Console.WriteLine("Time Constants:");
            foreach (var timeConstant in timeConstants)
            {
                Console.WriteLine(timeConstant);
            }

        //Console.ReadKey();
        PlotData();
        }

    private double CalculateMovingAverage(List<double> window, int windowSize, double newValue)
    {
        if (window.Count >= windowSize)
        {
            window.RemoveAt(0);
        }
        window.Add(newValue);
        return window.Average();

        if (window.Count >= windowSize)
        {
            window.RemoveAt(0);
        }
        window.Add(newValue);
        var sortedWindow = window.OrderBy(x => x).ToList();
        int midIndex = sortedWindow.Count / 2;
        return (sortedWindow.Count % 2 != 0) ? sortedWindow[midIndex] : (sortedWindow[midIndex - 1] + sortedWindow[midIndex]) / 2.0;
    }

    public Task ProcessData(L2L2_ProcessData data)
    {
        bool isWrapAround = previousSample == int.MaxValue && data.ProcessData.Sample == 0;
        bool isOutOfOrder = previousSample != -1 && !isWrapAround && previousSample + 1 != data.ProcessData.Sample;

        if (previousSample == -1 || isWrapAround || isOutOfOrder)
        {
            _stateSpaceModel.IsInitialized = false;

        }

        // Collecting new data
        double h1Raw = data.ProcessData.WaterLevelTank1;
        double h2Raw = data.ProcessData.WaterLevelTank2;
        double quRaw = data.ProcessData.InletFlow;
        double xvRaw = data.ProcessData.ValvePositionFeedback;
        double qi = data.ProcessData.OutletFlow;
        DateTime time = data.ProcessData.GetDateTime();
        bool flag = data.ProcessData.IsPumpActive;

        if (_stateSpaceModel.IsInitialized is false)
        {
            if (h1Raw <= h2Raw)
            {
                h1Raw = h2Raw + 0.1;
            }
            if(h1Raw < 0)
            {
                h1Raw = 0;
            }
            if(h2Raw < 0)
            {
                h2Raw = 0;
            }
            _stateSpaceModel.InitializeStateSpaceModel(h2Raw, h1Raw, quRaw,xvRaw);
            ModelStatesCalc.CalculateDiscreteFormZOH(_stateSpaceModel.A, _stateSpaceModel.B);
            previousXv = xvRaw; // Initialize previousXv with the initial value of xv
            previousQu = quRaw;
            previousH1 = h1Raw;
            previousH2 = h2Raw;

            previousQuPredicted = quRaw;
            previousH1Predicted = h1Raw;
            previousH2Predicted = h2Raw;

            previousQuNonLin = quRaw;
            previousH1NonLin = h1Raw;
            previousH2NonLin = h2Raw;

            h1Window.Clear();
            h2Window.Clear();
            quWindow.Clear();
        }
        else
        {
            // Calculate the change in xv using previous data
            double deltaXv = previousXv - _stateSpaceModel.xv0;

            // Update u_current with the change in xv
            ModelStatesCalc.u_current[0, 0] = deltaXv;

            // Set current state to previous predicted state
            ModelStatesCalc.x_current = new double[,] { { previousQuPredicted - _stateSpaceModel.qu0 }, { previousH1Predicted - _stateSpaceModel.h10 }, { previousH2Predicted - _stateSpaceModel.h20 } };

            // Calculate the next state based on previous predicted state and input
            double[,] nextState = ModelStatesCalc.CalculateNextState();

            double delta_qu = nextState[0, 0];
            double delta_h1 = nextState[1, 0];
            double delta_h2 = nextState[2, 0];

            double qu_lin = delta_qu + _stateSpaceModel.qu0;
            double h1_lin = delta_h1 + _stateSpaceModel.h10;
            double h2_lin = delta_h2 + _stateSpaceModel.h20;

            // Calculate nonlinear model states based on previous predicted state
            (double qu_nonLin, double h1_nonLin, double h2_nonLin) = ModelStatesCalc.CalculateNonLinearModelStates(xvRaw, previousQuNonLin, previousH1NonLin, previousH2NonLin);

            double qmn1 = quRaw * (60 / 1000.0);
            double qmn2 = qu_nonLin * (60 / 1000.0);
            double qmn3 = qi * (60 / 1000.0);
            // Create and send dynamic data message
            //L2L2_DynamicData dynamicData = new L2L2_DynamicData(
            //    (float)xvRaw, (float)qmn1, (float)h1Raw, (float)h2Raw, (float)qmn2,
            //    (float)h1_nonLin, (float)h2_nonLin, (float)qmn3, time, flag, previousSample, data.ProcessData.TargetWaterLevelTank2, 1);

            //if (_producerConsumer.IsConnected())
            //{
            //    _producerConsumer.SendMessage(MessageRouting.SampleDataRoutingKey, dynamicData);
            //}
            //try
            //{
            //    _producerConsumer.SendMessage("sample-data-routing-key", dynamicData);
            //}
            //catch(Exception  e) 
            //{

            //}

            // Update RLS algorithm with new data
            if (previousSample % rlsSample == 0)
            {
                double h1 = CalculateMovingAverage(h1Window, windowSizeH1, h1Raw - _stateSpaceModel.h10);
                double h2 = CalculateMovingAverage(h2Window, windowSizeH2, h2Raw - _stateSpaceModel.h20);
                double qu = CalculateMovingAverage(quWindow, windowSizeQu, quRaw - _stateSpaceModel.qu0);
                //double xv = xvRaw;
                Vector<double> yK = Vector<double>.Build.DenseOfArray(new double[] { qu, h1, h2});
                yHatKPrev = rlsAlgorithm.Update(yK, yHatKPrev, deltaXv, yPrev);
                // Update previous output
                yPrev = yK;

                // Store the current theta parameters for convergence check
                thetaHistory.Enqueue(rlsAlgorithm.theta.ToArray());
                if (thetaHistory.Count > convergenceCheckInterval)
                {
                    thetaHistory.Dequeue(); // Remove the oldest theta parameters
                }

                // Check if the theta parameters have converged
                if (thetaHistory.Count == convergenceCheckInterval && CheckConvergence())
                {
                    //send theta parameters via rabbit (also update mathematical model), where should I call lqr calculation?
                    // L2L2_ModelParameters parameters = new L2L2_ModelParameters(rlsAlgorithm.theta.ToList(),1);

                    var result = rlsAlgorithm.GetEstimatedParameters();
                    double[,] A_c1, B_c1;
                    ModelStatesCalc.CalculateContinuousFormFromDiscrete(out A_c1, out B_c1, result.A_est, result.B_est, rlsSample * 0.1);
                    var AcMatrix = Matrix<double>.Build.DenseOfArray(A_c1);
                    var BcMatrix = Matrix<double>.Build.DenseOfArray(B_c1);

                    // Check for NaN values in A_c1 and B_c1
                    bool hasNaN = false;

                    // Check A_c1 for NaN values
                    for (int i = 0; i < AcMatrix.RowCount; i++)
                    {
                        for (int j = 0; j < AcMatrix.ColumnCount; j++)
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
                        for (int i = 0; i < BcMatrix.RowCount; i++)
                        {
                            for (int j = 0; j < BcMatrix.ColumnCount; j++)
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
                    if (!hasNaN)
                    {

                        // Initialize an array to hold the theta parameters
                        double[] theta = new double[7];

                        // Assign the values from the continuous matrices to the theta parameters
                        theta[0] = AcMatrix[0, 0];
                        theta[1] = AcMatrix[1, 0];
                        theta[2] = AcMatrix[1, 1];
                        theta[3] = AcMatrix[1, 2];
                        theta[4] = AcMatrix[2, 1];
                        theta[5] = AcMatrix[2, 2];
                        theta[6] = BcMatrix[0, 0];

                        //try
                        //{
                        //    _producerConsumer.SendMessage("sample-data-routing-key", parameters);
                        //}
                        //catch (Exception e)
                        //{

                        //}
                        double[]resuT =ModelStatesCalc.CalculateTimeConstants(AcMatrix);
                        foreach (var t in resuT)
                        {
                            Console.WriteLine(t);
                        }
                        Console.WriteLine("---------------------------------------------");

                        Console.WriteLine(A_c1.ToString());
                        Console.WriteLine(B_c1.ToString());

                        ModelStatesCalc.CalculateDiscreteFormZOH(A_c1, B_c1);

                    }
                }
            }

            // Update previous predicted state for next iteration
            previousQuPredicted = qu_lin;
            previousH1Predicted = h1_lin;
            previousH2Predicted = h2_lin;

            previousQuNonLin = qu_nonLin;
            previousH1NonLin = h1_nonLin;
            previousH2NonLin = h2_nonLin;

            // Update previous state
            previousQu = quRaw;
            previousH1 = h1Raw;
            previousH2 = h2Raw;
            previousXv = xvRaw;


            h1LinValues.Add(h1_lin);
            h1RawValues.Add(data.ProcessData.WaterLevelTank1);
            h2LinValues.Add(h2_lin);
            h2RawValues.Add(data.ProcessData.WaterLevelTank2);
            quLinValues.Add(qu_lin);
            quRawValues.Add(data.ProcessData.InletFlow);
            sampleNumbers.Add(data.ProcessData.Sample);
        }
        previousSample = data.ProcessData.Sample;
        return Task.CompletedTask;
    }
    public void PlotData()
    {
        // Plot 1: h1_lin vs. h1_raw
        var plt1 = new ScottPlot.Plot();
        plt1.Add.Scatter(sampleNumbers.ToArray(), h1LinValues.ToArray());
        plt1.Add.Scatter(sampleNumbers.ToArray(), h1RawValues.ToArray());
        plt1.Add.Legend();
        plt1.Title("h1_lin vs h1_raw");
        plt1.XLabel("Sample Number");
        plt1.YLabel("Values");
        plt1.SavePng("plot_h1.png", 1200, 1200);

        // Plot 2: h2_lin vs. h2_raw
        var plt2 = new ScottPlot.Plot();
        plt2.Add.Scatter(sampleNumbers.ToArray(), h2LinValues.ToArray());
        plt2.Add.Scatter(sampleNumbers.ToArray(), h2RawValues.ToArray());
        plt1.Add.Legend();
        plt2.Title("h2_lin vs h2_raw");
        plt2.XLabel("Sample Number");
        plt2.YLabel("Values");
        plt2.SavePng("plot_h2.png", 1200, 1200);

        // Plot 3: qu_lin vs. qu_raw
        var plt3 = new ScottPlot.Plot();
        plt3.Add.Scatter(sampleNumbers.ToArray(), quLinValues.ToArray());
        plt3.Add.Scatter(sampleNumbers.ToArray(), quRawValues.ToArray());
        plt1.Add.Legend();
        plt3.Title("qu_lin vs qu_raw");
        plt3.XLabel("Sample Number");
        plt3.YLabel("Values");
        plt3.SavePng("plot_qu.png", 1200, 1200);
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

    public void Stop()
    {
        //_producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
        //    new L2L2_LogMessage(MathModelOnlineInfo.ServiceName,
        //    "Mathematical Model Online Service has stopped.",
        //    Severity.Warning, 1));

        _producerConsumer.Dispose();
    }

    private List<double> h1LinValues = new List<double>();
    private List<double> h1RawValues = new List<double>();
    private List<double> h2LinValues = new List<double>();
    private List<double> h2RawValues = new List<double>();
    private List<double> quLinValues = new List<double>();
    private List<double> quRawValues = new List<double>();
    private List<double> sampleNumbers = new List<double>();
    public static List<DataRecord> ReadDataFile(string filePath)
    {
        var dataRecords = new List<DataRecord>();

        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(',');

            if (parts.Length == 5)
            {
                var record = new DataRecord
                {
                    SampleNum = int.Parse(parts[0]),
                    H1Lin = double.Parse(parts[1]),
                    H2Lin = double.Parse(parts[2]),
                    QuLin = double.Parse(parts[3]),
                    XvLin = double.Parse(parts[4])
                };

                dataRecords.Add(record);
            }
        }

        return dataRecords;
    }

    


    public class DataRecord
    {
        public int SampleNum { get; set; }
        public double H1Lin { get; set; }
        public double H2Lin { get; set; }
        public double QuLin { get; set; }
        public double XvLin { get; set; }

        public override string ToString()
        {
            return $"{SampleNum},{H1Lin},{H2Lin},{QuLin},{XvLin}";
        }
    }
}


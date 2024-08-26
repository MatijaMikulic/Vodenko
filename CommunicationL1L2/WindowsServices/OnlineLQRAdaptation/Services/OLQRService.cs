using MessageBroker.Common.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using System.Diagnostics;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using SharedResources.Constants;
using SharedLibrary.Entities;
using DataAccess.Repositories;
namespace OnlineLQRAdaptation.Services
{
    public class OLQRService
    {
        private readonly IProducerConsumer _producerConsumer;
        private readonly DatabaseRepositories _databaseRepositories; // Database repositories

        // Private members to store the LQR gain and the closed-loop matrix
        private Matrix<double> K;
        private Matrix<double> Acl;
        

        public double[][] C { get; set; } = new double[][] {
            new double[] {0.0, 0.0 ,1.0}
        };
        public double[][] D { get; set; } = new double[][] {
            new double[] {0.0}
        };
        public OLQRService(IProducerConsumer producerConsumer, DatabaseRepositories databaseRepos)
        {
            _producerConsumer = producerConsumer;
            _databaseRepositories = databaseRepos;
        }

        public async Task Start()
        {

            await _producerConsumer.OpenCommunication("Online LQR");
            _producerConsumer.PurgeQueue(MessageRouting.MMServicePrivateQueue);

            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
            new L2L2_LogMessage("Online LQR",
            "Online LQR Service has started successfully.",
            Severity.Info, 1));

            await _producerConsumer.ReadMessageFromQueueAsync(MessageRouting.MMServicePrivateQueue, async (body) =>
            {
                //Console.WriteLine("Receiving new message");
                var message = MessageDeserializationUtilities.DeserializeMessage(body);
                if (message is L2L2_ModelParameters modelParameters)
                {
                    double[][] A = new double[3][];
                    A[0] = new double[3];
                    A[1] = new double[3];
                    A[2] = new double[3];

                    A[0][0] = modelParameters.Parameters[0];
                    A[1][0] = modelParameters.Parameters[1];
                    A[1][1] = modelParameters.Parameters[2];
                    A[1][2] = modelParameters.Parameters[3];
                    A[2][1] = modelParameters.Parameters[4];
                    A[2][2] = modelParameters.Parameters[5];

                    double[][] B = new double[3][];
                    B[0] = new double[1];
                    B[1] = new double[1];
                    B[2] = new double[1];

                    B[0][0] = modelParameters.Parameters[6];
                    B[1][0] = 0;
                    B[2][0] = 0;

                    double[][] Q = new double[][]
                    {
                        new double[] { 1, 0, 0, 0 },
                        new double[] { 0, 1, 0, 0 },
                        new double[] { 0, 0, 3, 0 },
                        new double[] { 0, 0, 0, 3 }
                    };

                    double[][] R = new double[][]
                    {
                         new double[] { 10 }
                    };

                    var data = new
                    {
                        A = A,
                        B = B,
                        C = C,
                        D = D,
                        Q = Q,
                        R = R
                    };

                    var httpClient = new HttpClient();

                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("http://127.0.0.1:5000/calculate-lqr-only", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<LQRData>(responseBody);
                        Console.WriteLine(res);

                        Message lqrParams = new Message
                        {
                            MessageId = 2,
                            Status = 0,
                            EnqueueDT = DateTime.Now,
                            PayloadDictionary = new Dictionary<string, string>
                            {
                                { "Method", "0" },
                                { "Proportional", "-1" },
                                { "Integral", "-1" },
                                { "Derivative", "-1" },
                                { "K1", res.Kx1.ToString() },
                                { "K2", res.Kx2.ToString() },
                                { "K3", res.Kx3.ToString() },
                                { "K4", res.Ki.ToString() }
                            },
                            RetryCount = 0,
                            ErrorLog = " ",
                            DequeueDT = DateTime.Now,
                        };

                        await _databaseRepositories.MessageRepository.AddAsync(lqrParams);

                    }
                    else
                    {
                        throw new Exception("Error: Unable to get LQR gain");
                    }

                }

            });
          
            
        }

        public void Stop()
        {
            _producerConsumer.Dispose();
        }

        private static (Matrix<double> A_aug, Matrix<double> B_aug, Matrix<double> C_aug) AugmentSystem(Matrix<double> A, Matrix<double> B, Matrix<double> C)
        {
            int n = A.RowCount;
            int m = B.ColumnCount;

            var A_aug = DenseMatrix.OfArray(new double[n + 1, n + 1]);
            A_aug.SetSubMatrix(0, n, 0, n, A);
            A_aug.SetSubMatrix(n, 1, 0, n, -C);

            var B_aug = DenseMatrix.OfArray(new double[n + 1, m]);
            B_aug.SetSubMatrix(0, n, 0, m, B);
            B_aug[n, 0] = 0;  // Integral action

            // The last row should handle the integral action
            A_aug[n, n] = 0; // This is the row corresponding to the integrator

            var C_aug = DenseMatrix.OfArray(new double[C.RowCount, C.ColumnCount + 1]);
            C_aug.SetSubMatrix(0, C.RowCount, 0, C.ColumnCount, C);

            return (A_aug, B_aug, C_aug);
        }

    }
    public class LQRData
    {
        public double Kx1 { get; set; }
        public double Kx2 { get; set; }
        public double Kx3 { get; set; }
        public double Ki { get; set; }

    }
}

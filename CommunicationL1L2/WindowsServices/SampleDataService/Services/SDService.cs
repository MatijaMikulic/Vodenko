using SharedLibrary.Entities;
using DataAccess.Repositories;
using MessageBroker.Common.Producer;
using MessageModel.Model.DataBlockModel;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using SharedResources.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using SampleDataService.Constants;

namespace SampleDataService.Services
{
    /// <summary>
    /// Service responsible for generating sample data, processing incoming messages, and storing data in the database.
    /// </summary>
    public class SDService
    {
        private readonly IProducerConsumer _producerConsumer;        // RabbitMQ producer-consumer interface
        private readonly DatabaseRepositories _databaseRepositories; // Database repositories
        private readonly System.Timers.Timer _timer;                 // Timer for generating sample data
        private readonly List<L2L2_DynamicData> _dataList;           // List to store dynamic data for bulk insertion
        private const int BulkInsertThreshold = 50;                  // Threshold for bulk inserting data
        private int _sample;                                         // Sample counter
        private readonly Random _random;                             // Random number generator for noise generation

        /// <summary>
        /// Initializes a new instance of the <see cref="SDService"/> class.
        /// </summary>
        /// <param name="producerConsumer">The RabbitMQ producer-consumer interface.</param>
        /// <param name="databaseRepositories">The database repositories.</param>
        public SDService(IProducerConsumer producerConsumer, DatabaseRepositories databaseRepositories)
        {
            _producerConsumer = producerConsumer;
            _databaseRepositories = databaseRepositories;
            _dataList = new List<L2L2_DynamicData>();
            _sample = 0;
            _random = new Random();
        }

        /// <summary>
        /// Starts the SDService, establishing communication with RabbitMQ and starting the timer.
        /// </summary>
        public async Task Start()
        {
            await _producerConsumer.OpenCommunication(SampleDataInfo.ServiceName);
            _producerConsumer.PurgeQueue(MessageRouting.SampleDataQueue);

            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(SampleDataInfo.ServiceName,
                "Sample Data Service has started successfully.",
                Severity.Info, 1));

            await _producerConsumer.ReadMessageFromQueueAsync(MessageRouting.SampleDataQueue, async (body) =>
            {
                var message = MessageDeserializationUtilities.DeserializeMessage(body);
                if (message is L2L2_DynamicData data)
                {
                    await ProcessDataAsync(data);
                }
                if (message is L2L2_Alarms al)
                {
                    Alarm alarm = new Alarm
                    {
                        AlarmCodeId = al.Alarm.AlarmNo,
                        DateTime = al.Alarm.GetDateTime()
                    };

                    await _databaseRepositories.AlarmRepository.AddAsync(alarm);
                }
                if (message is L2L2_ModelParameters modelParameters)
                {
                    ModelParameters parameters = new ModelParameters
                    {
                        Theta1 = modelParameters.Parameters[0],
                        Theta2 = modelParameters.Parameters[1],
                        Theta3 = modelParameters.Parameters[2],
                        Theta4 = modelParameters.Parameters[3],
                        Theta5 = modelParameters.Parameters[4],
                        Theta6 = modelParameters.Parameters[5],
                        Theta7 = modelParameters.Parameters[6],
                    };
                    await _databaseRepositories.ModelParametersRepository.AddAsync(parameters);
                    _producerConsumer.SendMessage(MessageRouting.GeneralDataRoutingKey, message);
                }
            });
        }

        /// <summary>
        /// Processes the dynamic data received from the queue and performs bulk insertion if the threshold is met.
        /// </summary>
        private async Task ProcessDataAsync(L2L2_DynamicData data)
        {
            lock (_dataList)
            {
                _dataList.Add(data);

                if (_dataList.Count >= BulkInsertThreshold)
                {
                    List<L2L2_DynamicData> bulkInsertList = new List<L2L2_DynamicData>(_dataList);
                    _dataList.Clear();

                    Task.Run(() =>
                    {
                        try
                        {
                            var entities = bulkInsertList.Select(d => new DynamicData
                            {
                                ValvePositionFeedback = d.ValvePositionFeedback,
                                InletFlow = d.InletFlow,
                                WaterLevelTank1 = d.WaterLevelTank1,
                                WaterLevelTank2 = d.WaterLevelTank2,
                                InletFlowNonLinModel = d.InletFlowNonLinModel,
                                WaterLevelTank1NonLinModel = d.WaterLevelTank1NonLinModel,
                                WaterLevelTank2NonLinModel = d.WaterLevelTank2NonLinModel,
                                InletFlowLinModel = d.InletFlowLinModel,
                                WaterLevelTank1LinModel = d.WaterLevelTank1LinModel,
                                WaterLevelTank2LinModel = d.WaterLevelTank2LinModel,
                                OutletFlow = d.OutletFlow,
                                DateTime = d.DateTime,
                                IsPumpActive = d.IsPumpActive,
                                Sample = d.Sample,
                                Target = d.TargetWaterLevelTank2Model
                            }).ToList();

                            _databaseRepositories.DynamicDataRepository.BulkInsert(entities);
                        }
                        catch (Exception ex)
                        {
                            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                                new L2L2_LogMessage(SampleDataInfo.ServiceName,
                                "Sample Data Service has failed to bulk insert DynamicData!",
                                Severity.Error, 1));
                        }
                    });
                }
            }

            _producerConsumer.SendMessage(MessageRouting.DynamicDataRoutingKey, data);
        }

        /// <summary>
        /// Stops the SDService, performs final bulk insertion if needed, and disposes resources.
        /// </summary>
        public void Stop()
        {
            lock (_dataList)
            {
                if (_dataList.Count > 0)
                {
                    var remainingData = new List<L2L2_DynamicData>(_dataList);
                    _dataList.Clear();

                    try
                    {
                        var entities = remainingData.Select(d => new DynamicData
                        {
                            ValvePositionFeedback = d.ValvePositionFeedback,
                            InletFlow = d.InletFlow,
                            WaterLevelTank1 = d.WaterLevelTank1,
                            WaterLevelTank2 = d.WaterLevelTank2,
                            InletFlowNonLinModel = d.InletFlowNonLinModel,
                            WaterLevelTank1NonLinModel = d.WaterLevelTank1NonLinModel,
                            WaterLevelTank2NonLinModel = d.WaterLevelTank2NonLinModel,
                            InletFlowLinModel = d.InletFlowLinModel,
                            WaterLevelTank1LinModel = d.WaterLevelTank1LinModel,
                            WaterLevelTank2LinModel = d.WaterLevelTank2LinModel,
                            OutletFlow = d.OutletFlow,
                            DateTime = d.DateTime,
                            IsPumpActive = d.IsPumpActive,
                            Sample = d.Sample,
                            Target = d.TargetWaterLevelTank2Model                           
                        }).ToList();

                        _databaseRepositories.DynamicDataRepository.BulkInsert(entities);
                    }
                    catch (Exception ex)
                    {
                        _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                            new L2L2_LogMessage(SampleDataInfo.ServiceName,
                            "Sample Data Service has failed to bulk insert DynamicData!",
                            Severity.Error, 1));
                    }
                }
            }

            _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey,
                new L2L2_LogMessage(SampleDataInfo.ServiceName,
                "Sample Data Service has exited.",
                Severity.Warning, 1));
            _producerConsumer.Dispose();
        }
    }
}

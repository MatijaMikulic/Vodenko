using MessageBroker.Common.Producer;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml;
using System.Collections.Concurrent;
using System.Globalization;
using VodenkoWeb.Hubs;
using VodenkoWeb.Model;

namespace VodenkoWeb.Services
{
    public class DynamicDataFetchService:BackgroundService
    {
        private readonly DynamicDataBuffer _dataBuffer;
        private readonly IHubContext<ChartHub> _hubContext;
        private readonly IProducerConsumer _producerConsumer;


        private readonly RecordService _recordService;

        public DynamicDataFetchService(DynamicDataBuffer dataBuffer, IHubContext<ChartHub> hubContext,IProducerConsumer producerConsumer, RecordService recordService)
        {
            _dataBuffer = dataBuffer;
            _hubContext = hubContext;
            _producerConsumer = producerConsumer;
            _recordService = recordService;

        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _producerConsumer.OpenCommunication("VodenkoWeb-DynamicData");
            _producerConsumer.PurgeQueue("DynamicDataQueue");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
                await _producerConsumer.ReadMessageFromQueueAsync("DynamicDataQueue", async (body) =>
                {
                    var message = MessageDeserializationUtilities.DeserializeMessage(body);
                    if (message is L2L2_DynamicData data)
                    {
                        
                        DataPoints dataPoints = new DataPoints(
                            data.ValvePositionFeedback,
                            data.InletFlow,
                            data.WaterLevelTank1,
                            data.WaterLevelTank2,
                            data.InletFlowNonLinModel,
                            data.WaterLevelTank1NonLinModel,
                            data.WaterLevelTank2NonLinModel,
                            data.InletFlowLinModel,
                            data.WaterLevelTank1LinModel,
                            data.WaterLevelTank2LinModel,
                            data.OutletFlow,
                            data.DateTime,
                            data.IsPumpActive,
                            data.Sample,
                            data.TargetWaterLevelTank2Model
                            );

                        await _hubContext.Clients.All.SendAsync(
                            "updateData",
                            _dataBuffer.AddNewData(dataPoints),
                            cancellationToken: stoppingToken
                        );

                        _recordService.RecordData(dataPoints);
                    }
                });           
        }

        public override void Dispose()
        {
            _producerConsumer.Dispose();
            base.Dispose();
        }
    }
}

using MessageBroker.Common.Producer;
using MessageModel.Model.Messages;
using MessageModel.Utilities;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using VodenkoWeb.Hubs;
using VodenkoWeb.Model;
using Microsoft.Extensions.Logging;
using VodenkoWeb.Services;
using ModelProvider.Interfaces;
using ModelProvider.Models;
using DataAccess.Entities;
using System.Drawing.Drawing2D;
using MessageModel.Model.DataBlockModel;

namespace VodenkoWeb.Services
{
    public class CachingService : BackgroundService
    {
        private readonly IModelProvider _modelProvider;
        private readonly IHubContext<CacheHub> _hubContext;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IProducerConsumer _producerConsumer;
        private readonly ILogger<CachingService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CachingService(
            IModelProvider modelProvider,
            IHubContext<CacheHub> hubContext,
            IHubContext<NotificationHub> notificationHub,
            IProducerConsumer producerConsumer,
            ILogger<CachingService> logger,
            IServiceProvider serviceProvider)
        {
            _modelProvider = modelProvider;
            _hubContext = hubContext;
            _notificationHubContext = notificationHub;
            _producerConsumer = producerConsumer;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _producerConsumer.OpenCommunication("VodenkoWeb-Cache");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _producerConsumer.ReadMessageFromQueueAsync("GeneralDataQueue", async (body) =>
            {
                try
                {
                    var message = MessageDeserializationUtilities.DeserializeMessage(body);
                    await ProcessMessageAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message.");
                }
            });
        }

        private async Task ProcessMessageAsync(object message, CancellationToken stoppingToken)
        {
            switch (message)
            {
                case L2L2_ControllerParams cntParams:
                    await HandleControllerParamsAsync(cntParams, stoppingToken);
                    break;
                case L2L2_SystemStatus sysStatus:
                    await HandleSystemStatusAsync(sysStatus, stoppingToken);
                    break;
                case L2L2_ControlMode cntMode:
                    await HandleControlModeAsync(cntMode, stoppingToken);
                    break;
                case L2L2_PlcConnectionStatus plcConnectionStatus:
                    await HandlePlcConnectionStatusAsync(plcConnectionStatus, stoppingToken);
                    break;
                case L2L2_ModelParameters modelParameters:
                    HandleModelParameters(modelParameters);
                    break;
            }
        }

        private async Task HandleControllerParamsAsync(L2L2_ControllerParams cntParams, CancellationToken stoppingToken)
        {
            var controlParameters = new ControllerParameters(
                cntParams.ControllerParams.Method,
                cntParams.ControllerParams.Proportional,
                cntParams.ControllerParams.Integral,
                cntParams.ControllerParams.Derivative,
                cntParams.ControllerParams.K1,
                cntParams.ControllerParams.K2,
                cntParams.ControllerParams.K3,
                cntParams.ControllerParams.K4
            );

            var currentParams = _modelProvider.ControllerParameters.Parameters;
            if (!AreControllerParamsEqual(currentParams, controlParameters))
            {
                _modelProvider.UpdateControllerParameters(controlParameters);
                await _hubContext.Clients.All.SendAsync("updateControlParameters", controlParameters, stoppingToken);

                string notificationMessage = controlParameters.Method switch
                {
                    1 or 0 => $"new PID parameters detected:," +
                    $" P: {controlParameters.Proportional}," +
                    $" I: {controlParameters.Integral}," +
                    $" D: {controlParameters.Derivative}",
                    2 => $"new LQR parameters detected:, " +
                    $"Kx1: {controlParameters.K1}, " +
                    $"Kx2: {controlParameters.K2}, " +
                    $"Kx3: {controlParameters.K3}, " +
                    $"Ki:  {controlParameters.K4}",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(notificationMessage))
                {
                    await SendNotificationAsync(notificationMessage, stoppingToken);
                }
            }
        }

        private async Task HandleSystemStatusAsync(L2L2_SystemStatus sysStatus, CancellationToken stoppingToken)
        {
            var systemStatus = new PlantStatus(
                sysStatus.SystemStatus.IsPumpActive,
                sysStatus.SystemStatus.IsLowLevelSwitchActive,
                sysStatus.SystemStatus.IsProportionalValveActive,
                sysStatus.SystemStatus.IsTank1CrtiticalLevel,
                sysStatus.SystemStatus.IsTank2CrtiticalLevel,
                sysStatus.SystemStatus.IsReady()
            );

            var currentStatus = _modelProvider.PlantStatus.Status;
            if (!ArePlantStatusesEqual(currentStatus, systemStatus))
            {
                _modelProvider.UpdatePlantStatus(systemStatus);
                await _hubContext.Clients.All.SendAsync("updateSystemStatus", systemStatus, stoppingToken);

                string notificationMessage = $"System status updated:, " +
                                             $"PumpActive:                         {systemStatus.PumpActive}, " +
                                             $"Low Level Switch:                   {systemStatus.LevelSwitch}, " +
                                             $"Proportional Valve Operating Range: {systemStatus.ValveOperatingRange}, " +
                                             $"Tank 1 Critical Level:              {systemStatus.Tank1Level}, " +
                                             $"Tank 2 Critical Level:              {systemStatus.Tank2Level}, " +
                                             $"System Ready:                       {systemStatus.SystemReady}";

                await SendNotificationAsync(notificationMessage, stoppingToken);
            }
        }

        private async Task HandleControlModeAsync(L2L2_ControlMode cntMode, CancellationToken stoppingToken)
        {
            var plcMode = new PlcMode(cntMode.ControlMode.ControlMode, GetControlModeDescription(cntMode.ControlMode.ControlMode));
            var currentMode = _modelProvider.PlcMode.Mode;

            if (currentMode.ControlMode != plcMode.ControlMode)
            {
                _modelProvider.UpdatePlcMode(plcMode);
                await _hubContext.Clients.All.SendAsync("updateControlMode", plcMode, stoppingToken);

                string notificationMessage = $"New control mode set:," +
                                             $" Mode: {plcMode.Description}";
                await SendNotificationAsync(notificationMessage, stoppingToken);
            }
        }

        private async Task HandlePlcConnectionStatusAsync(L2L2_PlcConnectionStatus plcConnectionStatus, CancellationToken stoppingToken)
        {
            var plcStatus = new PlcConnectionStatus(plcConnectionStatus.IsConnected);
            var currentStatus = _modelProvider.PlcConnectionStatus.Status;

            if (currentStatus.IsConnected != plcStatus.IsConnected)
            {
                _modelProvider.UpdatePlcConnectionStatus(plcStatus);
                await _hubContext.Clients.All.SendAsync("updatePlcConnectionStatus", plcStatus, stoppingToken);

                string notificationMessage = $"PLC Connection status updated:," +
                                             $" Connection: {plcStatus.IsConnected}";
                await SendNotificationAsync(notificationMessage, stoppingToken);
            }
        }

        private void HandleModelParameters(L2L2_ModelParameters modelParameters)
        {
            var A = new double[][]
            {
                new double[] { modelParameters.Parameters[0], 0, 0 },
                new double[] { modelParameters.Parameters[1], modelParameters.Parameters[2], modelParameters.Parameters[3] },
                new double[] { 0, modelParameters.Parameters[4], modelParameters.Parameters[5] }
            };

            var B = new double[][]
            {
                new double[] { modelParameters.Parameters[6] },
                new double[] { 0 },
                new double[] { 0 }
            };

            var mathematicalModel = new MathematicalModel(A, B);
            // Update the model provider with the new mathematical model if needed
        }

        private async Task SendNotificationAsync(string message, CancellationToken stoppingToken)
        {
            var notification = new Notification { Message = message, IsRead = false };

            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.AddNotification(notification);
                await _notificationHubContext.Clients.All.SendAsync("ReceiveNotification", notification, stoppingToken);
            }
        }

        private string GetControlModeDescription(ushort mode) => mode switch
        {
            0 => "Auto mode",
            1 => "Semi auto mode",
            _ => "Manual mode"
        };

        private bool AreControllerParamsEqual(ControllerParameters currentParams, ControllerParameters newParams)
        {
            return currentParams.Method == newParams.Method &&
                   currentParams.Proportional == newParams.Proportional &&
                   currentParams.Integral == newParams.Integral &&
                   currentParams.Derivative == newParams.Derivative &&
                   currentParams.K1 == newParams.K1 &&
                   currentParams.K2 == newParams.K2 &&
                   currentParams.K3 == newParams.K3 &&
                   currentParams.K4 == newParams.K4;
        }

        private bool ArePlantStatusesEqual(PlantStatus currentStatus, PlantStatus newStatus)
        {
            return currentStatus.PumpActive == newStatus.PumpActive &&
                   currentStatus.LevelSwitch == newStatus.LevelSwitch &&
                   currentStatus.ValveOperatingRange == newStatus.ValveOperatingRange &&
                   currentStatus.Tank1Level == newStatus.Tank1Level &&
                   currentStatus.Tank2Level == newStatus.Tank2Level &&
                   currentStatus.SystemReady == newStatus.SystemReady;
        }

        public override void Dispose()
        {
            _producerConsumer.Dispose();
            base.Dispose();
        }
    }

    public static class SystemStatusExtensions
    {
        public static bool IsReady(this L1L2_SystemStatus systemStatus) =>
            systemStatus.IsPumpActive
            && !systemStatus.IsLowLevelSwitchActive
            && systemStatus.IsProportionalValveActive
            && !systemStatus.IsTank1CrtiticalLevel
            && !systemStatus.IsTank2CrtiticalLevel;
    }
}


using DataMonitoringService.Constants;
using MessageBroker.Common.Producer;
using MessageModel.Model.Messages;
using TaskLog.Contracts;
using PlcCommunication.Model;
using PlcCommunication.Interfaces;
using SharedResources.Constants;

namespace DataMonitoringService.Services
{
    /// <summary>
    ///   Polls the PLC circular buffers and publishes <see cref="L2L2_DataBlockHeader"/>
    ///   messages to Rabbit MQ. Keeps running until <see cref="CancellationToken"/> is cancelled.
    /// </summary>
    public sealed class DataMonitoring
    {
        private readonly IProducerConsumer _producerConsumer;
        private readonly IConnectionManager _connectionManager;
        private readonly IPlcDataAccess _dataAccess;
        private readonly ILogger _log;

        private readonly Dictionary<ushort, DataBlockMetaData> _prevStates = new();
        private readonly Dictionary<ushort, int> _warmupCounts = new();
        private const int WarmupMessages = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMonitoring"/> class.
        /// </summary>
        /// <param name="mq">The RabbitMQ producer-consumer interface.</param>
        /// <param name="conn">The PLC communication service.</param>
        /// <param name="log">The logger interface.</param>
        public DataMonitoring(
             IProducerConsumer mq,
             IConnectionManager conn,
             IPlcDataAccess data,
             ILogger log)
        {
            _producerConsumer = mq;
            _connectionManager = conn;
            _dataAccess = data;
            _log = log;
        }

        /// <inheritdoc cref="RunAsync"/>
        public Task RunAsync(CancellationToken ct) => RunInternalAsync(ct);

        /// <summary>
        /// Connects to PLC & MQ, seeds state, then enters the polling loop
        /// until <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        public async Task RunInternalAsync(CancellationToken cancellationToken)
        {
            // 1) Start PLC communication
            try
            {
                _connectionManager.Open();
            }
            catch (Exception ex)
            {
                _log.Log(new L2L2_LogMessage(
                    DataMonitoringServiceInfo.ServiceName,
                    $"Initial PLC open failed: {ex.Message}",
                    Severity.Warning, 1));
            }
            _connectionManager.ConnectionStatusChanged += OnPlcConnectionChanged;

            // 2) Start RabbitMQ
            await _producerConsumer.OpenCommunication(DataMonitoringServiceInfo.ServiceName).ConfigureAwait(false);

            if (_producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(
                    MessageRouting.LoggerRoutingKey,
                    new L2L2_LogMessage(
                        DataMonitoringServiceInfo.ServiceName,
                        "Data Monitoring Service started",
                        Severity.Info, 1));
            }

            // 3) seed initial metadata & warmup counters
            var initial = (await _dataAccess.ReadMetaDataAsync().ConfigureAwait(false)).ToList();
            foreach (var meta in initial)
            {
                _prevStates[meta.DB] = meta;
                _warmupCounts[meta.DB] = 0;
            }

            // 4) Poll in a loop every 100 ms until cancelled
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        await PollOnceAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _log.Log(new L2L2_LogMessage(
                            DataMonitoringServiceInfo.ServiceName,
                            $"PollOnceAsync failed: {ex.Message}",
                            Severity.Error,
                            1));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
        }

        private async Task PollOnceAsync()
        {
            if (!_connectionManager.IsReady)
            {
                _connectionManager.Open();
                return;
            }

            List<DataBlockMetaData> current;
            try
            {
                current = (await _dataAccess.ReadMetaDataAsync().ConfigureAwait(false)).ToList();
            }
            catch (Exception ex)
            {
                _log.Log(new L2L2_LogMessage(
                   DataMonitoringServiceInfo.ServiceName,
                   $"PLC metadata read failed: {ex.Message}",
                   Severity.Warning, 1));
                return;
            }

            foreach (var now in current)
            {
                if (!_prevStates.TryGetValue(now.DB, out var prev))
                {
                     // new DB unexpectedly appeared—seed state
                    _prevStates[now.DB] = now;
                    _warmupCounts[now.DB] = 0;
                    continue;
                }

                bool hasNew = now.ChangeCounter != prev.ChangeCounter
                           || now.AuxiliaryCounter != prev.AuxiliaryCounter;
                _prevStates[now.DB] = now;
                if (!hasNew) continue;

                // per‑block warmup suppression
                int warmups = ++_warmupCounts[now.DB];
                if (warmups <= WarmupMessages) continue;

                int ready = (now.AuxiliaryCounter - prev.AuxiliaryCounter
                           + ushort.MaxValue + 1) % (ushort.MaxValue + 1);
                if (ready > now.BufferSize)
                {
                    LogDataLoss(now, ready);
                    ready = now.BufferSize;
                }

                PublishBatch(now, ready);
            }
        }

        private void PublishBatch(DataBlockMetaData meta, int count)
        {
            int start = (meta.FindBufferPointer()
                         - count + meta.BufferSize)
                        % meta.BufferSize;

            for (int j = 0; j < count; j++)
            {
                int ptr = (start + j + meta.BufferSize) % meta.BufferSize;
                if (ptr == 0) ptr = meta.BufferSize;

                var hdr = new L2L2_DataBlockHeader(meta.DB, (ushort)ptr, 1);
                try
                {
                    _producerConsumer.SendMessage(MessageRouting.DataRoutingKey, hdr);
                }
                catch (Exception ex)
                {
                    _log.Log(new L2L2_LogMessage(
                        DataMonitoringServiceInfo.ServiceName,
                        $"Failed to send data header: {ex.Message}",
                        Severity.Error, 1));
                }
            }
        }

        private void LogDataLoss(DataBlockMetaData meta, int ready)
        {
            int lost = ready - meta.BufferSize;
            var msg = new L2L2_LogMessage(
                DataMonitoringServiceInfo.ServiceName,
                $"Data loss in DB {meta.DB}: {lost} items",
                Severity.Warning, 1);

            if (_producerConsumer != null && _producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(MessageRouting.LoggerRoutingKey, msg);
            }
            _log.Log(msg);
        }

        private void OnPlcConnectionChanged(object? sender, bool isUp)
        {
            var sev = isUp ? Severity.Info : Severity.Warning;
            var text = isUp ? "PLC connected" : "PLC disconnected";

            if (_producerConsumer != null && _producerConsumer.IsConnected())
            {
                _producerConsumer.SendMessage(
                    MessageRouting.GeneralDataRoutingKey,
                    new L2L2_PlcConnectionStatus(isUp, 1));
            }
            _log.Log(new L2L2_LogMessage(
                DataMonitoringServiceInfo.ServiceName,
                text, sev, 1));
        }
    }
}


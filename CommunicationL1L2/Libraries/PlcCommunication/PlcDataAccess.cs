using MessageModel.Model.Messages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PlcCommunication.Interfaces;
using PlcCommunication.Model;
using S7.Net;
using S7.Net.Types;
using System.Net.Sockets;
namespace PlcCommunication
{
    /// <summary>
    /// Provides access and operations for reading and writing data to PLC Data blocks.
    /// </summary>
    public class PlcDataAccess : IPlcDataAccess
    {
        private readonly IList<DataBlockConfig> _cfgs;
        private readonly IConnectionManager _connectionManager;

        /// <summary>
        /// Initializes a new instance of the PlcDataAccess class.
        /// </summary>
        /// <param name="connectionManager"></param>
        /// <param name="options"></param>
        public PlcDataAccess(
            IConnectionManager connectionManager,
            IOptions<PlcCommunicationOptions> options)
        {
            this._cfgs = options.Value.DataBlocks;
            this._connectionManager = connectionManager;
        }

        // ─── GENERIC ───────────────────────────────────────────────
        /// <inheritdoc />
        public TModel ReadContent<TModel>(ushort dbId, ushort ptr)
             where TModel : new()
        {
            var cfg = _cfgs.FirstOrDefault(c => c.Id == dbId);
            return SafeExecute(() =>
            {
                int offset = ComputeOffset(cfg, ptr);
                var inst = new TModel();
                _connectionManager.PlcInstance.ReadClass(inst, dbId, offset);
                return inst;
            });
        }
        // ─── DYNAMIC ────────────────────────────────────────────────
        /// <inheritdoc />
        public object ReadContent(ushort dbId, ushort ptr)
        {
            var cfg = _cfgs.FirstOrDefault(c => c.Id == dbId);
            return SafeExecute(() =>
            {
                int offset = ComputeOffset(cfg, ptr);
                var inst = Activator.CreateInstance(cfg.ModelType)
                           ?? throw new InvalidOperationException(
                                $"Could not create instance of {cfg.ModelType.Name}");
                _connectionManager.PlcInstance.ReadClass(inst, dbId, offset);
                return inst;
            });
        }

        // ─── METADATA ───────────────────────────────────────────────
        /// <inheritdoc />
        public IReadOnlyList<DataBlockMetaData> ReadMetaData()
        {
            // build DataItem lists
            var changes = BuildDataItem(cfg => cfg.ChangeCounterStart);
            var pointers = BuildDataItem(cfg => cfg.BufferPointerStart);
            var auxs = BuildDataItem(cfg => cfg.AuxCounterStart);

            // single‐shot reads
            SafeExecute(() => _connectionManager.PlcInstance.ReadMultipleVars(changes));
            SafeExecute(() => _connectionManager.PlcInstance.ReadMultipleVars(pointers));
            SafeExecute(() => _connectionManager.PlcInstance.ReadMultipleVars(auxs));

            // zip into metadata
            return changes
                .Zip(pointers, (ch, pt) => (ch, pt))
                .Zip(auxs, (cp, au) => (cp.ch, cp.pt, au))
                .Select(tuple => {
                    ushort dbId = (ushort)tuple.ch.DB;
                    var cfg = _cfgs.Single(c => c.Id == dbId);
                    return new DataBlockMetaData(
                        changeCounter: (ushort)tuple.ch.Value!,
                        auxiliaryCounter: (ushort)tuple.au.Value!,
                        bufferPointer: (ushort)tuple.pt.Value!,
                        dB: dbId,
                        bufferSize: cfg.Size
                    );
                })
                .ToList();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<DataBlockMetaData>> ReadMetaDataAsync()
        {
            var changes = BuildDataItem(cfg => cfg.ChangeCounterStart);
            var pointers = BuildDataItem(cfg => cfg.BufferPointerStart);
            var auxs = BuildDataItem(cfg => cfg.AuxCounterStart);

            // fire off all three reads in parallel
            var taskChange = SafeExecuteAsync(() => _connectionManager.PlcInstance.ReadMultipleVarsAsync(changes));
            var taskPointer = SafeExecuteAsync(() => _connectionManager.PlcInstance.ReadMultipleVarsAsync(pointers));
            var taskAux = SafeExecuteAsync(() => _connectionManager.PlcInstance.ReadMultipleVarsAsync(auxs));

            await Task.WhenAll(taskChange, taskPointer, taskAux).ConfigureAwait(false);

            var ch = await taskChange;
            var pt = await taskPointer;
            var au = await taskAux;

            return ch
                .Zip(pt, (c, p) => (c, p))
                .Zip(au, (cp, a) => (cp.c, cp.p, a))
                .Select(tuple => {
                    ushort dbId = (ushort)tuple.c.DB;
                    var cfg = _cfgs.Single(c => c.Id == dbId);
                    return new DataBlockMetaData(
                        changeCounter: (ushort)tuple.c.Value,
                        auxiliaryCounter: (ushort)tuple.a.Value,
                        bufferPointer: (ushort)tuple.p.Value,
                        dB: dbId,
                        bufferSize: cfg.Size
                    );
                })
                .ToList();
        }

        /// <summary>Read the change‐counter (header) for a given DB.</summary>
        private ushort ReadChangeCounter(ushort dbId)
        {
            var cfg = FindConfig(dbId);
            return SafeExecute(() =>
            {
                var raw = _connectionManager.PlcInstance.Read(DataType.DataBlock, dbId,
                                     cfg.ChangeCounterStart, VarType.Word, 1);
                if (raw is null) throw new InvalidOperationException();
                return Convert.ToUInt16(raw);
            });
        }

        /// <summary>Read the auxiliary‐counter (footer) for a given DB.</summary>
        private ushort ReadAuxiliaryCounter(ushort dbId)
        {
            var cfg = FindConfig(dbId);
            return SafeExecute(() =>
            {
                var raw = _connectionManager.PlcInstance.Read(DataType.DataBlock, dbId,
                                     cfg.AuxCounterStart, VarType.Word, 1);
                if (raw is null) throw new InvalidOperationException();
                return Convert.ToUInt16(raw);
            });
        }

        /// <summary>Update the change‐counter (header) for a given DB.</summary>
        private void UpdateChangeCounter(ushort dbId, ushort value)
        {
            var cfg = FindConfig(dbId);

            SafeExecute(() =>
                _connectionManager.PlcInstance.Write(DataType.DataBlock,
                    dbId,
                    cfg.ChangeCounterStart,
                    value));
        }

        /// <summary>Update the auxiliary‐counter (footer) for a given DB.</summary>
        private void UpdateAuxiliaryCounter(ushort dbId, ushort value)
        {
            var cfg = FindConfig(dbId);
            SafeExecute(() =>
                _connectionManager.PlcInstance.Write(
                    DataType.DataBlock,
                    dbId,
                    cfg.AuxCounterStart,
                    value));
        }

        /// <inheritdoc />
        public void WriteContent(object model)
        {
            // 1) find the DB config whose ModelType matches this instance
            var cfg = _cfgs.FirstOrDefault(c => c.ModelType == model.GetType());

            // 2) Update change counter
            ushort chValue = ReadChangeCounter(cfg.Id);
            UpdateChangeCounter(cfg.Id, ++chValue);
              
            // 3) write the class at ContentStart
            SafeExecute(() =>
                _connectionManager.PlcInstance.WriteClass(
                    model, 
                    cfg.Id, 
                    cfg.ContentStart));

            // 4) Update aux counter
            ushort auxValue = ReadAuxiliaryCounter(cfg.Id);
            UpdateAuxiliaryCounter(cfg.Id, ++auxValue);
        }

        /// <summary>
        /// Helper to look up the DataBlockConfig or throw if missing.
        /// </summary>
        private DataBlockConfig FindConfig(ushort dbId)
            => _cfgs.SingleOrDefault(c => c.Id == dbId)
               ?? throw new ArgumentException($"No DataBlockConfig for DB {dbId}");

        private T SafeExecute<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex) when (
                   ex is PlcException or SocketException)
            {
                _connectionManager?.RefreshState();   
                throw;                               
            }
        }

        private void SafeExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex) when (ex is PlcException or SocketException)
            {
                _connectionManager.RefreshState();      
                throw;
            }
        }

        private async Task<T> SafeExecuteAsync<T>(Func<Task<T>> fn)
        {
            try
            {
                return await fn().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is PlcException or SocketException)
            {
                _connectionManager.RefreshState();
                throw;
            }
        }
        private int ComputeOffset(DataBlockConfig cfg, ushort ptr)
            => cfg.ContentStart + cfg.Offset * (ptr - 1);

        private List<DataItem> BuildDataItem(Func<DataBlockConfig, int> offsetSelector) =>
            _cfgs.Select(c => new DataItem
            {
                DataType = DataType.DataBlock,
                DB = c.Id,
                StartByteAdr = offsetSelector(c),
                VarType = VarType.Word,
                Count = 1
            }).ToList();
    }
}

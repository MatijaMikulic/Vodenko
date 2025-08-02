using V3.S7Plc.Communication.Interfaces;
using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.Common;
using V3.S7Plc.Communication.Enums;

namespace V3.S7Plc.Communication.Buffer
{

    /// <summary>
    /// Factory to create an IDataBlockHandler for a DataBlockDefinition.
    /// </summary>
    public class DataBlockHandlerFactory : IDataBlockHandlerFactory
    {
        private readonly IBufferStrategyFactory _bufFactory;
        private readonly IPlcConnection _conn;
        private readonly IDataBlockMapper _mapper;
        private readonly ConcurrentDictionary<Type, IDataBlockHandler> _handlerCache = new();

        public DataBlockHandlerFactory(
            IBufferStrategyFactory bufFactory,
            IPlcConnection conn,
            IDataBlockMapper mapper)
        {
            _bufFactory = bufFactory;
            _conn = conn;
            _mapper = mapper;
        }
        public IDataBlockHandler GetHandler(DataBlockFrameDefinition def)
        {
            return _handlerCache.GetOrAdd(def.ModelType, dbNumber =>
            {
                var bufferStrategy = _bufFactory.Create(def);
                return def.StructureType == StructureType.CircularBuffer
                    ? new CircularBufferHandler(def, bufferStrategy, _conn, _mapper)
                    : new SingleValueHandler(def, bufferStrategy, _conn, _mapper);
            });
        }
    }
    
}

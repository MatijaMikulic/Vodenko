using V3.S7Plc.Communication.Buffer;
using V3.S7Plc.Communication.Enums;
using V3.S7Plc.Communication.Interfaces;
using V3.S7Plc.Communication.Model;
namespace V3.S7Plc.Communication.Buffer
{
    ///<summary>
    ///Factory to choose Single or Circular buffer strategy based on element count.
    ///</summary>
    public class BufferStrategyFactory : IBufferStrategyFactory
    {
        public IBufferStrategy Create(DataBlockFrameDefinition definition)
            => definition.StructureType == StructureType.CircularBuffer
                ? new CircularBufferStrategy(definition.Body.ElementCount)
                : new SingleBufferStrategy();
    }
}

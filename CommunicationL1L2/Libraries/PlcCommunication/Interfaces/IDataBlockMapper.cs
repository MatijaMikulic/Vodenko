using V3.S7Plc.Communication.Model;
namespace V3.S7Plc.Communication.Interfaces
{
    /// <summary>
    /// Builds DataBlockDefinition from attributes and maps raw bytes ↔ model instances.
    /// </summary>
    public interface IDataBlockMapper
    {
        DataBlockFrameDefinition GetDefinition<T>() where T : class, new();
        T MapBytesToModel<T>(byte[] rawData) where T : class, new();
        byte[] MapModelToBytes<T>(T model) where T : class, new();
    }
}

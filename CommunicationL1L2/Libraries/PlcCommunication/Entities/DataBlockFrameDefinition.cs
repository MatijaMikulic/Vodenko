using V3.S7Plc.Communication.Enums;
using V3.S7Plc.Communication.Templates;


namespace V3.S7Plc.Communication.Model
{
    /// <summary>
    /// Represents the layout of a DataBlock: DB number, structure type, offsets, element size, count.
    /// </summary>
    public class DataBlockFrameDefinition
    {
        public required string Name { get; init; }
        public required int DbNumber { get; init; }
        public required StructureType StructureType { get; init; }
        public required Type ModelType { get; init; }
        public required SegmentLayout Header { get; init; }
        public required BodyLayout Body { get; init; }
        public required SegmentLayout Footer { get; init; }
    }
}

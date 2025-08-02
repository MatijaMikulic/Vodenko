namespace V3.S7Plc.Communication.Templates
{
    public class SegmentLayout
    {
        /// <summary>Absolute byte offset in the DB where element zero starts.</summary>
        public int StartOffset { get; }
        IReadOnlyList<TemplateField> Fields { get; }

        public SegmentLayout(int startOffset,IEnumerable<TemplateField> fields)
        {
            StartOffset = startOffset;
            Fields = new List<TemplateField>(fields);
        }
    }
}

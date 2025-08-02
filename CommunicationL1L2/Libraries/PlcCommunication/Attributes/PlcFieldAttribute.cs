namespace V3.S7Plc.Communication.Attributes
{
    /// <summary>
    /// Marks a property/field with its byte offset (and optional bit) relative to the start of the content area.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PlcFieldAttribute : Attribute
    {
        /// <summary>Byte offset within one element’s content.</summary>
        public int Offset { get; set; }

        /// <summary>(Optional) Bit index [0-7] if this field is a single bit (for bool fields).</summary>
        public int BitIndex { get; set; } = -1;

        public PlcFieldAttribute(int offset)
        {
            Offset = offset;
        }
    }
}

using V3.S7Plc.Communication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Templates
{
    public class BodyLayout : SegmentLayout
    {
        /// <summary>Byte size of each element.</summary>
        public int ElementSize { get; }

        /// <summary>How many elements the buffer holds.</summary>
        public int ElementCount { get; }

        public BodyLayout(int contentStartOffset, int elementSize, int elementCount, IReadOnlyList<TemplateField> fields)
            :base(contentStartOffset,fields)
        {
            ElementSize = elementSize;
            ElementCount = elementCount;
        }
    }
}

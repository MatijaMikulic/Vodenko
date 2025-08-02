using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Templates
{
    public class BufferTemplate
    {
        public IReadOnlyList<TemplateField> HeaderFields { get; }
        public IReadOnlyList<TemplateField> FooterFields { get; }

        public BufferTemplate(IEnumerable<TemplateField> headerFields, IEnumerable<TemplateField> footerFields)
        {
            HeaderFields = headerFields.ToArray();
            FooterFields = footerFields.ToArray();
        }
    }
}

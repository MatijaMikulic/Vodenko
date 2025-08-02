using V3.S7Plc.Communication.Enums;
using V3.S7Plc.Communication.Templates;
using V3.S7Plc.Communication.Templates.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Templates
{
    public class DataBufferTemplate : IBufferTemplate
    {
        public StructureType Type => StructureType.DataBuffer;
        public BufferTemplate Build() => new BufferTemplate(
            headerFields: new[]
            {
                new TemplateField("ChangeCounter",  0, 0, 2, typeof(ushort))
            },

            footerFields: new[]
            {
                 new TemplateField("AuxCounter",     0, 0, 2, typeof(ushort))
            }
        );

    }
}

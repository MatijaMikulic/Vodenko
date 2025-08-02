using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Templates
{
    public record TemplateField(string Name, int ByteOffset, int BitOffset, int Size, Type ClrType);
}

using V3.S7Plc.Communication.Enums;
using V3.S7Plc.Communication.Templates.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3.S7Plc.Communication.Templates
{
    /// <summary>
    /// Maps a BufferTemplateType to a raw BufferTemplate.
    /// Footer offsets are relative (shifted later).
    /// </summary>
    public class BufferTemplateFactory
    {
        private readonly Dictionary<StructureType, IBufferTemplate> _map;

        public BufferTemplateFactory(IEnumerable<IBufferTemplate> providers)
        {
            _map = providers.ToDictionary(p => p.Type);
        }

        public BufferTemplate Create(StructureType type)
          => _map.TryGetValue(type, out var prov)
             ? prov.Build()
             : throw new ArgumentException($"Unknown template: {type}");
    }
    
}

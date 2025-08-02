using V3.S7Plc.Communication.Interfaces;
using V3.S7Plc.Communication.Model;
using V3.S7Plc.Communication.Templates.Interfaces;
using V3.S7Plc.Communication.Templates;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using V3.S7Plc.Communication.Attributes;

namespace V3.S7Plc.Communication.Core
{
    /// <summary>
    /// Maps attributes on a model class to a DataBlockDefinition, and converts between byte arrays and model instances.
    /// </summary>
    public class AttributeDataBlockMapper(BufferTemplateFactory bufferTemplateFactory) : IDataBlockMapper
    {
        private readonly ConcurrentDictionary<Type, DataBlockFrameDefinition> _defs = new();
        public DataBlockFrameDefinition GetDefinition<T>() where T : class, new()
            => _defs.GetOrAdd(typeof(T), BuildDefinition);

        public T MapBytesToModel<T>(byte[] raw) where T : class, new()
        {
            var def = GetDefinition<T>();
            var model = new T();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<PlcFieldAttribute>();
                if (attr == null) continue;
                int offset = attr.Offset;

                object value = null;
                // Handle big-endian conversions for known types
                if (prop.PropertyType == typeof(int))
                {
                    byte[] bytes = raw.Skip(offset).Take(4).Reverse().ToArray();
                    value = BitConverter.ToInt32(bytes, 0);
                }
                else if (prop.PropertyType == typeof(float))
                {
                    byte[] bytes = raw.Skip(offset).Take(4).Reverse().ToArray();
                    value = BitConverter.ToSingle(bytes, 0);
                }
                else if (prop.PropertyType == typeof(short))
                {
                    byte[] bytes = raw.Skip(offset).Take(2).Reverse().ToArray();
                    value = BitConverter.ToInt16(bytes, 0);
                }
                else if (prop.PropertyType == typeof(double))
                {
                    byte[] bytes = raw.Skip(offset).Take(8).Reverse().ToArray();
                    value = BitConverter.ToDouble(bytes, 0);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    byte b = raw[offset];
                    if (attr.BitIndex >= 0) value = (b & (1 << attr.BitIndex)) != 0;
                    else value = b != 0;
                }

                if (value != null)
                    prop.SetValue(model, value);
            }
            return model;
        }

        public byte[] MapModelToBytes<T>(T model) where T : class, new()
        {
            var def = GetDefinition<T>();
            byte[] raw = new byte[def.Body.ElementSize];

            foreach (var prop in typeof(T).GetProperties())
            {
                var attr = prop.GetCustomAttribute<PlcFieldAttribute>();
                if (attr == null) continue;
                int offset = attr.Offset;
                var val = prop.GetValue(model);
                if (val == null) continue;

                if (prop.PropertyType == typeof(int))
                {
                    byte[] bytes = BitConverter.GetBytes((int)val);
                    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
                    Array.Copy(bytes, 0, raw, offset, 4);
                }
                else if (prop.PropertyType == typeof(float))
                {
                    byte[] bytes = BitConverter.GetBytes((float)val);
                    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
                    Array.Copy(bytes, 0, raw, offset, 4);
                }
                else if (prop.PropertyType == typeof(short))
                {
                    byte[] bytes = BitConverter.GetBytes((short)val);
                    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
                    Array.Copy(bytes, 0, raw, offset, 2);
                }
                else if (prop.PropertyType == typeof(double))
                {
                    byte[] bytes = BitConverter.GetBytes((double)val);
                    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
                    Array.Copy(bytes, 0, raw, offset, 8);
                }
                else if (prop.PropertyType == typeof(bool) && attr.BitIndex >= 0)
                {
                    byte b = raw[offset];
                    if ((bool)val) b |= (byte)(1 << attr.BitIndex);
                    else b &= (byte)~(1 << attr.BitIndex);
                    raw[offset] = b;
                }
            }
            return raw;
        }

        private DataBlockFrameDefinition BuildDefinition(Type t)
        {
            // 1) read class‐level attribute
            var attr = t.GetCustomAttribute<PlcDataBlockAttribute>()
               ?? throw new InvalidOperationException(
                   $"{t.Name} is missing [PlcDataBlock]");

            // 2) pick raw template
            var rawTemplate = bufferTemplateFactory.Create(attr.Type);

            // 3) reflect content‐fields ([PlcField])
            var contentFields = t.GetProperties()
                .Select(p =>
                {
                    var f = p.GetCustomAttribute<PlcFieldAttribute>();
                    if (f == null) return null;
                    int size = (f.BitIndex >= 0) ? 1 : GetTypeSize(p.PropertyType);
                    return new TemplateField(p.Name, f.Offset, f.BitIndex, size, p.PropertyType);
                })
                .Where(x => x != null)
                .Cast<TemplateField>()
                .ToList();

            // 4) decide element size
            int elementSize = contentFields.Max(f => f.ByteOffset + f.Size);

            // 5) number of elements
            int count = attr.Capacity;

            // 6) compute header size = end of last header field
            int headerSize = rawTemplate.HeaderFields
                .Max(f => f.ByteOffset + f.Size);

            // 7) build header layout (absolute offsets)
            var headerLayout = new SegmentLayout(0, rawTemplate.HeaderFields);

            // 8) build body layout
            var bodyLayout = new BodyLayout(headerSize, elementSize, count, contentFields);

            // 9) compute footer absolute offset
            int footerBase = headerSize + elementSize * count;
            var absFooter = rawTemplate.FooterFields
                .Select(f =>
                    new TemplateField(
                        f.Name,
                        f.ByteOffset,
                        f.BitOffset,
                        f.Size,
                        f.ClrType))
                .ToList();
            var footerLayout = new SegmentLayout(footerBase, absFooter);

            // 10) assemble definition
            return new DataBlockFrameDefinition
            {
                Name = t.Name,
                DbNumber = attr.DbNumber,
                StructureType = attr.Type,
                ModelType = t,
                Header = headerLayout,
                Body = bodyLayout,
                Footer = footerLayout
            };
        }

        private static int GetTypeSize(Type t) => t switch
        {
            _ when t == typeof(bool) => 1,
            _ when t == typeof(byte) => 1,
            _ when t == typeof(short) => 2,
            _ when t == typeof(ushort) => 2,
            _ when t == typeof(int) => 4,
            _ when t == typeof(uint) => 4,
            _ when t == typeof(float) => 4,
            _ when t == typeof(long) => 8,
            _ when t == typeof(ulong) => 8,
            _ when t == typeof(double) => 8,
            _ => throw new NotSupportedException($"Type {t.Name} not supported")
        };
    }
}

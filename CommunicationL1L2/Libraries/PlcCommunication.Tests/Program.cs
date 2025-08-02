// See https://aka.ms/new-console-template for more information
using PlcCommunication;
using PlcCommunication.Tests;
using V3.S7Plc.Communication.Core;
using V3.S7Plc.Communication.Templates;
using V3.S7Plc.Communication.Templates.Interfaces;

IEnumerable<IBufferTemplate> providers = new List<IBufferTemplate> { new CircularBufferTemplate() };
var factory = new BufferTemplateFactory(providers);
var mapper = new AttributeDataBlockMapper(factory);
mapper.GetDefinition<Entity>();


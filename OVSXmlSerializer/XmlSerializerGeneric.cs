namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml;
	using static XmlSerializer;

	public class XmlSerializer<T> where T : notnull
	{
		protected XmlSerializerConfig config;
		public XmlSerializer()
		{
			config = XmlSerializerConfig.Default;
		}
		public XmlSerializer(XmlSerializerConfig config)
		{
			this.config = config;
		}

		public virtual T Deserialize(Stream input)
		{
			XmlDocument document = new XmlDocument();
			document.Load(input);
			return (T)new XmlReaderSerializer(config).ReadDocument(document);
		}

		public virtual MemoryStream Serialize(T item)
		{
			return Serialize(item, item.GetType().Name);
		}
		public virtual MemoryStream Serialize(T item, string rootElementName)
		{
			var stream = new MemoryStream();
			XmlWriter writer = XmlWriter.Create(stream, config.AsWriterSettings());
			new XmlWriterSerializer(config, writer).WriteObject(rootElementName, item);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		//public virtual (T obj, string rootElementName) Deserialize(Stream stream)
		//{
		//
		//}
	}
}

namespace OVSXmlSerializer
{
	using System.IO;
	using System.Xml;

	/// <summary>
	/// Serializer that converts classes into XML Files and such.
	/// </summary>
	/// <typeparam name="T"> A Non-nullable value. </typeparam>
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

		#region Deserialization
		public virtual T Deserialize(Stream input)
		{
			XmlDocument document = new XmlDocument();
			document.Load(input);
			object output = new XmlReaderSerializer(config).ReadDocument(document);
			return (T)output;
		}
		public T Deserialize(FileInfo fileLocation)
		{
			using (var stream = fileLocation.OpenRead())
				return Deserialize(fileLocation);
		}
		public T Deserialize(string fileLocation)
		{
			using (var stream = File.OpenRead(fileLocation))
				return Deserialize(fileLocation);
		}

		public virtual T Deserialize(Stream input, out string rootElementName)
		{
			XmlDocument document = new XmlDocument();
			document.Load(input);
			rootElementName = document.ChildNodes.Item(document.ChildNodes.Count - 1)!.Name;
			object output = new XmlReaderSerializer(config).ReadDocument(document);
			return (T)output;
		}
		public virtual T Deserialize(string fileLocation, out string rootElementName)
		{
			using (var stream = File.OpenRead(fileLocation))
				return Deserialize(stream, out rootElementName);
		}
		public virtual T Deserialize(FileInfo fileLocation, out string rootElementName)
		{
			using (var stream = fileLocation.OpenRead())
				return Deserialize(stream, out rootElementName);
		}
		#endregion

		#region Serialization
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
		public virtual void Serialize(XmlWriter writer, T item)
		{
			Serialize(writer, item, item.GetType().Name);
		}
		public virtual void Serialize(XmlWriter writer, T item, string rootElementName)
		{
			new XmlWriterSerializer(config, writer).WriteObject(rootElementName, item);
			writer.Flush();
		}
		#endregion
	}
}

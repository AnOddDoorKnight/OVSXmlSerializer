namespace OVSXmlSerializer
{
	using System.IO;
	using System.Xml;

	/// <summary>
	/// Serializer that converts classes into XML Files and such.
	/// </summary>
	public class XmlSerializer<T>
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
		/// <summary>
		/// Converts a xml file into an object. 
		/// </summary>
		/// <param name="input"> The stream that contains the XML file. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public virtual T Deserialize(Stream input)
		{

			XmlDocument document = new XmlDocument();
			try
			{
				document.Load(input);
			}
			catch (XmlException exception) when (exception.Message == "Root element is missing.")
			{
				return default;
			}
			object output = new XmlReaderSerializer(config).ReadDocument(document);
			return (T)output;
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public T Deserialize(FileInfo fileLocation)
		{
			using (var stream = fileLocation.OpenRead())
				return Deserialize(stream);
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public T Deserialize(string fileLocation)
		{
			using (var stream = File.OpenRead(fileLocation))
				return Deserialize(stream);
		}
		/// <summary>
		/// Converts a xml file into an object. 
		/// </summary>
		/// <param name="input"> The stream that contains the XML file. </param>
		/// <param name="rootElementName"> The name of the root element. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public virtual T Deserialize(Stream input, out string rootElementName)
		{
			XmlDocument document = new XmlDocument();
			document.Load(input);
			rootElementName = document.ChildNodes.Item(document.ChildNodes.Count - 1).Name;
			object output = new XmlReaderSerializer(config).ReadDocument(document);
			return (T)output;
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <param name="rootElementName"> The name of the root element. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public virtual T Deserialize(string fileLocation, out string rootElementName)
		{
			using (var stream = File.OpenRead(fileLocation))
				return Deserialize(stream, out rootElementName);
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <param name="rootElementName"> The name of the root element. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public virtual T Deserialize(FileInfo fileLocation, out string rootElementName)
		{
			using (var stream = fileLocation.OpenRead())
				return Deserialize(stream, out rootElementName);
		}
		#endregion

		#region Serialization
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public virtual MemoryStream Serialize(T item)
		{
			return Serialize(item, item.GetType().Name);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream by copying it into the stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="stream"> The stream to serialize to. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public virtual void Serialize(Stream stream, T item)
		{
			Serialize(stream, item, item.GetType().Name);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream by copying it into the stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="rootElementName">The root element within the xml file. </param>
		/// <param name="stream"> The stream to serialize to. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public virtual void Serialize(Stream stream, T item, string rootElementName)
		{
			using (var memoryStream = Serialize(item, rootElementName))
				memoryStream.CopyTo(stream);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="rootElementName">The root element within the xml file. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public virtual MemoryStream Serialize(T item, string rootElementName)
		{
			var stream = new MemoryStream();
			XmlWriter writer = XmlWriter.Create(stream, config.AsWriterSettings());
			new XmlWriterSerializer(config, writer).WriteObject(rootElementName, new StructuredObject(item));
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
			new XmlWriterSerializer(config, writer).WriteObject(rootElementName, new StructuredObject(item));
			writer.Flush();
		}
		#endregion
	}
}

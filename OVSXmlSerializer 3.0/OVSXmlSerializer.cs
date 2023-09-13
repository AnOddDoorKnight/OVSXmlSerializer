namespace OVSXmlSerializer
{
	using global::OVSXmlSerializer.Internals;
	using System;
	using System.IO;
	using System.Xml;

	public class OVSXmlSerializer<T>
	{
		/// <summary>
		/// The configuration that changes the behaviour of the serializer.
		/// </summary>
		public OVSConfig Config { get; protected set; } = new OVSConfig();
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public MemoryStream Serialize(T item)
		{
			object testOutput = (object)item;
			if (testOutput is null)
				return new MemoryStream();
			return Serialize(item, item.GetType().Name);
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
			object testOutput = (object)item;
			MemoryStream stream = new MemoryStream();
			if (testOutput is null)
				return stream;
			var writer = XmlWriter.Create(stream, Config.AsWriterSettings());
			var OVSwriter = new OVSXmlWriter<T>(this);
			XmlDocument document = OVSwriter.SerializeObject(item, rootElementName);
			document.Save(writer);
			writer.Flush();
			writer.Close();
			stream.Position = 0;
			return stream;
		}

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
			object output = new OVSXmlReader<T>(this).ReadDocument(document, typeof(T));
			return (T)output;
		}
	}
}

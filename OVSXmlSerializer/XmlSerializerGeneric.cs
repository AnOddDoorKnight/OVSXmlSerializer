namespace OVSXmlSerializer
{
	using System;
	using System.IO;
	using System.Xml;
	using static XmlSerializer;

	/// <summary>
	/// Serializer that converts classes into XML Files and such.
	/// </summary>
	public class XmlSerializer<T>
	{
		/// <summary>
		/// Default Xml Serializer. Effectively creating a new blank instance.
		/// </summary>
		public static XmlSerializer<T> Default { get; } = new XmlSerializer<T>();
		/// <summary>
		/// The configuration that changes the behaviour of the serializer.
		/// </summary>
		public XmlSerializerConfig Config { get; protected set; }
		/// <summary>
		/// Constructs a generic of XmlSerializer. Uses default config.
		/// </summary>
		public XmlSerializer()
		{
			Config = XmlSerializerConfig.Default;
		}
		/// <summary>
		/// Constructs a generic of XmlSerializer. Uses the specified config.
		/// </summary>
		public XmlSerializer(XmlSerializerConfig config)
		{
			this.Config = config;
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
			object output = new XmlReaderSerializer<T>(this).ReadDocument(document, typeof(T));
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
			object output = new XmlReaderSerializer<T>(this).ReadDocument(document, typeof(T));
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
		public T Deserialize(string fileLocation, out string rootElementName)
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
		public T Deserialize(FileInfo fileLocation, out string rootElementName)
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
		public MemoryStream Serialize(T item)
		{
			object testOutput = (object)item;
			if (testOutput is null)
				return new MemoryStream();
			return Serialize(item, item.GetType().Name);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream by copying it into the stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="stream"> The stream to serialize to. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(Stream stream, T item)
		{
			object testOutput = (object)item;
			if (testOutput is null)
			{
				new MemoryStream().CopyTo(stream);
				return;
			}
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
		public void Serialize(Stream stream, T item, string rootElementName)
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
			object testOutput = (object)item;
			MemoryStream stream = new MemoryStream();
			if (testOutput is null)
				return stream;
			var writer = XmlWriter.Create(stream, Config.AsWriterSettings());
			XmlDocument document = new XmlWriterSerializer<T>(this).Serialize(item, rootElementName);
			document.Save(writer);
			writer.Flush();
			writer.Close();
			stream.Position = 0;
			return stream;
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="writer"> The writer to write to. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(XmlWriter writer, T item)
		{
			object testOutput = (object)item;
			if (testOutput is null)
				return;
			Serialize(writer, item, item.GetType().Name);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="writer"> The writer to write to. </param>
		/// <param name="rootElementName"> The custom name to store the root value to. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public virtual void Serialize(XmlWriter writer, T item, string rootElementName)
		{
			object testOutput = (object)item;
			if (testOutput is null)
				return;
			XmlDocument document = new XmlWriterSerializer<T>(this).Serialize(item, rootElementName);
			document.Save(writer);
		}
		#endregion

		#region Version Checking
		#endregion

		#region Other
		/// <summary>
		/// Deeply copies all the data down into primitives into a separate created 
		/// object via XML serializing and deserializing the object.
		/// </summary>
		/// <param name="input"> The object to copy. </param>
		public virtual T DeepCopy(T input)
		{
			using (MemoryStream stream = Serialize(input, "DeepCopy"))
				return Deserialize(stream);
		}
		/// <summary>
		/// Uses <see cref="object.MemberwiseClone"/> to create a new object,
		/// ignoring accessibility methods. Uses reflection instead of using the
		/// serializer.
		/// </summary>
		public T ShallowCopy(T input)
		{
			if ((object)input is null)
				return input;
			object output = input.GetType().GetMethod(nameof(MemberwiseClone))
				.Invoke(input, Array.Empty<object>());
			return (T)output;
		}
		#endregion
	}
}

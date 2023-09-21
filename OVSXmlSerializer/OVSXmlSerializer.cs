namespace OVSSerializer
{
	using global::OVSSerializer.Internals;
	using global::OVSSerializer.IO;
	using System;
	using System.IO;
	using System.Xml;

	/// <summary>
	/// A class that serializes or deserializes an object given <typeparamref name="T"/>
	/// into an xml document/format.
	/// </summary>
	public class OVSXmlSerializer<T>
	{
		/// <summary>
		/// Gets the generic shared version of the serializer.
		/// </summary>
		public static OVSXmlSerializer<T> Shared { get; } = new OVSXmlSerializer<T>();
		/// <summary>
		/// The configuration that changes the behaviour of the serializer.
		/// </summary>
		public OVSConfig Config { get; protected set; }
		/// <summary>
		/// Constructs a generic of XmlSerializer. Uses the default config.
		/// </summary>
		public OVSXmlSerializer()
		{
			Config = new OVSConfig();
		}
		/// <summary>
		/// Constructs a generic of XmlSerializer. Uses the specified config.
		/// </summary>
		public OVSXmlSerializer(OVSConfig config)
		{
			this.Config = config;
		}



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
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="file"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(FileInfo file, T item)
		{
			Serialize(file, item, item.GetType().Name);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="file"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="rootElementName">The root element within the xml file. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(FileInfo file, T item, string rootElementName)
		{
			using (MemoryStream stream = Serialize(item, rootElementName))
				using (FileStream fileStream = file.Open(FileMode.Create))
					stream.CopyTo(fileStream);
		}

		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="file"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(OSFile file, T item)
		{
			Serialize(file, item, item.GetType().Name);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="file"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="rootElementName">The root element within the xml file. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(OSFile file, T item, string rootElementName)
		{
			using (MemoryStream stream = Serialize(item, rootElementName))
				using (FileStream fileStream = file.Create())
					stream.CopyTo(fileStream);
		}

		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="fileLocation"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(string fileLocation, T item)
		{
			Serialize(fileLocation, item, item.GetType().Name);
		}
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="fileLocation"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <param name="rootElementName">The root element within the xml file. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(string fileLocation, T item, string rootElementName)
		{
			using (MemoryStream stream = Serialize(item, rootElementName))
			using (FileStream fileStream = File.Open(fileLocation, FileMode.Create))
				stream.CopyTo(fileStream);
		}
		/// <summary>
		/// Serializes into the inputted stream as an XML document.
		/// </summary>
		public void Serialize(Stream writeTo, T item)
		{
			using (MemoryStream memoryStream = Serialize(item))
				memoryStream.CopyTo(writeTo);
		}
		#endregion

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
			object output = new OVSXmlReader<T>(this).ReadDocument(document, typeof(T));
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
		#endregion


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
	}
}

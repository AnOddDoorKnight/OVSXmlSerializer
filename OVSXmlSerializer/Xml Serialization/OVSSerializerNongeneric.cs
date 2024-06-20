namespace OVS.XmlSerialization
{
	using System.IO;
	using System;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Serialization;
	using global::OVS.XmlSerialization.Internals;
	using global::OVS.XmlSerialization.Prefabs;
	using System.Globalization;
	using System.Text;

	/// <summary>
	/// A class that serializes or deserializes an object non-generically.
	/// into an xml document/format.
	/// </summary>
	public sealed class OVSXmlSerializer : IOVSConfig
	{
		/// <summary>
		/// Gets the non-generic shared version of the serializer.
		/// </summary>
		public static OVSXmlSerializer Shared { get; } = new OVSXmlSerializer();
		/// <summary>
		/// Whenever a type is unclear or is more defined than given in the object
		/// field, it will use the type attribute in order to successfully create the
		/// object.
		/// </summary>
		internal const string ATTRIBUTE = "type";
		internal const string CONDITION = "con";

		/// <summary>
		/// The default flags to serialize instance field data.
		/// </summary>
		internal static readonly BindingFlags defaultFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;



		#region Config
		/// <inheritdoc/>
		public CultureInfo CurrentCulture { get; set; } = CultureInfo.InvariantCulture;
		/// <inheritdoc/>
		public bool UseSingleInstanceInsteadOfMultiple { get; set; } = true;
		/// <inheritdoc/>
		public Version Version { get; set; } = null;
		/// <inheritdoc/>
		public bool IgnoreUndefinedValues { get; set; } = false;
		/// <inheritdoc/>
		public InterfaceSerializer CustomSerializers { get; set; } = InterfaceSerializer.GetDefault();
		/// <inheritdoc/>
		public bool OmitAutoGenerationComment { get; set; } = false;
		/// <inheritdoc/>
		public Versioning.Leniency VersionLeniency { get; set; } = Versioning.Leniency.Strict;
		/// <inheritdoc/>
		public bool NewLineOnAttributes { get; set; } = false;
		/// <inheritdoc/>
		public bool OmitXmlDelcaration { get; set; } = true;
		/// <inheritdoc/>
		public string IndentChars { get; set; } = "\t";
		/// <inheritdoc/>
		public bool Indent { get; set; } = true;
		/// <inheritdoc/>
		public bool? StandaloneDeclaration { get; set; } = null;
		/// <inheritdoc/>
		public IncludeTypes TypeHandling { get; set; } = IncludeTypes.SmartTypes;
		/// <inheritdoc/>
		public ReadonlyFieldHandle HandleReadonlyFields { get; set; } = ReadonlyFieldHandle.Continue;
		/// <inheritdoc/>
		public Encoding Encoding { get; set; } = Encoding.UTF8;
		/// <inheritdoc/>
		public XmlWriterSettings WriterSettings
		{
			get => new XmlWriterSettings()
			{
				Indent = Indent,
				IndentChars = IndentChars,
				Encoding = Encoding,
				OmitXmlDeclaration = OmitXmlDelcaration,
				NewLineOnAttributes = NewLineOnAttributes,
			};
			set
			{
				Indent = value.Indent;
				IndentChars = value.IndentChars;
				Encoding = value.Encoding;
				OmitXmlDelcaration = value.OmitXmlDeclaration;
				NewLineOnAttributes = value.NewLineOnAttributes;
			}
		}
		/// <inheritdoc/>
		public ParameterlessConstructorLevel ParameterlessConstructorSetting { get; set; } =
			ParameterlessConstructorLevel.OnlyWithReaderSpecific;
		#endregion


		/// <summary>
		/// The object to implicitly to serialize, comes in hand with <see cref="TypeHandling"/>
		/// for root objects. Typically is <see cref="object"/> as base unless otherwise
		/// pointed.
		/// </summary>
		public Type TargetType { get; set; }

		/// <summary>
		/// Initializes a new empty serializer with the default <see cref="object"/>
		/// type, and default config settings.
		/// </summary>
		public OVSXmlSerializer()
		{
			this.TargetType = typeof(object);
		}
		/// <summary>
		/// Initializes a new empty serializer with the specified type to implicitly
		/// serialize, along with default config settings.
		/// </summary>
		public OVSXmlSerializer(Type type)
		{
			this.TargetType = type;
		}
		/// <summary>
		/// Initializes a new empty serializer with the default <see cref="object"/>
		/// type, and along with the specified config settings.
		/// </summary>
		public OVSXmlSerializer(IOVSConfig config)
		{
			this.TargetType = typeof(object);
			CurrentCulture = config.CurrentCulture;
			UseSingleInstanceInsteadOfMultiple = config.UseSingleInstanceInsteadOfMultiple;
			Version = config.Version;
			IgnoreUndefinedValues = config.IgnoreUndefinedValues;
			CustomSerializers = config.CustomSerializers;
			OmitAutoGenerationComment = config.OmitAutoGenerationComment;
			IndentChars = config.IndentChars;
			Indent = config.Indent;
			StandaloneDeclaration = config.StandaloneDeclaration;
			TypeHandling = config.TypeHandling;
			HandleReadonlyFields = config.HandleReadonlyFields;
			Encoding = config.Encoding;
		}
		/// <summary>
		/// Initializes a new empty serializer with the specified type to implicitly
		/// serialize, and along with the specified config settings.
		/// </summary>
		public OVSXmlSerializer(Type type, IOVSConfig config)
		{
			this.TargetType = type;
			CurrentCulture = config.CurrentCulture;
			UseSingleInstanceInsteadOfMultiple = config.UseSingleInstanceInsteadOfMultiple;
			Version = config.Version;
			IgnoreUndefinedValues = config.IgnoreUndefinedValues;
			CustomSerializers = config.CustomSerializers;
			OmitAutoGenerationComment = config.OmitAutoGenerationComment;
			IndentChars = config.IndentChars;
			Indent = config.Indent;
			StandaloneDeclaration = config.StandaloneDeclaration;
			TypeHandling = config.TypeHandling;
			HandleReadonlyFields = config.HandleReadonlyFields;
			Encoding = config.Encoding;
		}


		#region Serialization
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// stream.
		/// </summary>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public MemoryStream Serialize(object item)
		{
			if (item is null)
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
		public MemoryStream Serialize(object item, string rootElementName)
		{
			MemoryStream stream = new MemoryStream();
			if (item is null)
				return stream;
			var writer = XmlWriter.Create(stream, WriterSettings);
			var OVSwriter = new OVSXmlWriter(this);
			XmlDocument document = OVSwriter.SerializeObject(item, TargetType, rootElementName);
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
		public void Serialize(FileInfo file, object item)
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
		public void Serialize(FileInfo file, object item, string rootElementName)
		{
			using (MemoryStream stream = Serialize(item, rootElementName))
			using (FileStream fileStream = file.Open(FileMode.Create))
				stream.CopyTo(fileStream);
		}

#if OSDIRECTORIES
		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="file"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(OVS.IO.OSFile file, object item)
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
		public void Serialize(OVS.IO.OSFile file, object item, string rootElementName)
		{
			using (MemoryStream stream = Serialize(item, rootElementName))
			using (FileStream fileStream = file.Create())
				stream.CopyTo(fileStream);
		}
#endif

		/// <summary>
		/// Serializes the specified <paramref name="item"/> as a XML file in a
		/// file.
		/// </summary>
		/// <param name="fileLocation"> The file to write to. </param>
		/// <param name="item"> The item to serialize. </param>
		/// <returns> A serialized object with the XML format. </returns>
		public void Serialize(string fileLocation, object item)
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
		public void Serialize(string fileLocation, object item, string rootElementName)
		{
			using (MemoryStream stream = Serialize(item, rootElementName))
			using (FileStream fileStream = File.Open(fileLocation, FileMode.Create))
				stream.CopyTo(fileStream);
		}
		/// <summary>
		/// Serializes into the inputted stream as an XML document.
		/// </summary>
		public void Serialize(Stream writeTo, object item)
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
		public object Deserialize(Stream input)
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
			object output = new OVSXmlReader(this).ReadDocument(document, TargetType);
			return output;
		}
		/// <summary>
		/// Converts a xml file into an object. 
		/// </summary>
		/// <param name="input"> The stream that contains the XML file. </param>
		/// <param name="rootElementName">The root element name.</param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public object Deserialize(Stream input, out string rootElementName)
		{
			XmlDocument document = new XmlDocument();
			try
			{
				document.Load(input);
			}
			catch (XmlException exception) when (exception.Message == "Root element is missing.")
			{
				rootElementName = null;
				return default;
			}
			rootElementName = document.LastChild.Name;
			object output = new OVSXmlReader(this).ReadDocument(document, TargetType);
			return output;
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public object Deserialize(FileInfo fileLocation)
		{
			using (FileStream stream = fileLocation.OpenRead())
				return Deserialize(stream);
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <param name="rootElementName">
		/// The root element name, in contrast of the <see cref="Serialize(FileInfo, object, string)"/>
		/// element name.
		/// </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public object Deserialize(FileInfo fileLocation, out string rootElementName)
		{
			using (FileStream stream = fileLocation.OpenRead())
				return Deserialize(stream, out rootElementName);
		}
#if OSDIRECTORIES
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public object Deserialize(OVS.IO.OSFile fileLocation)
		{
			using (FileStream stream = fileLocation.OpenRead())
				return Deserialize(stream);
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <param name="rootElementName">
		/// The root element name, in contrast of the <see cref="Serialize(FileInfo, object, string)"/>
		/// element name.
		/// </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public object Deserialize(OVS.IO.OSFile fileLocation, out string rootElementName)
		{
			using (FileStream stream = fileLocation.OpenRead())
				return Deserialize(stream, out rootElementName);
		}
#endif
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public object Deserialize(string fileLocation)
		{
			using (FileStream stream = File.OpenRead(fileLocation))
				return Deserialize(stream);
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <param name="rootElementName">
		/// The root element name, in contrast of the <see cref="Serialize(FileInfo, object, string)"/>
		/// element name.
		/// </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public object Deserialize(string fileLocation, out string rootElementName)
		{
			using (FileStream stream = File.OpenRead(fileLocation))
				return Deserialize(stream, out rootElementName);
		}
#endregion

		/// <summary>
		/// Uses <see cref="object.MemberwiseClone"/> to create a new object,
		/// ignoring accessibility methods. Uses reflection instead of using the
		/// serializer.
		/// </summary>
		public object ShallowCopy(object input)
		{
			if ((object)input is null)
				return input;
			object output = input.GetType().GetMethod(nameof(MemberwiseClone))
				.Invoke(input, Array.Empty<object>());
			return output;
		}
	}
}

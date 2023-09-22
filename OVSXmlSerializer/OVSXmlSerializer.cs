namespace OVSSerializer
{
	using global::OVSSerializer.Internals;
	using global::OVSSerializer.IO;
	using System;
	using System.Globalization;
	using System.IO;
	using System.Security;
	using System.Text;
	using System.Xml;
	using System.Xml.Serialization;

	/// <summary>
	/// A class that serializes or deserializes an object given <typeparamref name="T"/>
	/// into an xml document/format.
	/// </summary>
	public class OVSXmlSerializer<T> : IOVSConfig
	{
		/// <summary>
		/// Gets the generic shared version of the serializer.
		/// </summary>
		public static OVSXmlSerializer<T> Shared { get; } = new OVSXmlSerializer<T>();

		#region Config
		/// <inheritdoc/>
		public CultureInfo CurrentCulture { get; set; } = CultureInfo.InvariantCulture;
		/// <inheritdoc/>
		public bool UseSingleInstanceInsteadOfMultiple { get; set; } = true;
		/// <inheritdoc/>
		public Version Version { get; set; } = new Version(1, 0);
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
		public XmlWriterSettings AsWriterSettings()
		{
			return new XmlWriterSettings()
			{
				Indent = Indent,
				IndentChars = IndentChars,
				Encoding = Encoding,
				OmitXmlDeclaration = OmitXmlDelcaration,
				NewLineOnAttributes = NewLineOnAttributes,
			};
		}
		#endregion

		/// <summary>
		/// Creates a new serializer with default config settings.
		/// </summary>
		public OVSXmlSerializer()
		{

		}
		/// <summary>
		/// Creates a new serializer with the specified config settings.
		/// </summary>
		public OVSXmlSerializer(IOVSConfig source)
		{
			CurrentCulture = source.CurrentCulture;
			UseSingleInstanceInsteadOfMultiple = source.UseSingleInstanceInsteadOfMultiple;
			Version = source.Version;
			IgnoreUndefinedValues = source.IgnoreUndefinedValues;
			CustomSerializers = source.CustomSerializers;
			OmitAutoGenerationComment = source.OmitAutoGenerationComment;
			IndentChars = source.IndentChars;
			Indent = source.Indent;
			StandaloneDeclaration = source.StandaloneDeclaration;
			TypeHandling = source.TypeHandling;
			HandleReadonlyFields = source.HandleReadonlyFields;
			Encoding = source.Encoding;
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
			var writer = XmlWriter.Create(stream, AsWriterSettings());
			var OVSwriter = new OVSXmlWriter(this);
			XmlDocument document = OVSwriter.SerializeObject(item, typeof(T), rootElementName);
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
			object output = new OVSXmlReader(this).ReadDocument(document, typeof(T));
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
			using (FileStream stream = fileLocation.OpenRead())
				return Deserialize(stream);
		}
		/// <summary>
		/// Converts a xml file into an object.
		/// </summary>
		/// <param name="fileLocation">The file location that contains the XML contents. </param>
		/// <returns> 
		/// The object, default or <see langword="null"/> if the xml file or stream is empty. 
		/// </returns>
		public T Deserialize(OSFile fileLocation)
		{
			using (FileStream stream = fileLocation.OpenRead())
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
			using (FileStream stream = File.OpenRead(fileLocation))
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
	/// <summary>
	/// The condition of how the <see cref="XmlSerializer"/> should handle types
	/// of objects.
	/// </summary>
	public enum IncludeTypes : byte
	{
		/// <summary>
		/// It will ignore the derived type entirely.
		/// </summary>
		IgnoreTypes = 0,
		/// <summary>
		/// When the object is derived off the field, then it will write the
		/// object type.
		/// </summary>
		SmartTypes = 8,
		/// <summary>
		/// The XML file will always write the type of the object. 
		/// </summary>
		AlwaysIncludeTypes = 16,
	}
	/// <summary>
	/// How it should handle readonly fields.
	/// </summary>
	public enum ReadonlyFieldHandle : byte
	{
		/// <summary>
		/// Ignores the readonly field entirely.
		/// </summary>
		Ignore = 0,
		/// <summary>
		/// Allows the parser to continue serializing the field.
		/// </summary>
		Continue = 8,
		/// <summary>
		/// Throws an error if a readonly field is encountered.
		/// </summary>
		ThrowError = 16,
	}
}

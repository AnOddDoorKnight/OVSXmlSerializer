namespace OVSXmlSerializer
{
	using System;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// A single class that stores all the configuration settings. Things like
	/// <see cref="XmlWriterSettings"/> is included.
	/// </summary>
	[Serializable]
	public class XmlSerializerConfig
	{
		/// <summary>
		/// Using the <see cref="XmlSerializerConfig.AsWriterSettings"/>, converts
		/// the data stored inside as writer settings.
		/// </summary>
		/// <param name="config"></param>
		public static implicit operator XmlWriterSettings(XmlSerializerConfig config)
		{
			return config.AsWriterSettings();
		}
		/// <summary>
		/// Converts the parameters within the settings as a config. implicitly
		/// due to seamless compatibility to the original XML serializer.
		/// </summary>
		/// <param name="config"></param>
		public static implicit operator XmlSerializerConfig(XmlWriterSettings config)
		{
			// Im sure there is more settings i can port from the writer settings.
			// - Other people can if they want, I just dont find the use of them
			// - and I don't understand it exactly.
			return new XmlSerializerConfig()
			{
				indent = config.Indent,
				indentChars = config.IndentChars,
				encoding = config.Encoding,
				newLineOnAttributes = config.NewLineOnAttributes,
				omitXmlDelcaration = config.OmitXmlDeclaration,
			};
		}


		/// <summary>
		/// Default variation of the config.
		/// </summary>
		public static XmlSerializerConfig Default => new XmlSerializerConfig();

		public OVSXmlLogger logger = null;
		/// <summary>
		/// The current version of the XML file. Null if you don't want any
		/// attributes assigned to the root element
		/// </summary>
		public Version Version { get; set; } = null;
		/// <summary>
		/// Whenever it should add a new line when declaring attributes.
		/// </summary>
		public bool newLineOnAttributes = false;
		/// <summary>
		/// Gets or sets a value indicating whether to omit an XML declaration.
		/// The declaration mentions the beginning element, that typically mentions
		/// the encoding type and the version of the XML.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> to omit the XML declaration; otherwise, <see langword="false"/>. 
		/// Default is <see langword="false"/>.
		/// </returns>
		public bool omitXmlDelcaration = false;
		/// <summary>
		/// The single indentation that should occur. This will be repeated as 
		/// layers are added further.
		/// </summary>
		public string indentChars = "\t";
		/// <summary>
		/// When writing onto a file, if it should have indentation such as '\n'.
		/// </summary>
		public bool indent = true;
		/// <summary>
		/// how the XML file from <see cref="XmlSerializer"/> should handle types. 
		/// </summary>
		public IncludeTypes TypeHandling = IncludeTypes.SmartTypes;
		/// <summary>
		/// The encoding of the result of the file.
		/// </summary>
		public Encoding encoding = Encoding.UTF8;
		/// <summary>
		/// Converts all data from the config to the relevant writer settings.
		/// </summary>
		public XmlWriterSettings AsWriterSettings()
		{
			return new XmlWriterSettings()
			{
				Indent = indent,
				IndentChars = indentChars,
				Encoding = encoding,
				OmitXmlDeclaration = omitXmlDelcaration,
				NewLineOnAttributes = newLineOnAttributes,
			};
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
		///// <summary>
		///// Has the properties of <see cref="SmartTypes"/> and always has the 
		///// top-level value shown as its type.
		///// </summary>
		//SmartButTopAlways = 12,
		/// <summary>
		/// The XML file will always write the type of the object. 
		/// </summary>
		AlwaysIncludeTypes = 16,
	}
}

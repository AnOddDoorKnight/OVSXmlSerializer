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
		public static implicit operator XmlWriterSettings(XmlSerializerConfig config)
		{
			return config.AsWriterSettings();
		}
		public static implicit operator XmlSerializerConfig(XmlWriterSettings config)
		{
			return new XmlSerializerConfig()
			{
				indent = config.Indent,
				indentChars = config.IndentChars,
				
			};
		}



		public static XmlSerializerConfig Default => new XmlSerializerConfig();
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
		/// If the XML file should always include types.
		/// </summary>
		public bool alwaysIncludeTypes = true;
		/// <summary>
		/// If the XML file should include a type if it is derived from type,
		/// such as an int from an object type.
		/// </summary>
		public bool smartTypes = true;
		/// <summary>
		/// Setting that changes <see cref="alwaysIncludeTypes"/> and <see cref="smartTypes"/>
		/// based on the input value. Since both fields are set as <see langword="true"/> 
		/// by default, you can set them both to <see langword="false"/> here.
		/// </summary>
		public bool IncludeTypes { set
			{
				alwaysIncludeTypes = value;
				smartTypes = value;
			} }
		public Encoding encoding = Encoding.UTF8;

		public XmlWriterSettings AsWriterSettings()
		{
			return new XmlWriterSettings()
			{
				Indent = indent,
				IndentChars = indentChars,
				Encoding = encoding,
			};
		}
	}
}

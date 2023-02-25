namespace OVSXmlSerializer
{
	using System.Text;
	using System.Xml;

	/// <summary>
	/// A single class that stores all the configuration settings. Things like
	/// <see cref="XmlWriterSettings"/> is included.
	/// </summary>
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
		/// If the XML file should include types. 
		/// </summary>
		/// <remarks>
		/// Currently, setting this to <see langword="false"/> will guarantee 
		/// issues, as it is not properly implemented!
		/// </remarks>
		public bool includeTypes = true;
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

namespace OVSXmlSerializer
{
	using System.Xml;

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

		public string indentChars = "\t";
		public bool indent = true;
		public bool includeTypes = true;

		public XmlWriterSettings AsWriterSettings()
		{
			return new XmlWriterSettings()
			{
				Indent = indent,
				IndentChars = indentChars
			};
		}
	}
}

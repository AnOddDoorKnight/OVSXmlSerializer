namespace OVSXmlSerializer
{
	using System;
	using System.Xml;

	public class XmlSerializerConfig
	{
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

namespace OVSXmlSerializer.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	public static class XmlNodeUtility
	{
		public static string ReadValue(this XmlNode node)
		{
			if (node is XmlElement element)
				return element.InnerText;
			else if (node is XmlAttribute attribute)
				return attribute.Value;
			throw new InvalidCastException();
		}
	}
}

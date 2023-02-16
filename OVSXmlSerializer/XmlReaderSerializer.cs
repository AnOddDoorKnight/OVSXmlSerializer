namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml;

	internal class XmlReaderSerializer
	{
		public const string ATTRIBUTE = XmlSerializer.ATTRIBUTE;

		protected XmlReader reader;
		protected XmlSerializerConfig config;
		public XmlReaderSerializer(XmlSerializerConfig config, XmlReader reader)
		{
			this.config = config;
			this.reader = reader;
		}

		public virtual object ReadObject(object obj)
		{
			reader
		}
	}
}

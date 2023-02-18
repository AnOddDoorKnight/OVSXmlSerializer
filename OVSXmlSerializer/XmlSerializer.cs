namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml;

	public class XmlSerializer : XmlSerializer<object>
	{
		internal const string ATTRIBUTE = "type",
			ATTRIBUTE_ARRAY = "typeArray",
			ATTRIBUTE_ENUMERABLE = "typeEnumerable";

		internal static readonly BindingFlags defaultFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;




		protected Type activeType;
		public XmlSerializer(Type type) : base()
		{
			activeType = type;
		}
		public XmlSerializer(Type type, XmlSerializerConfig config) : base(config)
		{
			activeType = type;
		}
	}
}

namespace OVSXmlSerializer
{
	using System;
	using System.Reflection;

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

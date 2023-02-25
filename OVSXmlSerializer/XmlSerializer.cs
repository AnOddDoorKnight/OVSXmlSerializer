namespace OVSXmlSerializer
{
	using System;
	using System.Reflection;

	/// <summary>
	/// Xml Serializer that converts values into XML files and such. Effectively
	/// <see cref="XmlSerializer{T}"/> which has a generic of <see cref="object"/>
	/// </summary>
	public class XmlSerializer : XmlSerializer<object>
	{
		internal const string ATTRIBUTE = "type";
		internal const string AUTO_IMPLEMENTED_PROPERTY = "autoImp";
		internal const string CONDITION = "con";

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

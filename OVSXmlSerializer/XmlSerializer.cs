namespace OVSXmlSerializer
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Xml;

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

		public override object Deserialize(Stream input)
		{
			XmlDocument document = new XmlDocument();
			try
			{
				document.Load(input);
			}
			catch (XmlException exception) when (exception.Message == "Root element is missing.")
			{
				return default;
			}
			object output = new XmlReaderSerializer(config).ReadDocument(document, activeType);
			return output;
		}
		public override object Deserialize(Stream input, out string rootElementName)
		{
			XmlDocument document = new XmlDocument();
			document.Load(input);
			rootElementName = document.ChildNodes.Item(document.ChildNodes.Count - 1).Name;
			object output = new XmlReaderSerializer(config).ReadDocument(document, activeType);
			return output;
		}
	}
}

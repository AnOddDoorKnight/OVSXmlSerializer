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
		internal const string CONDITION = "con";

		internal static readonly BindingFlags defaultFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
		/// <summary>
		/// The type to reference to. Can be manually changed after creation.
		/// </summary>
		public Type ActiveType { get; protected set; }
		/// <summary>
		/// Creates a new instance of a non-generic XML Serializer. Uses default
		/// config. References to default <see cref="object"/> as default type.
		/// </summary>
		public XmlSerializer() : base()
		{
			ActiveType = typeof(object);
		}
		/// <summary>
		/// Creates a new instance of a non-generic XML Serializer. Uses default
		/// config.
		/// </summary>
		/// <param name="type"> The type to reference to. </param>
		public XmlSerializer(Type type) : base()
		{
			ActiveType = type;
		}
		/// <summary>
		/// Creates a new instance of a non-generic XML Serializer. Uses default
		/// config. References to default <see cref="object"/> as default type.
		/// Uses a config that changes behaviour.
		/// </summary>
		public XmlSerializer(XmlSerializerConfig config) : base(config)
		{
			ActiveType = typeof(object);
		}
		/// <summary>
		/// Creates a new instance of a non-generic XML Serializer. Uses default
		/// config. Uses a config that changes behaviour.
		/// </summary>
		public XmlSerializer(Type type, XmlSerializerConfig config) : base(config)
		{
			ActiveType = type;
		}

		/// <inheritdoc/>
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
			object output = new XmlReaderSerializer(config).ReadDocument(document, ActiveType);
			return output;
		}

		/// <inheritdoc/>
		public override object Deserialize(Stream input, out string rootElementName)
		{
			XmlDocument document = new XmlDocument();
			document.Load(input);
			rootElementName = document.ChildNodes.Item(document.ChildNodes.Count - 1).Name;
			object output = new XmlReaderSerializer(config).ReadDocument(document, ActiveType);
			return output;
		}
	}
}

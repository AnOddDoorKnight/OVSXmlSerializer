namespace OVS.XmlSerialization.Prefabs
{
	using OVS.XmlSerialization.Internals;
	using OVS.XmlSerialization.Utility;
	using System;
	using System.Data;
	using System.Xml;
	using System.Xml.Linq;

	public class PrimitiveSerializer : ICustomSerializer
	{
		/// <inheritdoc/>
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			// Since string is arguably a class or char array, its it own check.
			if (type.IsPrimitive || type == typeof(string))
			{
				string unparsed = node.ReadValue();
				output = Convert.ChangeType(unparsed, type, reader.Config.CurrentCulture);
				return true;
			}
			if (type.IsEnum)
			{
				output = Enum.Parse(type, node.InnerText);
				return true;
			}
			output = null;
			return false;
		}

		/// <inheritdoc/>
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parentNode, StructuredObject primitive, string name, out XmlNode output)
		{
			if (primitive.IsPrimitive)
			{
				output = writer.CreateNode(parentNode, name, writer.ToStringPrimitive(primitive), primitive);
				return true;
			}
			if (primitive.ValueType.IsEnum)
			{
				output = writer.CreateNode(parentNode, name, primitive.Value.ToString(), primitive);
				return true;
			}
			output = null;
			return false;
		}
	}
}

namespace OVS.XmlSerialization.Prefabs
{
	using global::OVS.XmlSerialization.Utility;
	using global::OVS.XmlSerialization.Internals;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using System.Data;

	/// <summary>
	/// Serializes arrays.
	/// </summary>
	internal class ArraySerializer : ICustomSerializer
	{
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (!type.IsArray)
			{
				output = null;
				return false;
			}
			XmlNodeList childNodes = node.ChildNodes;
			Type elementType = type.GetElementType();
			Array array = Array.CreateInstance(elementType, childNodes.Count);
			reader.AddReferenceTypeToDictionary((XmlElement)node, array);
			for (int i = 0; i < childNodes.Count; i++)
				array.SetValue(reader.ReadObject(childNodes.Item(i), elementType), i);
			output = array;
			return true;
		}

		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!@object.ValueType.IsArray)
			{
				output = null;
				return false;
			}
			XmlElement arrayElement = writer.CreateElement(parentNode, suggestedName, @object);
			Array arrValue = (Array)@object.Value;
			Type elementType = @object.ValueType.GetElementType();
			for (int i = 0; i < arrValue.Length; i++)
			{
				StructuredObject currentValue = new StructuredObject(arrValue.GetValue(i), elementType);
				writer.WriteObject(currentValue, arrayElement, "Item");
			}
			output = arrayElement;
			return true;
		}
	}
}
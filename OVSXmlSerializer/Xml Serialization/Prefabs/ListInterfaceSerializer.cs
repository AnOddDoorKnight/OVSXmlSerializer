namespace OVS.XmlSerialization.Prefabs
{
	using OVS.XmlSerialization.Internals;
	using OVS.XmlSerialization.Prefabs;
	using System.Collections.Generic;
	using System.Collections;
	using System.Xml;
	using System;
	using OVS.XmlSerialization.Utility;

	/// <summary>
	/// Serializes <see cref="IList"/>
	/// </summary>
	public class ListInterfaceSerializer : ICustomSerializer
	{
		/// <inheritdoc/>
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!(@object.Value is IList list))
			{
				output = null;
				return false;
			}
			Type baseType = GetBaseType(@object.ValueType);
			//OVSXmlWriter.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			for (int i = 0; i < list.Count; i++)
			{
				StructuredObject currentValue = new StructuredObject(list[i], baseType);
				writer.WriteObject(currentValue, enumerableElement, "Item");
			}
			output = enumerableElement;
			return true;
		}
		/// <inheritdoc/>
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (typeof(IList).IsAssignableFrom(type) == false)
			{
				output = null;
				return false;
			}
			Type baseType = GetBaseType(type);
			List<XmlNode> xmlNodes = node.ChildNodes.ToList();
			IList list = (IList)reader.CreateNewObject(type, node, out bool dontOverride);
			if (dontOverride)
			{
				output = list;
				return true;
			}
			for (int i = 0; i < xmlNodes.Count; i++)
			{
				object input = reader.ReadObject(xmlNodes[i], baseType);
				list.Add(input);
			}
			output = list;
			return true;
		}
		private Type GetBaseType(Type assigningType)
		{
			Type output = typeof(object);
			if (assigningType.Namespace == typeof(List<object>).Namespace)
				output = assigningType.GetGenericArguments()[0];
			return output;
		}
	}
}
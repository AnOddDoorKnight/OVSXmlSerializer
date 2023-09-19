namespace OVSXmlSerializer.Internals
{
	using global::OVSXmlSerializer.Extras;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	public class ListInterfaceSerializer : ICustomSerializer
	{
		public bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!(@object.Value is IList list))
			{
				output = null;
				return false;
			}
			OVSXmlWriter<T>.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			for (int i = 0; i < list.Count; i++)
			{
				StructuredObject currentValue = new StructuredObject(list[i], typeof(object));
				writer.WriteObject(currentValue, enumerableElement, "Item");
			}
			output = enumerableElement;
			return true;
		}
		public bool CheckAndRead<T>(OVSXmlReader<T> reader, Type type, XmlNode node, out object output)
		{
			if (typeof(IList).IsAssignableFrom(type) == false)
			{
				output = null;
				return false;
			}
			List<XmlNode> xmlNodes = node.ChildNodes.ToList();
			IList list = (IList)Activator.CreateInstance(type, true);
			for (int i = 0; i < xmlNodes.Count; i++)
			{
				object input = reader.ReadObject(xmlNodes[i], typeof(object));
				list.Add(input);
			}
			output = list;
			return true;
		}
	}
	internal class DictionarySerializer : ICustomSerializer
	{
		public bool CheckAndRead<T>(OVSXmlReader<T> reader, Type type, XmlNode node, out object output)
		{
			if (typeof(IDictionary).IsAssignableFrom(type) == false)
			{
				output = null;
				return false;
			}
			XmlNodeList nodeList = node.ChildNodes;
			IDictionary dictionary = (IDictionary)Activator.CreateInstance(type, true);
			for (int i = 0; i < nodeList.Count; i++)
			{
				XmlNode child = nodeList.Item(i);
				object key = reader.ReadObject(child.GetNode("key"), typeof(object));
				object value = reader.ReadObject(child.GetNode("value"), typeof(object));
				dictionary.Add(key, value);
			}
			output = dictionary;
			return true;
		}
		public bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!(@object.Value is IDictionary dictionary))
			{
				output = null;
				return false;
			}
			OVSXmlWriter<T>.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			// creating key element attribute
			IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				object key = enumerator.Key;
				object value = enumerator.Value;
				XmlElement pair = writer.CreateElement(enumerableElement, "item");
				writer.WriteObject(new StructuredObject(key, typeof(object)), pair, "key");
				writer.WriteObject(new StructuredObject(value, typeof(object)), pair, "value");
			}
			output = enumerableElement;
			return true;
		}
	}
	internal class ArraySerializer : ICustomSerializer
	{
		public bool CheckAndRead<T>(OVSXmlReader<T> reader, Type type, XmlNode node, out object output)
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

		public bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output)
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
	internal class DatetimeSerializer : ICustomSerializer
	{
		public bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (@object.ValueType != typeof(DateTime))
			{
				output = null;
				return false;
			}
			string value = ((DateTime)@object.Value).ToString();
			output = writer.CreateNode(parentNode, suggestedName, value, @object);
			return true;
		}
		public bool CheckAndRead<T>(OVSXmlReader<T> reader, Type type, XmlNode node, out object output)
		{
			if (type != typeof(DateTime))
			{
				output = null;
				return false;
			}
			output = DateTime.Parse(node.ReadValue());
			return true;
		}
	}
	internal class TimespanSerializer : ICustomSerializer
	{
		public bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (@object.ValueType != typeof(TimeSpan))
			{
				output = null;
				return false;
			}
			string value = ((TimeSpan)@object.Value).ToString();
			output = writer.CreateNode(parentNode, suggestedName, value, @object);
			return true;
		}
		public bool CheckAndRead<T>(OVSXmlReader<T> reader, Type type, XmlNode node, out object output)
		{
			if (type != typeof(TimeSpan))
			{
				output = null;
				return false;
			}
			output = TimeSpan.Parse(node.ReadValue());
			return true;
		}
	}
}

namespace OVSSerializer.Internals
{
	using global::OVSSerializer.Extras;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	public class LinkedListSerializer : ICustomSerializer
	{
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!CanSerialize(@object.ValueType))
			{
				output = null;
				return false;
			}
			OVSXmlWriter.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			IEnumerator enumerator = ((IEnumerable)@object.Value).GetEnumerator();
			while (enumerator.MoveNext())
			{
				StructuredObject currentValue = new StructuredObject(enumerator.Current, typeof(object));
				writer.WriteObject(currentValue, enumerableElement, "LinkedItem");
			}
			output = enumerableElement;
			return true;
		}
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (!CanSerialize(type))
			{
				output = null;
				return false;
			}
			List<XmlNode> xmlNodes = node.ChildNodes.ToList();
			object linkedList = Activator.CreateInstance(type, true);
			MethodInfo addItem = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(meth => meth.Name == "AddLast")
				.First(meth => !meth.GetParameters()[0].ParameterType.Name.StartsWith("LinkedListNode"));
			for (int i = 0; i < xmlNodes.Count; i++)
			{
				object input = reader.ReadObject(xmlNodes[i], typeof(object));
				addItem.Invoke(linkedList, new object[] { input });
			}
			output = linkedList;
			return true;
		}
		private bool CanSerialize(Type inType)
		{
			if (inType.Namespace != inType.Namespace)
				return false;
			if (!inType.Name.StartsWith(nameof(LinkedList<object>)))
				return false;
			return true;
		}
	}
	public class ListInterfaceSerializer : ICustomSerializer
	{
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!(@object.Value is IList list))
			{
				output = null;
				return false;
			}
			Type baseType = GetBaseType(@object.ValueType);
			OVSXmlWriter.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			for (int i = 0; i < list.Count; i++)
			{
				StructuredObject currentValue = new StructuredObject(list[i], baseType);
				writer.WriteObject(currentValue, enumerableElement, "Item");
			}
			output = enumerableElement;
			return true;
		}
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (typeof(IList).IsAssignableFrom(type) == false)
			{
				output = null;
				return false;
			}
			Type baseType = GetBaseType(type);
			List<XmlNode> xmlNodes = node.ChildNodes.ToList();
			IList list = (IList)Activator.CreateInstance(type, true);
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
	internal class DictionarySerializer : ICustomSerializer
	{
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (typeof(IDictionary).IsAssignableFrom(type) == false)
			{
				output = null;
				return false;
			}
			XmlNodeList nodeList = node.ChildNodes;
			KeyValuePair<Type, Type> types = GetBaseTypes(type);
			IDictionary dictionary = (IDictionary)Activator.CreateInstance(type, true);
			for (int i = 0; i < nodeList.Count; i++)
			{
				XmlNode child = nodeList.Item(i);
				object key = reader.ReadObject(child.GetNode("key"), types.Key);
				object value = reader.ReadObject(child.GetNode("value"), types.Value);
				dictionary.Add(key, value);
			}
			output = dictionary;
			return true;
		}
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!(@object.Value is IDictionary dictionary))
			{
				output = null;
				return false;
			}
			OVSXmlWriter.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			// creating key element attribute
			KeyValuePair<Type, Type> types = GetBaseTypes(@object.ValueType);
			IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				object key = enumerator.Key;
				object value = enumerator.Value;
				XmlElement pair = writer.CreateElement(enumerableElement, "item");
				writer.WriteObject(new StructuredObject(key, types.Key), pair, "key");
				writer.WriteObject(new StructuredObject(value, types.Value), pair, "value");
			}
			output = enumerableElement;
			return true;
		}
		private KeyValuePair<Type, Type> GetBaseTypes(Type assigningType)
		{
			if (assigningType.Namespace == typeof(Dictionary<object, object>).Namespace)
			{
				Type[] types = assigningType.GetGenericArguments();
				return new KeyValuePair<Type, Type>(types[0], types[1]);
			}
			return new KeyValuePair<Type, Type>(typeof(object), typeof(object));
		}
	}
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
	internal class DatetimeSerializer : ICustomSerializer
	{
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output)
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
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
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
	internal class TimeSpanSerializer : ICustomSerializer
	{
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output)
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
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
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

namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using static XmlSerializer;

	internal class XmlReaderSerializer
	{
		// https://stackoverflow.com/questions/20008503/get-type-by-name
		internal static Type ByName(string name)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
			{
				var tt = assembly.GetType(name);
				if (tt != null)
				{
					return tt;
				}
			}
			return typeof(object);
		}


		protected XmlSerializerConfig config;
		public XmlReaderSerializer(XmlSerializerConfig config)
		{
			this.config = config;
		}
		public virtual object ReadDocument(XmlDocument document)
		{
			XmlNode rootNode = document.ChildNodes.Item(document.ChildNodes.Count - 1);
			return ReadObject(rootNode);
		}
		public virtual object ReadObject(XmlNode node)
		{
			if (node == null)
				return null;
			XmlNode attributeNode = node.Attributes.GetNamedItem(ATTRIBUTE)
				?? node.Attributes.GetNamedItem(ATTRIBUTE_ENUMERABLE)
				?? node.Attributes.GetNamedItem(ATTRIBUTE_ARRAY);
			string typeValue = attributeNode is null ? "" : attributeNode.Value;
			Type type = ByName(typeValue);
			if (TryReadPrimitive(type, node, out object output))
				return output;
			if (TryReadEnumerable(type, node, out object objectEnumerable))
				return objectEnumerable;
			object obj = Activator.CreateInstance(type, true);
			Dictionary<string, FieldInfo> fieldDictionary = new Dictionary<string, FieldInfo>();
			FieldInfo[] fieldInfos = type.GetFields(defaultFlags);
			Array.ForEach(fieldInfos, field => fieldDictionary.Add(field.Name, field));
			XmlNodeList childNodes = node.ChildNodes;
			for (int i = 0; i < childNodes.Count; i++)
			{
				XmlNode childNode = childNodes.Item(i);
				if (fieldDictionary.ContainsKey(childNode.Name))
					fieldDictionary[childNode.Name].SetValue(obj, ReadObject(childNode));
			}
			return obj;
		}
		internal protected virtual bool TryReadPrimitive(Type type, XmlNode node, out object output)
		{
			if (!type.IsPrimitive && type != typeof(string))
			{
				output = null;
				return false;
			}
			string unparsed = node.InnerText;
			if (type == typeof(string))
				output = unparsed;
			else if (int.TryParse(unparsed, out int result))
				output = result;
			else if (float.TryParse(unparsed, out float result1))
				output = result1;
			else if (bool.TryParse(unparsed, out bool result2))
				output = result2;
			else
				throw new NotImplementedException(type.ToString());
			return true;
		}
		internal protected virtual bool TryReadEnumerable(Type type, XmlNode node, out object output)
		{
			XmlNodeList nodeList = node.ChildNodes;
			if (type.IsArray)
			{
				Array array = Array.CreateInstance(type.GetElementType(), nodeList.Count);
				for (int i = 0; i < nodeList.Count; i++)
					array.SetValue(ReadObject(nodeList.Item(i)), i);
				output = array;
				return true;
			}
			if (!typeof(ICollection).IsAssignableFrom(type))
			{
				output = null;
				return false;
			}
			output = Activator.CreateInstance(type, true);
			if (output is IList list)
			{
				for (int i = 0; i < nodeList.Count; i++)
					list.Add(ReadObject(nodeList.Item(i)));
				return true;
			}
			if (output is IDictionary dictionary)
			{
				for (int i = 0; i < nodeList.Count; i++)
				{
					XmlNode key = nodeList.Item(i).SelectSingleNode("key");
					XmlNode value = nodeList.Item(i).SelectSingleNode("value");
					dictionary.Add(ReadObject(key), ReadObject(value));
				}
				return true;
			}
			//MethodInfo? method = type.GetMethod("Add");
			//if (method is not null)
			//{
			//	method.CreateDelegate(type, output).DynamicInvoke()
			//}
			throw new NotImplementedException();
		}
	}
}

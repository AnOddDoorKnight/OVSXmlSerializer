﻿namespace OVSXmlSerializer
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
		internal static string AddAutoImplementedTag(string input)
		{
			return $"<{input}>k__BackingField";
		}


		protected XmlSerializerConfig config;
		public XmlReaderSerializer(XmlSerializerConfig config)
		{
			this.config = config;
		}
		public virtual object ReadDocument(XmlDocument document, Type rootType)
		{
			XmlNode rootNode = document.ChildNodes.Item(document.ChildNodes.Count - 1);
			return ReadObject(rootNode, rootType);
		}
		public virtual object ReadObject(XmlNode node, Type currentType)
		{
			if (node == null)
				return null;
			if (!currentType.IsValueType) // Class Type probably is defined, but not derived. 
			{
				XmlNode attributeNode = node.Attributes.GetNamedItem(ATTRIBUTE);
				string possibleDerivedTypeName = null;
				if (attributeNode != null)
					possibleDerivedTypeName = attributeNode.Value;
				if (!string.IsNullOrEmpty(possibleDerivedTypeName))
				{
					Type possibleDerivedType = ByName(possibleDerivedTypeName);
					bool isDerived = currentType.IsAssignableFrom(possibleDerivedType);
					if (isDerived)
						currentType = possibleDerivedType;
				}
			}
			else if (currentType == null)
			{
				XmlNode attributeNode = node.Attributes.GetNamedItem(ATTRIBUTE);
				string typeValue = null;
				if (attributeNode != null) 
					typeValue = attributeNode.Value;
				if (string.IsNullOrEmpty(typeValue))
					throw new Exception();
				currentType = ByName(typeValue);
			}
			
			if (typeof(IXmlSerializable).IsAssignableFrom(currentType))
			{
				object serializableOutput = Activator.CreateInstance(currentType, true);
				IXmlSerializable xmlSerializable = (IXmlSerializable)serializableOutput;
				xmlSerializable.Read(node);
				return serializableOutput;
			}
			if (TryReadPrimitive(currentType, node, out object output))
				return output;
			if (TryReadEnumerable(currentType, node, out object objectEnumerable))
				return objectEnumerable;
			object obj = Activator.CreateInstance(currentType, true);
			Dictionary<string, FieldInfo> fieldDictionary = new Dictionary<string, FieldInfo>();
			FieldInfo[] fieldInfos = currentType.GetFields(defaultFlags);
			Array.ForEach(fieldInfos, field => fieldDictionary.Add(field.Name, field));
			XmlNodeList childNodes = node.ChildNodes;
			for (int i = 0; i < childNodes.Count; i++)
			{
				XmlNode childNode = childNodes.Item(i);
				string fieldName = childNode.Name;
				if (childNode.Attributes != null)
				{
					XmlNode attributeCon = childNode.Attributes.GetNamedItem(CONDITION);
					if (childNode.Attributes.GetNamedItem(CONDITION)?.Value == AUTO_IMPLEMENTED_PROPERTY)
						fieldName = AddAutoImplementedTag(fieldName);
				}
				if (fieldDictionary.TryGetValue(fieldName, out FieldInfo info))
					fieldDictionary[fieldName].SetValue(obj, ReadObject(childNode, info.FieldType));
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
			else if (type == typeof(double))
				output = double.Parse(unparsed);
			else if (type == typeof(int))
				output = int.Parse(unparsed);
			else if (type == typeof(bool))
				output = bool.Parse(unparsed);
			else
				throw new NotImplementedException(type.ToString());
			return true;
		}
		internal protected virtual bool TryReadEnumerable(Type type, XmlNode node, out object output)
		{
			XmlNodeList nodeList = node.ChildNodes;
			if (type.IsArray)
			{
				Type elementType = type.GetElementType();
				Array array = Array.CreateInstance(elementType, nodeList.Count);
				for (int i = 0; i < nodeList.Count; i++)
					array.SetValue(ReadObject(nodeList.Item(i), elementType), i);
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
				Type elementType = type.GenericTypeArguments[0];
				for (int i = 0; i < nodeList.Count; i++)
					list.Add(ReadObject(nodeList.Item(i), elementType));
				return true;
			}
			if (output is IDictionary dictionary)
			{
				Type keyType = type.GenericTypeArguments[0];
				Type valueType = type.GenericTypeArguments[1];
				for (int i = 0; i < nodeList.Count; i++)
				{
					XmlNode key = nodeList.Item(i).SelectSingleNode("key");
					XmlNode value = nodeList.Item(i).SelectSingleNode("value");
					dictionary.Add(ReadObject(key, keyType), ReadObject(value, valueType));
				}
				return true;
			}
			throw new NotImplementedException();
		}
	}
}

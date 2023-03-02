namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http.Headers;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Serialization;
	using static XmlSerializer;

	internal class XmlReaderSerializer
	{
		// https://stackoverflow.com/questions/20008503/get-type-by-name
		/// <summary>
		/// Gets the type by the full name in every existing assembly.
		/// </summary>
		/// <param name="name"> The full type name. </param>
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
		/// <summary>
		/// Adds the auto-implementation tag to an existing name.
		/// </summary>
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
			if (currentType == null)
			{
				XmlNode attributeNode = node.Attributes.GetNamedItem(ATTRIBUTE);
				string typeValue = null;
				if (attributeNode != null)
					typeValue = attributeNode.Value;
				if (string.IsNullOrEmpty(typeValue))
					throw new Exception();
				currentType = ByName(typeValue);
			}
			else if (!currentType.IsValueType && XmlAttributeAttribute.IsAttribute(currentType, out _))
			{
				// Class Type probably is defined, but not derived. 
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
			
			// Letting the jesus take the wheel
			if (typeof(IXmlSerializable).IsAssignableFrom(currentType))
			{
				object serializableOutput = Activator.CreateInstance(currentType, true);
				IXmlSerializable xmlSerializable = (IXmlSerializable)serializableOutput;
				xmlSerializable.Read(node);
				return serializableOutput;
			}
			if (TryReadEnum(currentType, node, out object objectEnum))
				return objectEnum;
			if (TryReadPrimitive(currentType, node, out object output))
				return output;
			if (TryReadEnumerable(currentType, node, out object objectEnumerable))
				return objectEnumerable;
			// Standard class with regular serialization.
			object obj = Activator.CreateInstance(currentType, true);
			
			// Serializes fields by getting fields by name, and matching it from
			// - the node list.
			FieldInfo[] allFields = currentType.GetFields(defaultFlags);
			string[] keys = new string[allFields.Length];
			Dictionary<string, FieldInfo> fieldDictionary = new Dictionary<string, FieldInfo>(allFields.Length);
			// Modify for auto-implemented properties
			for (int i = 0; i < allFields.Length; i++)
			{
				FieldInfo currentField = allFields[i];
				string key = keys[i] =
					FieldObject.IsProbablyAutoImplementedProperty(currentField.Name)
					? FieldObject.RemoveAutoPropertyTags(currentField.Name)
					: currentField.Name;
				fieldDictionary.Add(key, currentField);
			}
			// Getting Attributes
			List<(string Key, FieldInfo Value)> 
				attributes = new List<(string Key, FieldInfo Value)>(), 
				elements = new List<(string Key, FieldInfo Value)>();
			(string Key, FieldInfo Value)? text = null;
			for (int i = 0; i < allFields.Length; i++)
			{
				FieldInfo field = allFields[i];
				string fieldName = FieldObject.IsProbablyAutoImplementedProperty(field.Name)
					? FieldObject.RemoveAutoPropertyTags(field.Name)
					: field.Name;
				// Getting Named Arguments
				if (XmlNamedAsAttribute.HasName(field, out string name))
					fieldName = name;
				if (XmlAttributeAttribute.IsAttribute(field, out var attributeContents))
					attributes.Add(
						(string.IsNullOrEmpty(attributeContents.CustomName) 
						? fieldName 
						: attributeContents.CustomName, field));
				else if (XmlTextAttribute.IsText(field))
					text = (fieldName, field);
				else
					elements.Add((fieldName, field));
			}
			// Reading attributes first
			for (int i = 0; i < attributes.Count; i++)
			{
				string key = attributes[i].Key;
				XmlNode attribute = node.Attributes.GetNamedItem(key);
				FieldInfo field = attributes[i].Value;
				field.SetValue(obj, ReadObject(attribute, field.FieldType));
			}
			// Reading elements
			if (text.HasValue)
			{
				TryReadPrimitive(text.Value.Value.FieldType, node, out object textOutput);
				text.Value.Value.SetValue(obj, textOutput);
			}
			else
				for (int i = 0; i < elements.Count; i++)
				{
					string key = elements[i].Key;
					XmlNode element = node.SelectSingleNode(key);
					FieldInfo field = elements[i].Value;
					field.SetValue(obj, ReadObject(element, field.FieldType));
				}
			return obj;
		}
		internal protected virtual bool TryReadEnum(Type type, XmlNode node, out object output)
		{
			if (!type.IsEnum)
			{
				output = null;
				return false;
			}
			output = Convert.ChangeType(node.InnerText, type);
			return true;
		}
		internal protected virtual bool TryReadPrimitive(Type type, XmlNode node, out object output)
		{
			// Since string is arguably a class or char array, its it own check.
			if (!type.IsPrimitive && type != typeof(string))
			{
				output = null;
				return false;
			}
			string unparsed = node is XmlAttribute ? node.Value : node.InnerText;
			// I wonder if there is a method or library that does this for me..
			output = Convert.ChangeType(unparsed, type);
			return true;
		}
		/// <summary>
		/// Parses the enumerable if it implements <see cref="ICollection"/>.
		/// </summary>
		/// <param name="type"> The type. </param>
		/// <param name="node"> The data. </param>
		/// <param name="output"> The resulting item. </param>
		/// <returns>If it has successfully parsed. </returns>
		/// <exception cref="NotImplementedException"> 
		/// If there is no implementation for the collection. 
		/// </exception>
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
			// All lists or dictionaries derive from collections.
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

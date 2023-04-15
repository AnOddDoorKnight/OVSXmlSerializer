﻿namespace OVSXmlSerializer
{
	using OVSXmlSerializer.Extras;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Linq;
	using static XmlSerializer;

	internal class XmlWriterSerializer<T>
	{
		private const string SOURCE_WRITER = "Writer";
		private static string LogFieldObject(FieldObject fieldObject)
			=> $"field '{fieldObject.Field.Name}' from type '{fieldObject.ParentType}'";
		private static string StartLogObject(StructuredObject structuredObject)
		{
			if (structuredObject is null)
				return string.Empty;
			if (structuredObject is FieldObject fieldObject)
				return LogFieldObject(fieldObject);
			return $"'{structuredObject.Value}'";
		}
		/// <summary>
		/// Throws an exception if the specified type does not have a parameterless
		/// constructor. Otherwise does nothing.
		/// </summary>
		/// <param name="type"> The type to check if it has one. </param>
		internal void EnsureParameterlessConstructor(Type type)
		{
			if (type.GetConstructor(defaultFlags, null, Array.Empty<Type>(), null) == null && type.IsClass)
			{
				string message = $"{type.Name} does not have an empty constructor!";
				config.Logger?.InvokeMessage(SOURCE_WRITER, message);
				throw new NullReferenceException(message);
			}
		}


		//internal protected XmlWriter writer;
		internal protected XmlDocument document;
		protected XmlSerializerConfig config;
		protected XmlSerializer<T> source;

		protected Dictionary<object, XmlNode> referenceTypes;
		protected int nextRefIndex = 0;
		public XmlWriterSerializer(XmlSerializer<T> source)
		{
			this.config = source.Config;
			//this.writer = writer;
			this.source = source;
			// Writing base..
			document = new XmlDocument();
		}

		public XmlDocument Serialize(T obj, string name)
		{
			referenceTypes = new Dictionary<object, XmlNode>();
			var structuredObject = new StructuredObject(obj);
			config.Logger?.InvokeMessage(SOURCE_WRITER, $"Started XML Serialization of {(!structuredObject.IsNull ? structuredObject.Value.ToString() : "Null")}");
			startObject = true;
			if (!structuredObject.IsNull)
			{
				if (!config.OmitAutoGenerationComment)
					document.AppendChild(document.CreateComment("Auto-generated by OVSXmlSerializer"));
				WriteObject(structuredObject, document, EnsureName(name, structuredObject));
			}
			config.Logger?.InvokeMessage(SOURCE_WRITER, "Serialization finished!");
			return document;
		}

		private bool startObject = false;

		internal void WriteObject(StructuredObject obj, XmlNode parent, string name)
		{
			if (obj.IsNull)
			{
				if (obj is FieldObject fieldObject)
					config.Logger?.InvokeMessage(SOURCE_WRITER, $"{LogFieldObject(fieldObject)} is null. Skipping.");
				else
					config.Logger?.InvokeMessage(SOURCE_WRITER, "object value is null. Returning.");
				return;
			}
			if (XmlIgnoreAttribute.Ignore(obj))
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} contains {nameof(XmlIgnoreAttribute)}, ignoring.");
				return;
			}
			if (obj.Value is IXmlSerializable serializable)
			{
				// Not sure how the actual scheme works at all since I never used
				// - one. I don't this applies here at all anyways.
				if (serializable.ShouldWrite == false)
					return;
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} has {nameof(IXmlSerializable)} implemented, using.");
				EnsureParameterlessConstructor(obj.ValueType);
				XmlElement serializableElement = CreateElement(parent, name);
				serializable.Write(document, serializableElement);
				return;
			}
			if (obj.Value is Delegate)
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} is a delegate, skipping.");
				return;
			}

			if (ReferenceType(name, obj, parent))
				return;
			if (TryWriteEnum(name, obj, parent))
				return;
			if (TryWritePrimitive(name, obj, parent))
				return;
			if (XmlAttributeAttribute.IsAttribute(obj, out _)) // Not primitive, but struct or class
				throw new Exception($"{obj.Value} is not a primitive type!");
			if (TryWriteEnumerable(name, obj, parent))
				return;

			EnsureParameterlessConstructor(obj.ValueType);
			XmlNode currentNode = CreateElement(parent, name, obj);

			FieldInfo[] infos = obj.ValueType.GetFields(defaultFlags);
			config.Logger?.InvokeMessage(SOURCE_WRITER, $"Serializing {StartLogObject(obj)} normally..");

			bool didText = false;
			bool hasElements = false;
			for (int i = 0; i < infos.Length; i++)
			{
				FieldInfo field = infos[i];
				FieldObject fieldObject = new FieldObject(field, obj.Value);
				// Doing ignore again for compatibility with text attribute
				if (XmlIgnoreAttribute.Ignore(field))
				{
					continue;
				}

				if (XmlTextAttribute.IsText(field))
				{
					if (didText)
						throw new InvalidOperationException($"There can only be one [{nameof(XmlText)}]!");
					if (hasElements)
						throw new InvalidOperationException($"There are elements in the field in {obj.Value}, be sure to ignore or put them as attributes!");
					didText = true;
					if (!TryWritePrimitive(ref currentNode, fieldObject, parent))
						throw new InvalidCastException();
					continue;
				}
				if (XmlAttributeAttribute.IsAttribute(field, out var contents))
				{
					WriteObject(fieldObject, currentNode, EnsureName(field.Name, fieldObject));
					continue;
				}
				if (didText)
					throw new Exception();
				hasElements = true;
				WriteObject(fieldObject, currentNode, EnsureName(field.Name, fieldObject));
			}
		}
		internal bool ReferenceType(in string name, StructuredObject obj, XmlNode parent)
		{
			if (obj.ValueType.IsValueType)
				return false;
			if (obj.ValueType == typeof(string))
				return false;
			if (!referenceTypes.TryGetValue(obj.Value, out XmlNode referencedNode))
				return false;
			const string refAttName = REFERENCE_ATTRIBUTE;
			XmlElement referenceElement = CreateElement(parent, $"Reference_{name}");
			int? ID = null;
			for (int i = 0; i < referencedNode.Attributes.Count; i++)
				if (referencedNode.Attributes[i].Name == refAttName)
				{
					XmlNode attribute = referencedNode.Attributes[i];
					ID = int.Parse(attribute.Value);
					break;
				}
			// New Reference type
			if (!ID.HasValue)
			{
				ID = nextRefIndex;
				nextRefIndex++;
				XmlAttribute attribute = document.CreateAttribute(refAttName);
				attribute.Value = ID.Value.ToString();
				referencedNode.Attributes.Prepend(attribute);
			}
			referenceElement.InnerText = ID.Value.ToString();
			return true;
		}
		internal bool TryWriteEnum(in string name, StructuredObject @enum, XmlNode parent)
		{
			if (!@enum.ValueType.IsEnum)
				return false;
			config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(@enum)} is an enum, serializing element with .ToString().");
			XmlElement element = CreateElement(parent, name, @enum);
			element.InnerText = @enum.Value.ToString();
			return true;
		}
		internal bool TryWritePrimitive(string name, StructuredObject primitive, XmlNode parent)
		{
			if (!IsPrimitive(primitive))
				return false;
			if (XmlAttributeAttribute.IsAttribute(primitive, out var attribute))
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is a {primitive.ValueType}, serializing primitive as attribute..");
				if (primitive is FieldObject fieldObj && fieldObj.IsDerivedFromBase)
				{
					const string MESSAGE_ERROR = "Cannot serialize as the field " +
						"type doesn't match the object type. This is typically " +
						"caused as the primitive value is derived off of object " +
						"in an XML Attribute, unable to write the derived attribute " +
						"type.";
					config.Logger?.InvokeMessage(SOURCE_WRITER, MESSAGE_ERROR);
					throw new Exception(MESSAGE_ERROR);
				}
				if (XmlNamedAsAttribute.HasName(primitive, out string namedAs))
				{
					config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is renamed to '{namedAs}', as having the {nameof(XmlNamedAsAttribute)} attribute");
					name = namedAs;
				}
				if (!string.IsNullOrEmpty(attribute.CustomName))
					name = attribute.CustomName;
				XmlAttribute xmlAttribute = CreateAttribute(parent, name, ToStringPrimitive(primitive.Value));
				return true;
			}
			config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is a {primitive.ValueType}, serializing primitive..");
			XmlNode currentNode = CreateElement(parent, name, primitive);
			currentNode.InnerText = ToStringPrimitive(primitive.Value);
			return true;
		}
		internal bool TryWritePrimitive(ref XmlNode currentNode, StructuredObject primitive, XmlNode parent)
		{
			if (!IsPrimitive(primitive))
				return false;
			if (XmlAttributeAttribute.IsAttribute(primitive, out var attribute))
			{
				if (!(currentNode is XmlAttribute))
					throw new Exception();
				if (primitive is FieldObject fieldObj && fieldObj.IsDerivedFromBase)
				{
					const string MESSAGE_ERROR = "Cannot serialize as the field " +
						"type doesn't match the object type. This is typically " +
						"caused as the primitive value is derived off of object " +
						"in an XML Attribute, unable to write the derived attribute " +
						"type.";
					config.Logger?.InvokeMessage(SOURCE_WRITER, MESSAGE_ERROR);
					throw new Exception(MESSAGE_ERROR);
				}
				if (XmlNamedAsAttribute.HasName(primitive, out string namedAs))
				{
					parent.RemoveChild(currentNode);
					currentNode = CreateAttribute(parent, namedAs, "");
					config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is renamed to '{namedAs}', as having the {nameof(XmlNamedAsAttribute)} attribute");
				}
				currentNode.Value = ToStringPrimitive(primitive.Value);
				return true;
			}
			config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is a {primitive.ValueType}, serializing primitive..");
			currentNode.InnerText = ToStringPrimitive(primitive.Value);
			return true;
		}
		private string ToStringPrimitive(object value)
		{
			if (value is IFormattable formattable)
				return formattable.ToString(null, config.CurrentCulture);
			return value.ToString();
		}
		internal bool TryWriteEnumerable(string name, StructuredObject values, XmlNode parent)
		{
			if (XMLIgnoreEnumerableAttribute.Ignore(values))
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(values)} may be an enumerable, but not serialize as one due to {nameof(XMLIgnoreEnumerableAttribute)} being present");
				return false;
			}
			if (values.ValueType.IsArray)
			{
				XmlElement arrayElement = CreateElement(parent, name, values);
				Array arrValue = (Array)values.Value;
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"Serializing {StartLogObject(values)} as an array with {arrValue.Length} elements..");
				for (int i = 0; i < arrValue.Length; i++)
				{
					StructuredObject currentValue = new StructuredObject(arrValue.GetValue(i));
					WriteObject(currentValue, arrayElement, "Item");
				}
				return true;
			}
			object value = values.Value;
			if (value is IEnumerable enumerable)
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"Serializing {StartLogObject(values)} as an ordinary enumerable..");
				EnsureParameterlessConstructor(values.ValueType);
				XmlElement enumerableElement = CreateElement(parent, name, values);
				IEnumerator enumerator = enumerable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					StructuredObject currentValue = new StructuredObject(enumerator.Current);
					WriteObject(currentValue, enumerableElement, "Item");
				}
				try { enumerator.Reset(); } catch { }
				return true;
			}
			return false;
		}



		internal XmlElement CreateElement(XmlNode parent, string name, StructuredObject obj)
		{
			XmlElement element = CreateElement(parent, name);
			WriteTypeAttribute(element, obj);
			TryWriteVersionNumber(element);
			AddReferenceType(obj, element);
			return element;
		}
		internal XmlElement CreateElement(XmlNode parent, string elementName)
		{
			XmlElement element = document.CreateElement(elementName);
			(parent ?? document).AppendChild(element);
			TryWriteVersionNumber(element);
			return element;
		}
		internal XmlAttribute CreateAttribute(XmlNode parent, string name, string value)
		{
			XmlAttribute attribute = document.CreateAttribute(name);
			attribute.Value = value;
			parent.Attributes.Append(attribute);
			return attribute;
		}
		internal XmlAttribute WriteTypeAttribute(XmlNode parent, StructuredObject obj)
		{
			if (config.TypeHandling == IncludeTypes.AlwaysIncludeTypes)
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"serializing type's {StartLogObject(obj)}, as it is set to always include types..");
				return CreateAttribute(parent, ATTRIBUTE, obj.ValueType.FullName);
			}
			if (config.TypeHandling == IncludeTypes.SmartTypes)
				if (obj is FieldObject fieldObj && fieldObj.IsDerivedFromBase)
				{
					config.Logger?.InvokeMessage(SOURCE_WRITER, $"serializing type's {StartLogObject(obj)}, as it is a derived type from field type '{fieldObj.Field.FieldType}'..");
					return CreateAttribute(parent, ATTRIBUTE, obj.ValueType.FullName);
				}
			return null;
		}
		private bool TryWriteVersionNumber(XmlNode parent)
		{
			if (!startObject)
				return false;
			if (config.Version is null)
				return false;
			startObject = false;
			CreateAttribute(parent, Versioning.VERSION_ATTRIBUTE, config.Version.ToString());
			return true;
		}
		internal string EnsureName(string name, StructuredObject obj)
		{
			if (obj.IsNull)
				return "";
			if (XmlNamedAsAttribute.HasName(obj, out string namedAtt))
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} is renamed to '{namedAtt}', as having the {nameof(XmlNamedAsAttribute)} attribute");
				name = namedAtt;
			}
			if (obj is FieldObject fieldObj && fieldObj.IsAutoImplementedProperty)
			{
				config.Logger?.InvokeMessage(SOURCE_WRITER, $"Converting '{name}' to '{FieldObject.RemoveAutoPropertyTags(name)}'");
				name = FieldObject.RemoveAutoPropertyTags(name);
			}
			name = name.Replace('`', '_');
			name = name.TrimEnd('[', ']');
			return name;
		}
		internal bool IsPrimitive(StructuredObject primitive)
		{
			return primitive.ValueType.IsPrimitive || primitive.ValueType == typeof(string);
		}
		internal bool AddReferenceType(StructuredObject obj, XmlNode representingNode)
		{
			if (obj.ValueType.IsValueType)
				return false;
			if (obj.ValueType == typeof(string))
				return false;
			referenceTypes.Add(obj.Value, representingNode);
			return true;
		}
	}
}
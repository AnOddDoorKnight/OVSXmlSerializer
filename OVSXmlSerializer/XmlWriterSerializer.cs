namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using static XmlSerializer;

	internal class XmlWriterSerializer
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
				config.logger?.InvokeMessage(SOURCE_WRITER, message);
				throw new NullReferenceException(message);
			}
		}
		

		internal protected XmlWriter writer;
		protected XmlSerializerConfig config;
		public XmlWriterSerializer(XmlSerializerConfig config, XmlWriter writer)
		{
			this.config = config;
			this.writer = writer;
		}

		internal bool ApplySmartType(StructuredObject obj)
		{
			if (config.TypeHandling != IncludeTypes.SmartTypes)
				return false;
			if (obj is FieldObject fieldObj && fieldObj.IsDerivedFromBase)
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"serializing type's {StartLogObject(obj)}, as it is a derived type from field type '{fieldObj.Field.FieldType}'..");
				return true;
			}
			return false;
		}
		internal void WriteAttributeType(StructuredObject obj)
		{
			if (config.TypeHandling == IncludeTypes.AlwaysIncludeTypes)
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"serializing type's {StartLogObject(obj)}, as it is set to always include types..");
				writer.WriteAttributeString(ATTRIBUTE, obj.ValueType.FullName);
				return;
			}
			if (ApplySmartType(obj))
				writer.WriteAttributeString(ATTRIBUTE, obj.ValueType.FullName);
		}
		internal bool TryWriteEnumerable(string name, StructuredObject values)
		{
			if (XMLIgnoreEnumerableAttribute.Ignore(values))
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(values)} may be an enumerable, but not serialize as one due to {nameof(XMLIgnoreEnumerableAttribute)} being present");
				return false;
			}
			if (values.ValueType.IsArray)
			{
				WriteStartElement(name.TrimEnd('[', ']'), values);
				WriteAttributeType(values);
				Array arrValue = (Array)values.Value;
				config.logger?.InvokeMessage(SOURCE_WRITER, $"Serializing {StartLogObject(values)} as an array with {arrValue.Length} elements..");
				for (int i = 0; i < arrValue.Length; i++)
					WriteObject("Item", new StructuredObject(arrValue.GetValue(i)));
				writer.WriteEndElement();
				return true;
			}
			object value = values.Value;
			if (value is IEnumerable enumerable)
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"Serializing {StartLogObject(values)} as an ordinary enumerable..");
				EnsureParameterlessConstructor(values.ValueType);
				WriteStartElement(name.Replace('`', '_'), values);
				WriteAttributeType(values);
				IEnumerator enumerator = enumerable.GetEnumerator();
				while (enumerator.MoveNext())
					WriteObject("Item", new StructuredObject(enumerator.Current));
				try
				{
					enumerator.Reset();
				}
				catch 
				{
				
				}
				writer.WriteEndElement();
				return true;
			}
			return false;
		}
		internal bool TryWritePrimitive(string name, StructuredObject primitive, bool startElement = true)
		{
			if (!primitive.ValueType.IsPrimitive && primitive.ValueType != typeof(string))
				return false;
			if (XmlAttributeAttribute.IsAttribute(primitive, out var attribute))
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is a {primitive.ValueType}, serializing primitive as attribute..");
				if (primitive is FieldObject fieldObj && fieldObj.IsDerivedFromBase)
				{
					const string MESSAGE_ERROR = "Cannot serialize as the field type doesn't match the object type. " +
						"This is typically caused as the primitive value is derived off of object in an XML Attribute, unable to write the derived attribute type.";
					config.logger?.InvokeMessage(SOURCE_WRITER, MESSAGE_ERROR);
					throw new Exception(MESSAGE_ERROR);
				}
				if (XmlNamedAsAttribute.HasName(primitive, out string namedAs))
				{
					config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is renamed to '{namedAs}', as having the {nameof(XmlNamedAsAttribute)} attribute");
					name = namedAs;
				}
				if (!string.IsNullOrEmpty(attribute.CustomName))
					name = attribute.CustomName;
				writer.WriteAttributeString(name, primitive.Value.ToString());
				return true;
			}
			config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(primitive)} is a {primitive.ValueType}, serializing primitive..");
			if (startElement)
				WriteStartElement(name, primitive);
			WriteAttributeType(primitive);
			writer.WriteString(primitive.Value.ToString());
			writer.WriteEndElement();
			return true;
		}
		internal bool TryWriteEnum(in string name, StructuredObject @enum)
		{
			if (!@enum.ValueType.IsEnum)
				return false;
			config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(@enum)} is an enum, serializing element with .ToString().");
			WriteStartElement(name, @enum);
			WriteAttributeType(@enum);
			writer.WriteString(@enum.Value.ToString());
			writer.WriteEndElement();
			return true;
		}

		internal void StartWriteObject(in string name, StructuredObject obj)
		{
			config.logger?.InvokeMessage(SOURCE_WRITER, $"Started XML Serialization of {(!obj.IsNull ? obj.Value.ToString() : "Null")}");
			startObject = true;
			WriteObject(name, obj);
			config.logger?.InvokeMessage(SOURCE_WRITER, "Serialization finished!");
		}
		private bool startObject = false;
		private bool TryWriteVersionNumber()
		{
			if (!startObject)
				return false;
			if (config.Version is null)
				return false;
			startObject = false;
			writer.WriteAttributeString(VERSION_ATTRIBUTE, config.Version.ToString());
			return true;
		}
		internal void WriteObject(in string name, StructuredObject obj)
		{
			if (obj.IsNull)
			{
				if (obj is FieldObject fieldObject)
					config.logger?.InvokeMessage(SOURCE_WRITER, $"{LogFieldObject(fieldObject)} is null. Skipping.");
				else
					config.logger?.InvokeMessage(SOURCE_WRITER, "object value is null. Returning.");
				return;
			}
			if (XmlIgnoreAttribute.Ignore(obj))
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} contains {nameof(XmlIgnoreAttribute)}, ignoring.");
				return;
			}

			if (obj.Value is IXmlSerializable serializable)
			{
				// Not sure how the actual scheme works at all since I never used
				// - one. I don't this applies here at all anyways.
				if (serializable.ShouldWrite == false)
					return;
				config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} has {nameof(IXmlSerializable)} implemented, using.");
				EnsureParameterlessConstructor(obj.ValueType);
				WriteStartElement(name, obj);
				WriteAttributeType(obj);
				serializable.Write(writer);
				writer.WriteEndElement();
				return;
			}
			if (obj.Value is Delegate)
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} is a delegate, skipping.");
				return;
			}
			if (TryWriteEnum(name, obj))
				return;
			if (TryWritePrimitive(name, obj))
				return;
			if (XmlAttributeAttribute.IsAttribute(obj, out _)) // Not primitive, but struct or class
				throw new Exception($"{obj.Value} is not a primitive type!");
			if (TryWriteEnumerable(name, obj))
				return;
			EnsureParameterlessConstructor(obj.ValueType);
			WriteStartElement(name, obj);
			WriteAttributeType(obj);


			FieldInfo[] infos = obj.ValueType.GetFields(defaultFlags);
			config.logger?.InvokeMessage(SOURCE_WRITER, $"Serializing {StartLogObject(obj)} normally..");



			List<FieldInfo> attributes = new List<FieldInfo>(),
				elements = new List<FieldInfo>(),
				text = new List<FieldInfo>();
			// order by values that has the attribute attributes
			for (int i = 0; i < infos.Length; i++)
			{
				FieldInfo field = infos[i];
				// Doing ignore again for compatibility with text attribute
				if (XmlIgnoreAttribute.Ignore(field))
					continue;
				else if (XmlTextAttribute.IsText(field))
					text.Add(field);
				else if (XmlAttributeAttribute.IsAttribute(field, out var attributeContents))
					attributes.Add(field);
				else
					elements.Add(field);
			}
			config.logger?.InvokeMessage(SOURCE_WRITER, $"Collected fields from {StartLogObject(obj)}: " +
				$"\n{attributes.Count} attributes \n{{\n\t{string.Join("\n\t", attributes)} \n}}" +
				$"\n{elements.Count} elements \n{{\n\t{string.Join("\n\t", elements)} \n}}" +
				$"\n{text.Count} textFields \n{{\n\t{string.Join("\n\t", text)} \n}}");
			WriteValues(attributes);
			if (text.Count > 0)
			{
				if (text.Count > 1)
					throw new InvalidOperationException($"There can only be one [{nameof(XmlText)}]!");
				if (elements.Count > 0)
					throw new InvalidOperationException($"There are elements in the field in {obj.Value}, be sure to ignore or put them as attributes!");
				FieldObject textField = new FieldObject(text.Single(), obj.Value);
				if (!TryWritePrimitive(null, textField, false))
					throw new InvalidCastException();
				return;
			}
			WriteValues(elements);
			writer.WriteEndElement();

			void WriteValues(List<FieldInfo> fieldInfos)
			{
				for (int i = 0; i < fieldInfos.Count; i++)
				{
					FieldInfo field = fieldInfos[i];
					FieldObject @struct = new FieldObject(field, obj.Value);
					WriteObject(field.Name, @struct);
				}
			}
		}
		/// <summary>
		/// Writes the starting element.
		/// </summary>
		/// <remarks>
		/// This fixes issues when considering auto-implemented properties.
		/// </remarks>
		/// <param name="name"> The name of the element. </param>
		/// <param name="obj"> The object to write about. </param>
		internal void WriteStartElement(string name, StructuredObject obj)
		{
			if (XmlNamedAsAttribute.HasName(obj, out string namedAtt))
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"{StartLogObject(obj)} is renamed to '{namedAtt}', as having the {nameof(XmlNamedAsAttribute)} attribute");
				name = namedAtt;
			}
			if (obj is FieldObject fieldObj && fieldObj.IsAutoImplementedProperty)
			{
				config.logger?.InvokeMessage(SOURCE_WRITER, $"Converting '{name}' to '{FieldObject.RemoveAutoPropertyTags(name)}'");
				name = FieldObject.RemoveAutoPropertyTags(name);
			}
			name = name.Replace('`', '_');
			writer.WriteStartElement(name);
			TryWriteVersionNumber();
		}
	}
}

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
		/// <summary>
		/// Throws an exception if the specified type does not have a parameterless
		/// constructor. Otherwise does nothing.
		/// </summary>
		/// <param name="type"> The type to check if it has one. </param>
		internal static void EnsureParameterlessConstructor(Type type)
		{
			if (type.GetConstructor(defaultFlags, null, Array.Empty<Type>(), null) == null && type.IsClass)
				throw new NullReferenceException($"{type.Name} does not have an empty constructor!");
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
				return true;
			return false;
		}
		internal void WriteAttributeType(StructuredObject obj)
		{
			if (config.TypeHandling == IncludeTypes.AlwaysIncludeTypes)
			{
				writer.WriteAttributeString(ATTRIBUTE, obj.ValueType.FullName);
				return;
			}
			if (ApplySmartType(obj))
				writer.WriteAttributeString(ATTRIBUTE, obj.ValueType.FullName);
		}
		internal bool TryWriteEnumerable(string name, StructuredObject values)
		{
			if (XMLIgnoreEnumerableAttribute.Ignore(values))
				return false;
			if (values.ValueType.IsArray)
			{
				WriteStartElement(name.TrimEnd('[', ']'), values);
				WriteAttributeType(values);
				Array arrValue = (Array)values.Value;
				for (int i = 0; i < arrValue.Length; i++)
					WriteObject("Item", new StructuredObject(arrValue.GetValue(i)));
				writer.WriteEndElement();
				return true;
			}
			object value = values.Value;
			if (value is IEnumerable enumerable)
			{
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
				if (primitive is FieldObject fieldObj && fieldObj.IsDerivedFromBase)
					throw new Exception("Cannot serialize as the field type doesn't match the object type.");
				if (XmlNamedAsAttribute.HasName(primitive, out string namedAs))
					name = namedAs;
				if (!string.IsNullOrEmpty(attribute.CustomName))
					name = attribute.CustomName;
				writer.WriteAttributeString(name, primitive.Value.ToString());
				return true;
			}
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
			WriteStartElement(name, @enum);
			WriteAttributeType(@enum);
			writer.WriteString(@enum.Value.ToString());
			writer.WriteEndElement();
			return true;
		}

		internal void StartWriteObject(in string name, StructuredObject obj)
		{
			WriteObject(name, obj);
		}
		internal void WriteObject(in string name, StructuredObject obj)
		{
			if (obj.IsNull)
				return;
			if (XmlIgnoreAttribute.Ignore(obj))
				return;
			if (obj.Value is IXmlSerializable serializable)
			{
				// Not sure how the actual scheme works at all since I never used
				// - one. I don't this applies here at all anyways.
				if (serializable.ShouldWrite == false)
					return;
				EnsureParameterlessConstructor(obj.ValueType);
				WriteStartElement(name, obj);
				WriteAttributeType(obj);
				serializable.Write(writer);
				writer.WriteEndElement();
				return;
			}
			if (obj.Value is Delegate)
				return;
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
				name = namedAtt;
			if (obj is FieldObject fieldObj && fieldObj.IsAutoImplementedProperty)
				name = FieldObject.RemoveAutoPropertyTags(name);
			name = name.Replace('`', '_');
			writer.WriteStartElement(name);
		}
	}
}

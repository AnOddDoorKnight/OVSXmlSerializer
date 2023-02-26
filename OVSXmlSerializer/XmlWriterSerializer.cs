namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Serialization;
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
			if (obj.parent is null || !obj.IsDerivedFromBase)
				return false;
			return true;
		}
		internal void WriteAttributeType(StructuredObject obj)
		{
			if (config.TypeHandling == IncludeTypes.AlwaysIncludeTypes)
			{
				writer.WriteAttributeString(ATTRIBUTE, obj.valueType.FullName);
				return;
			}
			if (ApplySmartType(obj))
				writer.WriteAttributeString(ATTRIBUTE, obj.valueType.FullName);
		}
		/// <summary>
		/// If it should ignore the obejct if the field or object has the
		/// <see cref="XmlIgnoreAttribute"/>
		/// </summary>
		internal bool IgnoreObject(StructuredObject obj)
		{
			if (Attribute.GetCustomAttributes(obj.valueType, typeof(XmlIgnoreAttribute)).Length > 0)
				return true;
			if (obj.parent is null)
				return false;
			return obj.HasAttribute<XmlIgnoreAttribute>();
		}
		internal bool TryWriteEnumerable(string name, StructuredObject values)
		{
			if (values.valueType.IsArray)
			{
				WriteStartElement(name.TrimEnd('[', ']'), values);
				WriteAttributeType(values);
				Array arrValue = (Array)values.value;
				for (int i = 0; i < arrValue.Length; i++)
					WriteObject("Item", new StructuredObject(arrValue.GetValue(i)));
				writer.WriteEndElement();
				return true;
			}
			object value = values.value;
			if (value is IEnumerable enumerable)
			{
				EnsureParameterlessConstructor(values.valueType);
				WriteStartElement(name.Replace('`', '_'), values);
				WriteAttributeType(values);
				IEnumerator enumerator = enumerable.GetEnumerator();
				while (enumerator.MoveNext())
					WriteObject("Item", new StructuredObject(enumerator.Current));
				enumerator.Reset();
				writer.WriteEndElement();
				return true;
			}

			return false;
		}
		internal bool TryWritePrimitive(string name, StructuredObject primitive)
		{
			if (!primitive.valueType.IsPrimitive && primitive.valueType != typeof(string))
				return false;
			if (primitive.HasAttribute<XmlAttributeAttribute>())
			{
				if (primitive.IsDerivedFromBase)
					throw new Exception("Cannot serialize as the field type doesn't match the object type.");
				writer.WriteAttributeString(name, primitive.value.ToString());
				return true;
			}
			WriteStartElement(name, primitive);
			WriteAttributeType(primitive);
			writer.WriteString(primitive.value.ToString());
			writer.WriteEndElement();
			return true;
		}
		internal bool TryWriteEnum(in string name, StructuredObject @enum)
		{
			if (!@enum.valueType.IsEnum)
				return false;
			WriteStartElement(name, @enum);
			WriteAttributeType(@enum);
			writer.WriteString(@enum.value.ToString());
			writer.WriteEndElement();
			return true;
		}
		internal void WriteObject(in string name, StructuredObject obj)
		{
			if (obj.isNull)
				return;
			if (IgnoreObject(obj))
				return;
			if (obj.value is IXmlSerializable serializable)
			{
				// Not sure how the actual scheme works at all since I never used
				// - one. I don't this applies here at all anyways.
				if (serializable.ShouldWrite == false)
					return;
				EnsureParameterlessConstructor(obj.valueType);
				WriteStartElement(name, obj);
				WriteAttributeType(obj);
				serializable.Write(writer);
				writer.WriteEndElement();
				return;
			}
			if (TryWriteEnum(name, obj))
				return;
			if (TryWritePrimitive(name, obj))
				return;
			if (obj.HasAttribute<XmlAttributeAttribute>()) // Not primitive, but struct or class
				throw new Exception();
			if (TryWriteEnumerable(name, obj))
				return;
			EnsureParameterlessConstructor(obj.valueType);
			WriteStartElement(name, obj);
			WriteAttributeType(obj);

			FieldInfo[] infos = obj.valueType.GetFields(defaultFlags);
			List<FieldInfo> left = new List<FieldInfo>(), right = new List<FieldInfo>();
			// order by values that has the attribute attributes
			for (int i = 0; i < infos.Length; i++)
			{
				FieldInfo field = infos[i];
				if (field.GetCustomAttribute<XmlAttributeAttribute>() != null)
					left.Add(field);
				else
					right.Add(field);
			}
			WriteValues(left);
			WriteValues(right);
			writer.WriteEndElement();

			void WriteValues(List<FieldInfo> fieldInfos)
			{
				for (int i = 0; i < fieldInfos.Count; i++)
				{
					FieldInfo field = fieldInfos[i];
					StructuredObject @struct = new StructuredObject(field, obj);
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
			if (obj.IsAutoImplementedProperty)
			{
				writer.WriteStartElement(StructuredObject.RemoveAutoPropertyTags(name));
				return;
			}
			writer.WriteStartElement(name);
		}
	}
}

namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
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

		internal void WriteAttributeType(StructuredObject obj)
		{
			if (config.alwaysIncludeTypes)
			{
				writer.WriteAttributeString(ATTRIBUTE, obj.valueType.FullName);
				return;
			}
			if (!config.smartTypes)
				return;
			// Smart types here
			if (obj.parent is null || !obj.IsDerivedFromBase)
				return;
			writer.WriteAttributeString(ATTRIBUTE, obj.valueType.FullName);
		}
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
			WriteStartElement(name, primitive);
			WriteAttributeType(primitive);
			writer.WriteString(primitive.value.ToString());
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
			if (TryWritePrimitive(name, obj))
				return;
			if (TryWriteEnumerable(name, obj))
				return;
			EnsureParameterlessConstructor(obj.valueType);
			WriteStartElement(name, obj);
			WriteAttributeType(obj);

			FieldInfo[] infos = obj.valueType.GetFields(defaultFlags);
			for (int i = 0; i < infos.Length; i++)
			{
				FieldInfo field = infos[i];
				StructuredObject @struct = new StructuredObject(field, obj);
				WriteObject(field.Name, @struct);
			}
			writer.WriteEndElement();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// This fixes issues when considering auto-implemented properties.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		internal void WriteStartElement(string name, StructuredObject obj)
		{
			if (obj.IsAutoImplementedProperty)
			{
				writer.WriteStartElement(name.Substring(1, name.IndexOf('>') - 1));
				writer.WriteAttributeString(CONDITION, AUTO_IMPLEMENTED_PROPERTY);
				return;
			}
			writer.WriteStartElement(name);
		}
	}
}

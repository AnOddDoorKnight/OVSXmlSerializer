namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Serialization;
	using static XmlSerializer;

	internal class XmlWriterSerializer
	{
		internal protected XmlWriter writer;
		protected XmlSerializerConfig config;
		public XmlWriterSerializer(XmlSerializerConfig config, XmlWriter writer)
		{
			this.config = config;
			this.writer = writer;
		}

		internal bool TryWriteEnumerable(string name, StructuredObject values)
		{
			if (values.ValueType.IsArray)
			{
				writer.WriteStartElement(name.TrimEnd('[', ']'));
				if (config.includeTypes)
					writer.WriteAttributeString(ATTRIBUTE_ARRAY, values.ValueType.FullName);
				Array arrValue = (Array)values.Value;
				for (int i = 0; i < arrValue.Length; i++)
					WriteObject("Item", new StructuredObject(arrValue.GetValue(i)));
				writer.WriteEndElement();
				return true;
			}
			if (values.Value is IEnumerable enumerable)
			{
				if (values.ValueType.GetConstructor(defaultFlags, null, Array.Empty<Type>(), null) == null)
					throw new NullReferenceException($"{values.ValueType.FullName} does not have an empty constructor!");
				writer.WriteStartElement(name.Replace('`', '_'));
				if (config.includeTypes)
					writer.WriteAttributeString(ATTRIBUTE_ENUMERABLE, values.ValueType.FullName);
				var enumerator = enumerable.GetEnumerator();
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
			if (!primitive.ValueType.IsPrimitive && primitive.ValueType != typeof(string))
				return false;
			writer.WriteStartElement(name);
			if (config.includeTypes)
				writer.WriteAttributeString(ATTRIBUTE, primitive.ValueType.FullName);
			writer.WriteString(primitive.Value.ToString());
			writer.WriteEndElement();
			return true;
		}
		internal void WriteObject(in string name, StructuredObject obj)
		{
			if (obj.IsNull)
				return;
			if (Attribute.GetCustomAttributes(obj.ValueType, typeof(XmlIgnoreAttribute)).Length > 0)
				return;
			if (TryWritePrimitive(name, obj))
				return;
			if (TryWriteEnumerable(name, obj))
				return;
			if (obj.ValueType.GetConstructor(defaultFlags, null, Array.Empty<Type>(), null) == null && obj.ValueType.IsClass)
				throw new NullReferenceException($"{obj.ValueType.Name} does not have an empty constructor!");

			writer.WriteStartElement(name);
			if (config.includeTypes)
				writer.WriteAttributeString(ATTRIBUTE, obj.ValueType.FullName);

			FieldInfo[] infos = obj.ValueType.GetFields(defaultFlags);
			for (int i = 0; i < infos.Length; i++)
			{
				FieldInfo field = infos[i];
				StructuredObject value = new StructuredObject(field.GetValue(obj.Value));
				WriteObject(field.Name, value);
			}
			writer.WriteEndElement();
		}
	}
}

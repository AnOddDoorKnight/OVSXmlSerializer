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
				writer.WriteStartElement(name.TrimEnd('[', ']'));
				if (config.includeTypes)
					writer.WriteAttributeString(ATTRIBUTE, values.valueType.FullName);
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
				writer.WriteStartElement(name.Replace('`', '_'));
				if (config.includeTypes)
					writer.WriteAttributeString(ATTRIBUTE, values.valueType.FullName);
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
			writer.WriteStartElement(name);
			if (config.includeTypes)
				writer.WriteAttributeString(ATTRIBUTE, primitive.valueType.FullName);
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
				if (serializable.ShouldWrite == false)
					return;
				EnsureParameterlessConstructor(obj.valueType);
				writer.WriteStartElement(name);
				if (config.includeTypes)
					writer.WriteAttributeString(ATTRIBUTE, obj.valueType.FullName);
				serializable.Write(writer);
				writer.WriteEndElement();
				return;
			}
			if (TryWritePrimitive(name, obj))
				return;
			if (TryWriteEnumerable(name, obj))
				return;
			EnsureParameterlessConstructor(obj.valueType);
			writer.WriteStartElement(name);
			if (config.includeTypes)
				writer.WriteAttributeString(ATTRIBUTE, obj.valueType.FullName);

			FieldInfo[] infos = obj.valueType.GetFields(defaultFlags);
			for (int i = 0; i < infos.Length; i++)
			{
				FieldInfo field = infos[i];
				StructuredObject @struct = new StructuredObject(field, obj);
				WriteObject(field.Name, @struct);
			}
			writer.WriteEndElement();
		}
	}
}

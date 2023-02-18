namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml;
	using static XmlSerializer;

	internal class XmlWriterSerializer
	{
		protected XmlWriter writer;
		protected XmlSerializerConfig config;
		public XmlWriterSerializer(XmlSerializerConfig config, XmlWriter writer)
		{
			this.config = config;
			this.writer = writer;
		}

		internal bool TryWriteEnumerable(string name, object values)
		{
			Type valueType = values.GetType();
			if (valueType.IsArray)
			{
				writer.WriteStartElement(name);
				if (config.includeTypes)
					writer.WriteAttributeString(ATTRIBUTE_ARRAY, valueType.FullName);//valueType.GetElementType().FullName);
				// Cannot find any other method, since int32[] exist and
				// - cannot be casted as object[]
				Array arrValue = (Array)values;
				for (int i = 0; i < arrValue.Length; i++)
					WriteObject("Item", arrValue.GetValue(i)!);
				writer.WriteEndElement();
				return true;
			}
			if (values is not IEnumerable enumerable)
				return false;
			if (valueType.GetConstructor(XmlSerializer.defaultFlags, null, Array.Empty<Type>(), null) == null)
				throw new NullReferenceException($"{valueType.FullName} does not have an empty constructor!");
			writer.WriteStartElement(name);
			if (config.includeTypes)
				writer.WriteAttributeString(ATTRIBUTE_ENUMERABLE, valueType.FullName);
			var enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
				WriteObject("Item", enumerator.Current);
			enumerator.Reset();
			writer.WriteEndElement();
			return true;
		}
		internal bool TryWritePrimitive(string name, object primitive)
		{
			Type type = primitive.GetType();
			if (!type.IsPrimitive && type != typeof(string))
				return false;
			writer.WriteStartElement(name);
			if (config.includeTypes)
				writer.WriteAttributeString(ATTRIBUTE, type.FullName);
			writer.WriteString(primitive.ToString());
			writer.WriteEndElement();
			return true;
		}
		internal void WriteObject(in string name, object obj)
		{
			if (obj is null)
				return;
			if (TryWritePrimitive(name, obj))
				return;
			if (TryWriteEnumerable(name, obj))
				return;
			Type type = obj.GetType();
			if (type.GetConstructor(defaultFlags, null, Array.Empty<Type>(), null) == null && type.IsClass)
				throw new NullReferenceException($"{type.Name} does not have an empty constructor!");

			writer.WriteStartElement(name);
			if (config.includeTypes)
				writer.WriteAttributeString(ATTRIBUTE, type.FullName);

			FieldInfo[] infos = type.GetFields(defaultFlags);
			for (int i = 0; i < infos.Length; i++)
			{
				FieldInfo field = infos[i];
				object value = field.GetValue(obj)!;
				WriteObject(field.Name, value);
			}
			writer.WriteEndElement();
		}
	}
}

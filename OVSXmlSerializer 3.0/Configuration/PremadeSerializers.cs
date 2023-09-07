namespace OVSXmlSerializer.Internals
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	internal class ListSerializer : IInterfaceSerializer
	{
		public Type TargetedInterface => typeof(IList);
		public void Write<T>(OVSXmlWriter<T> writer, XmlNode parent, StructuredObject @object, string suggestedName)
		{
			OVSXmlWriter<T>.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			IList list = (IList)@object.Value;
			for (int i = 0; i < list.Count; i++)
			{
				StructuredObject currentValue = new StructuredObject(list[i], typeof(object));
				writer.WriteObject(currentValue, enumerableElement, "Item");
			}
		}
	}
	internal class DictionarySerializer : IInterfaceSerializer
	{
		public Type TargetedInterface => typeof(IDictionary);
		public void Write<T>(OVSXmlWriter<T> writer, XmlNode parent, StructuredObject @object, string suggestedName)
		{
			OVSXmlWriter<T>.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			IList list = (IList)@object.Value;
			for (int i = 0; i < list.Count; i++)
			{
				StructuredObject currentValue = new StructuredObject(list[i], typeof(object));
				writer.WriteObject(currentValue, enumerableElement, "Item");
			}
		}
	}
	internal class ArraySerializer : ICustomSerializer
	{
		public bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName)
		{
			if (!@object.ValueType.IsArray)
				return false;
			XmlElement arrayElement = writer.CreateElement(parentNode, suggestedName, @object);
			Array arrValue = (Array)@object.Value;
			Type elementType = @object.ValueType.GetElementType();
			for (int i = 0; i < arrValue.Length; i++)
			{
				StructuredObject currentValue = new StructuredObject(arrValue.GetValue(i), elementType);
				writer.WriteObject(currentValue, arrayElement, "Item");
			}
			return true;
		}
	}
	internal class DatetimeSerializer : ICustomSerializer
	{
		public bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName)
		{
			if (@object.ValueType != typeof(DateTime))
				return false;
			string value = ((DateTime)@object.Value).ToString();
			writer.CreateNode(parentNode, suggestedName, value, @object);
			return true;
		}
	}
}

namespace OVSXmlSerializer
{
	using global::OVSXmlSerializer.Internals;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// Instead of serializing the object's fields, this serializes the object
	/// via interface.
	/// </summary>
	public interface IInterfaceSerializer
	{
		Type TargetedInterface { get; }
		void Write<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName);
	}
	public interface ICustomSerializer
	{
		bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName);
	}
	/// <summary>
	/// A custom-made list that handles parsing various interfaces and objects
	/// together, latest added parsers first.
	/// </summary>
	public class InterfaceSerializer
	{
		public static InterfaceSerializer GetDefault()
		{
			var serializer = new InterfaceSerializer();
			serializer.Add(new ListSerializer());
			serializer.Add(new ArraySerializer());
			serializer.Add(new DictionarySerializer());
			return serializer;
		}


		private List<object> serializerInterfaces;
		public InterfaceSerializer()
		{
			serializerInterfaces = new List<object>();
		}
		public void Add(object obj)
		{
			if (obj is IInterfaceSerializer)
				serializerInterfaces.Add(obj);
			else if (obj is ICustomSerializer) 
				serializerInterfaces.Add(obj);
			else
				throw new InvalidCastException(obj.GetType().FullName);
		}
		public void Add(IInterfaceSerializer serializer) => serializerInterfaces.Add(serializer);
		public void Add(ICustomSerializer serializer) => serializerInterfaces.Add(serializer);

		internal bool Write<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject structuredObject, string name)
		{
			for (int i = serializerInterfaces.Count; i > 0; i--)
			{
				if (serializerInterfaces[i] is IInterfaceSerializer interfacer)
				{
					if (!interfacer.TargetedInterface.IsAssignableFrom(structuredObject.ValueType))
						continue;
					interfacer.Write<T>(writer, parentNode, structuredObject, name);
					return true;
				}
				if (serializerInterfaces[i] is ICustomSerializer customSerializer)
				{
					if (customSerializer.CheckAndWrite<T>(writer, parentNode, structuredObject, name))
						return true;
				}
			}
			return false;
		}
	}
}

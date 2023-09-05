namespace OVSXmlSerializer
{
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
		void Write<T>(OVSXmlWriter<T> writer, XmlNode targetNode);
	}
	/// <summary>
	/// A custom-made list that handles parsing various interfaces and objects
	/// together, latest added parsers first.
	/// </summary>
	public class InterfaceSerializer
	{
		public static bool ImplementsInterface(IInterfaceSerializer serializer, Type member)
		{
			if (!serializer.TargetedInterface.IsInterface)
				throw new InvalidOperationException($"{serializer.TargetedInterface} is not an interface!");
			return serializer.TargetedInterface.IsAssignableFrom(member);
		}
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
			else
				throw new InvalidCastException(obj.GetType().FullName);
		}
		public void Add(IInterfaceSerializer serializer) => serializerInterfaces.Add(serializer);

		internal void Write<T>(OVSXmlWriter<T> writer, XmlNode node)
		{
			for (int i = serializerInterfaces.Count; i > 0; i--)
			{
				if (serializerInterfaces[i] is IInterfaceSerializer interfacer)
					interfacer.Write<T>(writer, node);
			}
		}
	}
}

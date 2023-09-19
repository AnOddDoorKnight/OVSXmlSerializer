namespace OVSXmlSerializer
{
	using global::OVSXmlSerializer.Internals;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using System.Xml;

	public interface ICustomSerializer
	{
		bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output);
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Note when you create a reference type, please add them in the writer dictionary.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="reader"></param>
		/// <param name="type"></param>
		/// <param name="node"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		bool CheckAndRead<T>(OVSXmlReader<T> reader, Type type, XmlNode node, out object output);
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
			serializer.Add(new ListInterfaceSerializer());
			serializer.Add(new ArraySerializer());
			serializer.Add(new DictionarySerializer());
			serializer.Add(new DatetimeSerializer());
			serializer.Add(new TimespanSerializer());
			return serializer;
		}


		private LinkedList<ICustomSerializer> serializerInterfaces;
		public InterfaceSerializer()
		{
			serializerInterfaces = new LinkedList<ICustomSerializer>();
		}
		public void Add(ICustomSerializer serializer) => serializerInterfaces.AddLast(serializer);

		internal bool Write<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject structuredObject, string name, out XmlNode output)
		{
			for (var node = serializerInterfaces.Last; node != null; node = node.Previous)
			{
				if (node.Value.CheckAndWrite<T>(writer, parentNode, structuredObject, name, out output))
					return true;
			}
			output = null;
			return false;
		}
		internal bool Read<T>(OVSXmlReader<T> reader, in Type type, in XmlNode node, out object output)
		{
			for (var listNode = serializerInterfaces.Last; listNode != null; listNode = listNode.Previous)
			{
				if (listNode.Value.CheckAndRead<T>(reader, type, node, out output))
					return true;
			}
			output = null;
			return false;
		}
	}
}

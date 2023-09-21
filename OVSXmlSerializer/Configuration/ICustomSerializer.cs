namespace OVSSerializer
{
	using global::OVSSerializer.Internals;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// A customizable serializer, 
	/// </summary>
	public interface ICustomSerializer
	{
		/// <summary>
		/// Checks if it can first parse the item, then attempts to write it into
		/// the document given the parameters.
		/// </summary>
		/// <typeparam name="T">The root object type, can sometimes be just <see cref="object"/> and is made to fill the credentials of <see cref="OVSXmlWriter{T}"/>.</typeparam>
		/// <param name="writer">The source writer. Mostly used to continue parsing as normal for things like enumerables.</param>
		/// <param name="parentNode">The parent of the current node.</param>
		/// <param name="object">The object to serialize. May sometimes be a <see cref="FieldObject"/>.</param>
		/// <param name="suggestedName">The name to write as the element or attribute.</param>
		/// <param name="output">The given XmlNode. Note that you still need to write to the <paramref name="parentNode"/>, this is for niche internals.</param>
		/// <returns>If it can actually parse the item.</returns>
		bool CheckAndWrite<T>(OVSXmlWriter<T> writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output);
		/// <summary>
		/// Checks if it can first parse the item, then attempts to read it
		/// given the parameters.
		/// </summary>
		/// <remarks>
		/// Note when you create a reference type, please add them in the writer dictionary.
		/// </remarks>
		/// <typeparam name="T">The root object type, can sometimes be just <see cref="object"/> and is made to fill the credentials of <see cref="OVSXmlReader{T}"/>.</typeparam>
		/// <param name="reader">The source reader. Mostly used to continue parsing as normal for things like enumerables.</param>
		/// <param name="type">The suspected type of the object.</param>
		/// <param name="node">The node where which it came from.</param>
		/// <param name="output">The read node.</param>
		/// <returns>If it can actually parse the item.</returns>
		bool CheckAndRead<T>(OVSXmlReader<T> reader, Type type, XmlNode node, out object output);
	}
	/// <summary>
	/// A custom-made list that handles parsing various interfaces and objects
	/// together, latest added parsers first.
	/// </summary>
	public class InterfaceSerializer
	{
		/// <summary>
		/// Creates a new default version with the recommended serializers in order:
		/// <list type="number">
		/// <item><see cref="ListInterfaceSerializer"/></item>
		/// <item><see cref="ArraySerializer"/></item>
		/// <item><see cref="DictionarySerializer"/></item>
		/// <item><see cref="DatetimeSerializer"/></item>
		/// <item><see cref="TimeSpanSerializer"/></item>
		/// </list>
		/// </summary>
		public static InterfaceSerializer GetDefault()
		{
			var serializer = new InterfaceSerializer();
			serializer.Add(new ListInterfaceSerializer());
			serializer.Add(new ArraySerializer());
			serializer.Add(new DictionarySerializer());
			serializer.Add(new DatetimeSerializer());
			serializer.Add(new TimeSpanSerializer());
			return serializer;
		}


		private LinkedList<ICustomSerializer> serializerInterfaces;
		/// <summary>
		/// Initializes a new empty instance of a list of serializers.
		/// </summary>
		public InterfaceSerializer()
		{
			serializerInterfaces = new LinkedList<ICustomSerializer>();
		}
		/// <summary>
		/// Adds a new custom serializer to parse in a custom way.
		/// </summary>
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

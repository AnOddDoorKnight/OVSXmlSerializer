namespace OVS.XmlSerialization.Prefabs
{
	using OVS.XmlSerialization.Internals;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// A custom-made list that handles parsing various interfaces and objects
	/// together, latest added parsers first.
	/// </summary>
	public class InterfaceSerializer : List<ICustomSerializer>
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
			var serializer = new InterfaceSerializer()
			{
				new ListInterfaceSerializer(),
				new ArraySerializer(),
				new DictionarySerializer(),
				new LinkedListSerializer(),
				new DatetimeSerializer(),
				new TimeSpanSerializer(),
				//new DelegateSerializer(),
				new PrimitiveSerializer(),
			};
			//serializer.Add(new SystemEnumerableSerializer());
			return serializer;
		}

		internal bool Write(OVSXmlWriter writer, XmlNode parentNode, StructuredObject structuredObject, string name, out XmlNode output)
		{
			//for (int i = 0; i < Count; i++)
			
			for (int i = Count - 1; i > 0; i--)
			{
				if (this[i].CheckAndWrite(writer, parentNode, structuredObject, name, out output))
					return true;
			}
			output = null;
			return false;
		}
		internal bool Read(OVSXmlReader reader, in Type type, in XmlNode node, out object output)
		{
			for (int i = Count - 1; i > 0; i--)
			{
				if (this[i].CheckAndRead(reader, type, node, out output))
					return true;
			}
			output = null;
			return false;
		}
	}
}

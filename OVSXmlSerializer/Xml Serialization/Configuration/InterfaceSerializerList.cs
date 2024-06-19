namespace OVS.XmlSerialization.Prefabs
{
	using OVS.XmlSerialization.Internals;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

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
			serializer.Add(new LinkedListSerializer());
			serializer.Add(new DatetimeSerializer());
			serializer.Add(new TimeSpanSerializer());
			return serializer;
		}


		private readonly List<KeyValuePair<byte, object>> serializerInterfaces;
		/// <summary>
		/// Initializes a new empty instance of a list of serializers.
		/// </summary>
		public InterfaceSerializer()
		{
			serializerInterfaces = new List<KeyValuePair<byte, object>>();
		}
		/// <summary>
		/// Adds a new custom serializer to parse in a custom way.
		/// </summary>
		public void Add(ICustomSerializer serializer) => serializerInterfaces.Add(new KeyValuePair<byte, object>(0, serializer));

		internal bool Write(OVSXmlWriter writer, XmlNode parentNode, StructuredObject structuredObject, string name, out XmlNode output)
		{
			for (int i = serializerInterfaces.Count - 1; i > 0; i--)
			{
				switch (serializerInterfaces[i].Key)
				{
					case 0:
						ICustomSerializer custom = (ICustomSerializer)serializerInterfaces[i].Value;
						if (custom.CheckAndWrite(writer, parentNode, structuredObject, name, out output))
							return true;
						break;

				}
			}
			output = null;
			return false;
		}
		internal bool Read(OVSXmlReader reader, in Type type, in XmlNode node, out object output)
		{
			for (int i = serializerInterfaces.Count - 1; i > 0; i--)
			{
				switch (serializerInterfaces[i].Key)
				{
					case 0:
						ICustomSerializer custom = (ICustomSerializer)serializerInterfaces[i].Value;
						if (custom.CheckAndRead(reader, type, node, out output))
							return true;
						break;
				}
			}
			output = null;
			return false;
		}
	}
}

namespace OVS.XmlSerialization.Prefabs
{
	using OVS.XmlSerialization.Internals;
	using OVS.XmlSerialization.Utility;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// Serializes <see cref="IDictionary"/>, can take generic types if it is <see cref="Dictionary{TKey, TValue}"/>
	/// </summary>
	internal class DictionarySerializer : ICustomSerializer
	{
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (typeof(IDictionary).IsAssignableFrom(type) == false)
			{
				output = null;
				return false;
			}
			XmlNodeList nodeList = node.ChildNodes;
			KeyValuePair<Type, Type> types = GetBaseTypes(type);
			IDictionary dictionary = (IDictionary)reader.CreateNewObject(type, node, out bool dontOverride);
			if (dontOverride)
			{
				output = dictionary;
				return true;
			}
			for (int i = 0; i < nodeList.Count; i++)
			{
				XmlNode child = nodeList.Item(i);
				object key = reader.ReadObject(child.GetNode("key"), types.Key);
				object value = reader.ReadObject(child.GetNode("value"), types.Value);
				dictionary.Add(key, value);
			}
			output = dictionary;
			return true;
		}
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!(@object.Value is IDictionary dictionary))
			{
				output = null;
				return false;
			}
			//OVSXmlWriter.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			// creating key element attribute
			KeyValuePair<Type, Type> types = GetBaseTypes(@object.ValueType);
			IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				object key = enumerator.Key;
				object value = enumerator.Value;
				XmlElement pair = writer.CreateElement(enumerableElement, "item");
				writer.WriteObject(new StructuredObject(key, types.Key), pair, "key");
				writer.WriteObject(new StructuredObject(value, types.Value), pair, "value");
			}
			output = enumerableElement;
			return true;
		}
		private KeyValuePair<Type, Type> GetBaseTypes(Type assigningType)
		{
			if (assigningType.Namespace == typeof(Dictionary<object, object>).Namespace)
			{
				Type[] types = assigningType.GetGenericArguments();
				return new KeyValuePair<Type, Type>(types[0], types[1]);
			}
			return new KeyValuePair<Type, Type>(typeof(object), typeof(object));
		}
	}
	/*
	/// <summary>
	/// uses a hashcode to serialize specified types.
	/// </summary>
	public interface ITypeSerializer
	{
		Type Type { get; }
		/// <summary>
		/// Checks if it can first parse the item, then attempts to write it into
		/// the document given the parameters.
		/// </summary>
		/// <param name="writer">The source writer. Mostly used to continue parsing as normal for things like enumerables.</param>
		/// <param name="parentNode">The parent of the current node.</param>
		/// <param name="object">The object to serialize. May sometimes be a <see cref="FieldObject"/>.</param>
		/// <param name="suggestedName">The name to write as the element or attribute.</param>
		/// <param name="output">The given XmlNode. Note that you still need to write to the <paramref name="parentNode"/>, this is for niche internals.</param>
		/// <returns>If it can actually parse the item.</returns>
		void Write(OVSXmlWriter writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output);
		/// <summary>
		/// Checks if it can first parse the item, then attempts to read it
		/// given the parameters.
		/// </summary>
		/// <remarks>
		/// Note when you create a reference type, please add them in the writer dictionary.
		/// </remarks>
		/// <param name="reader">The source reader. Mostly used to continue parsing as normal for things like enumerables.</param>
		/// <param name="type">The suspected type of the object.</param>
		/// <param name="node">The node where which it came from.</param>
		/// <param name="output">The read node.</param>
		/// <returns>If it can actually parse the item.</returns>
		bool Read(OVSXmlReader reader, Type type, XmlNode node, out object output);
	}
	*/
}

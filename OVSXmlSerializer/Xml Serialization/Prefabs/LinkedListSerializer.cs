namespace OVS.XmlSerialization.Prefabs
{
	using global::OVS.XmlSerialization.Utility;
	using global::OVS.XmlSerialization.Internals;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using System.Data;

	/// <summary>
	/// Serializer that serializes linked lists.
	/// </summary>
	public class LinkedListSerializer : ICustomSerializer
	{
		/// <inheritdoc/>
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (!CanSerialize(@object.ValueType))
			{
				output = null;
				return false;
			}
			//OVSXmlWriter.EnsureParameterlessConstructor(@object.ValueType);
			XmlElement enumerableElement = writer.CreateElement(parent, suggestedName, @object);
			IEnumerator enumerator = ((IEnumerable)@object.Value).GetEnumerator();
			while (enumerator.MoveNext())
			{
				StructuredObject currentValue = new StructuredObject(enumerator.Current, typeof(object));
				writer.WriteObject(currentValue, enumerableElement, "LinkedItem");
			}
			output = enumerableElement;
			return true;
		}
		/// <inheritdoc/>
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (!CanSerialize(type))
			{
				output = null;
				return false;
			}
			List<XmlNode> xmlNodes = node.ChildNodes.ToList();
			object linkedList = reader.CreateNewObject(type, node, out bool dontOverride);
			if (dontOverride)
			{
				output = linkedList;
				return true;
			}
			MethodInfo addItem = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(meth => meth.Name == "AddLast")
				.First(meth => !meth.GetParameters()[0].ParameterType.Name.StartsWith("LinkedListNode"));
			for (int i = 0; i < xmlNodes.Count; i++)
			{
				object input = reader.ReadObject(xmlNodes[i], typeof(object));
				addItem.Invoke(linkedList, new object[] { input });
			}
			output = linkedList;
			return true;
		}
		private bool CanSerialize(Type inType)
		{
			if (inType.Namespace != inType.Namespace)
				return false;
			if (!inType.Name.StartsWith(nameof(LinkedList<object>)))
				return false;
			return true;
		}
	}
}

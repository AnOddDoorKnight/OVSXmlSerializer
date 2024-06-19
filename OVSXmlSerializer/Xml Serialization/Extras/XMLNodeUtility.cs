namespace OVS.XmlSerialization.Utility
{
	using global::OVS.XmlSerialization.Internals;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// Additional utilities for handling <see cref="XmlNode"/>s.
	/// </summary>
	public static class XMLNodeUtility
	{
		/// <summary>
		/// Converts all node children to a generic .NET list.
		/// </summary>
		public static List<XmlNode> ToList(this XmlNodeList list)
		{
			List<XmlNode> result = new List<XmlNode>(list.Count);
			for (int i = 0; i < list.Count; i++)
				result.Add(list[i]);
			return result;
		}

		/// <summary>
		/// Finds the index from the first success of predicate.
		/// </summary>
		public static int FindIndex(this XmlNodeList childNodes, Predicate<XmlNode> match)
		{
			for (int i = 0; i < childNodes.Count; i++)
				if (match.Invoke(childNodes[i]))
					return i;
			return -1;
		}
		/// <summary>
		/// Finds the node from the first success of predicate.
		/// </summary>
		public static XmlNode Find(this XmlNodeList childNodes, Predicate<XmlNode> match)
		{
			return childNodes[childNodes.FindIndex(match)];
		}

		/// <summary>
		/// Finds the node from the first matching name.
		/// </summary>
		public static XmlNode FindNamedNode(this XmlNodeList childNodes, string name)
		{
			return childNodes[FindNamedNodeIndex(childNodes, name)];
		}
		/// <summary>
		/// Finds the index from the first matching name.
		/// </summary>
		public static int FindNamedNodeIndex(this XmlNodeList childNodes, string name)
		{
			return FindIndex(childNodes, node => name == node.Name);
		}

		/// <summary>
		/// Gets the node from <paramref name="name"/>, reading both attributes and
		/// children.
		/// </summary>
		/// <exception cref="IndexOutOfRangeException"/>
		public static XmlNode GetNode(this XmlNode node, string name)
		{
			int index = FindNamedNodeIndex(node.ChildNodes, name);
			if (index != -1)
				return node.ChildNodes[index];
			if (node.Attributes != null)
			{
				XmlNode output = node.Attributes.GetNamedItem(name);
				if (output != null)
					return output;
			}
			throw new IndexOutOfRangeException(name);
		}

		/// <summary>
		/// Gets all children, compiling the attributes and child nodes excluding
		/// <see cref="OVSXmlSerializer"/>'s custom attributes.
		/// </summary>
		public static List<XmlNode> GetAllChildren(this XmlNode node)
		{
			List<XmlNode> children = new List<XmlNode>();
			if (!(node.ChildNodes is null))
				for (int i = 0; i < node.ChildNodes.Count; i++)
					children.Add(node.ChildNodes[i]);
			if (!(node.Attributes is null))
				for (int i = 0; i < node.Attributes.Count; i++)
				{
					if (node.Attributes[i].Name != OVSXmlSerializer.ATTRIBUTE 
						&& node.Attributes[i].Name != OVSXmlReferencer.REFERENCE_ATTRIBUTE)
						children.Add(node.Attributes[i]);
				}
			return children;
		}

		/// <summary>
		/// Reads the value of the node, ignoring the type of the node.
		/// </summary>
		public static string ReadValue(this XmlNode node)
		{
			if (node is XmlElement element)
				return element.InnerText;
			else if (node is XmlAttribute attribute)
				return attribute.Value;
			throw new InvalidCastException();
		}
	}
}

namespace OVSXmlSerializer.Extras
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	public static class XMLNodeUtility
	{
		public static int FindIndex(this XmlNodeList childNodes, Predicate<XmlNode> match)
		{
			for (int i = 0; i < childNodes.Count; i++)
				if (match.Invoke(childNodes[i]))
					return i;
			return -1;
		}
		public static XmlNode Find(this XmlNodeList childNodes, Predicate<XmlNode> match)
		{
			return childNodes[childNodes.FindIndex(match)];
		}


		public static XmlNode FindNamedNode(this XmlNodeList childNodes, string name)
		{
			return childNodes[FindNamedNodeIndex(childNodes, name)];
		}
		public static int FindNamedNodeIndex(this XmlNodeList childNodes, string name)
		{
			return FindIndex(childNodes, node => name == node.Name);
		}

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

		public static List<XmlNode> GetAllChildren(this XmlNode node)
		{
			List<XmlNode> children = new List<XmlNode>();
			if (!(node.ChildNodes is null))
				for (int i = 0; i < node.ChildNodes.Count; i++)
					children.Add(node.ChildNodes[i]);
			if (!(node.Attributes is null))
				for (int i = 0; i < node.Attributes.Count; i++)
					children.Add(node.Attributes[i]);
			return children;
		}
	}
}

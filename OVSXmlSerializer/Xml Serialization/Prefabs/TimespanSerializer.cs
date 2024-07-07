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
	/// Serializes <see cref="TimeSpan"/> with its own parsing system.
	/// </summary>
	internal class TimeSpanSerializer : ICustomSerializer
	{
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			if (@object.ValueType != typeof(TimeSpan))
			{
				output = null;
				return false;
			}
			string value = ((TimeSpan)@object.Value).ToString();
			output = writer.CreateNode(parentNode, suggestedName, value, @object);
			return true;
		}
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			if (type != typeof(TimeSpan))
			{
				output = null;
				return false;
			}
			output = TimeSpan.Parse(node.ReadValue());
			return true;
		}
	}
}

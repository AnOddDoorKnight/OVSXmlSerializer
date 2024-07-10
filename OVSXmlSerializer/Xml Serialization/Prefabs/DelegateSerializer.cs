namespace OVS.XmlSerialization.Prefabs
{
	using OVS.XmlSerialization.Internals;
	using OVS.XmlSerialization.Utility;
	using System;
	using System.Data;
	using System.Xml;
	using System.Xml.Linq;

	/// <summary>
	/// Serializes delegates. No code for now, so it just throws a <see cref="NotImplementedException"/>
	/// </summary>
	public class DelegateSerializer : ICustomSerializer
	{
		/// <inheritdoc/>
		public bool CheckAndRead(OVSXmlReader reader, Type delType, XmlNode node, out object output)
		{
			if (!typeof(Delegate).IsAssignableFrom(delType))
			{
				output = null;
				return false;
			}
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parentNode, StructuredObject obj, string name, out XmlNode output)
		{
			if (!(obj.Value is Delegate del))
			{
				output = null;
				return false;
			}
			throw new NotImplementedException();
		}
	}
}

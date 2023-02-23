namespace OVSXmlSerializer
{
	using System;
	using System.Xml;

	[Obsolete("Not Implemented")]
	public interface IXmlSerializable
	{
		void Read(XmlReader reader);
		void Write(XmlWriter writer);
	}
}
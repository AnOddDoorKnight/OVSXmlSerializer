namespace OVSXmlSerializer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	internal class ListSerializer : IInterfaceSerializer
	{
		public Type TargetedInterface => typeof(IList);
		public void Write(XmlDocument document, XmlNode targetNode)
		{
			
		}
		public object Read(XmlNode targetNode)
		{

		}
	}
	internal class DictionarySerializer : IInterfaceSerializer
	{
		public Type TargetedInterface => typeof(IDictionary);
		public void Write(XmlDocument document, XmlNode targetNode)
		{
			throw new NotImplementedException();
		}
	}
	internal class ArraySerializer : IInterfaceSerializer
	{

	}
}

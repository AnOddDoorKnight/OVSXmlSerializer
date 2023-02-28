namespace OVSXmlSerializer.Configuration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using static XmlSerializer;

	public abstract class EnumerableModify : OverrideObject
	{
		public string entryName = "Item";
	}
	public class OverrideArray : EnumerableModify
	{
		public OverrideArray()
		{

		}
	}
	public class OverrideList : EnumerableModify
	{
		public OverrideList()
		{

		}
	}
	public class OverrideDictionary : EnumerableModify
	{
		//public List<OVSAttribute> KeyAttributes;
		//public List<OVSAttribute> ValueAttributes;

		public OverrideDictionary(IDictionary dictionary)
		{
			//KeyAttributes = new List<OVSAttribute>();
			//var enumerator = dictionary.GetEnumerator();
			//while (enumerator.MoveNext())
			//	KeyAttributes.Add()
		}
	}
}

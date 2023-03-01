namespace OVSXmlSerializer.Configuration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using static XmlSerializer;

	public class EnumerableTarget : OverrideTarget
	{
		public string entryName = "Item";
	}
	public class OverrideArray : EnumerableTarget
	{
		public OverrideArray()
		{

		}
	}
	public class OverrideList : EnumerableTarget
	{
		public OverrideList()
		{

		}
	}
	public class OverrideDictionary : EnumerableTarget
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

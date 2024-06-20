namespace OVS.XmlSerialization
{
	using OVS.XmlSerialization.Internals;
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// Specialized class or struct that allows custom constructors to be made
	/// instead of creating and adding fields directly or using blanks.
	/// </summary>
	public struct OVSXmlReaderInput
	{
		public OVSXmlReader Source { get; set; }
	}
}

namespace OVS.XmlSerialization
{
	using OVS.XmlSerialization.Internals;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading;
	using System.Xml;

	/// <summary>
	/// Specialized class or struct that allows custom constructors to be made
	/// instead of creating and adding fields directly or using blanks.
	/// <para>
	/// In consideration of the new special constructor and the interface, when being
	/// constructed a new object, the constructor will run first, then the interface.
	/// However, if there is no interface, then it will listen and stop from there.
	/// </para>
	/// </summary>
	public struct OVSXmlReaderInput
	{
		/// <summary>
		/// The source reader.
		/// </summary>
		public OVSXmlReader Source { get; set; }
		/// <summary>
		/// Special override when cancelled, will not allow the reader to override
		/// the values, relying more on the constructor. This also blocks special
		/// interface overrides.
		/// </summary>
		internal Action CancelReaderOverrides { get; set; }
		/// <summary>
		/// Special override when cancelled, will not allow the reader to override
		/// the values, relying more on the constructor. This also blocks special
		/// interface overrides.
		/// </summary>
		public void CancelOverrides() => CancelReaderOverrides.Invoke();
		/// <summary>
		/// The source xml node.
		/// </summary>
		public XmlNode SourceNode { get; set; }
	}
}

namespace OVSXmlSerializer
{
	using System;
	using System.Xml;

	public interface IXmlSerializable
	{
		/// <summary>
		/// If the value should be serialized at all. Roughly similar to 
		/// <see cref="System.Xml.Serialization.XmlIgnoreAttribute"/>
		/// </summary>
		bool ShouldWrite { get; }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"> Nullable. </param>
		void Read(XmlNode value);
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// The element is already generated for you, so no need to start or end an element.
		/// </remarks>
		/// <param name="writer"></param>
		void Write(XmlWriter writer);
	}
}
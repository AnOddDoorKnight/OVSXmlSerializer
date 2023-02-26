namespace OVSXmlSerializer
{
	using System;
	using System.Xml;

	/// <summary>
	/// Instead of writing automatically, this allows you to have more control
	/// over saving the object.
	/// </summary>
	public interface IXmlSerializable
	{
		/// <summary>
		/// If the value should be serialized at all. Roughly similar to 
		/// <see cref="System.Xml.Serialization.XmlIgnoreAttribute"/>
		/// </summary>
		bool ShouldWrite { get; }
		/// <summary>
		/// Reads the data of the existing node.
		/// </summary>
		/// <param name="value"> Nullable. </param>
		void Read(XmlNode value);
		/// <summary>
		/// Writes the content of class or struct.
		/// </summary>
		/// <remarks>
		/// The element is already generated for you, so no need to start or end an element.
		/// </remarks>
		/// <param name="writer"></param>
		void Write(XmlWriter writer);
	}
}
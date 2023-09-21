namespace OVSSerializer
{
	using global::OVSSerializer.Internals;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;


	/// <summary>
	/// Instead of writing automatically, this allows you to have more control
	/// over saving the object.
	/// </summary>
	/// <remarks>
	/// Something like <see cref="ICustomSerializer"/> as their own class provides
	/// more control and allows usage of writers and readers to serialize.
	/// </remarks>
	public interface IOVSXmlSerializable
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
		/// Writes the content of class or struct. Use <see cref="XmlNode.OwnerDocument"/>
		/// in order to create attributes or elements.
		/// </summary>
		/// <remarks>
		/// The element is already generated for you, so no need to start or end an element.
		/// </remarks>
		/// <param name="currentNode">A non-parent node to store the custom data.</param>
		void Write(XmlNode currentNode);
	}
}

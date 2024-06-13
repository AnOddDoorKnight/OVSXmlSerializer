namespace OVSSerializer
{
	using OVSSerializer.Internals;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	/*
	/// <summary>
	/// uses a hashcode to serialize specified types.
	/// </summary>
	public interface ITypeSerializer
	{
		Type Type { get; }
		/// <summary>
		/// Checks if it can first parse the item, then attempts to write it into
		/// the document given the parameters.
		/// </summary>
		/// <param name="writer">The source writer. Mostly used to continue parsing as normal for things like enumerables.</param>
		/// <param name="parentNode">The parent of the current node.</param>
		/// <param name="object">The object to serialize. May sometimes be a <see cref="FieldObject"/>.</param>
		/// <param name="suggestedName">The name to write as the element or attribute.</param>
		/// <param name="output">The given XmlNode. Note that you still need to write to the <paramref name="parentNode"/>, this is for niche internals.</param>
		/// <returns>If it can actually parse the item.</returns>
		void Write(OVSXmlWriter writer, XmlNode parentNode, StructuredObject @object, string suggestedName, out XmlNode output);
		/// <summary>
		/// Checks if it can first parse the item, then attempts to read it
		/// given the parameters.
		/// </summary>
		/// <remarks>
		/// Note when you create a reference type, please add them in the writer dictionary.
		/// </remarks>
		/// <param name="reader">The source reader. Mostly used to continue parsing as normal for things like enumerables.</param>
		/// <param name="type">The suspected type of the object.</param>
		/// <param name="node">The node where which it came from.</param>
		/// <param name="output">The read node.</param>
		/// <returns>If it can actually parse the item.</returns>
		bool Read(OVSXmlReader reader, Type type, XmlNode node, out object output);
	}
	*/
}

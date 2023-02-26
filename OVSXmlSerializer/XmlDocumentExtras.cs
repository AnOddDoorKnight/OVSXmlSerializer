﻿namespace OVSXmlSerializer.Extras
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	public static class XmlDocumentExtras
	{
		#region Load a new file
		/// <summary>
		/// Creates a new <see cref="XmlDocument"/> along with loading a xml file.
		/// </summary>
		public static XmlDocument LoadNew(Stream stream)
		{
			var output = new XmlDocument();
			output.Load(stream);
			return output;
		}
		/// <summary>
		/// Creates a new <see cref="XmlDocument"/> along with loading a xml file.
		/// </summary>
		public static XmlDocument LoadNew(FileInfo file) => LoadNew(file.FullName);
		/// <summary>
		/// Creates a new <see cref="XmlDocument"/> along with loading a xml file.
		/// </summary>
		public static XmlDocument LoadNew(string fileName)
		{
			var output = new XmlDocument();
			output.Load(fileName);
			return output;
		}
		/// <summary>
		/// Creates a new <see cref="XmlDocument"/> along with loading a xml file.
		/// </summary>
		public static XmlDocument LoadNew(XmlReader reader)
		{
			var output = new XmlDocument();
			output.Load(reader);
			return output;
		}
		/// <summary>
		/// Creates a new <see cref="XmlDocument"/> along with loading a xml file.
		/// </summary>
		public static XmlDocument LoadNew(TextReader reader)
		{
			var output = new XmlDocument();
			output.Load(reader);
			return output;
		}
		#endregion

		#region Load an existing file
		/// <summary>
		/// Loads an existing <see cref="XmlDocument"/> and allows it to be
		/// chainable.
		/// </summary>
		/// <param name="document"> The document to write to. </param>
		public static XmlDocument Load(this XmlDocument document, Stream stream)
		{
			document.Load(stream);
			return document;
		}
		/// <summary>
		/// Loads an existing <see cref="XmlDocument"/> and allows it to be
		/// chainable.
		/// </summary>
		/// <param name="document"> The document to write to. </param>
		public static XmlDocument Load(this XmlDocument document, FileInfo file) => Load(document, file.FullName);
		/// <summary>
		/// Loads an existing <see cref="XmlDocument"/> and allows it to be
		/// chainable.
		/// </summary>
		/// <param name="document"> The document to write to. </param>
		public static XmlDocument Load(this XmlDocument document, string fileName)
		{
			document.Load(fileName);
			return document;
		}
		/// <summary>
		/// Loads an existing <see cref="XmlDocument"/> and allows it to be
		/// chainable.
		/// </summary>
		/// <param name="document"> The document to write to. </param>
		public static XmlDocument Load(XmlDocument document, XmlReader reader)
		{
			document.Load(reader);
			return document;
		}
		/// <summary>
		/// Loads an existing <see cref="XmlDocument"/> and allows it to be
		/// chainable.
		/// </summary>
		/// <param name="document"> The document to write to. </param>
		public static XmlDocument Load(XmlDocument document, TextReader reader)
		{
			document.Load(reader);
			return document;
		}
		#endregion
	}
}

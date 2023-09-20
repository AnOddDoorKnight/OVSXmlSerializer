﻿namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Security;
	using System.Text;
	using System.Xml;
	using System.Xml.Serialization;

	/// <summary>
	/// The configuration for <see cref="OVSXmlSerializer"/>s.
	/// </summary>
	public sealed class OVSConfig
	{
		/// <summary>
		/// Gets or sets the culture, which will handle primitive printing. Set
		/// as <see cref="CultureInfo.InvariantCulture"/> for cross-compatibility
		/// between computers, but can be set to other cultures if needed.
		/// </summary>
		public CultureInfo CurrentCulture { get; set; } = CultureInfo.InvariantCulture;
		/// <summary>
		/// Whenever a reference type is made, and multiple things use the same
		/// instance of the reference type, then it will simply use an ID system
		/// to refer to said instance. Disabling this removes this feature. Disabled
		/// by default.
		/// </summary>
		public bool UseSingleInstanceInsteadOfMultiple { get; set; } = true;
		/// <summary>
		/// The current version of the XML file. Null if you don't want any
		/// attributes assigned to the root element
		/// </summary>
		public Version Version { get; set; } = new Version(1, 0);
		/// <summary>
		/// If the object is set to <see langword="null"/> on the reader, or missing entirely,
		/// it will instead allow the default to be set instead on enabled. Where
		/// leaving it disabled to set it to <see langword="null"/> instead.
		/// </summary>
		public bool IgnoreUndefinedValues { get; set; } = false;
		/// <summary>
		/// A list that overrides the traditional field system, sometimes for
		/// readability. Uses <see cref="InterfaceSerializer.GetDefault"/> as default.
		/// </summary>
		public InterfaceSerializer CustomSerializers { get; set; } = InterfaceSerializer.GetDefault();
		/// <summary>
		/// Disables the use of the comment about being auto-generated by
		/// OVSXmlSerializer. Enabled by default.
		/// </summary>
		public bool OmitAutoGenerationComment { get; set; } = false;
		/// <summary>
		/// When reading a file, it will check the version. This setting will determine
		/// the leniancy that it has to allow parsing it.
		/// </summary>
		public Versioning.Leniency VersionLeniency { get; set; } = Versioning.Leniency.Strict;
		/// <summary>
		/// Whenever it should add a new line when declaring attributes.
		/// </summary>
		public bool NewLineOnAttributes { get; set; } = false;
		/// <summary>
		/// Gets or sets a value indicating whether to omit an XML declaration.
		/// The declaration mentions the beginning element, that typically mentions
		/// the encoding type and the version of the XML.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> to omit the XML declaration; otherwise, <see langword="false"/>. 
		/// Default is <see langword="true"/>, as it is autogenerated and most likely
		/// be handled by the <see cref="OVSXmlSerializer{T}"/> again.
		/// </returns>
		public bool OmitXmlDelcaration { get; set; } = true;
		/// <summary>
		/// The single indentation that should occur. This will be repeated as 
		/// layers are added further.
		/// </summary>
		public string IndentChars { get; set; } = "\t";
		/// <summary>
		/// When writing onto a file, if it should have indentation such as '\n'.
		/// </summary>
		public bool Indent { get; set; } = true;
		/// <summary>
		/// When writing the XML Declaration, there is a value that tells if the
		/// XML document is standalone, which is 'no' or <see langword="false"/>
		/// by most documents. In order to be <see langword="true"/>, or 'yes', 
		/// it has to pass the following conditions:
		/// <list type="bullet">
		/// <item>No default attribute values are specified for elements</item>
		/// <item>No entity references used in the instance document are defined</item>
		/// <item>No attribute values need to be normalized</item>
		/// <item>No elements contain ignorable white space.</item>
		/// </list>
		/// There is more information about this here: http://www.cafeconleche.org/books/effectivexml/chapters/01.html,
		/// but feel free to leave this as <see langword="null"/>, as this is the
		/// default setting for <see cref="XmlDocument"/> or <see cref="XmlWriter"/>
		/// and serialization.
		/// </summary>
		public bool? StandaloneDeclaration { get; set; } = null;
		/// <summary>
		/// how the XML file from <see cref="XmlSerializer"/> should handle types. 
		/// </summary>
		public IncludeTypes TypeHandling { get; set; } = IncludeTypes.SmartTypes;
		/// <summary>
		/// How the writer should handle readonly field. <see cref="ReadonlyFieldHandle.Continue"/>
		/// as default. This can be manipulated when concerning coding in restricted 
		/// environment.when it throws a <see cref="VerificationException"/>
		/// </summary>
		public ReadonlyFieldHandle HandleReadonlyFields { get; set; } = ReadonlyFieldHandle.Continue;
		/// <summary>
		/// The encoding of the result of the file.
		/// </summary>
		public Encoding Encoding { get; set; } = Encoding.UTF8;
		/// <summary>
		/// Converts all data from the config to the relevant writer settings.
		/// </summary>
		public XmlWriterSettings AsWriterSettings()
		{
			return new XmlWriterSettings()
			{
				Indent = Indent,
				IndentChars = IndentChars,
				Encoding = Encoding,
				OmitXmlDeclaration = OmitXmlDelcaration,
				NewLineOnAttributes = NewLineOnAttributes,
			};
		}
	}

	/// <summary>
	/// The condition of how the <see cref="XmlSerializer"/> should handle types
	/// of objects.
	/// </summary>
	public enum IncludeTypes : byte
	{
		/// <summary>
		/// It will ignore the derived type entirely.
		/// </summary>
		IgnoreTypes = 0,
		/// <summary>
		/// When the object is derived off the field, then it will write the
		/// object type.
		/// </summary>
		SmartTypes = 8,
		/// <summary>
		/// The XML file will always write the type of the object. 
		/// </summary>
		AlwaysIncludeTypes = 16,
	}
	/// <summary>
	/// How it should handle readonly fields.
	/// </summary>
	public enum ReadonlyFieldHandle : byte
	{
		/// <summary>
		/// Ignores the readonly field entirely.
		/// </summary>
		Ignore = 0,
		/// <summary>
		/// Allows the parser to continue serializing the field.
		/// </summary>
		Continue = 8,
		/// <summary>
		/// Throws an error if a readonly field is encountered.
		/// </summary>
		ThrowError = 16,
	}
}

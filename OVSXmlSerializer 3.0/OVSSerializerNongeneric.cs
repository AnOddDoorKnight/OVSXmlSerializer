﻿namespace OVSXmlSerializer
{
	using System.IO;
	using System;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Serialization;
	using global::OVSXmlSerializer.Internals;

	/// <summary>
	/// A class that serializes or deserializes an object assuming that base class is a
	/// <see cref="object"/>
	/// into an xml document/format.
	/// </summary>
	public sealed class OVSXmlSerializer : OVSXmlSerializer<object>
	{
		/// <summary>
		/// Gets the non-generic shared version of the serializer.
		/// </summary>
		public static new OVSXmlSerializer Shared { get; } = new OVSXmlSerializer();
		/// <summary>
		/// Whenever a type is unclear or is more defined than given in the object
		/// field, it will use the type attribute in order to successfully create the
		/// object.
		/// </summary>
		internal const string ATTRIBUTE = "type";
		internal const string CONDITION = "con";

		/// <summary>
		/// The default flags to serialize instance field data.
		/// </summary>
		internal static readonly BindingFlags defaultFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

		/// <summary>
		/// Creates a new instance of a non-generic XML Serializer. Uses default
		/// config. References to default <see cref="object"/> as default type.
		/// </summary>
		public OVSXmlSerializer() : base()
		{
			
		}
		/// <summary>
		/// Creates a new instance of a non-generic XML Serializer. Uses default
		/// config. References to default <see cref="object"/> as default type.
		/// Uses a config that changes behaviour.
		/// </summary>
		public OVSXmlSerializer(OVSConfig config) : base(config)
		{
			
		}
	}
}

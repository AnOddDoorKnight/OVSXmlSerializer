﻿namespace OVS.XmlSerialization
{
	using global::OVS.XmlSerialization.Internals;
	using System;
	using System.Reflection;

	/// <summary>
	/// Writes the value as a primitive type, with some additional elements left over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class OVSXmlTextAttribute : Attribute
	{
		/// <summary>
		/// Checks if an object is the text field, instead of having elements.
		/// </summary>
		public static bool IsText(StructuredObject @object)
		{
			return @object.HasAttribute<OVSXmlTextAttribute>()
				|| @object.HasAttribute<System.Xml.Serialization.XmlTextAttribute>();
		}
		/// <summary>
		/// Checks if an field is the text field, instead of having elements.
		/// </summary>
		public static bool IsText(FieldInfo field)
		{
			Type type = field.FieldType;
			if (type.GetCustomAttribute<OVSXmlTextAttribute>() != null)
				return true;
			if (type.GetCustomAttribute<System.Xml.Serialization.XmlTextAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<OVSXmlTextAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<System.Xml.Serialization.XmlTextAttribute>() != null)
				return true;
			return false;
		}
		/// <summary>
		/// Creates a new instance of <see cref="OVSXmlTextAttribute"/>
		/// </summary>
		public OVSXmlTextAttribute()
		{

		}
	}
}
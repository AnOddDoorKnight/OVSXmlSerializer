﻿namespace OVS.XmlSerialization
{
	using global::OVS.XmlSerialization.Internals;
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	/// <summary>
	/// Writes an attribute to the containing class or struct instead of making
	/// its own element. Only primitive types can apply, and field must be not 
	/// an object to apply.
	/// <para>
	/// If <see cref="OVSXmlNamedAsAttribute"/> is on the same field, it will be 
	/// overrided by <see cref="CustomName"/>
	/// </para>
	/// </summary>
	/// <remarks>
	/// In the code, these are parsed first instead of the elements later listed
	/// in the object.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class OVSXmlAttributeAttribute : Attribute
	{
		/// <summary>
		/// Converts The normal xml attribute into the OVSXml Attribute;
		/// </summary>
		/// <param name="castie"> The victim to cast into with extreme amounts of bytes. </param>
		public static explicit operator OVSXmlAttributeAttribute(System.Xml.Serialization.XmlAttributeAttribute castie)
		{
			return castie == null ? null : new OVSXmlAttributeAttribute(castie.AttributeName);
		}

		/// <summary>
		/// Checks if the object is an attribute attribute, or the xml attribute's
		/// attribute attribute.
		/// </summary>
		/// <param name="object">The object to check.</param>
		/// <param name="attribute">Gets the attributed attributes attribute for the object that wants to be an attribute for the xml elements that can hold attributes.</param>
		public static bool IsAttribute(StructuredObject @object, out OVSXmlAttributeAttribute attribute)
		{
			if (@object.HasAttribute(out attribute))
				return true;
			bool hasAttribute = @object.HasAttribute<System.Xml.Serialization.XmlAttributeAttribute>(out var extAttribute);
			attribute = (OVSXmlAttributeAttribute)extAttribute;
			return hasAttribute;
		}
		/// <summary>
		/// Ensures that the field or type has the <see cref="OVSXmlAttributeAttribute"/>,
		/// The regular <see cref="System.Xml.Serialization.XmlAttributeAttribute"/>
		/// is applied as well.
		/// </summary>
		/// <param name="field"> The field to check. </param>
		/// <param name="contents"> 
		/// If <see langword="true"/>, then it will return the attribute contents.
		/// </param>
		public static bool IsAttribute(FieldInfo field, out OVSXmlAttributeAttribute contents)
		{
			Type type = field.FieldType;
			// Retrieve type attributes, first the type and then field for performance,
			// - then tries to get the alternate version of itself.
			var attributeType = type.GetCustomAttribute<OVSXmlAttributeAttribute>();
			if (attributeType != null)
			{
				contents = attributeType;
				return true;
			}
			attributeType = field.GetCustomAttribute<OVSXmlAttributeAttribute>();
			if (attributeType != null)
			{
				contents = attributeType;
				return true;
			}
			var altAttributeType = type.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>();
			if (altAttributeType != null)
			{
				contents = (OVSXmlAttributeAttribute)altAttributeType;
				return true;
			}
			altAttributeType = field.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>();
			if (altAttributeType != null)
			{
				contents = (OVSXmlAttributeAttribute)altAttributeType;
				return true;
			}
			contents = null;
			return false;
		}

		/// <summary>
		/// Checks if the type has an attribute attribute, or the xml attribute's
		/// attribute attribute.
		/// </summary>
		/// <param name="currentType">The object to check.</param>
		/// <param name="output">Gets the attributed attributes attribute for the object that wants to be an attribute for the xml elements that can hold attributes.</param>
		public static bool IsAttribute(Type currentType, out OVSXmlAttributeAttribute output)
		{
			var attributeType = currentType.GetCustomAttribute<OVSXmlAttributeAttribute>();
			if (attributeType != null)
			{
				output = attributeType;
				return true;
			}
			var AltAttributeType = currentType.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>();
			if (AltAttributeType != null)
			{
				output = (OVSXmlAttributeAttribute)AltAttributeType;
				return true;
			}
			output = null;
			return false;
		}

		/// <summary>
		/// The name to override the attribute name with. Note that it will override
		/// the value from <see cref="OVSXmlNamedAsAttribute.Name"/>.
		/// </summary>
		public string CustomName { get; set; } = null;
		/// <summary>
		/// Creates a new <see cref="OVSXmlAttributeAttribute"/> that utilizes the
		/// attribute's field name to assign to.
		/// </summary>
		public OVSXmlAttributeAttribute()
		{

		}
		/// <summary>
		/// Creates a new <see cref="OVSXmlAttributeAttribute"/> with a customized
		/// name that will override <see cref="OVSXmlNamedAsAttribute"/>'s name.
		/// </summary>
		/// <param name="customName"> A custom name for the attribute. </param>
		public OVSXmlAttributeAttribute(string customName)
		{
			this.CustomName = customName;
		}
	}
}
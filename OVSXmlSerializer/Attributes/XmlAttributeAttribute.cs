namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	/// <summary>
	/// Writes an attribute to the containing class or struct instead of making
	/// its own element. Only primitive types can apply, and field must be not 
	/// an object to apply.
	/// <para>
	/// If <see cref="XmlNamedAsAttribute"/> is on the same field, it will be 
	/// overrided by <see cref="CustomName"/>
	/// </para>
	/// </summary>
	/// <remarks>
	/// In the code, these are parsed first instead of the elements later listed
	/// in the object.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class XmlAttributeAttribute : Attribute
	{
		/// <summary>
		/// Converts The normal xml attribute into the OVSXml Attribute;
		/// </summary>
		/// <param name="castie"> The victim to cast into with extreme amounts of bytes. </param>
		public static explicit operator XmlAttributeAttribute(System.Xml.Serialization.XmlAttributeAttribute castie)
		{
			return castie == null ? null : new XmlAttributeAttribute(castie.AttributeName);
		}

		internal static bool IsAttribute(StructuredObject @object, out XmlAttributeAttribute attribute)
		{
			if (@object.HasAttribute(out attribute))
				return true;
			bool hasAttribute = @object.HasAttribute<System.Xml.Serialization.XmlAttributeAttribute>(out var extAttribute);
			attribute = (XmlAttributeAttribute)extAttribute;
			return hasAttribute;
		}
		/// <summary>
		/// Ensures that the field or type has the <see cref="XmlAttributeAttribute"/>,
		/// The regular <see cref="System.Xml.Serialization.XmlAttributeAttribute"/>
		/// is applied as well.
		/// </summary>
		/// <param name="field"> The field to check. </param>
		/// <param name="contents"> 
		/// If <see langword="true"/>, then it will return the attribute contents.
		/// </param>
		internal protected static bool IsAttribute(FieldInfo field, out XmlAttributeAttribute contents)
		{
			Type type = field.FieldType;
			// Retrieve type attributes, first the type and then field for performance,
			// - then tries to get the alternate version of itself.
			var attributeType = type.GetCustomAttribute<XmlAttributeAttribute>();
			if (attributeType != null)
			{
				contents = attributeType;
				return true;
			}
			attributeType = field.GetCustomAttribute<XmlAttributeAttribute>();
			if (attributeType != null)
			{
				contents = attributeType;
				return true;
			}
			var altAttributeType = type.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>();
			if (altAttributeType != null)
			{
				contents = (XmlAttributeAttribute)altAttributeType;
				return true;
			}
			altAttributeType = field.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>();
			if (altAttributeType != null)
			{
				contents = (XmlAttributeAttribute)altAttributeType;
				return true;
			}
			contents = null;
			return false;
		}

		internal static bool IsAttribute(Type currentType, out XmlAttributeAttribute output)
		{
			var attributeType = currentType.GetCustomAttribute<XmlAttributeAttribute>();
			if (attributeType != null)
			{
				output = attributeType;
				return true;
			}
			var AltAttributeType = currentType.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>();
			if (AltAttributeType != null)
			{
				output = (XmlAttributeAttribute)AltAttributeType;
				return true;
			}
			output = null;
			return false;
		}

		/// <summary>
		/// The name to override the attribute name with. Note that it will override
		/// the value from <see cref="XmlNamedAsAttribute.Name"/>.
		/// </summary>
		public string CustomName { get; set; }
		/// <summary>
		/// Creates a new <see cref="XmlAttributeAttribute"/> that utilizes the
		/// attribute's field name to assign to.
		/// </summary>
		public XmlAttributeAttribute()
		{

		}
		/// <summary>
		/// Creates a new <see cref="XmlAttributeAttribute"/> with a customized
		/// name that will override <see cref="XmlNamedAsAttribute"/>'s name.
		/// </summary>
		/// <param name="customName"> A custom name for the attribute. </param>
		public XmlAttributeAttribute(string customName)
		{
			this.CustomName = customName;
		}
	}
}
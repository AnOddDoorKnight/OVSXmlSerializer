namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	/// <summary>
	/// Writes an attribute to the containing class or struct instead of making
	/// its own element. Only primitive types can apply, and field must be not 
	/// an object to apply.
	/// </summary>
	/// <remarks>
	/// In the code, these are parsed first instead of the elements later listed
	/// in the object.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class XmlAttributeAttribute : Attribute
	{
		internal static bool IsAttribute(StructuredObject @object)
		{
			return @object.HasAttribute<XmlAttributeAttribute>()
				|| @object.HasAttribute<System.Xml.Serialization.XmlIgnoreAttribute>();
		}
		internal static bool IsAttribute(FieldInfo field)
		{
			Type type = field.FieldType;
			if (type.GetCustomAttribute<XmlAttributeAttribute>() != null)
				return true;
			if (type.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<XmlAttributeAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<System.Xml.Serialization.XmlAttributeAttribute>() != null)
				return true;
			return false;
		}
		public XmlAttributeAttribute()
		{

		}
	}
}
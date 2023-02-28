namespace OVSXmlSerializer
{
	using OVSXmlSerializer.Configuration;
	using System;
	using System.Reflection;

	/// <summary>
	/// When being serialized by <see cref="XmlSerializer"/>, it will write the
	/// name listed here instead of the field name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class XmlIgnoreAttribute : OVSAttribute
	{
		internal static bool Ignore(StructuredObject @object)
		{
			return @object.HasAttribute<XmlIgnoreAttribute>() 
				|| @object.HasAttribute<System.Xml.Serialization.XmlIgnoreAttribute>();
		}
		internal static bool Ignore(FieldInfo field)
		{
			Type type = field.FieldType;
			if (type.GetCustomAttribute<XmlIgnoreAttribute>() != null)
				return true;
			if (type.GetCustomAttribute<System.Xml.Serialization.XmlIgnoreAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<XmlIgnoreAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<System.Xml.Serialization.XmlIgnoreAttribute>() != null)
				return true;
			return false;
		}
		public XmlIgnoreAttribute()
		{
			
		}
	}
}
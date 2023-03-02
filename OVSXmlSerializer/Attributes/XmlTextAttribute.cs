namespace OVSXmlSerializer
{
	using OVSXmlSerializer.Configuration;
	using OVSXmlSerializer.Internals;
	using System;
	using System.Reflection;

	/// <summary>
	/// Writes the value as a primitive type, with some additional elements left over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class XmlTextAttribute : OVSAttribute
	{
		internal static bool IsText(StructuredObject @object)
		{
			return @object.HasAttribute<XmlTextAttribute>()
				|| @object.HasAttribute<System.Xml.Serialization.XmlTextAttribute>();
		}
		internal static bool IsText(FieldInfo field)
		{
			Type type = field.FieldType;
			if (type.GetCustomAttribute<XmlTextAttribute>() != null)
				return true;
			if (type.GetCustomAttribute<System.Xml.Serialization.XmlTextAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<XmlTextAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<System.Xml.Serialization.XmlTextAttribute>() != null)
				return true;
			return false;
		}
		/// <summary>
		/// Creates a new instance of <see cref="XmlTextAttribute"/>
		/// </summary>
		public XmlTextAttribute()
		{

		}
	}
}
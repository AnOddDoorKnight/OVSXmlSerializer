namespace OVS.XmlSerialization
{
	using global::OVS.XmlSerialization.Internals;
	using System;
	using System.Reflection;

	/// <summary>
	/// An attribute that specifically causes an object or field to ignore
	/// an end-user's custom serializers. Useful for considering interfaces.
	/// </summary>
	public class OVSXmlIgnoreConfigsAttribute : Attribute
	{
		internal static bool Ignore(StructuredObject @object)
		{
			return @object.HasAttribute<OVSXmlIgnoreConfigsAttribute>();
		}

		internal static bool Ignore(FieldInfo field)
		{
			Type type = field.FieldType;
			if (type.GetCustomAttribute<OVSXmlIgnoreConfigsAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<OVSXmlIgnoreConfigsAttribute>() != null)
				return true;
			return false;
		}
	}
}
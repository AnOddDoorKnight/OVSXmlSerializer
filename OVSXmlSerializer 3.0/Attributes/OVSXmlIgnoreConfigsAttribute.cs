namespace OVSXmlSerializer
{
	using global::OVSXmlSerializer.Internals;
	using System;
	using System.Reflection;

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
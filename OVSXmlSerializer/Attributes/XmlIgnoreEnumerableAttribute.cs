namespace OVSXmlSerializer
{
	using System;
	using System.Reflection;

	/// <summary>
	/// Since serializing as enumerable is prioritized, this will block the priority
	/// system to serialize it as a regular object instead.
	/// </summary>
	public class XMLIgnoreEnumerableAttribute : Attribute
	{
		internal static bool Ignore(StructuredObject @object)
		{
			return @object.HasAttribute<XMLIgnoreEnumerableAttribute>();
		}

		internal static bool Ignore(FieldInfo field)
		{
			Type type = field.FieldType;
			if (type.GetCustomAttribute<XMLIgnoreEnumerableAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<XMLIgnoreEnumerableAttribute>() != null)
				return true;
			return false;
		}
	}
}
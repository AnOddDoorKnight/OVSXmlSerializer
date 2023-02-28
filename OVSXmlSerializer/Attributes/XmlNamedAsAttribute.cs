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
	public class XmlNamedAsAttribute : OVSAttribute
	{
		internal static bool HasName(StructuredObject @object, out string name)
		{
			XmlNamedAsAttribute attribute = @object.ValueType.GetCustomAttribute<XmlNamedAsAttribute>();
			if (attribute is null)
			{
				if (@object is FieldObject fieldObject)
					return HasName(fieldObject.Field, out name);
				name = null;
				return false;
			}
			name = attribute.Name;
			return true;
		}
		internal static bool HasName(FieldInfo field, out string name)
		{
			Type type = field.FieldType;
			XmlNamedAsAttribute attribute = type.GetCustomAttribute<XmlNamedAsAttribute>();
			if (attribute != null)
			{
				name = attribute.Name;
				return true;
			}
			attribute = field.GetCustomAttribute<XmlNamedAsAttribute>();
			if (attribute != null)
			{
				name = attribute.Name;
				return true;
			}
			name = null;
			return false;
		}
		public string Name { get; set; }
		public XmlNamedAsAttribute(string name)
		{
			Name = name;
		}
	}
}
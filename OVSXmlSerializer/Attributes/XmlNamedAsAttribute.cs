namespace OVSXmlSerializer
{
	using OVSXmlSerializer.Configuration;
	using OVSXmlSerializer.Internals;
	using System;
	using System.Reflection;

	/// <summary>
	/// When being serialized by <see cref="XmlSerializer"/>, it will write the
	/// name listed here instead of the field name. 
	/// 
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
		/// <summary>
		/// Renames the specified element with this name. Make sure to not have
		/// any spaces in the name.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Creates a new <see cref="XmlNamedAsAttribute"/> for naming elements
		/// and attributes.
		/// </summary>
		/// <param name="name"> The name to rename the object in the XML file. </param>
		public XmlNamedAsAttribute(string name)
		{
			Name = name;
		}
	}
}
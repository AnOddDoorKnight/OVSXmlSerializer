namespace OVS.XmlSerialization
{
	using global::OVS.XmlSerialization.Internals;
	using System;
	using System.Reflection;

	/// <summary>
	/// When being serialized by <see cref="OVSXmlSerializer"/>, it will write the
	/// name listed here instead of the field name. 
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class OVSXmlNamedAsAttribute : Attribute
	{
		/// <summary>
		/// Checks if the object has the overrided name. If <see langword="true"/>,
		/// then returns said custom name.
		/// </summary>
		public static bool HasName(StructuredObject @object, out string name)
		{
			OVSXmlNamedAsAttribute attribute = @object.ValueType.GetCustomAttribute<OVSXmlNamedAsAttribute>();
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
		/// <summary>
		/// Checks if the field has the overrided name. If <see langword="true"/>,
		/// then returns said custom name.
		/// </summary>
		public static bool HasName(FieldInfo field, out string name)
		{
			Type type = field.FieldType;
			OVSXmlNamedAsAttribute attribute = type.GetCustomAttribute<OVSXmlNamedAsAttribute>();
			if (attribute != null)
			{
				name = attribute.Name;
				return true;
			}
			attribute = field.GetCustomAttribute<OVSXmlNamedAsAttribute>();
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
		/// Creates a new <see cref="OVSXmlNamedAsAttribute"/> for naming elements
		/// and attributes.
		/// </summary>
		/// <param name="name"> The name to rename the object in the XML file. </param>
		public OVSXmlNamedAsAttribute(string name)
		{
			Name = name;
		}
	}
}
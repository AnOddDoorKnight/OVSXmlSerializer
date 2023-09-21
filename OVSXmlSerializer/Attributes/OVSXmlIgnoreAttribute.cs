namespace OVSSerializer
{
	using global::OVSSerializer.Internals;
	using System;
	using System.Reflection;

	/// <summary>
	/// When being serialized by <see cref="OVSXmlSerializer"/>, it will write the
	/// name listed here instead of the field name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public class OVSXmlIgnoreAttribute : Attribute
	{
		/// <summary>
		/// Checks if the object has the ignore attribute, or the normal system
		/// xml equivalent.
		/// </summary>
		public static bool Ignore(StructuredObject @object)
		{
			return @object.HasAttribute<OVSXmlIgnoreAttribute>() 
				|| @object.HasAttribute<System.Xml.Serialization.XmlIgnoreAttribute>();
		}
		/// <summary>
		/// Checks if the field has the ignore attribute, the object itself, 
		/// along with the normal xml system equivalent.
		/// </summary>
		public static bool Ignore(FieldInfo field)
		{
			Type type = field.FieldType;
			if (type.GetCustomAttribute<OVSXmlIgnoreAttribute>() != null)
				return true;
			if (type.GetCustomAttribute<System.Xml.Serialization.XmlIgnoreAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<OVSXmlIgnoreAttribute>() != null)
				return true;
			if (field.GetCustomAttribute<System.Xml.Serialization.XmlIgnoreAttribute>() != null)
				return true;
			return false;
		}
		/// <summary>
		/// Constructs a new ignore attribute used to completely not assign the
		/// values in the XML file.
		/// </summary>
		public OVSXmlIgnoreAttribute()
		{
			
		}
	}
}
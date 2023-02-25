namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Serialization;
	using static XmlSerializer;

	internal readonly struct StructuredObject : IEquatable<StructuredObject>
	{
		public readonly object value;
		public readonly string fieldName;
		public readonly Type valueType;
		public readonly bool isNull;
		public readonly object parent;
		public Type ParentType => parent.GetType();
		public bool HasAttribute<T>() where T : Attribute
		{
			FieldInfo field = ParentType.GetField(fieldName, defaultFlags);
			IEnumerable<T> attributes = field.GetCustomAttributes<T>(); // For Debug Purposes
			return !(attributes.FirstOrDefault() is null);
		}

		public StructuredObject(FieldInfo field, StructuredObject parentObject)
		{
			value = field.GetValue(parentObject.value);
			fieldName = field.Name;
			parent = parentObject.value;
			if (isNull = value is null)
				valueType = field.FieldType;
			else
				// Gets a more defined type than what FieldType can offer
				valueType = field.GetValue(parent).GetType(); 
		}
		public StructuredObject(object value, FieldInfo valueReference, object parentObject) : this(value)
		{
			fieldName = valueReference.Name;
			parent = parentObject;
		}
		public StructuredObject(object value)
		{
			this.value = value;
			if (!(isNull = value is null))
				valueType = value.GetType();
			else
				valueType = default;
			parent = null;
			fieldName = string.Empty;
		}

		public override bool Equals(object obj)
		{
			if (obj is StructuredObject @struct)
				return Equals(@struct);
			return base.Equals(obj);
		}
		public bool Equals(StructuredObject other)
		{
			return value.Equals(other.value);
		}
	}
}
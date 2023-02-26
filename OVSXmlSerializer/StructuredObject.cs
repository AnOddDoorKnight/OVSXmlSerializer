namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Serialization;
	using static XmlSerializer;

	/// <summary>
	/// A singular struct that stores information about the object, the type,
	/// and the parent with its own type. There is additional values to consider
	/// as well.
	/// </summary>
	internal readonly struct StructuredObject : IEquatable<StructuredObject>
	{
		public static bool IsProbablyAutoImplementedProperty(string name) 
			=> name.Contains("<") && name.Contains(">");
		public static string RemoveAutoPropertyTags(string name) =>
			name.Substring(1, name.IndexOf('>') - 1);

		/// <summary>
		/// The value of the <see cref="StructuredObject"/>.
		/// </summary>
		public readonly object value;
		/// <summary>
		/// The field name of the value. <see langword="null"/> if it is standalone,
		/// or the initial input parameters.
		/// </summary>
		public readonly string fieldName;
		/// <summary>
		/// The type of <see cref="value"/>.
		/// </summary>
		public readonly Type valueType;
		/// <summary>
		/// If the value is null.
		/// </summary>
		public readonly bool isNull;
		/// <summary>
		/// The parent of the <see cref="value"/>. <see langword="null"/> if 
		/// </summary>
		public readonly object parent;
		/// <summary>
		/// The <see cref="parent"/>'s type.
		/// </summary>
		/// <exception cref="NullReferenceException"/>
		public Type ParentType => parent?.GetType();
		/// <summary>
		/// If the object is an auto-implemented property. Determined by if the
		/// field name contains the requirements.
		/// </summary>
		public bool IsAutoImplementedProperty => 
			IsProbablyAutoImplementedProperty(fieldName);
		/// <summary>
		/// If the object from a field, then it will determine if it has a derived
		/// class from the field. If it does, then <see langword="true"/>. Otherwise,
		/// <see langword="false"/>.
		/// </summary>
		public bool IsDerivedFromBase
		{
			get
			{
				Type fieldType = ParentType.GetField(fieldName, defaultFlags).FieldType;
				return !fieldType.IsAssignableFrom(valueType);
			}
		}
		/// <summary>
		/// If the field or object contains the attribute.
		/// </summary>
		/// <typeparam name="T"> The attribute. </typeparam>
		public bool HasAttribute<T>() where T : Attribute
		{
			bool output = false;
			if (!(valueType is null))
			{
				output = !(valueType.GetCustomAttribute<T>() is null);
			}
			if (output == false && parent != null)
			{
				FieldInfo field = ParentType.GetField(fieldName, defaultFlags);
				output |= !(field.GetCustomAttribute<T>() is null);
			}
			return output;
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
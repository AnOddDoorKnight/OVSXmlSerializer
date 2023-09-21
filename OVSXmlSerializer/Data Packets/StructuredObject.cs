namespace OVSSerializer.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Xml.Serialization;

	/// <summary>
	/// Metadata of the current object, along with the object itself.
	/// </summary>
	public class StructuredObject
	{
		/// <summary>
		/// Ensures the name is not an auto-property, and removes some jargon to
		/// make it XML safe, along with changing it as well depending on some 
		/// attributes.
		/// </summary>
		public static string EnsureName(string name, StructuredObject obj)
		{
			if (obj.IsNull)
				return "";
			if (OVSXmlNamedAsAttribute.HasName(obj, out string namedAtt))
				name = namedAtt;
			if (obj is FieldObject fieldObj && fieldObj.IsAutoImplementedProperty)
				name = RemoveAutoPropertyTags(name);
			name = name.Replace('`', '_');
			name = name.TrimEnd('[', ']');
			return name;
		}

		/// <summary>
		/// Because auto-implemented properties are serialized a bit differently,
		/// this attempts to check if it is a property.
		/// </summary>
		/// <param name="name">The name of the assumed property.</param>
		/// <returns>if it is actually a property.</returns>
		public static bool IsProbablyAutoImplementedProperty(string name)
			=> name.Contains("<") && name.Contains(">");
		/// <summary>
		/// Removes the tags that makes it an auto-property so it can be properly
		/// serialized.
		/// </summary>
		/// <param name="name">The name of the auto-property.</param>
		/// <returns>The formatted string.</returns>
		public static string RemoveAutoPropertyTags(string name) =>
			name.Substring(1, name.IndexOf('>') - 1);
		/// <summary>
		/// Checks if it is an auto-property. If it is, then it will filter it.
		/// </summary>
		/// <param name="input">The assumed property name.</param>
		/// <returns>The potentially filtered name.</returns>
		public static string TryRemoveAutoImplementedPropertyTags(in string input)
			=> IsProbablyAutoImplementedProperty(input) 
			? RemoveAutoPropertyTags(input)
			: input;

		/// <summary>
		/// The actual object it is trying to serialize.
		/// </summary>
		public object Value { get; }
		/// <summary>
		/// The type of the object.
		/// </summary>
		public Type ValueType { get; }
		/// <summary>
		/// The original or base type. Only really matters for root objects 
		/// or fields.
		/// </summary>
		public Type OriginatedType { get; protected set; }
		/// <summary>
		/// If the object is deriving from the base.
		/// </summary>
		public virtual bool IsDerivedFromBase
		{
			get
			{
				if (OriginatedType == null)
					return false;
				return OriginatedType.IsAssignableFrom(ValueType) && OriginatedType != ValueType;
			}
		}
		/// <summary>
		/// If the object is null.
		/// </summary>
		public bool IsNull { get; }
		/// <summary>
		/// If the object is primitive or a string.
		/// </summary>
		public bool IsPrimitive => ValueType.IsPrimitive || ValueType == typeof(string);

		/// <summary>
		/// Initializes the metadata for the object, storing info of the object
		/// itself.
		/// </summary>
		public StructuredObject(object value)
		{
			Value = value;
			if (IsNull = value is null)
				ValueType = null;
			else
				ValueType = value.GetType();
		}
		/// <summary>
		/// Initializes the metadata for the object, storing info of the object
		/// itself, along with measuring the target type.
		/// </summary>
		/// <param name="value">The object to keep track of.</param>
		/// <param name="targetType">The originating type.</param>
		public StructuredObject(object value, Type targetType) : this(value)
		{
			OriginatedType = targetType;
		}

		/// <summary>
		/// If the field or object contains the attribute.
		/// </summary>
		/// <typeparam name="T"> The attribute. </typeparam>
		public virtual bool HasAttribute<T>() where T : Attribute
		{
			bool output = false;
			if (!(ValueType is null))
			{
				output = !(ValueType.GetCustomAttribute<T>() is null);
			}
			return output;
		}
		/// <summary>
		/// If the field or object contains the attribute.
		/// </summary>
		/// <typeparam name="T"> The attribute. </typeparam>
		public virtual bool HasAttribute<T>(out T attribute) where T : Attribute
		{
			if (ValueType is null)
			{
				attribute = null;
				return false;
			}
			attribute = ValueType.GetCustomAttribute<T>();
			return attribute != null;
		}
	}
	
}
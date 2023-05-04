namespace OVSXmlSerializer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Xml.Serialization;
	using static XmlSerializer;

	internal class StructuredObject
	{
		public static bool IsProbablyAutoImplementedProperty(string name)
			=> name.Contains("<") && name.Contains(">");
		public static string RemoveAutoPropertyTags(string name) =>
			name.Substring(1, name.IndexOf('>') - 1);

		public object Value { get; }
		public Type ValueType { get; }
		public Type OriginatedType { get; protected set; }
		public virtual bool IsDerivedFromBase
		{
			get
			{
				if (OriginatedType == null)
					return false;
				return OriginatedType.IsAssignableFrom(ValueType) && OriginatedType != ValueType;
			}
		}

		public bool IsNull { get; }



		public StructuredObject(object value)
		{
			Value = value;
			if (IsNull = value is null)
				ValueType = null;
			else
				ValueType = value.GetType();
		}
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
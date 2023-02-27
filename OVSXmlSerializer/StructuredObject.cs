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
		public bool IsNull { get; }



		public StructuredObject(object value)
		{
			Value = value;
			if (IsNull = value is null)
				ValueType = null;
			else
				ValueType = value.GetType();
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
			//if (output == false && parent != null)
			//{
			//	FieldInfo field = ParentType.GetField(fieldName, defaultFlags);
			//	output |= !(field.GetCustomAttribute<T>() is null);
			//}
			return output;
		}
	}
	internal class FieldObject : StructuredObject
	{
		/// <summary>
		/// The parent of the <see cref="Value"/>. <see langword="null"/> if 
		/// </summary>
		public object Parent { get; }
		/// <summary>
		/// 
		/// </summary>
		public FieldInfo Field { get; }
		/// <summary>
		/// The <see cref="parent"/>'s type.
		/// </summary>
		/// <exception cref="NullReferenceException"/>
		public Type ParentType { get; }
		/// <summary>
		/// If the object is an auto-implemented property. Determined by if the
		/// field name contains the requirements.
		/// </summary>
		public bool IsAutoImplementedProperty =>
			IsProbablyAutoImplementedProperty(Field.Name);
		/// <summary>
		/// If the object from a field, then it will determine if it has a derived
		/// class from the field. If it does, then <see langword="true"/>. Otherwise,
		/// <see langword="false"/>.
		/// </summary>
		public bool IsDerivedFromBase
		{
			get
			{
				if (m_isDerivedFromBase != null)
					return m_isDerivedFromBase.Value;
				if (Parent == null)
					return false;
				Type fieldType = Field.FieldType;
				return (bool)(m_isDerivedFromBase = fieldType.IsAssignableFrom(ValueType) && fieldType != ValueType);
			}
		}
		private bool? m_isDerivedFromBase = null;

		public FieldObject(object value, FieldInfo field, object parent) : base(value)
		{
			Parent = parent;
			Field = field;
			ParentType = parent?.GetType();
		}
		public FieldObject(FieldInfo field, object parent) : this(field.GetValue(parent), field, parent)
		{

		}

		public override bool HasAttribute<T>()
		{
			bool output = base.HasAttribute<T>();
			if (!output)
				output |= !(Field.GetCustomAttribute<T>() is null);
			return output;
		}
	}
}
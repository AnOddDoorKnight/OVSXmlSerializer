namespace OVSXmlSerializer
{
	using System;

	internal readonly struct StructuredObject : IEquatable<StructuredObject>
	{
		public object Value { get; }
		public Type ValueType { get; }
		public bool IsNull { get; }

		public StructuredObject(object value)
		{
			Value = value;
			if (!(IsNull = value is null))
				ValueType = value.GetType();
			else
				ValueType = default;
		}

		public override bool Equals(object obj)
		{
			if (obj is StructuredObject @struct)
				return Equals(@struct);
			return base.Equals(obj);
		}
		public bool Equals(StructuredObject other)
		{
			return Value.Equals(other.Value);
		}
	}
}
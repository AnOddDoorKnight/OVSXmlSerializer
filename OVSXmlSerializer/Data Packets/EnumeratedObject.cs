namespace OVSXmlSerializer
{
	using System.Reflection;
	using System;

	internal class EnumeratedObject : StructuredObject
	{
		public EnumeratedObject(object value) : base(value)
		{
			
		}
		public EnumeratedObject(object value, Type targetType) : base(value, targetType)
		{
			
		}
	}
}
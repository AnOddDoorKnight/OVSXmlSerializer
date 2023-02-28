namespace OVSXmlSerializer.Configuration
{
	using System;
	using static XmlSerializer;

	public abstract class OverrideObject
	{

	}

	
	public abstract class OverrideTarget : OverrideObject
	{
		public Type TargetType { get; set; }
	}
	public class OverrideFieldTarget : OverrideTarget
	{
		public Type ParentType { get; set; }
		public string FieldName { get; set; }
	}
}
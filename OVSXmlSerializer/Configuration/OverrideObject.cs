namespace OVSXmlSerializer.Configuration
{
	using System;
	using static XmlSerializer;

	public abstract class OverrideTarget
	{
		public Type TargetType { get; set; }
	}
	public class OverrideFieldTarget : OverrideTarget
	{
		public Type ParentType { get; set; }
		public string FieldName { get; set; }
	}
}
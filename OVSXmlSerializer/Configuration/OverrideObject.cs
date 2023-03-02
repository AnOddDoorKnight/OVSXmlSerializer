namespace OVSXmlSerializer.Configuration
{
	using OVSXmlSerializer.Internals;
	using System;
	using System.IO;
	using System.Reflection;
	using System.Collections.ObjectModel;
	using System.Xml;
	using static XmlSerializer;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;

	public sealed class OverrideTarget
	{
		public static OverrideTarget FileInfo
		{
			get
			{
				var output = new OverrideTarget(typeof(FileInfo));
				bool gotFull = false;
				for (int i = 0; i < output.AllFields.Count; i++)
				{
					var (customAttributes, info) = output.AllFields[i];
					if (gotFull || info.Name == nameof(FileSystemInfo.FullName))
					{
						gotFull = true;
						customAttributes.Clear();
						customAttributes.Add(new XmlTextAttribute());
						continue;
					}
					customAttributes.Clear();
					customAttributes.Add(new XmlIgnoreAttribute());
				}
				return output;
			}
		}

		public Type TargetType { get; }
		public ParentTarget? Parent { get; }

		public OverrideTarget(Type value)
		{
			TargetType = value;
			FieldInfo[] fields = TargetType.GetFields(defaultFlags);
			AllFields = Array.AsReadOnly(Array.ConvertAll(fields, field =>
			{
				IEnumerable<OVSAttribute> attributes =
					from attribute in field.GetCustomAttributes(true)
					let output = attribute as OVSAttribute
					where !(output is null)
					select output;
				return (new List<OVSAttribute>(attributes), field);
			}));
			Parent = null;
		}
		public OverrideTarget(FieldInfo masterField, Type parent)
		{
			Parent = new ParentTarget()
			{
				ParentType = parent,
				FieldName = masterField.Name
			};
			TargetType = masterField.FieldType;
			FieldInfo[] fields = TargetType.GetFields(defaultFlags);
			AllFields = Array.AsReadOnly(Array.ConvertAll(fields, field => 
				(new List<OVSAttribute>(field.GetCustomAttributes<OVSAttribute>()), field)));
		}

		public ReadOnlyCollection<(List<OVSAttribute> customAttributes, FieldInfo info)> AllFields { get; private set; }
	}
	public struct ParentTarget
	{
		public Type ParentType { get; set; }
		public string FieldName { get; set; }
	}
}
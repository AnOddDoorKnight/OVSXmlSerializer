namespace OVSXmlSerializer.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using static XmlSerializer;

	public sealed class XmlSerializerOverrideConfig
	{
		public static readonly XmlSerializerOverrideConfig Default = new XmlSerializerOverrideConfig();
		
		
		
		public XmlSerializerOverrideConfig()
		{
			normalTargets = new List<OverrideTarget>();
			fieldTargets = new List<OverrideFieldTarget>();
			enumerableTargets = new List<EnumerableTarget>();
		}

		public IReadOnlyList<OverrideTarget> NormalTargets => normalTargets;
		private List<OverrideTarget> normalTargets;
		public bool HasTarget(object obj, out OverrideTarget overrideTarget)
		{
			Type type = obj.GetType();
			for (int i = 0; i < normalTargets.Count; i++)
			{
				OverrideTarget currentTarget = normalTargets[i];
				if (type != currentTarget.TargetType) 
					continue;
				overrideTarget = currentTarget;
				return true;
			}
			overrideTarget = null;
			return false;
		}

		public IReadOnlyList<OverrideTarget> FieldTargets => fieldTargets;
		private List<OverrideFieldTarget> fieldTargets;
		public bool HasFieldTarget(object obj, object parent, out OverrideFieldTarget overrideTarget)
		{
			Type type = obj.GetType(),
				parentType = parent.GetType();
			for (int i = 0; i < fieldTargets.Count; i++)
			{
				OverrideFieldTarget currentTarget = fieldTargets[i];
				if (type != currentTarget.TargetType)
					continue;
				if (parentType != currentTarget.ParentType)
					continue;
				FieldInfo fieldInfo = parentType.GetField(currentTarget.FieldName, defaultFlags);
				if (fieldInfo == null)
					continue;
				overrideTarget = currentTarget;
				return true;
			}
			overrideTarget = null;
			return false;
		}
		public bool HasFieldTarget(object parent, out OverrideFieldTarget overrideTarget)
		{
			Type parentType = parent.GetType();
			for (int i = 0; i < fieldTargets.Count; i++)
			{
				OverrideFieldTarget currentTarget = fieldTargets[i];
				if (parentType != currentTarget.ParentType)
					continue;
				FieldInfo fieldInfo = parentType.GetField(currentTarget.FieldName, defaultFlags);
				if (fieldInfo == null)
					continue;
				if (fieldInfo.GetValue(parent).GetType() != currentTarget.TargetType)
					continue;
				overrideTarget = currentTarget;
				return true;
			}
			overrideTarget = null;
			return false;
		}

		public IReadOnlyList<EnumerableTarget> EnumerableTargets => enumerableTargets;
		private List<EnumerableTarget> enumerableTargets;
		public EnumerableTarget DefaultEnumerableTarget { get; private set; } =
			new EnumerableTarget();
		public bool HasEnumerableTarget(object enumerable, out EnumerableTarget enumerableTarget)
		{
			Type type = enumerable.GetType();
			for (int i = 0; i < enumerableTargets.Count; i++)
			{
				EnumerableTarget currentTarget = enumerableTargets[i];
				if (type != currentTarget.TargetType)
					continue;
				enumerableTarget = currentTarget;
				return true;
			}
			enumerableTarget = null;
			return false;
		}
	}
}
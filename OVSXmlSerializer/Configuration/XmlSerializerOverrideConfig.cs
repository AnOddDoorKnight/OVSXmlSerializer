namespace OVSXmlSerializer.Configuration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;
	using static XmlSerializer;

	public sealed class XmlSerializerOverrideConfig
	{
		public static readonly XmlSerializerOverrideConfig Default = new XmlSerializerOverrideConfig();
		
		
		
		public XmlSerializerOverrideConfig()
		{
			normalTargets = new List<OverrideTarget>();
			enumerableTargets = new List<EnumerableTarget>();
		}

		public IReadOnlyList<OverrideTarget> NormalTargets => normalTargets;
		private List<OverrideTarget> normalTargets;
		public bool HasTarget(object obj, out OverrideTarget overrideTarget, object parent = null)
		{
			Type type = obj.GetType();
			for (int i = 0; i < normalTargets.Count; i++)
			{
				OverrideTarget currentTarget = normalTargets[i];
				if (type != currentTarget.TargetType) 
					continue;
				if (currentTarget.Parent.HasValue)
				{
					Type parentType = parent.GetType();
					if (parentType != currentTarget.Parent.Value.ParentType)
						continue;
					FieldInfo fieldInfo = parentType.GetField(currentTarget.Parent.Value.FieldName, defaultFlags);
					if (fieldInfo == null)
						continue;
				}
				overrideTarget = currentTarget;
				return true;
			}
			overrideTarget = null;
			return false;
		}

		
		public IReadOnlyList<EnumerableTarget> EnumerableTargets => enumerableTargets;
		private List<EnumerableTarget> enumerableTargets;
		public EnumerableTarget DefaultEnumerableTarget => EnumerableTarget.Default;
		public bool HasEnumerableTarget(object parent, out EnumerableTarget enumerableTarget)
		{
			Type type = parent.GetType();
			if (!type.IsArray && !typeof(IEnumerable).IsAssignableFrom(type))
				goto end;
			for (int i = 0; i < enumerableTargets.Count; i++)
			{
				var target = enumerableTargets[i];
				if (target.Predicate(parent))
				{
					enumerableTarget = target;
					return true;
				}
			} end:
			enumerableTarget = null;
			return false;
		}
	}
}
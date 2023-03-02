namespace OVSXmlSerializer.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	/// <summary>
	/// Caches all types when first recieved to reduce time.
	/// </summary>
	public class TypeCacher
	{
		private Dictionary<string, Type> FullNameTypes { get; } = new Dictionary<string, Type>();
		private Assembly[] AllAssemblies { get; } = AppDomain.CurrentDomain.GetAssemblies();
		public Type ByName(string fullName)
		{
			if (FullNameTypes.TryGetValue(fullName, out Type output))
				return output;
			for (int i = 0; i < AllAssemblies.Length; i++)
			{
				var type = AllAssemblies[i].GetType(fullName, false, true);
				if (type != null)
					return FullNameTypes[fullName] = type;
			}
			return FullNameTypes[fullName] = typeof(object);
		}
	}
}
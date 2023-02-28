namespace OVSXmlSerializer.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	/// <summary>
	/// Differentiates from a normal attribute and a attribute for serialization.
	/// </summary>
	public abstract class OVSAttribute : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns> All attributes that are derived from </returns>
		internal static List<CustomAttributeData> GetAllAttributes()
		{
			List<CustomAttributeData> types = new List<CustomAttributeData>();
			using (var enumerator = typeof(OVSAttribute).Assembly.CustomAttributes
				.Where(att => att.AttributeType != typeof(OVSAttribute) &&
				typeof(OVSAttribute).IsAssignableFrom(att.AttributeType)).GetEnumerator())
				while (enumerator.MoveNext())
					types.Add(enumerator.Current);
			return types;
		}
	}
}

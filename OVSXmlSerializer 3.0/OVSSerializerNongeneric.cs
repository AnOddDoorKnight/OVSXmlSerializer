namespace OVSXmlSerializer
{
	using System.Reflection;
	using System.Xml.Serialization;

	public sealed class OVSXmlSerializer : OVSXmlSerializer<object>
	{
		/// <summary>
		/// Whenever a type is unclear or is more defined than given in the object
		/// field, it will use the type attribute in order to successfully create the
		/// object.
		/// </summary>
		internal const string ATTRIBUTE = "type";
		internal const string CONDITION = "con";

		/// <summary>
		/// The default flags to serialize instance field data.
		/// </summary>
		internal static readonly BindingFlags defaultFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
	}
}

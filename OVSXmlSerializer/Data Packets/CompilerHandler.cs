namespace OVS.XmlSerialization.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml.Linq;

	internal static class CompilerHandler
	{
		public static bool IsCompilerGenerated(string objectName) 
			=> objectName.Contains("<") && objectName.Contains(">");
		public static bool IsCompilerGenerated(this FieldObject fieldObj)
			=> IsCompilerGenerated(fieldObj.Field.Name);

		public static string RemoveCompilerTags(this string taggedName)
		{
			int startIndex = taggedName.IndexOf('<'); int endIndex = taggedName.IndexOf(">");
			if (startIndex == -1 || endIndex == -1) return taggedName;
			return taggedName.Substring(startIndex + 1, endIndex - startIndex - 1);
		}
			
	}
}

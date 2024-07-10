namespace OVS.XmlSerialization.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;
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
			string newName = taggedName.Substring(startIndex + 1, endIndex - startIndex - 1);
			if (newName.Length <= 0)
				newName = taggedName.Remove(startIndex, endIndex - startIndex + 1);
			return newName;
		}

		public static string ValidateName(this string name)
		{
			name = name.RemoveCompilerTags();
			name = name.Replace('`', '_');
			name = name.TrimStart('1', '2', '3', '4', '5', '6', '7', '8', '9', '0');
			name = name.Trim('[', ']', '_');
			return name;
		}
	}
}

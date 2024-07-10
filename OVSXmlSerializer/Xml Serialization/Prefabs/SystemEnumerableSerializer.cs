namespace OVS.XmlSerialization.Prefabs
{
	using OVS.XmlSerialization.Internals;
	using OVS.XmlSerialization.Utility;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;

	/*
	/// <summary>
	/// Serializes <see cref="IList"/>, can take generic types if it is <see cref="List{T}"/>
	/// </summary>
	public class SystemEnumerableSerializer : ICustomSerializer
	{
		public static Regex MatchingRegex { get; } = new Regex("<[^>]*>d__[0-9]+");
		public const string NAME_ENDING = "sysEnum";
		public const string NAME_STATE = "state";
		public const string NAME_CURRENT = "current";

		//public static (int state, string current);


		/// <inheritdoc/>
		public bool CheckAndWrite(OVSXmlWriter writer, XmlNode parent, StructuredObject @object, string suggestedName, out XmlNode output)
		{
			bool match = MatchingRegex.IsMatch(@object.ValueType.Name);
			if (!match)
			{
				output = null;
				return false;
			}
			XmlElement specialElement = parent.OwnerDocument.CreateElement(suggestedName);
			parent.AppendChild(specialElement);
			XmlAttribute att = parent.OwnerDocument.CreateAttribute(NAME_ENDING);
			att.Value = "";
			specialElement.Attributes.Append(att);

			FieldInfo[] fields = @object.ValueType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			var state = new FieldObject(fields[0].GetValue(@object.Value), fields[0], @object);
			writer.WriteObject(state, specialElement, NAME_STATE);
			var current = new FieldObject(fields[1].GetValue(@object.Value), fields[1], @object);
			writer.WriteObject(current, specialElement, NAME_CURRENT);

			output = specialElement;
			return true;
		}
		/// <inheritdoc/>
		public bool CheckAndRead(OVSXmlReader reader, Type type, XmlNode node, out object output)
		{
			XmlNode attribute = node.Attributes.GetNamedItem(NAME_ENDING);
			if (attribute is null)
			{
				output = null;
				return false;
			}
			//Type realType = ;
			FormatterServices.GetUninitializedObject(type);
			output = null;
			return true;
		}
		/*
		private Type GetBaseType(Type assigningType)
		{
			Type output = typeof(object);
			if (assigningType.Namespace == typeof(List<object>).Namespace)
				output = assigningType.GetGenericArguments()[0];
			return output;
		}
		*/
	//}
}

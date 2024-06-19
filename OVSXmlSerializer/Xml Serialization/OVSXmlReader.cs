namespace OVS.XmlSerialization.Internals
{
	using global::OVS.XmlSerialization.Exceptions;
	using global::OVS.XmlSerialization.Utility;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Runtime.Serialization;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Serialization;

	/// <summary>
	/// A reader that converts a single XML document by breaking down types to
	/// read their fields.
	/// </summary>
	/// <remarks>
	/// Not thread safe if you want to use the writer for multiple serializers; 
	/// uses quite a bit of global variables.
	/// </remarks>
	public class OVSXmlReader
	{
		private static readonly IReadOnlyList<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Reverse().ToArray();
		/// <summary>
		/// Searches through all assemblies trying to match the string name
		/// given, considering generics along the way.
		/// </summary>
		/// <returns>The type that is defined by <paramref name="fullName"/>.</returns>
		/// <exception cref="MissingMemberException">The <paramref name="fullName"/> is invalid and doesn't lead to anything. </exception>
		public static Type ByName(string fullName)
		{
			for (int i = 0; i < allAssemblies.Count; i++)
			{
				Type type = allAssemblies[i].GetType(fullName, false);
				if (type == null)
					continue;
				return type;
			}
			throw new MissingMemberException(fullName);
		}

		/// <summary>
		/// Adds the auto-implementation tag to an existing name.
		/// </summary>
		public static string AddAutoImplementedTag(string input)
		{
			return $"<{input}>k__BackingField";
		}

		/// <summary>
		/// Gets all reference types by the index.
		/// </summary>
		protected Dictionary<int, object> referenceTypes;

		/// <summary>
		/// All changes that is written to. Not thread safe.
		/// </summary>
		public XmlDocument Document { get; private set; }
		/// <summary>
		/// The creator or source of the reader.
		/// </summary>
		public IOVSConfig Config { get; }
		internal OVSXmlReferencer Referencer { get; private set; }

		/// <summary>
		/// Creates a new instance, assuming that <paramref name="config"/> is its
		/// creator.
		/// </summary>
		/// <param name="config">The creator.</param>
		public OVSXmlReader(IOVSConfig config)
		{
			Config = config;
			referenceTypes = new Dictionary<int, object>();
		}

		/// <summary>
		/// Reads a document, checking the version before trying to read it as
		/// a node or element.
		/// </summary>
		/// <param name="document">The document to deserialize.</param>
		/// <param name="rootType">The root type.</param>
		/// <exception cref="VersionMismatchException"></exception>
		public virtual object ReadDocument(XmlDocument document, Type rootType)
		{
			referenceTypes.Clear();
			XmlNode rootNode = document.LastChild;
			if (!Versioning.IsVersion(document, Config.Version, Config.VersionLeniency))
				throw new VersionMismatchException(Version.Parse(rootNode.Attributes.GetNamedItem(Versioning.VERSION_NAME).Value), Config.Version);//$"object '{rootNode.Name}' of version '{}' is not version '{Config.Version}'!");
			object returnObject = ReadObject(rootNode, rootType);
			return returnObject;
		}
		/// <summary>
		/// Reads an object, and hopefully returning a result.
		/// </summary>
		/// <param name="targetNode">The current object's representative node.</param>
		/// <param name="toType">The objects assumed defined type.</param>
		/// <returns>The parsed object, and the fields under it.</returns>
		/// <exception cref="InvalidCastException"></exception>
		/// <exception cref="MissingFieldException"></exception>
		public object ReadObject(XmlNode targetNode, Type toType = null)
		{
			if (targetNode == null)
				return null;

			if (toType == null)
			{
				if (targetNode is XmlAttribute)
					throw new InvalidCastException($"{targetNode.Name} is an attribute with a null type!");
				XmlAttribute attributeNode = (XmlAttribute)targetNode.Attributes.GetNamedItem(OVSXmlSerializer.ATTRIBUTE);
				if (attributeNode is null)
					throw new MissingFieldException();
				toType = ByName(attributeNode.Value);
			}
			else if (!toType.IsValueType && !OVSXmlAttributeAttribute.IsAttribute(toType, out _) && targetNode.Attributes != null)
			{
				// Class Type probably is defined, but not derived. 
				XmlNode attributeNode = targetNode.Attributes.GetNamedItem(OVSXmlSerializer.ATTRIBUTE);
				string possibleDerivedTypeName = null;
				if (attributeNode != null)
					possibleDerivedTypeName = attributeNode.Value;
				if (!string.IsNullOrEmpty(possibleDerivedTypeName))
				{
					Type possibleDerivedType = ByName(possibleDerivedTypeName);
					bool isDerived = toType.IsAssignableFrom(possibleDerivedType);
					if (isDerived)
					{
						toType = possibleDerivedType;
					}
				}
			}
			if (TryReadReference(out object @ref))
				return @ref;


			// Letting the jesus take the wheel
			if (typeof(IOVSXmlSerializable).IsAssignableFrom(toType))
			{
				object serializableOutput = Activator.CreateInstance(toType, true);
				AddReferenceTypeToDictionary((XmlElement)targetNode, serializableOutput);
				IOVSXmlSerializable xmlSerializable = (IOVSXmlSerializable)serializableOutput;
				xmlSerializable.Read(targetNode);
				return serializableOutput;
			}

			if (TryReadPrimitive(toType, targetNode, out object output))
				return output;
			if (TryReadCustom(toType, targetNode, out output))
				return output;



			// Standard class with regular serialization.
			//object obj = Activator.CreateInstance(toType, true);
			object obj = FormatterServices.GetUninitializedObject(toType);
			AddReferenceTypeToDictionary((XmlElement)targetNode, obj);

			// Serializes fields by getting fields by name, and matching it from
			// - the node list.
			FieldInfo[] allFields = toType.GetFields(OVSXmlSerializer.defaultFlags);
			string[] keys = new string[allFields.Length];
			Dictionary<string, FieldInfo> fieldDictionary = new Dictionary<string, FieldInfo>(allFields.Length);
			// Modify for auto-implemented properties
			for (int i = 0; i < allFields.Length; i++)
			{
				FieldInfo currentField = allFields[i];
				string key = keys[i] = currentField.Name.RemoveCompilerTags();
				fieldDictionary.Add(key, currentField);
			}
			// Getting Attributes
			List<(string Key, FieldInfo Value)>
				attributes = new List<(string Key, FieldInfo Value)>(),
				elements = new List<(string Key, FieldInfo Value)>();
			(string Key, FieldInfo Value)? text = null;
			for (int i = 0; i < allFields.Length; i++)
			{
				FieldInfo field = allFields[i];
				string fieldName = field.Name.RemoveCompilerTags();
				// Getting Named Arguments
				if (OVSXmlNamedAsAttribute.HasName(field, out string name))
					fieldName = name;
				// Getting special conditions
				if (OVSXmlAttributeAttribute.IsAttribute(field, out var attributeContents))
					attributes.Add(
						(string.IsNullOrEmpty(attributeContents.CustomName)
						? fieldName
						: attributeContents.CustomName, field));
				else if (OVSXmlTextAttribute.IsText(field))
					text = (fieldName, field);
				else
					elements.Add((fieldName, field));
			}
			// Reading attributes first
			for (int i = 0; i < attributes.Count; i++)
			{
				string key = attributes[i].Key;
				XmlNode attribute = targetNode.Attributes.GetNamedItem(key);
				FieldInfo field = attributes[i].Value;
				SetFieldValue(field, obj, ReadObject(attribute, field.FieldType));
			}
			// Reading elements
			if (text.HasValue)
			{
				TryReadPrimitive(text.Value.Value.FieldType, targetNode, out object textOutput);
				SetFieldValue(text.Value.Value, obj, textOutput);
			}
			else
				for (int i = 0; i < elements.Count; i++)
				{
					string key = elements[i].Key;
					XmlNode element = targetNode.SelectSingleNode(key) 
						?? targetNode.SelectSingleNode($"Reference_{elements[i].Key}");
					FieldInfo field = elements[i].Value;
					object outputField = ReadObject(element, field.FieldType);
					if (outputField is null)
						if (Config.IgnoreUndefinedValues)
						{
							continue;
						}
					SetFieldValue(field, obj, outputField);
				}
			return obj;

			bool TryReadPrimitive(Type type, XmlNode node, out object primitiveOut)
			{
				// Since string is arguably a class or char array, its it own check.
				if (type.IsPrimitive || type == typeof(string))
				{
					string unparsed = node.ReadValue();
					primitiveOut = Convert.ChangeType(unparsed, type, Config.CurrentCulture);
					return true;
				}
				if (type.IsEnum)
				{
					primitiveOut = Enum.Parse(toType, targetNode.InnerText);
					return true;
				}
				primitiveOut = null;
				return false;
			}
			bool TryReadReference(out object reference)
			{
				if (toType.IsValueType)
				{
					reference = null;
					return false;
				}
				if (toType == typeof(string))
				{
					reference = null;
					return false;
				}
				if (!targetNode.Name.StartsWith("Reference_"))
				{
					reference = null;
					return false;
				}
				int ID = int.Parse(targetNode.InnerText);
				reference = referenceTypes[ID];
				return true;
			}
		}
		/// <summary>
		/// Adds a reference type to its internal dictionary, assuming it is enabled
		/// and has attributes to find such data.
		/// </summary>
		/// <param name="element">The element to search through and mark.</param>
		/// <param name="value">The created class to reference to.</param>
		public void AddReferenceTypeToDictionary(XmlElement element, object value)
		{
			if (Config.UseSingleInstanceInsteadOfMultiple == false)
				return;
			if (element.Attributes == null || element.Attributes.Count == 0)
				return;
			XmlAttribute attribute = (XmlAttribute)element.Attributes.GetNamedItem(OVSXmlReferencer.REFERENCE_ATTRIBUTE);
			if (attribute is null)
				return;
			int id = int.Parse(attribute.Value);
			referenceTypes.Add(id, value);
		}
		internal bool TryReadCustom(in Type toType, in XmlNode targetNode, out object customOut)
		{
			if (Config.CustomSerializers.Read(this, toType, targetNode, out customOut))
				return true;
			return false;
		}
		/// <summary>
		/// Sets the field info via the <paramref name="parent"/>. It is its own
		/// method for considering readonly fields.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="parent"></param>
		/// <param name="setting"></param>
		/// <exception cref="Exception"></exception>
		public void SetFieldValue(FieldInfo info, object parent, object setting)
		{
			if (info.IsInitOnly)
				switch (Config.HandleReadonlyFields)
				{
					case ReadonlyFieldHandle.ThrowError:
						throw new ReadOnlyException($"{info.Name} field is readonly!");
					case ReadonlyFieldHandle.Ignore:
						return;
					case ReadonlyFieldHandle.Continue:
						goto normal;
				}
			normal:
			info.SetValue(parent, setting);
		}
	}
}

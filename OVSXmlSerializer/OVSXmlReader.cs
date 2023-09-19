namespace OVSXmlSerializer.Internals
{
	using global::OVSXmlSerializer.Extras;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Serialization;

	public class OVSXmlReader<T>
	{
		private static readonly IReadOnlyList<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Reverse().ToArray();
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

		protected Dictionary<int, object> referenceTypes;

		/// <summary>
		/// All changes that is written to. Not thread safe.
		/// </summary>
		public XmlDocument Document { get; private set; }
		public OVSXmlSerializer<T> Source { get; }
		public OVSConfig Config => Source.Config;
		internal OVSXmlReferencer Referencer { get; private set; }

		public OVSXmlReader(OVSXmlSerializer<T> source)
		{
			this.Source = source;
			referenceTypes = new Dictionary<int, object>();
		}

		public virtual T ReadDocument(XmlDocument document, Type rootType)
		{
			referenceTypes.Clear();
			XmlNode rootNode = document.LastChild;
			if (!Versioning.IsVersion(document, Config.Version, Config.VersionLeniency))
				throw new InvalidCastException($"object '{rootNode.Name}' of version '{rootNode.Attributes.GetNamedItem(Versioning.VERSION_NAME).Value}' is not version '{Config.Version}'!");
			T returnObject = (T)ReadObject(rootNode, rootType);
			return returnObject;
		}
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
			if (Config.CustomSerializers.Read(this, toType, targetNode, out object customOut))
				return customOut;



			// Standard class with regular serialization.
			object obj = Activator.CreateInstance(toType, true);
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
				string key = keys[i] =
					FieldObject.IsProbablyAutoImplementedProperty(currentField.Name)
					? FieldObject.RemoveAutoPropertyTags(currentField.Name)
					: currentField.Name;
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
				string fieldName = StructuredObject.TryRemoveAutoImplementedPropertyTags(field.Name);
				// Getting Named Arguments
				if (OVSXmlNamedAsAttribute.HasName(field, out string name))
					fieldName = name;
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
					XmlNode element = targetNode.SelectSingleNode(key);
					if (element == null)
						element = targetNode.SelectSingleNode($"Reference_{elements[i].Key}");
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
		public string GetStringValue(in FieldInfo info, XmlNode parent)
		{
			string nameToSearch = info.Name;
			if (OVSXmlNamedAsAttribute.HasName(info, out string namedAs))
				nameToSearch = namedAs;
			if (OVSXmlAttributeAttribute.IsAttribute(info, out var contents))
			{
				if (!string.IsNullOrEmpty(contents.CustomName))
					nameToSearch = contents.CustomName;
				XmlNode attribute = parent.Attributes.GetNamedItem(nameToSearch);
				return attribute.Value;
			}
			XmlNode element = parent.SelectSingleNode(nameToSearch);

			return element.InnerText;
		}
		public void SetFieldValue(FieldInfo info, object parent, object setting)
		{
			if (info.IsInitOnly)
				switch (Config.HandleReadonlyFields)
				{
					case ReadonlyFieldHandle.ThrowError:
						throw new Exception($"{info.Name} field is readonly!");
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

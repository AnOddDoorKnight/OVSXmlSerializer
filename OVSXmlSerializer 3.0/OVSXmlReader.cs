namespace OVSXmlSerializer.Internals
{
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
		/// <summary>
		/// Gets a type 
		/// </summary>
		public static IReadOnlyDictionary<string, Type> Types { get; } = ((Func<IReadOnlyDictionary<string, Type>>)(() =>
		{
			var output = new Dictionary<string, Type>();
			Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < allAssemblies.Length; i++)
			{
				Assembly assembly = allAssemblies[i];
				Type[] allTypes = assembly.GetTypes();
				for (int ii = 0; ii < allTypes.Length; ii++)
				{
					output.Add(allTypes[ii].FullName, allTypes[ii]);
				}
			}
			return output;
		})).Invoke();
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
		public virtual object ReadObject(XmlNode node, Type currentType = null)
		{
			if (node == null)
				return null;

			// Handling types
			if (currentType == null)
			{
				XmlNode attributeNode = node.Attributes.GetNamedItem(OVSXmlSerializer.ATTRIBUTE);
				string typeValue = null;
				if (attributeNode != null)
					typeValue = attributeNode.Value;
				if (string.IsNullOrEmpty(typeValue))
					throw new Exception();
				currentType = Types[typeValue];
			}
			else if (!currentType.IsValueType && !OVSXmlAttributeAttribute.IsAttribute(currentType, out _) && node.Attributes != null)
			{
				// Class Type probably is defined, but not derived. Only if
				// - its not a struct or attribute (& has attributes).
				XmlNode attributeNode = node.Attributes.GetNamedItem(OVSXmlSerializer.ATTRIBUTE);
				string possibleDerivedTypeName = null;
				if (attributeNode != null)
					possibleDerivedTypeName = attributeNode.Value;
				if (!string.IsNullOrEmpty(possibleDerivedTypeName))
				{
					Type possibleDerivedType = Types[possibleDerivedTypeName];
					bool isDerived = currentType.IsAssignableFrom(possibleDerivedType);
					if (isDerived)
						currentType = possibleDerivedType;
				}
			}

			if (TryReadReference(currentType, node, out object @ref))
				return @ref;

			// Letting the jesus take the wheel
			if (typeof(IOVSXmlSerializable).IsAssignableFrom(currentType))
			{
				object serializableOutput = Activator.CreateInstance(currentType, true);
				AddReferenceTypeToDictionary((XmlElement)node, serializableOutput);
				IOVSXmlSerializable xmlSerializable = (IOVSXmlSerializable)serializableOutput;
				xmlSerializable.Read(node);
				return serializableOutput;
			}
			if (TryReadEnum(currentType, node, out object objectEnum))
				return objectEnum;
			if (TryReadPrimitive(currentType, node, out object output))
				return output;
			if (TryReadCustom(currentType, node, out object objectCustom))
				return objectCustom;


			// Standard class with regular serialization.
			object obj = Activator.CreateInstance(currentType, true);
			AddReferenceTypeToDictionary((XmlElement)node, obj);

			// Serializes fields by getting fields by name, and matching it from
			// - the node list.
			FieldInfo[] allFields = currentType.GetFields(OVSXmlSerializer.defaultFlags);
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
				string fieldName = FieldObject.IsProbablyAutoImplementedProperty(field.Name)
					? FieldObject.RemoveAutoPropertyTags(field.Name)
					: field.Name;
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
				XmlNode attribute = node.Attributes.GetNamedItem(key);
				FieldInfo field = attributes[i].Value;
				SetFieldValue(field, obj, ReadObject(attribute, field.FieldType));
			}
			// Reading elements
			if (text.HasValue)
			{
				TryReadPrimitive(text.Value.Value.FieldType, node, out object textOutput);
				SetFieldValue(text.Value.Value, obj, textOutput);
			}
			else
				for (int i = 0; i < elements.Count; i++)
				{
					string key = elements[i].Key;
					XmlNode element = node.SelectSingleNode(key);
					if (element == null)
						element = node.SelectSingleNode($"Reference_{elements[i].Key}");
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
		}
		internal protected virtual bool TryReadEnum(Type type, XmlNode node, out object output)
		{
			if (!type.IsEnum)
			{
				output = null;
				return false;
			}
			output = Enum.Parse(type, node.ReadValue());
			return true;
		}
		internal protected virtual bool TryReadPrimitive(Type type, XmlNode node, out object output)
		{
			// Since string is arguably a class or char array, its it own check.
			if (!type.IsPrimitive && type != typeof(string))
			{
				output = null;
				return false;
			}
			string unparsed = node.ReadValue();
			output = Convert.ChangeType(unparsed, type, Config.CurrentCulture);
			return true;
		}
		/// <summary>
		/// Parses the enumerable if it implements <see cref="ICollection"/>.
		/// </summary>
		/// <param name="type"> The type. </param>
		/// <param name="node"> The data. </param>
		/// <param name="output"> The resulting item. </param>
		/// <returns>If it has successfully parsed. </returns>
		/// <exception cref="NotImplementedException"> 
		/// If there is no implementation for the collection. 
		/// </exception>
		internal protected virtual bool TryReadCustom(Type type, XmlNode node, out object output)
		{
			if (type.GetCustomAttribute<OVSXmlIgnoreConfigsAttribute>() != null)
			{
				output = null;
				return false;
			}
			return Config.CustomSerializers.Read(this, type, node, out output);
		}
		internal bool IsPrimitive(StructuredObject primitive)
		{
			return primitive.ValueType.IsPrimitive || primitive.ValueType == typeof(string);
		}
		/// <summary>
		/// tries to get the reference ID to dictionary from the attribute stored.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		internal void AddReferenceTypeToDictionary(XmlElement element, object value)
		{
			XmlAttribute attribute = (XmlAttribute)element.Attributes.GetNamedItem(OVSXmlReferencer.REFERENCE_ATTRIBUTE)
				?? throw new InvalidOperationException();
			int id = int.Parse(attribute.Value);
			referenceTypes.Add(id, value);
		}
		internal protected virtual bool TryReadReference(Type type, XmlNode node, out object reference)
		{
			if (type.IsValueType)
			{
				reference = null;
				return false;
			}
			if (type == typeof(string))
			{
				reference = null;
				return false;
			}
			if (!node.Name.StartsWith("Reference_"))
			{
				reference = null;
				return false;
			}
			int ID = int.Parse(node.InnerText);
			reference = referenceTypes[ID];
			return true;
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

	}
}

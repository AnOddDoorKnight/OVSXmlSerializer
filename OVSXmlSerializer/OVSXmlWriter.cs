﻿namespace OVSSerializer.Internals
{
	using global::OVSSerializer.Exceptions;
	using global::OVSSerializer;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Serialization;
	using System.Data;

	/// <summary>
	/// A writer that converts a single object into a full <see cref="XmlDocument"/>,
	/// which can be written as a memory stream for serializing in documents on the
	/// computer.
	/// </summary>
	/// <remarks>
	/// Not thread safe if you want to use the writer for multiple serializers; 
	/// uses quite a bit of global variables.
	/// </remarks>
	/// <typeparam name="T">The object being serialized as their own 'parent' type.</typeparam>
	public class OVSXmlWriter
	{
		/// <summary>
		/// Throws an exception if the specified type does not have a parameterless
		/// constructor. Otherwise does nothing.
		/// </summary>
		/// <param name="type"> The type to check if it has one. </param>
		internal static void EnsureParameterlessConstructor(Type type)
		{
			if (type.GetConstructor(OVSXmlSerializer.defaultFlags, null, Array.Empty<Type>(), null) == null && type.IsClass)
			{
				string message = $"{type.Name} does not have an empty constructor!";
				throw new ParameteredOnlyException(message);
			}
		}

		/// <summary>
		/// All changes that is written to. Not thread safe.
		/// </summary>
		public XmlDocument Document { get; private set; }
		/// <summary>
		/// Taking from <see cref="Source"/> to get the config.
		/// </summary>
		public IOVSConfig Config { get; }
		internal OVSXmlReferencer Referencer { get; private set; }

		/// <summary>
		/// Creates a new instance with a source serializer, which tracks configs.
		/// </summary>
		public OVSXmlWriter(IOVSConfig config)
		{
			Config = config;
		}

		/// <summary>
		/// Serializes a single object into a xml file, using <see cref="Document"/>
		/// as to save its changes.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <param name="type">The assumed base type of field.</param>
		/// <param name="name">The root name of the XML document.</param>
		public XmlDocument SerializeObject(in object obj, Type type, string name)
		{
			Document = new XmlDocument();
			if (Config.UseSingleInstanceInsteadOfMultiple)
				Referencer = new OVSXmlReferencer(Document, Config);
			else
				Referencer = null; 
			Type startingType = typeof(object);
			if (type.IsAssignableFrom(obj?.GetType() ?? typeof(object)))
				startingType = type;
			var structuredObject = new StructuredObject(obj, startingType);
			if (!structuredObject.IsNull)
			{
				if (!Config.OmitAutoGenerationComment)
					Document.AppendChild(Document.CreateComment("Auto-generated by OVSXmlSerializer"));
				WriteObject(structuredObject, Document, StructuredObject.EnsureName(name, structuredObject));
				if (!Config.OmitXmlDelcaration)
				{
					string standalone = null;
					switch (Config.StandaloneDeclaration)
					{
						case true: standalone = "yes"; break;
						case false: standalone = "no"; break;
					}
					XmlDeclaration declaration = Document.CreateXmlDeclaration("1.0", Config.Encoding.ToString(), standalone);
					Document.PrependChild(declaration);
				}
				// Then write version number if enabled
				if (!(Config.Version is null) && Document?.LastChild?.Attributes != null)
				{
					XmlAttribute versionAttribute = Document.CreateAttribute(Versioning.VERSION_NAME);
					versionAttribute.Value = Config.Version.ToString();
					Document.LastChild.Attributes.Append(versionAttribute);
				}
			}
			return Document;
		}
		/// <summary>
		/// Writes an object, goes through the checklist:
		/// <list type="number">
		/// <item>The <see cref="IOVSXmlSerializable"/> interface, assuming <see cref="IOVSXmlSerializable.ShouldWrite"/> is true.</item>
		/// <item>Serializing it as a reference.</item>
		/// </list>
		/// </summary>
		/// <param name="obj">The object to serialize to.</param>
		/// <param name="parent">The parent of the object.</param>
		/// <param name="name">The name of the element or attribute.</param>
		/// <param name="forceNormalSerialize">If it ignores all the extras and just serializes the fields, useful for external serializers.</param>
		/// <exception cref="Exception"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="InvalidCastException"></exception>
		public XmlNode WriteObject(StructuredObject obj, XmlNode parent, string name, bool forceNormalSerialize = false)
		{
			if (obj.IsNull)
				return null;
			try
			{
				if (OVSXmlIgnoreAttribute.Ignore(obj))
					return null;
				if (obj.Value is IOVSXmlSerializable serializable)
				{
					// Not sure how the actual scheme works at all since I never used
					// - one. I don't this applies here at all anyways.
					if (serializable.ShouldWrite == false)
						return null;
					EnsureParameterlessConstructor(obj.ValueType);
					XmlElement serializableElement = CreateElement(parent, name);
					serializable.Write(serializableElement);
					return serializableElement;
				}
				if (obj.Value is Delegate)
					return null;



				if (ReferenceType(name, obj, parent, out XmlNode output))
					return output;
				if (TryWritePrimitive(name, obj, parent, out output))
					return output;
				if (Config.CustomSerializers.Write(this, parent, obj, name, out output))
					return output;
				if (OVSXmlAttributeAttribute.IsAttribute(obj, out _)) // Not primitive, but struct or class
					throw new Exception($"{obj.Value} is not a primitive type!");

				EnsureParameterlessConstructor(obj.ValueType);
				XmlNode currentNode = CreateElement(parent, name, obj);
				
				FieldInfo[] infos = obj.ValueType.GetFields(OVSXmlSerializer.defaultFlags);

				bool didText = false;
				bool hasElements = false;
				for (int i = 0; i < infos.Length; i++)
				{
					FieldInfo field = infos[i];
					FieldObject fieldObject = new FieldObject(field, obj.Value);
					if (field.IsInitOnly)
						switch (Config.HandleReadonlyFields)
						{
							case ReadonlyFieldHandle.Ignore:
								continue;
							case ReadonlyFieldHandle.ThrowError:
								throw new ReadOnlyException($"{field.Name} from {obj.ValueType.Name} is readonly!");
							case ReadonlyFieldHandle.Continue:
								break;
						}
					// Doing ignore again for compatibility with text attribute
					if (OVSXmlIgnoreAttribute.Ignore(field))
						continue;

					if (OVSXmlTextAttribute.IsText(field))
					{
						if (didText)
							throw new InvalidOperationException($"There can only be one [{nameof(XmlText)}]!");
						if (hasElements)
							throw new InvalidOperationException($"There are elements in the field in {obj.Value}, be sure to ignore or put them as attributes!");
						didText = true;
						currentNode.InnerText = ToStringPrimitive(fieldObject);
						continue;
					}
					if (OVSXmlAttributeAttribute.IsAttribute(field, out var contents))
					{
						WriteObject(fieldObject, currentNode, StructuredObject.EnsureName(field.Name, fieldObject));
						continue;
					}
					if (didText)
						throw new Exception();
					hasElements = true;
					WriteObject(fieldObject, currentNode, StructuredObject.EnsureName(field.Name, fieldObject));
				}
				return currentNode;
			}
			// This is to add a more descriptive exception message to easily debug
			// - and report.
			catch (Exception ex)
			{
				var descriptor = new StringBuilder($"Failed to serialize '{obj.ValueType.FullName}'");
				if (obj is FieldObject fieldObject)
					descriptor.Append($" with field name '{fieldObject.Field.Name}' from '{fieldObject.ParentType.FullName}'");
				throw new SerializationFailedException(descriptor.ToString(), ex);
			}
		}


		/// <summary>
		/// Turns the ordinary field into a reference, given there is a reference
		/// defined first.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public bool ReferenceType(in string name, StructuredObject obj, XmlNode parent, out XmlNode output)
		{
			output = null;
			if (!Config.UseSingleInstanceInsteadOfMultiple)
				return false;
			if (OVSXmlReferencer.CanReference(obj) == false)
				return false;
			if (Referencer.IsAlreadyReferenced(obj, out int indexID))
			{
				XmlElement referenceElement = CreateElement(parent, $"Reference_{name}");
				referenceElement.InnerText = indexID.ToString();
				output = referenceElement;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Tries to write a primitive, ensuring that it is a XML element or
		/// XML attribute via class attribute.
		/// </summary>
		/// <param name="name">The name of the element or attribute.</param>
		/// <param name="primitive">The object that is hopefully primitive to serialize.</param>
		/// <param name="parent">The parent of the attribute or element.</param>
		/// <returns>If it successfully serialized it as primitive.</returns>
		public bool TryWritePrimitive(string name, StructuredObject primitive, XmlNode parent, out XmlNode output)
		{
			if (primitive.IsPrimitive)
			{
				output = CreateNode(parent, name, ToStringPrimitive(primitive), primitive);
				return true;
			}
			if (primitive.ValueType.IsEnum)
			{
				output = CreateNode(parent, name, primitive.Value.ToString(), primitive);
				return true;
			}
			output = null;
			return false;
		}

		/// <summary>
		/// Converts a primitive data type, considering formattable interfaces.
		/// </summary>
		/// <param name="value">The primitive data type.</param>
		/// <returns>The string sexy boy</returns>
		public string ToStringPrimitive(StructuredObject value)
		{
			if (!value.IsPrimitive)
				throw new InvalidCastException(value.ValueType.FullName);
			if (value.Value is IFormattable formattable)
				return formattable.ToString(null, Config.CurrentCulture);
			return value.Value.ToString();
		}

		/// <summary>
		/// Converts a primitive data type, considering formattable interfaces.
		/// </summary>
		/// <param name="value">The primitive data type.</param>
		/// <returns>The string sexy boy</returns>
		public string ToStringPrimitive(object value)
		{
			if (value is null)
				throw new NullReferenceException(nameof(value));
			Type type = value.GetType();
			if (!type.IsPrimitive && type != typeof(string))
				throw new InvalidCastException(type.FullName);
			if (value is IFormattable formattable)
				return formattable.ToString(null, Config.CurrentCulture);
			return value.ToString();
		}
		/// <summary>
		/// Tries to handle the object with custom interface serializers that
		/// is outside of primitive data type abilities.
		/// </summary>
		internal bool TryWriteCustoms(string name, StructuredObject values, XmlNode parent, out XmlNode output)
		{
			//if (XMLIgnoreCustomsAttribute.Ignore(values))
			//	return false;
			InterfaceSerializer serializer = Config.CustomSerializers;
			return serializer.Write(this, parent, values, name, out output);
		}

		/// <summary>
		/// Creates a node and stores information in the value or inner text
		/// depending on the settings. Useful for things that print primitive
		/// values.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name">The name of the element or attribute.</param>
		/// <param name="value"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public XmlNode CreateNode(XmlNode parent, string name, string value, StructuredObject obj)
		{
			if (OVSXmlAttributeAttribute.IsAttribute(obj, out var attribute))
			{
				if (OVSXmlNamedAsAttribute.HasName(obj, out string namedAs))
					name = namedAs;
				if (!string.IsNullOrEmpty(attribute.CustomName))
					name = attribute.CustomName;
				if (obj is FieldObject fieldObj && fieldObj.IsDerivedFromBase)
					throw new Exception($"Cannot serialize {name} as the field " +
						"type doesn't match the object type. This is typically " +
						"caused as the primitive value is derived off of object " +
						"in an XML Attribute, unable to write the derived attribute " +
						"type.");
				XmlAttribute xmlAttribute = CreateAttribute(parent, name, value);
				return xmlAttribute;
			}
			XmlElement element = CreateElement(parent, name, obj);
			element.InnerText = value;
			return element;
		}
		/// <summary>
		/// Creates a new element, adding derived types and references.
		/// </summary>
		/// <param name="parent">The parent node to append to.</param>
		/// <param name="name">The name of the new element.</param>
		/// <param name="obj">The object to consider to add types and references.</param>
		/// <returns>The new element to modify and change.</returns>
		public XmlElement CreateElement(XmlNode parent, string name, StructuredObject obj)
		{
			if (OVSXmlNamedAsAttribute.HasName(obj, out string namedAs))
				name = namedAs;
			XmlElement element = CreateElement(parent, name);
			WriteTypeAttribute(element, obj);
			if (Config.UseSingleInstanceInsteadOfMultiple && OVSXmlReferencer.CanReference(obj))
				Referencer.AddReference(obj, element);
			return element;
		}
		/// <summary>
		/// Creates a new empty element.
		/// </summary>
		/// <param name="parent">The parent to append.</param>
		/// <param name="elementName">The name of the new element.</param>
		/// <returns>An empty element with the custom name.</returns>
		public XmlElement CreateElement(XmlNode parent, string elementName)
		{
			XmlElement element = Document.CreateElement(elementName);
			parent.AppendChild(element);
			return element;
		}
		/// <summary>
		/// Adds an attribute to a <see cref="XmlElement"/>
		/// </summary>
		/// <param name="parentElement"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns>The attribute that has the value assigned.</returns>
		public XmlAttribute CreateAttribute(XmlNode parentElement, string name, string value)
		{
			XmlAttribute attribute = Document.CreateAttribute(name);
			attribute.Value = value;
			parentElement.Attributes.Append(attribute);
			return attribute;
		}
		/// <summary>
		/// Adds a type attribute to a element
		/// </summary>
		/// <param name="element"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public XmlAttribute WriteTypeAttribute(XmlElement element, StructuredObject obj)
		{
			if (Config.TypeHandling == IncludeTypes.AlwaysIncludeTypes)
				return CreateAttribute(element, OVSXmlSerializer.ATTRIBUTE, obj.ValueType.FullName);
			if (Config.TypeHandling == IncludeTypes.SmartTypes)
				if (obj.IsDerivedFromBase)
					return CreateAttribute(element, OVSXmlSerializer.ATTRIBUTE, obj.ValueType.FullName);
			return null;
		}
	}
}

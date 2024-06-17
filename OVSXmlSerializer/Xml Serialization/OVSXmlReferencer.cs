namespace OVSSerializer.Xml.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	/// <summary>
	/// Handles references of classes
	/// </summary>
	internal class OVSXmlReferencer
	{
		public static bool CanReference(in StructuredObject obj) => CanReference(obj.ValueType);
		public static bool CanReference(in object obj) => CanReference(obj.GetType());
		public static bool CanReference(in Type type)
		{
			if (type.IsPrimitive)
				return false;
			if (type.IsValueType)
				return false;
			if (type == typeof(string))
				return false;
			return true;
		}

		internal const string REFERENCE_ATTRIBUTE = "reference_id";


		private readonly List<ReferencedObject> objects; 
		public IOVSConfig Config { get; }
		public XmlDocument Document { get; }
		public OVSXmlReferencer(XmlDocument document, IOVSConfig config)
		{
			this.Config = config;
			this.Document = document;
			objects = new List<ReferencedObject>();
		}

		/*
		public bool IsAlreadyReferenced(StructuredObject @object)
		{
			int index = objects.FindIndex(refer => refer.ReferenceEquals(@object));
			return index != -1;
		}
		internal bool IsAlreadyReferenced(StructuredObject @object, out int index)
		{
			index = objects.FindIndex(refer => refer.ReferenceEquals(@object));
			return index != -1;
		}
		*/
		public bool TryGetReference(StructuredObject input, out ReferencedObject? reference)
		{
			int index = objects.FindIndex(refer => refer.ReferenceEquals(input));
			if (index == -1)
			{
				reference = null;
				return false;
			}
			reference = objects[index];
			return true;
		}

		public void AddReference(StructuredObject input, XmlElement element)
		{
			if (TryGetReference(input, out _))
				throw new InvalidCastException();
			ReferencedObject obj = new ReferencedObject()
			{ 
				objectReference = input,
				original = element,
				index = objects.Count
			};
			objects.Add(obj);
		}


		public struct ReferencedObject
		{
			public StructuredObject objectReference;
			public XmlElement original;
			internal int index;
			public bool ReferenceEquals(in StructuredObject obj)
			{
				if (!CanReference(obj))
					return false;
				return ReferenceEquals(obj.Value, objectReference.Value);
			}
			public int GetReference()
			{
				if (original.Attributes.GetNamedItem(REFERENCE_ATTRIBUTE) == null)
				{
					XmlAttribute att = original.OwnerDocument.CreateAttribute(REFERENCE_ATTRIBUTE);
					att.Value = index.ToString();
					original.Attributes.Append(att);
				}
				return index;
			}
		}
	}
}

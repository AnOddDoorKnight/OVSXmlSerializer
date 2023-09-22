namespace OVSSerializer.Internals
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

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

		public void AddReference(StructuredObject input, XmlElement element)
		{
			if (IsAlreadyReferenced(input))
				throw new InvalidCastException();
			XmlAttribute att = element.OwnerDocument.CreateAttribute(REFERENCE_ATTRIBUTE);
			att.Value = objects.Count.ToString();
			element.Attributes.Prepend(att);
			objects.Add(new ReferencedObject { objectReference = input, original = element });
		}


		internal struct ReferencedObject
		{
			public StructuredObject objectReference;
			public XmlElement original;
			public bool ReferenceEquals(in StructuredObject obj)
			{
				if (!CanReference(obj))
					return false;
				return ReferenceEquals(obj.Value, objectReference.Value);
			}
		}
	}
}

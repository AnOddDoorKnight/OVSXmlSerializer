﻿namespace OVSXmlSerializer
{
	using System;
	using System.IO;
	using System.Xml;

	
	public static class Versioning
	{
		internal const string VERSION_ATTRIBUTE = "ver";
		public enum Leniency : byte
		{
			/// <summary>
			/// If the values are not exactly the same, then it will return false
			/// </summary>
			Strict = 0,
			/// <summary>
			/// Only revisions are allowed, which is typically considered as security
			/// updates.
			/// </summary>
			Revisions = 1,
			/// <summary>
			/// Only revisions and builds are allowed, which would be changes such 
			/// as bug fixes
			/// </summary>
			Builds = 2,
			/// <summary>
			/// Only things that consider backwards compatibility are allowed,
			/// which are simple feature addons.
			/// </summary>
			Minor = 3,
			/// <summary>
			/// Even majors are acceptable, so this is a 'use-all' case with the 
			/// rare exception of null Versions.
			/// </summary>
			All = 4,
		}

		public static bool IsVersion(string fileLocation, Version version, Leniency leniency = Leniency.Strict)
		{
			using (Stream stream = File.OpenRead(fileLocation))
				return IsVersion(stream, version, leniency);
		}
		public static bool IsVersion(FileInfo fileLocation, Version version, Leniency leniency = Leniency.Strict)
		{
			using (Stream stream = fileLocation.OpenRead())
				return IsVersion(stream, version, leniency);
		}
		public static bool IsVersion(Stream input, Version version, Leniency leniency = Leniency.Strict)
		{
			XmlDocument document = new XmlDocument();
			try
			{
				document.Load(input);
			}
			catch (XmlException exception) when (exception.Message == "Root element is missing.")
			{
				return default;
			}
			return IsVersion(document, version, leniency);
		}
		public static bool IsVersion(XmlDocument document, Version version, Leniency leniency = Leniency.Strict)
		{
			XmlNode rootNode = document.ChildNodes.Item(document.ChildNodes.Count - 1);
			XmlNode versionNode = rootNode.Attributes.GetNamedItem(VERSION_ATTRIBUTE);
			if (versionNode != null)
			{
				Version inputVersion = Version.Parse(versionNode.Value);
				return IsVersion(inputVersion, version, leniency);
			}
			return version == null;
		}
		public static bool IsVersion(Version version, Version altVersion, Leniency leniency = Leniency.Strict)
		{
			switch (leniency)
			{
				case Leniency.Strict:
					return altVersion.Major == version.Major
						&& altVersion.Minor == version.Minor
						&& altVersion.Build == version.Build
						&& altVersion.Revision == version.Revision;
				case Leniency.Revisions:
					return altVersion.Major == version.Major
						&& altVersion.Minor == version.Minor
						&& altVersion.Build == version.Build;
				case Leniency.Builds:
					return altVersion.Major == version.Major
						&& altVersion.Minor == version.Minor;
				case Leniency.Minor:
					return altVersion.Major == version.Major;
				case Leniency.All:
					return true;
			}
			throw new Exception();
		}
	}
}
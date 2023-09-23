namespace OVSSerializer
{
	using System;
	using System.IO;
	using System.Xml;

	/// <summary>
	/// Measures Xml files to check if they are a specific version or not.
	/// </summary>
	public static class Versioning
	{
		/// <summary>
		/// The attribute's name for finding the version on the document.
		/// </summary>
		public const string VERSION_NAME = "ver";
		/// <summary>
		/// Measurement on how <see cref="Versioning"/> should handle variation changes
		/// 
		/// </summary>
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

		/// <summary>
		/// Checks the version of the XML file.
		/// </summary>
		/// <param name="fileLocation"> The full file location. </param>
		/// <param name="version"> Comparing to/expected value </param>
		/// <param name="leniency">The leinency of the check</param>
		public static bool IsVersion(string fileLocation, Version version, Leniency leniency = Leniency.Strict)
		{
			using (Stream stream = File.OpenRead(fileLocation))
				return IsVersion(stream, version, leniency);
		}
		/// <summary>
		/// Checks the version of the XML file.
		/// </summary>
		/// <param name="fileLocation"> The file location. </param>
		/// <param name="version"> Comparing to/expected value </param>
		/// <param name="leniency">The leinency of the check</param>
		public static bool IsVersion(FileInfo fileLocation, Version version, Leniency leniency = Leniency.Strict)
		{
			using (Stream stream = fileLocation.OpenRead())
				return IsVersion(stream, version, leniency);
		}

		/// <summary>
		/// Checks the version of the XML file.
		/// </summary>
		/// <param name="input"> The document as a stream. </param>
		/// <param name="version"> Comparing to/expected value </param>
		/// <param name="leniency">The leinency of the check</param>
		public static bool IsVersion(Stream input, Version version, Leniency leniency = Leniency.Strict)
		{
			XmlDocument document = new XmlDocument();
			document.Load(input);
			return IsVersion(document, version, leniency);
		}
		/// <summary>
		/// Checks the version of the XML file.
		/// </summary>
		/// <param name="document"> The document. </param>
		/// <param name="version"> Comparing to/expected value </param>
		/// <param name="leniency">The leinency of the check</param>
		public static bool IsVersion(XmlDocument document, Version version, Leniency leniency = Leniency.Strict)
		{
			XmlNode rootNode = document.LastChild;
			XmlNode versionNode = rootNode.Attributes.GetNamedItem(VERSION_NAME);
			Version inputVersion = versionNode == null ? new Version(1, 0) : Version.Parse(versionNode.Value);
			return IsVersion(inputVersion, version, leniency);
		}
		/// <summary>
		/// Checks the version of the XML file.
		/// </summary>
		/// <param name="altVersion"> The comparator of one version </param>
		/// <param name="version"> Comparing to/expected value </param>
		/// <param name="leniency">The leinency of the check</param>
		public static bool IsVersion(Version version, Version altVersion, Leniency leniency = Leniency.Strict)
		{
			if (version is null)
				version = new Version(1, 0);
			if (altVersion is null)
				altVersion = new Version(1, 0);
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
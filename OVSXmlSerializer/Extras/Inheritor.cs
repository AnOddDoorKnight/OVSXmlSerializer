namespace OVSSerializer
{
	using OVSSerializer.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Text;

	/// <summary>
	/// Introduces the Inherit method to the serializers, although it can be used
	/// directly. 
	/// </summary>
	public static class Inheritor
	{
		/// <summary>
		/// Takes info from <paramref name="stream"/> and copies its fields
		/// into the <paramref name="inputReference"/>; Effectively shallow copying.
		/// </summary>
		/// <param name="serializer">The serializer to get the info to copy from.</param>
		/// <param name="inputReference">The item to copy info to.</param>
		/// <param name="stream">The info is stored from in the stream, should be XML.</param>
		public static void Inherit<T>(this OVSXmlSerializer<T> serializer, T inputReference, Stream stream) where T : class
		{
			T output = serializer.Deserialize(stream);
			Inherit(inputReference, output);
		}
		/// <summary>
		/// Takes info from <paramref name="filePath"/> and copies its fields
		/// into the <paramref name="inputReference"/>; Effectively shallow copying.
		/// </summary>
		/// <param name="serializer">The serializer to get the info to copy from.</param>
		/// <param name="inputReference">The item to copy info to.</param>
		/// <param name="filePath">The info is stored from in file path.</param>
		public static void Inherit<T>(this OVSXmlSerializer<T> serializer, T inputReference, FileInfo filePath) where T : class
		{
			T output = serializer.Deserialize(filePath);
			Inherit(inputReference, output);
		}
		/// <summary>
		/// Takes info from <paramref name="filePath"/> and copies its fields
		/// into the <paramref name="inputReference"/>; Effectively shallow copying.
		/// </summary>
		/// <param name="serializer">The serializer to get the info to copy from.</param>
		/// <param name="inputReference">The item to copy info to.</param>
		/// <param name="filePath">The info is stored from in file path.</param>
		public static void Inherit<T>(this OVSXmlSerializer<T> serializer, T inputReference, OSFile filePath) where T : class
		{
			T output = serializer.Deserialize(filePath);
			Inherit(inputReference, output);
		}
		/// <summary>
		/// Takes info from <paramref name="filePath"/> and copies its fields
		/// into the <paramref name="inputReference"/>; Effectively shallow copying.
		/// </summary>
		/// <param name="serializer">The serializer to get the info to copy from.</param>
		/// <param name="inputReference">The item to copy info to.</param>
		/// <param name="filePath">The info is stored from in file path.</param>
		public static void Inherit<T>(this OVSXmlSerializer<T> serializer, T inputReference, string filePath) where T : class
		{
			T output = serializer.Deserialize(filePath);
			Inherit(inputReference, output);
		}
		/// <summary>
		/// Takes info from <paramref name="deserialized"/> and copies its fields
		/// into the <paramref name="inputReference"/>; Effectively shallow copying.
		/// </summary>
		/// <param name="inputReference">The item to copy info to.</param>
		/// <param name="deserialized">The info is stored from.</param>
		public static void Inherit<T>(T inputReference, in T deserialized) where T : class
		{
			FieldInfo[] fields = inputReference.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				field.SetValue(inputReference, field.GetValue(deserialized));
			}
		}
	}
}

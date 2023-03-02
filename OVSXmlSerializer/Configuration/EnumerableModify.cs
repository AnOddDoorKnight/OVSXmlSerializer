namespace OVSXmlSerializer.Configuration
{
	using OVSXmlSerializer.Internals;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Xml;
	using static XmlSerializer;

	public class EnumerableTarget : IDisposable
	{
		public static EnumerableTarget Default => new EnumerableTarget();
		private static void TypeCheck(Type type)
		{
			if (typeof(IEnumerable).IsAssignableFrom(type))
				return;
			throw new InvalidCastException(type.ToString());
		}
		/// <summary>
		/// The list or array that is checked through this customizable method.
		/// </summary>
		public virtual Predicate<object> Predicate { get; }
		public virtual string EntryName { get; } = "Item";
		/// <summary>
		/// Within the element, these are applied to all entries that are named
		/// <see cref="EntryName"/>.
		/// </summary>
		public virtual OVSAttribute[] customEntries { get; }
		private EnumerableTarget()
		{
			customEntries = Array.Empty<OVSAttribute>();
			Predicate = (value) => true;
		}
		public EnumerableTarget(Type storing)
		{
			TypeCheck(storing);
			customEntries = storing.GetCustomAttributes<OVSAttribute>().ToArray();
			Predicate = (value) =>
			{
				if (value is IEnumerable enumerable)
				{
					var enumerator = enumerable.GetEnumerator();
					enumerator.MoveNext();
					if (enumerator.Current != null)
						return enumerator.Current.GetType() == storing;
				}
				return false;
			};
		}
		public EnumerableTarget(Type storing, Predicate<object> customPredicate)
		{
			TypeCheck(storing);
			customEntries = storing.GetCustomAttributes<OVSAttribute>().ToArray();
			Predicate = customPredicate;
		}
		public EnumerableTarget(Type parent, Type storing)
		{
			TypeCheck(storing);
			customEntries = storing.GetCustomAttributes<OVSAttribute>().ToArray();
			Predicate = (value) =>
			{
				if (parent != value.GetType())
					return false;
				if (value is IEnumerable enumerable)
				{
					var enumerator = enumerable.GetEnumerator();
					enumerator.MoveNext();
					if (enumerator.Current != null)
						return enumerator.Current.GetType() == storing;
				}
				return false;
			};
		}

		/// <summary>
		/// Cleans up derived types of <see cref="EnumerableTarget"/>.
		/// </summary>
		public virtual void Dispose()
		{

		}
	}
}

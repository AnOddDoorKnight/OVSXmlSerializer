namespace OVS.XmlSerialization.Utility
{
	using System;

	/// <summary>
	/// A value that alerts other classes and fields when modified.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ChangablePrimitive<T> where T : IConvertible
	{
		/// <summary>
		/// Allows to seamlessly get the values inside of the value.
		/// </summary>
		/// <param name="input"></param>
		public static implicit operator T(ChangablePrimitive<T> input)
		{
			return input.Value;
		}
		[OVSXmlText]
		private T value;
		/// <summary>
		/// All delegates are invoked when the value is changed.
		/// </summary>
		[field: OVSXmlIgnore]
		public event Action<T> ValueChanged;
		/// <summary>
		/// Gets or sets the current Value.
		/// </summary>
		public T Value
		{
			get => value;
			set
			{
				this.value = value;
				ValueChanged?.Invoke(value);
			}
		}
		/// <summary>
		/// Creates a new value, using default values as initializer.
		/// </summary>
		public ChangablePrimitive()
		{
			value = default;
		}
		/// <summary>
		/// Creates a new value.
		/// </summary>
		/// <param name="default"> The starting value. Usually as initial value. </param>
		public ChangablePrimitive(T @default)
		{
			value = @default;
		}
		/// <summary>
		/// Setting the expected field and alerts future changes to the delegate.
		/// </summary>
		/// <param name="input"> The delegate that targets the field. </param>
		public void AttachValue(Action<T> input)
		{
			input.Invoke(Value);
			ValueChanged += input;
		}
	}
	/// <summary>
	/// A value that alerts other classes and fields when modified. Allows usage
	/// of structs and classes.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ChangableStructure<T>
	{
		/// <summary>
		/// Allows to seamlessly get the values inside of the value.
		/// </summary>
		/// <param name="input"></param>
		public static implicit operator T(ChangableStructure<T> input)
		{
			return input.Value;
		}
		private T internalValue;
		/// <summary>
		/// All delegates are invoked when the value is changed.
		/// </summary>
		[field: OVSXmlIgnore]
		public event Action<T> ValueChanged;
		/// <summary>
		/// Gets or sets the current Value.
		/// </summary>
		public T Value
		{
			get => internalValue;
			set
			{
				this.internalValue = value;
				ValueChanged?.Invoke(value);
			}
		}
		/// <summary>
		/// Creates a new value, using default values as initializer.
		/// </summary>
		public ChangableStructure()
		{
			internalValue = default;
		}
		/// <summary>
		/// Creates a new value.
		/// </summary>
		/// <param name="default"> The starting value. Usually as initial value. </param>
		public ChangableStructure(T @default)
		{
			internalValue = @default;
		}
		/// <summary>
		/// Setting the expected field and alerts future changes to the delegate.
		/// </summary>
		/// <param name="input"> The delegate that targets the field. </param>
		public void AttachValue(Action<T> input)
		{
			input.Invoke(Value);
			ValueChanged += input;
		}
	}
}

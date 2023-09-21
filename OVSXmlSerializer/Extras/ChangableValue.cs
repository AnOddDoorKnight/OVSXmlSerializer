namespace OVSSerializer.Extras
{
	using System;

	/// <summary>
	/// A value that alerts other classes and fields when modified.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ChangableValue<T>
	{
		/// <summary>
		/// Allows to seamlessly get the values inside of the value.
		/// </summary>
		/// <param name="input"></param>
		public static implicit operator T(ChangableValue<T> input)
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
		public ChangableValue()
		{
			value = default;
		}
		/// <summary>
		/// Creates a new value.
		/// </summary>
		/// <param name="default"> The starting value. Usually as initial value. </param>
		public ChangableValue(T @default)
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
}

using System;

namespace Sandbox;

public struct SAttribute<T> where T : System.IEquatable<T>
{
	private T _value;

	public T Value
	{
		get => _value;
		set
		{
			if(_value.Equals(value))
				return;
			
			_value = value;
			ChangeEvent?.Invoke(_value);
		}
	}
	
	public event Action<T> ChangeEvent;
	
	private SAttribute(T value)
	{
		_value = value;
	}
	
	// Implicit conversion from T to SAttribute<T>
	public static implicit operator SAttribute<T>(T value)
	{
		return new SAttribute<T>(value);
	}

	// Implicit conversion from SAttribute<T> to T
	public static implicit operator T(SAttribute<T> attribute)
	{
		return attribute._value;
	}
}

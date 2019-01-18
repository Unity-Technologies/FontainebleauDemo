using System;
using UnityEngine;

namespace Polybrush
{
	public class z_LocalPref<T> where T : IEquatable<T>
	{
		public string key;

		[SerializeField] private T _value;

		public T prefValue
		{
			get
			{
				return _value;
			}

			set
			{
				if(  !_value.Equals(value) )
				{
					_value = value;

					if( typeof(T) == typeof(bool) )
						z_Pref.SetBool(key, (bool) ((object) _value));
					else if( typeof(T) == typeof(Color) )
						z_Pref.SetColor(key, (Color) ((object) _value));
					else if( typeof(T) == typeof(int) )
						z_Pref.SetInt(key, (int) ((object) _value));
					else if( typeof(T) == typeof(float) )
						z_Pref.SetFloat(key, (float) ((object) _value));
					else if( typeof(T) == typeof(Gradient) )
						z_Pref.SetGradient(key, (Gradient) ((object) _value));
				}
			}
		}

		public z_LocalPref(string key, T initialValueIfNoKey = default(T))
		{
			this.key = key;

			// box and unbox because due to casting.  not ideal, but the alternative is writing
			// z_LocalPref overloads for each type.
			if( typeof(T) == typeof(bool) )
				this._value = (T)((object)z_Pref.GetBool(key, (bool) (object) initialValueIfNoKey));
			else if( typeof(T) == typeof(Color) )
				this._value = (T)((object)z_Pref.GetColor(key, (Color) (object) initialValueIfNoKey));
			else if( typeof(T) == typeof(int) )
				this._value = (T)((object)z_Pref.GetInt(key, (int) (object) initialValueIfNoKey));
			else if( typeof(T) == typeof(float) )
				this._value = (T)((object)z_Pref.GetFloat(key, (float) (object) initialValueIfNoKey));
			else if( typeof(T) == typeof(Gradient) )
				this._value = (T)((object)z_Pref.GetGradient(key));
			else
				this._value = default(T);
		}

		public static implicit operator T(z_LocalPref<T> pref)
		{
			if(pref != null)
				return pref._value;
			return default(T);
		}
	}
}

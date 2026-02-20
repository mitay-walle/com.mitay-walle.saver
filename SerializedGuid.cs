using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saving
{
	[Serializable, InlineProperty]
	public struct SerializedGuid : IEquatable<SerializedGuid>, ISerializationCallbackReceiver
	{
		[SerializeField, HideInInspector] string _value;

		public SerializedGuid(Guid guid)
		{
			Guid = guid;
			_value = Guid.ToString();
		}

		[ShowInInspector, HideLabel] public Guid Guid { get; set; }

		public bool Equals(SerializedGuid other) => Guid.Equals(other.Guid);

		public static SerializedGuid NewGuid() => new() { Guid = Guid.NewGuid() };

		public override string ToString() => Guid.ToString();
		public override int GetHashCode() => Guid.GetHashCode();
		public void OnBeforeSerialize() => _value = Guid.ToString();

		public void OnAfterDeserialize()
		{
			if (Guid.TryParse(_value, out Guid temp))
			{
				Guid = temp;
			}
			else
			{
				Debug.LogError($"can't deserialize Guid {_value}");
			}
		}

		public static implicit operator Guid(SerializedGuid value) => value.Guid;
	}
}
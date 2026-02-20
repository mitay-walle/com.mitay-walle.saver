using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saving
{
	[Serializable, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, IsReadOnly = true)]
	public class GuidToSaveDataDictionary : SerializedReferenceDictionary<SerializedGuid, ISaveData, string, ISaveData>, ISaveData
	{
		[field: SerializeField, PropertyOrder(-1)] public SerializedGuid Guid { get; set; } = SerializedGuid.NewGuid();

		public override string SerializeKey(SerializedGuid key) => key.Guid.ToString();
		public override ISaveData SerializeValue(ISaveData value) => value;
		public override ISaveData DeserializeValue(ISaveData serializedValue) => serializedValue;

		public override SerializedGuid DeserializeKey(string serializedKey)
		{
			if (System.Guid.TryParse(serializedKey, out Guid temp))
			{
				return new SerializedGuid { Guid = temp };
			}

			Debug.LogError($"can't deserialize Guid {serializedKey}");
			return default;
		}

		public string ToDisplayString() => $"[{GetType().Name}]" + '\n' + JsonUtility.ToJson(this, true);
		public ISaveable SpawnSceneObject() => null;
	}
}
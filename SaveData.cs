using UnityEngine;

namespace Saving
{
	public abstract class SaveData : ISaveData
	{
		[field: SerializeField] public SerializedGuid Guid { get; set; } = SerializedGuid.NewGuid();
		public string ToDisplayString() => $"[{GetType().Name}]" + '\n' + JsonUtility.ToJson(this, true);
		public abstract ISaveable SpawnSceneObject();
	}
}
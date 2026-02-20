using UnityEngine;

namespace Saving
{
	public interface ISaveable
	{
		GameObject gameObject { get; }
		eFile File { get; }
		ISaveData Data { get; }
		ISaveData Save();
		void Load(ISaveData data);
		string ToDisplayString();
	}
}
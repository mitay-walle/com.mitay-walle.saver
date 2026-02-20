namespace Saving
{
	public interface ISaveData
	{
		SerializedGuid Guid { get; set; }

		string ToDisplayString();

		ISaveable SpawnSceneObject();
	}
}
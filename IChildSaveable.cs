namespace Saving
{
	public interface IChildSaveable
	{
		ISaveData Data { get; }
		ISaveData Save();
		void Load(ISaveData data);
	}
}
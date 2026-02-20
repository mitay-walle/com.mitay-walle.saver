using System;

namespace Saving
{
	[Serializable]
	public class SaveFileDictionary : SerializedReferenceDictionary<eFile, SaveFile> { }
}
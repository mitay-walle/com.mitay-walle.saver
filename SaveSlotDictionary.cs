using System;

namespace Saving
{
	[Serializable]
	public class SaveSlotDictionary : SerializedReferenceDictionary<eSlot, SaveSlot> { }
}
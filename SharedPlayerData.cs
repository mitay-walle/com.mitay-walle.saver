using System;
using System.Collections.Generic;
using Craft;
using Entities.Containers;
using Quests;

namespace Saving
{
	[Serializable]
	public class SharedPlayerData : SaveData
	{
		public eSlot LastSlot;
		public int SceneBuildIndex;
		public TransformSaveData Transform;
		public Container Inventory = new();
		public QuestSaveData QuestSaveData = new();
		public ReceiptsSaveData ReceiptsSaveData = new();
		public override ISaveable SpawnSceneObject() => null;

		public List<string> VisitedScenes = new();
	}
}
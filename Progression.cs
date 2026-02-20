using System;
using System.Collections.Generic;

namespace Saving
{
	[Serializable]
	public class Progression
	{
		public static Progression Instance { get; private set; }

		public void Initialize()
		{
			Instance = new Progression();
		}

		public void Load(Progression fromSave)
		{
			Instance = fromSave;
		}
	}

	public enum eQuestId
	{
		FixCar,
		OldMan,
	}
}
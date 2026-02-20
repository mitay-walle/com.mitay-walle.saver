using System;
using System.IO;
using UnityEngine;

namespace Saving
{
	[Serializable]
	public class SaveSlot
	{
		[field: SerializeField] public string Folder = "Autosave";

		public static bool FileNeedFolder(eFile file) => file != eFile.Settings;

		public void CreateFolder()
		{
			Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, Folder));
		}
	}
}
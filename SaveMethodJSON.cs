using System;
using System.IO;
using UnityEngine;

namespace Saving
{
	[Serializable]
	public class SaveMethodJSON : SaveFile
	{
		[SerializeField] bool _isPrettyPrint = true;

		public SaveMethodJSON(string fileName = "")
		{
			FileName = fileName;
		}

		public override void Save()
		{
			string contents = JsonUtility.ToJson(Saved, _isPrettyPrint);
			if (Saver.LOGS) Debug.Log($"Save Json to file: '{FilePath}'"); //\n\n{contents}");
			File.WriteAllText(FilePath, contents);
		}

		public override void Load()
		{
			if (File.Exists(FilePath))
			{
				string contents = File.ReadAllText(FilePath);

				JsonUtility.FromJsonOverwrite(contents, Saved);
				if (Saver.LOGS)
					Debug.Log(
						$"Load Json from file: '{FilePath}'\n\nfile content:\n{contents}\n\ndeserialize result:\n\n{JsonUtility.ToJson(Saved)}");

				if (Saver.LOGS) Debug.Log($"file loaded count {Saved.Count}");
			}
		}
	}
}
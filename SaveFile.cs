using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saving
{
	[Serializable, HideReferenceObjectPicker, InlineProperty]
	public abstract class SaveFile : SaveMethod
	{
		public string FileName = "Save.save";

		[NonSerialized, ShowInInspector, ReadOnly] public string Folder;
		[ShowInInspector] public bool IsSaveExist() => File.Exists(FilePath);

		[ShowInInspector, VerticalGroup(1)] protected string FilePath
		{
			get
			{
				string path = Application.persistentDataPath;
				if (!string.IsNullOrEmpty(Folder))
				{
					path = Path.Combine(path, Folder);
				}

				return Path.GetFullPath(FileName, path);
			}
		}

		public override void Delete() => File.Delete(FilePath);
		[Button, VerticalGroup(2)] public void Open() => System.Diagnostics.Process.Start(FilePath);
		[Button, VerticalGroup(2)] public void OpenFolder() => System.Diagnostics.Process.Start(Path.GetDirectoryName(FilePath));
	}
}
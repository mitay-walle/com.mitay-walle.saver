using System;
using System.Collections.Generic;
using System.Linq;
using Components.InteractableObjects;
using GameLoop;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Saving
{
	[CreateAssetMenu]
	public class Saver : ScriptableObject
	{
		public const bool LOGS = true;

		private static eFile[] allFiles = { eFile.Scene, eFile.Shared, eFile.Settings };
		private static eSlot[] allSlots = { eSlot.Auto };

		public static Saver Load() => Resources.Load<Saver>("Saver");
		[SerializeField] bool _isEnabled = true;
		[NonSerialized] public eSlot CurrentSlot;
		[NonSerialized, ShowInInspector] SaveFileDictionary _files = new()
		{
			{ eFile.Settings, new SaveMethodJSON("Settings.save") },
			{ eFile.Shared, new SaveMethodJSON("Shared.save") },
			{ eFile.Scene, new SaveMethodJSON("AnyScene.save") },
		};
		[NonSerialized] SaveSlotDictionary _slots = new()
		{
			{
				eSlot.Auto, new SaveSlot()
			}
		};

		public bool IsSaveExist(eFile file, eSlot? slot = null)
		{
			slot ??= CurrentSlot;
			SetCurrentSlot(slot.Value);
			return _files[file].IsSaveExist();
		}

		public static void ValidateSavingScene()
		{
			IEnumerable<ISaveable> saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
				.OfType<ISaveable>();

			HashSet<Guid> found = new();

			foreach (ISaveable saveable in saveables)
			{
				if (found.Contains(saveable.Data.Guid))
				{
					saveable.Data.Guid = SerializedGuid.NewGuid();
#if UNITY_EDITOR
					EditorUtility.SetDirty(saveable as MonoBehaviour);
#endif
				}

				found.Add(saveable.Data.Guid);
			}
		}

		public SharedPlayerData GetSharedData()
		{
			if (!IsSaveExist(eFile.Shared))
			{
				return null;
			}

			ISaveData found = _files[eFile.Shared].Saved.Values.FirstOrDefault(d => d is SharedPlayerData);
			return (SharedPlayerData)found;
		}

		public void SaveSettings() => Save(eFile.Settings, eSlot.Auto);
		public void SaveCurrentSlot() => Save(eFile.Scene | eFile.Shared, CurrentSlot);

		public void TryLoadShared()
		{
			Load(eFile.Shared);
		}

		public bool LoadSettings()
		{
			if (!_files[eFile.Settings].IsSaveExist())
			{
				if (LOGS) Debug.Log($"No settings file exists. Creating new one");
				SaveSettings();
			}

			return Load(eFile.Settings, eSlot.Auto);
		}

		public bool LoadCurrentSlot() => Load(eFile.Scene | eFile.Shared);

		void Save(eFile files, eSlot slot)
		{
			if (!_isEnabled) return;

			SetCurrentSlot(slot);
			if (LOGS) Debug.Log($"Save {CurrentSlot}");

			IEnumerable<ISaveable> saveable = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
				.OfType<ISaveable>().Where(s => files.HasFlag(s.File));

			foreach (eFile f in allFiles)
			{
				if (files.HasFlag(f))
				{
					_files[f].Saved.Clear();
				}
			}

			foreach (ISaveable s in saveable)
			{
				ISaveData saved = s.Save();
				_files[s.File].Saved[saved.Guid] = saved;
			}

			foreach (eFile f in allFiles)
			{
				if (files.HasFlag(f))
				{
					if (f == eFile.Scene)
					{
						_files[f].FileName = GameSceneManager.GetActiveScene().name + ".save";
					}

					_files[f].Save();
				}
			}
		}

		bool Load(eFile files, eSlot? slot = null)
		{
			if (!_isEnabled) return false;

			slot ??= CurrentSlot;
			SetCurrentSlot(slot.Value);

			SaveFile sceneFile = _files[eFile.Scene];
			sceneFile.FileName = GameSceneManager.GetActiveScene().name + ".save";

			bool anyExists = allFiles.Any(f => files.HasFlag(f) && _files[f].IsSaveExist());
			if (!anyExists)
			{
				if (LOGS) Debug.LogWarning($"[LOAD] Slot {CurrentSlot}: загрузка невозможна, файл не найден.");
				return false;
			}

			Dictionary<SerializedGuid, ISaveable> sceneObjects =
				FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>()
					.Where(s => files.HasFlag(s.File)).ToDictionary(s => s.Data.Guid);

			//---------------------------------------------------------
			// ЭТАП 3: Загрузка файлов (_files[f].Saved заполняются)
			//---------------------------------------------------------
			if (LOGS) Debug.Log($"[LOAD-3] Чтение файлов сохранений");

			if (LOGS) Debug.Log($"scene loaded count {sceneFile.Saved.Count}");

			foreach (eFile f in allFiles)
				if (files.HasFlag(f))
					_files[f].Saved.Clear();

			foreach (ISaveable s in sceneObjects.Values)
				_files[s.File].Saved[s.Data.Guid] = s.Data;

			foreach (eFile f in allFiles)
				if (files.HasFlag(f))
					_files[f].Load();

			
			sceneFile.Load();
			
			if (LOGS) Debug.Log($"scene loaded count {sceneFile.Saved.Count}");

			//---------------------------------------------------------
			// ЭТАП 4: Спавн отсутствующих объектов
			//---------------------------------------------------------

			if (files.HasFlag(eFile.Scene))
			{
				if (LOGS) Debug.Log($"[LOAD-4] Удаление объектов отсутствующих в сохранении");

				List<ISaveable> toRemove = sceneObjects.Values.Where(o => o.File.HasFlag(eFile.Scene) && !sceneFile.Saved.ContainsKey(o.Data.Guid))
					.ToList();

				foreach (ISaveable obj in toRemove)
				{
					if (LOGS) Debug.Log($"[LOAD-REMOVE] Удаление объекта GUID={obj.Data.Guid} ({obj})");
					Destroy(obj.gameObject);
					sceneObjects.Remove(obj.Data.Guid);
				}

				if (LOGS) Debug.Log($"[LOAD-5] Спавн объектов, которых нет в сцене");

				foreach (KeyValuePair<SerializedGuid, ISaveData> kvp in sceneFile.Saved)
				{
					SerializedGuid guid = kvp.Key;
					ISaveData savedData = kvp.Value;

					if (!sceneObjects.ContainsKey(guid))
					{
						if (LOGS) Debug.LogWarning($"Spawn! not found in scene {sceneObjects.ContainsKey(guid)} {savedData.ToDisplayString()}");
						ISaveable spawned = savedData.SpawnSceneObject();
						if (savedData is ItemSave itemSave)
						{
							if (LOGS) Debug.Log($"found {itemSave.ToDisplayString()}");
						}

						if (spawned != null)
						{
							sceneObjects[guid] = spawned;
							if (LOGS) Debug.Log($"[LOAD-SPAWN] Спавнен объект GUID={guid} ({spawned})");
						}
					}
					else
					{
						if (LOGS) Debug.Log($"found in scene {sceneObjects.ContainsKey(guid)} {savedData.ToDisplayString()}");
					}
				}
			}

			//---------------------------------------------------------
			// ЭТАП 6: Применение сохранённых данных
			//---------------------------------------------------------
			if (LOGS) Debug.Log($"[LOAD-6] Применение Load() к объектам");

			foreach (eFile f in allFiles)
			{
				if (!files.HasFlag(f)) continue;

				foreach (KeyValuePair<SerializedGuid, ISaveData> kvp in _files[f].Saved)
				{
					if (sceneObjects.TryGetValue(kvp.Key, out ISaveable instance))
					{
						instance.Load(kvp.Value);
					}
				}
			}

			return true;
		}

		public void ClearSlot(eSlot slot)
		{
			SetCurrentSlot(slot);
			foreach (SaveFile file in _files.Values)
			{
				file.Delete();
			}
		}

		public void ClearAllSlots()
		{
			foreach (eSlot slot in allSlots)
			{
				SetCurrentSlot(slot);
				foreach (KeyValuePair<eFile, SaveFile> kvp in _files)
				{
					if (kvp.Key != eFile.Settings)
					{
						kvp.Value.Delete();
					}
				}
			}
		}

		void SetCurrentSlot(eSlot s)
		{
			CurrentSlot = s;

			_slots[s].CreateFolder();

			foreach (KeyValuePair<eFile, SaveFile> kpv in _files)
			{
				kpv.Value.Folder = SaveSlot.FileNeedFolder(kpv.Key) ? _slots[CurrentSlot].Folder : string.Empty;
			}
		}
	}

	// [Serializable]
	// public class SaveMethodBinary : SaveMethodToFile
	// {
	// 	[SerializeField] private bool _isPrettyPrint = true;
	// 	public override void Save(GuidToSaveDataDictionary data)
	// 	{
	// 		BinaryFormatter formater = new BinaryFormatter();
	// 		formater.AssemblyFormat = FormatterAssemblyStyle.Full;
	// 		MemoryStream mstream = new MemoryStream();
	// 		formater.Serialize(mstream, data);
	// 		File.WriteAllBytes(FilePath, mstream.ToArray());
	// 	}
	// 	public override void Load(ref GuidToSaveDataDictionary data)
	// 	{
	// 		var mstream = new FileStream(FilePath, FileMode.Open);
	// 		BinaryFormatter formater = new BinaryFormatter();
	// 		formater.AssemblyFormat = FormatterAssemblyStyle.Full;
	// 		data = formater.Deserialize(mstream) as GuidToSaveDataDictionary;
	// 	}
	// }
}
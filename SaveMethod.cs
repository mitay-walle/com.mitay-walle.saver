using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saving
{
	[Serializable]
	public abstract class SaveMethod
	{
		public abstract void Save();
		public abstract void Load();
		[Button, VerticalGroup(2)] public abstract void Delete();
		[SerializeReference, NonSerialized, ShowInInspector, HideReferenceObjectPicker]
		public GuidToSaveDataDictionary Saved = new();
	}
}
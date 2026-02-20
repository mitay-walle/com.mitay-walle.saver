using System;
using Plugins.Mathem;
using Plugins.TransformOperations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saving
{
	[Serializable, HideReferenceObjectPicker]
	public struct TransformSaveData
	{
		public static TransformSaveData Default = new() { localScale = Vector3.one };
		public string ToDisplayString() => $"[{GetType().Name}]" + '\n' + JsonUtility.ToJson(this, true);

		public Vector3 position;
		public Quaternion rotation;
		public Vector3 localScale;

		public TransformSaveData(Transform transform)
		{
			position = transform.position.Round(2);
			rotation = transform.rotation.Round(1);
			localScale = transform.localScale.Round(3);
		}

		public void Load(Transform transform, bool logs = false)
		{
			Rigidbody rb = transform.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.position = position;
				rb.rotation = rotation == default ? Quaternion.identity : rotation;
				if (logs)
				{
					Debug.Log($"{transform.GetPathRecursive()} {transform.ToCompactString()} {ToDisplayString()}");
				}
			}
			else
			{
				transform.SetPositionAndRotation(position, rotation);
			}

			transform.localScale = localScale;
		}
	}
}
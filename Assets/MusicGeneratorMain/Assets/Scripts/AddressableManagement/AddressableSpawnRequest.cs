using System;
using UnityEngine;

namespace ProcGenMusic
{
	/// <summary>
	/// Holds data pertaining to a request to spawn an addressable.
	/// </summary>
	public class AddressableSpawnRequest
	{
		public AddressableSpawnRequest(Vector3 position, Quaternion rotation, Action<GameObject> onComplete, Transform parent = null)
		{
			Position = position;
			Rotation = rotation;
			Parent = parent;
			OnComplete = onComplete;
		}

		public readonly Vector3 Position;
		public readonly Quaternion Rotation;
		public readonly Transform Parent;
		/// <summary>
		/// Will be invoked by the Addressable manager once the gameObject has instantiated
		/// </summary>
		public readonly Action<GameObject> OnComplete;
	}
}

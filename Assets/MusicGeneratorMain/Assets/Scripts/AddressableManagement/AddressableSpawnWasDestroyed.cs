using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProcGenMusic
{
	public class AddressableSpawnWasDestroyed : MonoBehaviour
	{
		public event Action<AssetReference, AddressableSpawnWasDestroyed> Destroyed;
		public AssetReference AssetReference { get; set; }

		public void OnDestroy()
		{
			Destroyed?.Invoke(AssetReference, this);
		}
	}
}

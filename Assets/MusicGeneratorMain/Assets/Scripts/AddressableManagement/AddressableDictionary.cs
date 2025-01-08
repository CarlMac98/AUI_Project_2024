using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProcGenMusic
{
	[Serializable]
	public class AddressableEntry
	{
		public string Key => mKey;
		public AssetReference Value => mValue;

		public AddressableEntry( string key, AssetReference value )
		{
			mKey = key;
			mValue = value;
		}

		[SerializeField] private string mKey = null;
		[SerializeField] private AssetReference mValue = null;
	}

	[CreateAssetMenu( fileName = "AddressableDictionary", menuName = "ProcGenMusic/Addressables/AddressableDictionary", order = 1 )]
	public class AddressableDictionary : ScriptableObject
	{
		public Dictionary<string, AssetReference> Addressables => mAddressableDictionary;

		[SerializeField] private List<AddressableEntry> mAssetReferences = null;

		private Dictionary<string, AssetReference> mAddressableDictionary = new Dictionary<string, AssetReference>();

		public void Initialize()
		{
			foreach ( var entry in mAssetReferences )
			{
				mAddressableDictionary.Add( entry.Key, entry.Value );
			}
		}

		public void AddAssetReference( AddressableEntry entry )
		{
			for ( var index = 0; index < mAssetReferences.Count; index++ )
			{
				if ( mAssetReferences[index].Key == entry.Key )
				{
					mAssetReferences[index] = entry;
					Debug.LogError( "Addressable Dictionary already contains an entry with this key. Overwriting" );
					return;
				}
			}

			if ( mAssetReferences.Contains( entry ) == false )
			{
				mAssetReferences.Add( entry );
			}
		}
	}
}

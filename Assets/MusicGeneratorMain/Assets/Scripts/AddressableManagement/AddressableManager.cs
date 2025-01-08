using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Loads and manages spawned addressables.
	/// Workflow:
	/// 1: Create AddressableDictionary of (string, AssetReference)
	/// 2: Inject dictionary into this manager via the editor.
	/// 3: spawn using assetReference name
	/// 4: this should not be destroyed. Is managing memory
	/// </summary>
	public class AddressableManager : MonoBehaviour
	{
		public void Initialize()
		{
			mAssetReferences.Initialize();
		}

		public void SpawnAddressableInstance( AssetReference assetReference, AddressableSpawnRequest spawnRequest )
		{
			SpawnFromAssetReference( assetReference, spawnRequest );
		}

		/// <summary>
		/// Spawns an instance of an addressable object
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="spawnRequest"></param>
		public void SpawnAddressableInstance( string assetName, AddressableSpawnRequest spawnRequest )
		{
			AssetReference assetReference;
			if ( mAssetReferences.Addressables.TryGetValue( assetName, out var reference ) )
			{
				assetReference = reference;
			}
			else
			{
				return;
			}

			if ( assetReference.RuntimeKeyIsValid() == false )
			{
				Debug.LogWarning( $"Unable to spawn asset. RuntimeKey = {assetReference.RuntimeKey}" );
				return;
			}

			SpawnFromAssetReference( assetReference, spawnRequest );
		}

		private void SpawnFromAssetReference( AssetReference assetReference, AddressableSpawnRequest spawnRequest )
		{
			if ( mAsyncOperationhandles.TryGetValue( assetReference, out var handle ) )
			{
				// if we've already finished loading an instance of this addressable, simply spawn a new instance
				if ( handle.IsDone )
				{
					SpawnFromLoadedReference( assetReference, spawnRequest );
				}
				else //< otherwise, if we have a handle but it isn't finished, enqueue the request to spawn when done loading.
				{
					EnqueueSpawn( assetReference, spawnRequest );
				}
			}
			else //< otherwise we need to both load and spawn the asset
			{
				LoadAndSpawn( assetReference, spawnRequest );
			}
		}

		[SerializeField, Tooltip( "Reference to our addressable AssetReferences" )]
		private AddressableDictionary mAssetReferences;

		/// <summary>
		/// Dictionary containing our spawned assets (used for releasing addressable)
		/// </summary>
		/// <returns></returns>
		private readonly Dictionary<AssetReference, List<GameObject>> mSpawnedAssets = new Dictionary<AssetReference, List<GameObject>>();

		/// <summary>
		/// Dictionary containing any queued spawn requests
		/// </summary>
		/// <returns></returns>
		private readonly Dictionary<AssetReference, Queue<AddressableSpawnRequest>> mQueuedSpawnRequests =
			new Dictionary<AssetReference, Queue<AddressableSpawnRequest>>();

		/// <summary>
		/// Dictionary containing our async operation handles
		/// </summary>
		/// <returns></returns>
		private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> mAsyncOperationhandles =
			new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

		private void Awake()
		{
			DontDestroyOnLoad( this );
		}

		/// <summary>
		/// Loads and spawns an instance of an addressable
		/// </summary>
		/// <param name="assetReference"></param>
		/// <param name="spawnRequest"></param>
		private void LoadAndSpawn( AssetReference assetReference, AddressableSpawnRequest spawnRequest )
		{
			if ( assetReference == null )
			{
				Debug.LogError( "Please run the PMG setup scripts as outlined in the MusicGenerator/Documentation/Procedural Music Generator Documentation.pdf" );
				spawnRequest.OnComplete?.Invoke( null );
				return;
			}

			var handle = Addressables.LoadAssetAsync<GameObject>( assetReference );

			if ( handle.IsValid() == false )
			{
				Debug.LogError( "Please run the PMG setup scripts as outlined in the MusicGenerator/Documentation/Procedural Music Generator Documentation.pdf" );
				spawnRequest.OnComplete?.Invoke( null );
				return;
			}

			mAsyncOperationhandles[assetReference] = handle;
			handle.Completed += ( operation ) =>
			{
				if ( this == null )
				{
					return;
				}

				SpawnFromLoadedReference( assetReference, spawnRequest );
				if ( mQueuedSpawnRequests.TryGetValue( assetReference, out var queuedRequests ) )
				{
					while ( queuedRequests.Count > 0 )
					{
						var request = queuedRequests.Dequeue();
						SpawnFromLoadedReference( assetReference, request );
					}
				}
			};
		}

		/// <summary>
		/// Enqueues a spawn request to complete when the async asset load request is completed
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="spawnRequest"></param>
		private void EnqueueSpawn( AssetReference reference, AddressableSpawnRequest spawnRequest )
		{
			if ( mQueuedSpawnRequests.TryGetValue( reference, out var queuedRequests ) )
			{
				queuedRequests.Enqueue( spawnRequest );
			}
			else
			{
				var newQueue = new Queue<AddressableSpawnRequest>();
				newQueue.Enqueue( spawnRequest );
				mQueuedSpawnRequests.Add( reference, newQueue );
			}
		}

		/// <summary>
		/// Spawns an instance of a game object from a loaded assetReference
		/// </summary>
		/// <param name="assetReference"></param>
		/// <param name="spawnRequest"></param>
		private void SpawnFromLoadedReference( AssetReference assetReference, AddressableSpawnRequest spawnRequest )
		{
			assetReference.InstantiateAsync( spawnRequest.Position, spawnRequest.Rotation ).Completed += ( handle ) =>
			{
				if ( this == null )
				{
					if ( handle.Result != null )
					{
						Destroy( handle.Result );
					}

					return;
				}

				if ( mSpawnedAssets.TryGetValue( assetReference, out var spawnedAssets ) == false )
				{
					spawnedAssets = mSpawnedAssets[assetReference] = new List<GameObject>();
				}

				if ( handle.Result != null )
				{
					spawnedAssets.Add( handle.Result );
					handle.Result.transform.SetParent( spawnRequest.Parent );

					//We have to release the instance when the object is destroyed.
					//Rather than have every class manager this, we 'll handle it here.
					var notify = handle.Result.AddComponent<AddressableSpawnWasDestroyed>();
					notify.Destroyed += Remove;
					notify.AssetReference = assetReference;
				}

				spawnRequest.OnComplete?.Invoke( handle.Result );
			};
		}

		/// <summary>
		/// Removes the asset reference from our dictionary of spawned objects and releases the addressable instance.
		/// </summary>
		/// <param name="assetReference"></param>
		/// <param name="obj"></param>
		private void Remove( AssetReference assetReference, AddressableSpawnWasDestroyed obj )
		{
			//Debug.LogError(string.Format("Releasing addressable instance for{0}", obj.gameObject.name));
			Addressables.ReleaseInstance( obj.gameObject );
			if ( mSpawnedAssets.TryGetValue( assetReference, out var spawnedAssets ) )
			{
				spawnedAssets.Remove( obj.gameObject );
				if ( spawnedAssets.Count == 0 )
				{
					if ( mAsyncOperationhandles.TryGetValue( assetReference, out var handle ) )
					{
						if ( handle.IsValid() )
						{
							Addressables.ReleaseInstance( handle );
						}

						//Debug.LogError(string.Format("Releasing addressable async handle for {0}", handle.DebugName));
						mAsyncOperationhandles.Remove( assetReference );
					}
				}
			}
		}
	}
}

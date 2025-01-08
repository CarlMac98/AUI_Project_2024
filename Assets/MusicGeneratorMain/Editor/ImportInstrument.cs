using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

namespace ProcGenMusic
{
	/// <summary>
	/// Imports a new instrument into the project
	/// </summary>
	public class ImportInstrumentWindow : EditorWindow
	{
		private static EditorWindow mWindow;
		private static List<Object> mInstrumentDirectory = new List<Object>( ) { null };
		private static string mInstrumentName;
		private const string PREFAB_BUILD_DIR_NAME = "PMGAudioPrefabs";
		private const string ADDRESSABLES_GROUP_NAME = "PMGAudioAssets";
		private static bool mNeedsIncreasing;

		[MenuItem( "PMG/Import Instrument", isValidateFunction: false, priority: 505 )]
		private static void Initialize( )
		{
			mWindow = GetWindow<ImportInstrumentWindow>( );
			mWindow.Show( );
		}

		private void OnGUI( )
		{
			GUILayout.Label( "Import Instrument" );

			EditorGUI.indentLevel++;

			mInstrumentName = EditorGUILayout.TextField( "Instrument Name: ", mInstrumentName );

			EditorGUILayout.HelpBox(
				"Please see documentation. Drag the asset folder to the window",
				MessageType.Info );

			for ( var index = 0; index < mInstrumentDirectory.Count; index++ )
			{
				mInstrumentDirectory[index] =
					EditorGUILayout.ObjectField( "Instrument Directory: ", mInstrumentDirectory[index], typeof( Object ), true );
			}

			var hasPath = false;
			mNeedsIncreasing = true;
			for ( var index = 0; index < mInstrumentDirectory.Count; index++ )
			{
				if ( mInstrumentDirectory[index] != null )
				{
					if ( Directory.Exists( AssetDatabase.GetAssetPath( mInstrumentDirectory[index] ) ) )
					{
						hasPath = true;
						continue;
					}

					mInstrumentDirectory[index] = null;
				}
				else
				{
					mNeedsIncreasing = false;
				}
			}


			if ( hasPath == false || string.IsNullOrEmpty( mInstrumentName ) )
			{
				return;
			}

			if ( GUILayout.Button( "Import instrument!", GUILayout.Height( 40 ) ) )
			{
				if ( EditorUtility.DisplayDialog(
					    title: "Import Instrument",
					    message:
					    $"This process will require saving your current scene, is that okay?",
					    ok: "Great, do it",
					    cancel: "On second thought..." ) )
				{
					ImportInstrument( );
				}
			}
		}

		private void Update( )
		{
			if ( mNeedsIncreasing )
			{
				mInstrumentDirectory.Add( null );
				mNeedsIncreasing = false;
			}
		}

		/// <summary>
		/// Imports our instrument, creating prefab and addressable and registering with our data classes.
		/// </summary>
		/// <param name="path"></param>
		private static void ImportInstrument( )
		{
			var prefabDirectory = FindDirectory( Application.dataPath, PREFAB_BUILD_DIR_NAME );
			if ( prefabDirectory != null )
			{
				var instrumentPrefabObject = new GameObject( mInstrumentName );
				var instrumentName = instrumentPrefabObject.name;
				var instrumentAudio = instrumentPrefabObject.AddComponent<InstrumentAudio>( );

				foreach ( var directory in mInstrumentDirectory )
				{
					if ( directory == null )
					{
						continue;
					}

					instrumentAudio.AddInstrument( );
					var path = AssetDatabase.GetAssetPath( directory );
					var audioAssets = AssetDatabase.FindAssets( "t: AudioClip", searchInFolders: new string[] { path } );
					var audioClips = audioAssets.Select( AssetDatabase.GUIDToAssetPath ).Select( audioAssetPath =>
						AssetDatabase.LoadAssetAtPath( audioAssetPath, ( typeof( AudioClip ) ) ) as AudioClip ).ToList( );

					instrumentAudio.Instruments[instrumentAudio.Instruments.Count - 1].SetAudioClips( audioClips );
				}


				var relativePrefabDirectory = MusicConstants.GetRelativePath( prefabDirectory );
				var uniqueAssetPath =
					AssetDatabase.GenerateUniqueAssetPath( $"{Path.Combine( relativePrefabDirectory, instrumentName )}.prefab" );
				var audioPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect( instrumentPrefabObject, uniqueAssetPath, InteractionMode.UserAction );

				CreateAddressableEntry( audioPrefab );
				RegisterAddressable( audioPrefab );
				var sceneObjects = GameObject.FindObjectsOfType<InstrumentAudio>( );
				foreach ( var sceneObject in sceneObjects )
				{
					if ( sceneObject.name == instrumentName )
					{
						DestroyImmediate( sceneObject.gameObject, true );
						break;
					}
				}

				AssetDatabase.SaveAssets( );
				EditorSceneManager.MarkSceneDirty( EditorSceneManager.GetActiveScene( ) );
				EditorSceneManager.SaveOpenScenes( );
				Debug.Log( $"PMG Instrument Import Complete for {instrumentName}" );
			}
			else
			{
				Debug.LogError( $"Unable to find prefab directory {PREFAB_BUILD_DIR_NAME}" );
			}
		}

		/// <summary>
		/// Registers our addressables with our asset dictionary and instrument list.
		/// </summary>
		/// <param name="instrumentPrefab"></param>
		private static void RegisterAddressable( GameObject instrumentPrefab )
		{
			var addressableDictionaryGuid = AssetDatabase.FindAssets( MusicConstants.PMGAddressableDictionaryName );
			if ( addressableDictionaryGuid == null || addressableDictionaryGuid.Length <= 0 )
			{
				Debug.LogError( $"Cannot find {MusicConstants.PMGAddressableDictionaryName}, please ensure all PMG assets have been imported" );
			}

			var addressableDictionary =
				AssetDatabase.LoadAssetAtPath<AddressableDictionary>( AssetDatabase.GUIDToAssetPath( addressableDictionaryGuid?[0] ) );
			if ( addressableDictionary == null )
			{
				Debug.LogError( $"Cannot find {MusicConstants.PMGAddressableDictionaryName}, please ensure all PMG assets have been imported" );
			}

			AssetDatabase.TryGetGUIDAndLocalFileIdentifier( instrumentPrefab, out string guid, out long localID );
			addressableDictionary.AddAssetReference( new AddressableEntry( instrumentPrefab.name, new AssetReference( guid ) ) );
			EditorUtility.SetDirty( addressableDictionary );

			var musicGeneratorGameObjectGuid = AssetDatabase.FindAssets( MusicConstants.PMGGeneratorPrefabName );
			if ( musicGeneratorGameObjectGuid == null || musicGeneratorGameObjectGuid.Length <= 0 )
			{
				Debug.LogError( $"Cannot find {MusicConstants.PMGGeneratorPrefabName}, please ensure all PMG assets have been imported" );
			}

			var pmgMusicGeneratorPrefab =
				AssetDatabase.LoadAssetAtPath<GameObject>( AssetDatabase.GUIDToAssetPath( musicGeneratorGameObjectGuid?[0] ) );
			if ( pmgMusicGeneratorPrefab == null )
			{
				Debug.LogError( $"Cannot find {MusicConstants.PMGGeneratorPrefabName}, please ensure all PMG assets have been imported" );
			}

			var musicGenerator = pmgMusicGeneratorPrefab.GetComponent<MusicGenerator>( );
			if ( musicGenerator.BaseInstrumentPaths.Contains( instrumentPrefab.name ) == false )
			{
				musicGenerator.BaseInstrumentPaths.Add( instrumentPrefab.name );
			}

			musicGenerator.BaseInstrumentPaths.Sort( );
			EditorUtility.SetDirty( instrumentPrefab );
			EditorUtility.SetDirty( pmgMusicGeneratorPrefab );
			EditorUtility.SetDirty( musicGenerator );
			AssetDatabase.SaveAssets( );

			if ( EditorUtility.DisplayDialog(
				    title: "Build Addressables?",
				    message: $"If this is your only instrument, build addressables now, otherwise build when you're finished importing instruments",
				    ok: "Build Addressables",
				    cancel: "I have more to import" ) )
			{
				PMGSetup.FirstTimeAddressableSetup( );
			}

			mInstrumentName = string.Empty;
			for ( int index = 0; index < mInstrumentDirectory.Count; index++ )
			{
				mInstrumentDirectory[index] = null;
			}

			mInstrumentDirectory.Clear( );

			if ( mWindow != null )
			{
				mWindow.Repaint( );
			}
		}

		/// <summary>
		/// Adds our entry to our addressable group
		/// </summary>
		/// <param name="instrumentPrefab"></param>
		private static void CreateAddressableEntry( Object instrumentPrefab )
		{
			AssetDatabase.TryGetGUIDAndLocalFileIdentifier( instrumentPrefab, out var guid, out long localID );

			var pmgAddressableSettingsGuid = AssetDatabase.FindAssets( MusicConstants.PMGAddressableName );
			if ( pmgAddressableSettingsGuid == null || pmgAddressableSettingsGuid.Length <= 0 )
			{
				Debug.LogError( $"Cannot find {MusicConstants.PMGAddressableName}, please ensure all PMG assets have been imported" );
			}

			var pmgSettings =
				AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>( AssetDatabase.GUIDToAssetPath( pmgAddressableSettingsGuid?[0] ) );

			var entriesAdded = new List<AddressableAssetEntry>( );
			var entry = pmgSettings.CreateOrMoveEntry( guid, pmgSettings.FindGroup( ADDRESSABLES_GROUP_NAME ), readOnly: false, postEvent: false );
			entry.address = instrumentPrefab.name;
			entry.labels.Add( "Custom User Entry" );
			entriesAdded.Add( entry );
			pmgSettings.SetDirty( AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true );
		}

		/// <summary>
		/// Finds a directory recursively
		/// </summary>
		/// <param name="parentDirectory"></param>
		/// <param name="targetDirectoryName"></param>
		/// <returns></returns>
		private static string FindDirectory( string parentDirectory, string targetDirectoryName )
		{
			var directoryInfo = new DirectoryInfo( parentDirectory );
			var directories = directoryInfo.EnumerateDirectories( );

			foreach ( var dirInfo in directories )
			{
				if ( dirInfo.Name.Equals( targetDirectoryName ) )
				{
					return dirInfo.FullName;
				}

				var children = FindDirectory( dirInfo.FullName, targetDirectoryName );
				if ( children != null )
				{
					return children;
				}
			}

			return null;
		}
	}
}
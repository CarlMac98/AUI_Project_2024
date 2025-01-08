using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.VersionControl;
using UnityEngine;

namespace ProcGenMusic
{
	/// <summary>
	/// This generally shouldn't be necessary, but if you need to build addressables for a specific target and your build script isn't automatically creating them.
	/// </summary>
	public class BuildPreparationWindow : EditorWindow
	{
		private BuildTarget mBuildTarget = BuildTarget.NoTarget;
		private static EditorWindow mWindow;

		[MenuItem( "PMG/Prepare Build", isValidateFunction: false, priority: 204 )]
		private static void Initialize( )
		{
			mWindow = GetWindow<BuildPreparationWindow>( );
			mWindow.Show( );
		}

		private void OnGUI( )
		{
			GUILayout.Label( "Build Preparation" );

			EditorGUI.indentLevel++;

			EditorGUILayout.HelpBox(
				"After Selecting a build target, this will build addressables for the target platform",
				MessageType.Info );

			mBuildTarget = ( BuildTarget )EditorGUILayout.EnumPopup( "Build Target", mBuildTarget );

			if ( mBuildTarget != BuildTarget.NoTarget )
			{
				if ( GUILayout.Button( "Prepare the build!" ) )
				{
					BuildPreparation.CopyPersistentData( );
					PMGSetup.BuildAddressables( mBuildTarget );
					Debug.LogError( "PMG Build preparation Complete" );
					mWindow.Close( );
					mWindow = null;
				}
			}
		}
	}

	public class BuildPreparation : MonoBehaviour
	{
		[MenuItem( "PMG/Copy PersistentData", isValidateFunction: false, priority: 102 )]
		public static void CopyPersistentData( )
		{
			var pmgMainPath = MusicConstants.MusicGeneratorPath;

			if ( EditorUtility.DisplayDialog(
				    title: "Copy Persistent Data",
				    message:
				    $"Are you sure? This will copy all persistent data to {Path.Combine( pmgMainPath, "Assets", "PMGBaseConfigFiles" )} and rebuild addressables",
				    ok: "Do it!",
				    cancel: "On second thought..." ) )
			{
				Debug.Log( "So shall it be written, so shall it be done." );
				CopyConfigurations( MusicConstants.ConfigurationPersistentDataPath, MusicConstants.ConfigurationDataPath );
				CopyConfigurations( MusicConstants.PersistentClipsPath, MusicConstants.BaseInstrumentClipsPath );
				CopyConfigurations( MusicConstants.ConfigurationPersistentModDataPath, MusicConstants.BaseModsDataPath );
				PMGSetup.FirstTimeAddressableSetup( );
				AssetDatabase.ReleaseCachedFileHandles( );
				AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );
			}
		}

		private static void CreateAddressableEntry( Object configurationFile )
		{
			AssetDatabase.TryGetGUIDAndLocalFileIdentifier( configurationFile, out var guid, out long localID );

			var pmgAddressableSettingsGuid = AssetDatabase.FindAssets( MusicConstants.PMGAddressableName );
			if ( pmgAddressableSettingsGuid == null || pmgAddressableSettingsGuid.Length <= 0 )
			{
				Debug.LogError( $"Cannot find {MusicConstants.PMGAddressableName}, please ensure all PMG assets have been imported" );
			}

			var pmgSettings =
				AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>( AssetDatabase.GUIDToAssetPath( pmgAddressableSettingsGuid?[0] ) );
			const string ADDRESSABLES_GROUP_NAME = "PMGBaseAssets";
			var entriesAdded = new List<AddressableAssetEntry>( );
			var entry = pmgSettings.CreateOrMoveEntry( guid, pmgSettings.FindGroup( ADDRESSABLES_GROUP_NAME ), readOnly: false, postEvent: false );
			entry.address = configurationFile.name;
			entry.labels.Add( "Custom User Entry" );
			entriesAdded.Add( entry );
			pmgSettings.SetDirty( AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true );
		}

		private static void CopyConfigurations( string sourceDirectory, string targetDirectory )
		{
			if ( Directory.Exists( sourceDirectory ) == false )
			{
				return;
			}

			if ( Directory.Exists( targetDirectory ) == false )
			{
				Directory.CreateDirectory( targetDirectory );
			}

			var files = Directory.GetFiles( sourceDirectory );
			foreach ( var configuration in files )
			{
				var targetPath = Path.Combine( targetDirectory, Path.GetFileName( configuration ) );
				if ( File.Exists( targetPath ) )
				{
					File.Delete( targetPath );
				}

				Debug.Log( $"copying to {targetPath}" );

				try
				{
					File.Copy( configuration, targetPath );
					AssetDatabase.Refresh( );
					Debug.Log( $" {configuration} was moved to {targetPath}" );
				}
				catch ( IOException e )
				{
					Debug.Log( $"Unable to copy file with exception {e}" );
				}
			}
		}

		[MenuItem( "PMG/DELETE PersistentData", isValidateFunction: false, priority: 103 )]
		public static void ClearAllPersistentData( )
		{
			if ( EditorUtility.DisplayDialog(
				    title: "Delete PMG Persistent Data",
				    message: "Are you sure? This will delete all persistent data for the music generator.",
				    ok: "Do it!",
				    cancel: "On second thought..." ) )
			{
				RemoveConfigurations( MusicConstants.ConfigurationPersistentDataPath );
				RemoveConfigurations( MusicConstants.PersistentClipsPath );
				RemoveConfigurations( MusicConstants.ConfigurationPersistentModDataPath );
				AssetDatabase.ReleaseCachedFileHandles( );
				AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );

				Debug.Log( "It is done." );
			}
		}

		private static void RemoveConfigurations( string sourceDirectory )
		{
			if ( Directory.Exists( sourceDirectory ) == false )
			{
				return;
			}

			DirectoryInfo directoryInfo = new DirectoryInfo( sourceDirectory );

			foreach ( var fileInfo in directoryInfo.EnumerateFiles( ) )
			{
				try
				{
					Debug.Log( $"{fileInfo.Name} is being deleted" );
					fileInfo.Delete( );
				}
				catch ( IOException e )
				{
					Debug.Log( $"unable to delete {fileInfo.Name} with exception {e}" );
				}
			}
		}
	}
}
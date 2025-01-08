using UnityEditor.Compilation;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProcGenMusic
{
	// pretty shamelessly helped by https://answers.unity.com/questions/956123/add-and-select-game-view-resolution.html.
	// reference for details if you need to adjust
	// My deepest apologies if you're looking at this file. Here be dragons.
	public static class PMGSetup
	{
		static PMGSetup( )
		{
			mGameViewSizes = typeof( Editor ).Assembly.GetType( "UnityEditor.GameViewSizes" );
			var singleType = typeof( ScriptableSingleton<> ).MakeGenericType( mGameViewSizes );
			var instanceProp = singleType.GetProperty( "instance" );
			mGetGroup = mGameViewSizes.GetMethod( "GetGroup" );
			mGameViewSizesInstance = instanceProp?.GetValue( null, null );
		}

#region public

		[MenuItem( "PMG/FirstTimeSetup/1-Scene Setup", false, priority: 1 )]
		public static void FirstTimeSceneSetup( )
		{
			void OnSceneLoaded( Scene scene, OpenSceneMode mode )
			{
				EditorSceneManager.sceneOpened -= OnSceneLoaded;

				if ( scene.name != cSceneName )
				{
					return;
				}

				if ( EditorUtility.DisplayDialog(
					    title: "PMG Scene Setup",
					    message:
					    $"Please save and backup your work before starting and ensure you agree to import TMP Essentials and any package dependencies when asked." +
					    $"This will adjust the project audio settings for 'Max Virtual Voices' and 'Max Real Voices' as well as the aspect ratio for the pmg editor scene.",
					    ok: "Okay",
					    cancel: "On second thought..." ) )
				{
					CreateAspectRatio( );
					UpdateSettings();
					SceneRendererManager.LoadPMGRenderers( );
					EditorSceneManager.SaveScene( scene );
				}
			}

			EditorSceneManager.sceneOpened += OnSceneLoaded;
			LoadGeneratorEditorScene( );
		}

		[MenuItem( "PMG/Utility/Set PMG AudioSettings", isValidateFunction: false, priority: 102 )]
		public static void UpdateSettings()
		{
			var audioSettings = AudioSettings.GetConfiguration();
			audioSettings.numRealVoices = 128;
			audioSettings.numVirtualVoices = 2048;
			AudioSettings.Reset( audioSettings );
		}
		
		[MenuItem( "PMG/FirstTimeSetup/2-Addressable Setup", false, priority: 1 )]
		public static void FirstTimeAddressableSetup( )
		{
			BuildAddressables( EditorUserBuildSettings.activeBuildTarget );
			SceneRendererManager.LoadPMGRenderers( );
		}

		/// <summary>
		/// Builds our PMG Addressables
		/// </summary>
		/// <param name="buildTarget"></param>
		public static void BuildAddressables( BuildTarget buildTarget = BuildTarget.NoTarget )
		{
			if ( EditorUtility.DisplayDialog(
				    title: "Build Addressables",
				    message: $"Please agree to save the scene when asked. This will take some time and will build all pmg addressables for {buildTarget}",
				    ok: "Do it!",
				    cancel: "On second thought..." ) )
			{
				// ensure our directory exists
				var dataPath = Path.Combine( Application.dataPath, "AddressableAssetsData" );
				if ( Directory.Exists( dataPath ) == false )
				{
					Directory.CreateDirectory( dataPath );
				}

				// Cache our settings
				if ( AddressableAssetSettingsDefaultObject.SettingsExists )
				{
					mCachedSettings = AddressableAssetSettingsDefaultObject.Settings;
				}

				BuildFromSettingsAsset( MusicConstants.PMGAddressableName, buildTarget );
			}
		}

#endregion public

#region private

		private static readonly object mGameViewSizesInstance;
		private static readonly MethodInfo mGetGroup;
		private static readonly Type mGameViewSizes;
		private static AddressableAssetSettings mCachedSettings;
		private static bool mIsListening;
		private const string cSceneName = "MusicGeneratorUIEditorScene";

		/// <summary>
		/// Creates an aspect ratio
		/// </summary>
		private static void CreateAspectRatio( )
		{
			var group = mGetGroup.Invoke( mGameViewSizesInstance, new object[] { GameViewSizeGroupType.Standalone } );
			var addCustomSize = mGetGroup.ReturnType.GetMethod( "AddCustomSize" );
			var gvsType = typeof( Editor ).Assembly.GetType( "UnityEditor.GameViewSize" );

			// try to find a 16:9 aspect ratio:
			var desiredSizeIndex = 0;
			switch ( EditorUserBuildSettings.activeBuildTarget )
			{
				case BuildTarget.Android:
					desiredSizeIndex = FindSize( GameViewSizeGroupType.Android, "16:9 Landscape" );
					break;
				case BuildTarget.iOS:
					desiredSizeIndex = FindSize( GameViewSizeGroupType.iOS, "16:9 Landscape" );
					break;
				default:
					desiredSizeIndex = FindSize( GameViewSizeGroupType.Standalone, "16:9" );
					break;
			}

			// If 16:9 exists, we set to that.
			if ( desiredSizeIndex >= 0 )
			{
				SetAspectRatio( desiredSizeIndex );
				return;
			}

			// Check to see if we've already set an aspect ratio
			var customSizeIndex = FindSize( GameViewSizeGroupType.Standalone, "PMG" );
			if ( customSizeIndex > 0 )
			{
				SetAspectRatio( customSizeIndex );
				return;
			}

			/* If all else fails, create a custom aspect ratio. This is a last resort.
			 * This could also be better, and not hard-code to 1920x1080, but leaving as is as an edge case, and I'm tired of 
			 * fiddling with reflection, so slowly backing away from this method here.*/
			var newSize = gvsType.GetConstructors( )[0].Invoke( new object[]
			{
				/* forcing idx 0 here is pretty hacky, but was failing to return correct value from GetConstructor()
				 * with same parameters (enum it wants is internal, and passing int to GetConstructor for this parameter results in returning null)*/
				0, // internal enum GameViewSizeType.AspectRatio
				1920,
				1080,
				"PMG"
			} );

			addCustomSize?.Invoke( @group, new object[] { newSize } );

			var groupType = @group.GetType( );
			var getBuiltinCount = groupType.GetMethod( "GetBuiltinCount" );
			var getCustomCount = groupType.GetMethod( "GetCustomCount" );
			var total = ( int )getBuiltinCount.Invoke( @group, null ) + ( int )getCustomCount.Invoke( @group, null );
			SetAspectRatio( total - 1 );
			mGameViewSizes?.GetMethod( "SaveToHDD" )?.Invoke( mGameViewSizesInstance, null );
		}

		/// <summary>
		/// Finds a window size from available preset sizes
		/// </summary>
		/// <param name="sizeGroupType"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		private static int FindSize( GameViewSizeGroupType sizeGroupType, string text )
		{
			// GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
			// string[] texts = group.GetDisplayTexts();
			// for loop...

			var group = GetGroup( sizeGroupType );
			var getDisplayTexts = group.GetType( ).GetMethod( "GetDisplayTexts" );
			var displayTexts = getDisplayTexts?.Invoke( group, null ) as string[];
			for ( var i = 0; i < displayTexts?.Length; i++ )
			{
				var display = displayTexts[i];
				// the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
				// so if we're querying a custom size text we substring to only get the name
				// You could see the outputs by just logging
				// Debug.Log(display);
				var pren = display.IndexOf( '(' );
				if ( pren != -1 )
					display = display.Substring( 0, pren - 1 ); // -1 to remove the space that's before the prens. This is very implementation-depdenent
				if ( display == text )
					return i;
			}

			return -1;
		}

		private static object GetGroup( GameViewSizeGroupType type )
		{
			return mGetGroup.Invoke( mGameViewSizesInstance, new object[] { ( int )type } );
		}

		/// <summary>
		/// Copies a directory and its contents
		/// </summary>
		/// <param name="sourceDirName"></param>
		/// <param name="destDirName"></param>
		/// <param name="copySubDirs"></param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		private static void DirectoryCopy( string sourceDirName, string destDirName, bool copySubDirs )
		{
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo( sourceDirName );

			if ( !dir.Exists )
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName );
			}

			DirectoryInfo[] dirs = dir.GetDirectories( );

			// If the destination directory doesn't exist, create it.       
			Directory.CreateDirectory( destDirName );

			// Get the files in the directory and copy them to the new location.
			var files = dir.GetFiles( );
			foreach ( var file in files )
			{
				if ( file.Extension.Equals( ".meta" ) )
				{
					continue;
				}

				var tempPath = Path.Combine( destDirName, file.Name );
				file.CopyTo( tempPath, false );
			}

			// If copying subdirectories, copy them and their contents to new location.
			if ( copySubDirs )
			{
				foreach ( var subdir in dirs )
				{
					var tempPath = Path.Combine( destDirName, subdir.Name );
					DirectoryCopy( subdir.FullName, tempPath, copySubDirs );
				}
			}
		}

		/// <summary>
		/// Builds our addressables from an AddressableAssetsSettings file. Will lookup the settings asset.
		/// </summary>
		/// <param name="settingsAssetName"></param>
		/// <param name="buildTarget"></param>
		private static void BuildFromSettingsAsset( string settingsAssetName, BuildTarget buildTarget )
		{
			OverrideAddressableSettings( settingsAssetName );

			// Unused: test of android assets using profiles
			//var androidID = pmgSettings.profileSettings.GetProfileId("PMGAndroid");
			//pmgSettings.activeProfileId = androidID;

			if ( buildTarget != BuildTarget.NoTarget && buildTarget != EditorUserBuildSettings.activeBuildTarget )
			{
				EditorUserBuildSettings.SwitchActiveBuildTarget( BuildTargetGroup.Standalone, buildTarget );
				EditorUserBuildSettings.selectedStandaloneTarget = buildTarget;
			}

			if ( EditorApplication.isCompiling )
			{
				Debug.Log( "Delaying until compilation is finished..." );
				if ( mIsListening == false )
				{
					CompilationPipeline.compilationFinished += BuildContent;
				}

				mIsListening = true;
			}
			else
			{
				BuildContent( );
			}
		}

		/// <summary>
		/// Builds our content
		/// </summary>
		/// <param name="o"></param>
		private static void BuildContent( object o = null )
		{
			if ( mIsListening )
			{
				CompilationPipeline.compilationFinished -= BuildContent;
			}

			AddressableAssetSettings.CleanPlayerContent( );
			AddressableAssetSettings.BuildPlayerContent( );

			RestoreCaches( );

			Debug.Log( "Success!" );
		}

		/// <summary>
		/// Overrides the default addressable settings. This is probably terrible. Sorry. 
		/// </summary>
		/// <param name="settingsAssetName"></param>
		private static void OverrideAddressableSettings( string settingsAssetName )
		{
			var pmgAddressableSettingsGuid = AssetDatabase.FindAssets( settingsAssetName );
			if ( pmgAddressableSettingsGuid == null || pmgAddressableSettingsGuid.Length <= 0 )
			{
				Debug.LogError( $"Cannot find {settingsAssetName}, please ensure all PMG assets have been imported" );
			}

			var pmgSettings =
				AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>( AssetDatabase.GUIDToAssetPath( pmgAddressableSettingsGuid?[0] ) );


			// override our settings:
			AddressableAssetSettingsDefaultObject.Settings = pmgSettings;
		}

		/// <summary>
		/// Restores our caches if they exist.
		/// </summary>
		private static void RestoreCaches( )
		{
			if ( mCachedSettings != null )
			{
				AddressableAssetSettingsDefaultObject.Settings = mCachedSettings;
			}
		}

		/// <summary>
		/// Loads the generator scene
		/// </summary>
		private static void LoadGeneratorEditorScene( )
		{
			EditorSceneManager.SaveOpenScenes( );
			var scenePathGuid = AssetDatabase.FindAssets( cSceneName );
			if ( scenePathGuid?.Length > 0 )
			{
				var scenePath = AssetDatabase.GUIDToAssetPath( scenePathGuid[0] );
				EditorSceneManager.OpenScene( scenePath );
				Debug.Log( "PMG Editor Scene loaded successfully. It's happening!!!!!!!!!!" );
			}
			else
			{
				Debug.LogError( "Unable to locate base PMG UI Editor Scene in assets. Please ensure they're imported correctly" );
			}
		}

		/// <summary>
		/// Sets our aspect ratio (and fixes zoom issues)
		/// </summary>
		/// <param name="index"></param>
		private static void SetAspectRatio( int index )
		{
			var gvWndType = typeof( Editor ).Assembly.GetType( "UnityEditor.GameView" );
			var selectedSizeIndexProp = gvWndType.GetProperty( "selectedSizeIndex",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			var window = EditorWindow.GetWindow( gvWndType );
			var currentIndex = selectedSizeIndexProp?.GetValue( window, null );

			// If we're already set as the correct index, just leave now
			if ( currentIndex != null && ( int )currentIndex == index )
			{
				return;
			}

			selectedSizeIndexProp?.SetValue( window, index, null );

			// Fix for updating zoom to sensible value after switching up the aspect ratio
			var zoomProp = gvWndType.GetMethod( "SnapZoom",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			const float defaultZoom = 1f;
			zoomProp?.Invoke( window, new object[] { defaultZoom } );
		}

#endregion private
	}
}
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ProcGenMusic
{
    /*
     * This class handled setting/restoring the project's graphics renderer asset. PMG scenes utilize a specific URP renderer
     * we need to set when opening the scene. Then also restore the previous renderer, if any, when leaving the pmg scenes.
     * Manually invoking 'SetRenderCache' will set the cached render to the currently set renderers in the project's quality/graphics settings.
     * This should not be needed unless something has gone wrong, and should be cached automatically under normal circumstances.
     */
    [InitializeOnLoad]
    public static class SceneRendererManager
    {
        static SceneRendererManager()
        {
            EditorSceneManager.sceneOpening += OnSceneOpening;
            EditorSceneManager.newSceneCreated += OnSceneCreated;
        }

        [MenuItem( "PMG/Utility/Set Renderer Cache", false, priority: 1 )]
        public static void MenuSetRendererCache()
        {
            SetRendererCache( false );
        }

        [MenuItem( "PMG/Utility/Load PMG Renderers", false, priority: 1 )]
        public static void LoadPMGRenderers()
        {
            var pmgRenderer = AssetDatabase.LoadAssetAtPath( cPmgRendererPath, typeof( RenderPipelineAsset ) ) as RenderPipelineAsset;
            GraphicsSettings.defaultRenderPipeline = pmgRenderer;
            QualitySettings.renderPipeline = pmgRenderer;
            Debug.Log( "PMG Renderers have been set" );
        }

        private static readonly string[] PmgScenes = {"MusicGeneratorUIEditorScene.unity", "PMGExampleScene.unity", "PMGExampleScene2.unity"};
        private const string cPmgRendererPath = "Assets/MusicGeneratorMain/Assets/ScriptableObjects/PMG_RenderPipelineAsset.asset";
        private static bool IsInPmgScene => PmgScenes.Contains( $"{SceneManager.GetActiveScene().name}.unity" );
        public static string mCachedPath => Path.Combine( Application.persistentDataPath, "RendererSettingsCache" );

        [Serializable]
        private struct RendererCachedPaths
        {
            public string mDefaultRendererPath;
            public string mOverrideRendererPath;
        }

        private static void OnSceneCreated( Scene scene, NewSceneSetup setup, NewSceneMode mode )
        {
            LoadCachedRendererFromPath();
        }

        private static void OnSceneOpening( string path, OpenSceneMode mode )
        {
            SetRendererCache();
            var sceneName = Path.GetFileName( path );
            var isOpeningPmgScene = PmgScenes.Contains( sceneName );
            var isCurrentlyInPmgScene = IsInPmgScene;

            switch ( isOpeningPmgScene )
            {
                case true when isCurrentlyInPmgScene == false:
                {
                    LoadPMGRenderers();
                    break;
                }
                case false when isCurrentlyInPmgScene:
                    LoadCachedRendererFromPath();
                    break;
            }
        }

        private static void SetRendererCache( bool checkCurrentScene = true )
        {
            if ( IsInPmgScene && checkCurrentScene )
            {
                return;
            }

            var defaultRendererPath = GraphicsSettings.defaultRenderPipeline == null ? null : AssetDatabase.GetAssetPath( GraphicsSettings.defaultRenderPipeline );
            var overrideRendererPath = QualitySettings.renderPipeline == null ? null : AssetDatabase.GetAssetPath( QualitySettings.renderPipeline );
            var cache = new RendererCachedPaths() {mDefaultRendererPath = defaultRendererPath, mOverrideRendererPath = overrideRendererPath};
            SaveCachedRendererPaths( cache );
        }

        private static void SaveCachedRendererPaths( RendererCachedPaths cachedpaths )
        {
            #if UNITY_EDITOR == false
            return;
            #endif
            Debug.Log( $"saving cached renderer paths to {mCachedPath}" );

            if ( Directory.Exists( mCachedPath ) == false )
            {
                Directory.CreateDirectory( mCachedPath );
            }

            try
            {
                var path = Path.Combine( mCachedPath, "CachedRendererPaths.txt" );
                File.WriteAllText( path, JsonUtility.ToJson( cachedpaths, prettyPrint: true ) );
                Debug.Log( $"CachedRendererPaths.txt was successfully written to file at {path}" );
            }
            catch ( IOException e )
            {
                Debug.Log( $"failed to write cached renderer paths to file with exception {e}" );
            }
        }

        private static void LoadCachedRendererFromPath()
        {
#if UNITY_EDITOR == false
return;
#endif
            var path = Path.Combine( mCachedPath, "CachedRendererPaths.txt" );
            if ( File.Exists( path ) == false )
            {
                return;
            }

            var cachedPaths = JsonUtility.FromJson<RendererCachedPaths>( File.ReadAllText( path ) );
            var defaultRenderer = AssetDatabase.LoadAssetAtPath( cachedPaths.mDefaultRendererPath, typeof( RenderPipelineAsset ) ) as RenderPipelineAsset;
            var overrideRenderer = AssetDatabase.LoadAssetAtPath( cachedPaths.mOverrideRendererPath, typeof( RenderPipelineAsset ) ) as RenderPipelineAsset;
            GraphicsSettings.defaultRenderPipeline = defaultRenderer;
            QualitySettings.renderPipeline = overrideRenderer;

            var defaultName = defaultRenderer != null ? defaultRenderer.name : null;
            var overrideName = overrideRenderer != null ? overrideRenderer.name : null;
            Debug.Log( $"Loaded Default Renderer: {defaultName} | and Override Renderer: {overrideName}" );
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
    /// <summary>
    /// Our general UI Menu, controls all other sub menu UI panels except the Instrument and Instrument List panels
    /// </summary>
    public class GeneralMenuPanel : UIPanel
    {
        #region public

        ///<inheritdoc/>
        public override void UpdateUIElementValues() { }

        /// <summary>
        /// Adds a preset option to our presets dropdown
        /// </summary>
        /// <param name="presetName"></param>
        public void AddPresetOption( string presetName )
        {
            if ( mPresetFileNames.Contains( presetName ) )
            {
                return;
            }

            foreach ( var option in mPreset.Option.options )
            {
                if ( option.text == presetName )
                {
                    return;
                }
            }

            var newOption = new TMP_Dropdown.OptionData
            {
                text = presetName
            };

            if ( presetName.Equals( cNewConfigOption ) )
            {
                mPresetFileNames.Insert( 0, presetName );
                mPreset.Option.options.Insert( 0, newOption );
            }
            else
            {
                mPresetFileNames.Add( presetName );
                mPreset.Option.options.Add( newOption );
            }
        }

        ///<inheritdoc/>
        public override IEnumerator InitializeRoutine( UIManager uiManager, bool isEnabled = true )
        {
            mMusicGenerator = uiManager.MusicGenerator;
            yield return AddPresets();
            yield return base.InitializeRoutine( uiManager, isEnabled );
            mAdvancedSettingsPanel.ExitButton.onClick.AddListener( TogglePanel );
            mMusicGeneratorUIPanel.ExitButton.onClick.AddListener( TogglePanel );
            mGlobalEffectsPanel.ExitButton.onClick.AddListener( TogglePanel );
            mUILeitmotifEditor.ExitButton.onClick.AddListener( TogglePanel );
            mUIPercussionEditor.ExitButton.onClick.AddListener( TogglePanel );
            mUIEditorSettings.ExitButton.onClick.AddListener( TogglePanel );
            mUIModsEditor.ExitButton.onClick.AddListener( TogglePanel );
            mUIWaveformEditor.ExitButton.onClick.AddListener( TogglePanel );
        }

        #endregion public

        #region protected

        ///<inheritdoc/>
        protected override void InitializeListeners()
        {
            mGeneralSettingsButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mMusicGeneratorUIPanel.TogglePanel();
                TogglePanel();
            } );

            mAdvancedSettingsButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mAdvancedSettingsPanel.TogglePanel();
                TogglePanel();
            } );

            mGlobalEffectsButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mGlobalEffectsPanel.TogglePanel();
                TogglePanel();
            } );

            mMeasureEditorButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                StartCoroutine( mUIManager.EnableMeasureEditor( true, TogglePanel ) );
            } );

            mLeitmotifEditorButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mUILeitmotifEditor.TogglePanel();
                TogglePanel();
            } );

            mPercussionEditorButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mUIPercussionEditor.TogglePanel();
                TogglePanel();
            } );

            mUIEditorSettingsButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mUIEditorSettings.TogglePanel();
                TogglePanel();
            } );

            mModsPanelButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mUIModsEditor.TogglePanel();
                TogglePanel();
            } );

#if FMOD_ENABLED
			mUIWaveformEditorButton.gameObject.SetActive( false );
#else
            mUIWaveformEditorButton.onClick.AddListener( () =>
            {
                if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                {
                    return;
                }

                mUIManager.EnableWaveformEditor( true );
                TogglePanel();
            } );
#endif //FMOD_ENABLED
            mPreset.Option.value = GetDefaultConfigIndex();
            mPreset.Initialize( ( value ) =>
                {
                    if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
                    {
                        return;
                    }

                    if ( mPresetFileNames[mPreset.Option.value].Equals( cNewConfigName ) )
                    {
                        return;
                    }

                    if ( mPresetFileNames[mPresetFileNames.Count - 1].Equals( cNewConfigName ) )
                    {
                        mPreset.Option.options.Remove( mPreset.Option.options[mPreset.Option.options.Count - 1] );
                        mPresetFileNames.Remove( cNewConfigName );
                    }

                    mUIManager.UIKeyboard.Stop();
                    StartCoroutine( mUIManager.ReloadUIConfiguration( mPresetFileNames[mPreset.Option.value], true ) );

                    if ( value == 0 )
                    {
                        /* We use this index as the 'new' button, and don't ever want to 'select' it, so
                         * as to allow re-selection to load the new profile again.*/
                        AddPresetOption( cNewConfigName );
                        mPreset.Option.value = mPresetFileNames.Count - 1;
                    }
                },
                initialValue: null );

            mQuitButton.onClick.AddListener( () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
            } );
        }

        ///<inheritdoc/>
        protected override void OnDestroy()
        {
            mAdvancedSettingsPanel.ExitButton.onClick.RemoveListener( TogglePanel );
            mMusicGeneratorUIPanel.ExitButton.onClick.RemoveListener( TogglePanel );
            mGlobalEffectsPanel.ExitButton.onClick.RemoveListener( TogglePanel );
            mUILeitmotifEditor.ExitButton.onClick.RemoveListener( TogglePanel );
            mUIPercussionEditor.ExitButton.onClick.RemoveListener( TogglePanel );
            mUIModsEditor.ExitButton.onClick.RemoveListener( TogglePanel );
            mUIWaveformEditor.ExitButton.onClick.RemoveListener( TogglePanel );
            mGeneralSettingsButton.onClick.RemoveAllListeners();
            mAdvancedSettingsButton.onClick.RemoveAllListeners();
            mGlobalEffectsButton.onClick.RemoveAllListeners();
            mMeasureEditorButton.onClick.RemoveAllListeners();
            mLeitmotifEditorButton.onClick.RemoveAllListeners();
            mPercussionEditorButton.onClick.RemoveAllListeners();
            mUIEditorSettingsButton.onClick.RemoveAllListeners();
            mModsPanelButton.onClick.RemoveAllListeners();
            mQuitButton.onClick.RemoveAllListeners();

#if FMOD_ENABLED == false
            mUIWaveformEditorButton.onClick.RemoveAllListeners();
#endif // FMOD_ENABLED == false

            base.OnDestroy();
        }

        #endregion protected

        #region private

        [Tooltip( "Button for General Settings" )]
        [SerializeField] private Button mGeneralSettingsButton;

        [Tooltip( "Reference to our MusicGeneratorUIPanel" )]
        [SerializeField] private MusicGeneratorUIPanel mMusicGeneratorUIPanel;

        [Tooltip( "Button to toggle the Advanced Settings Panel" )]
        [SerializeField] private Button mAdvancedSettingsButton;

        [Tooltip( "Reference to our AdvanceSettingsPanel" )]
        [SerializeField] private AdvancedSettingsPanel mAdvancedSettingsPanel;

        [Tooltip( "Button to toggle our GlobalEffectsPanel" )]
        [SerializeField] private Button mGlobalEffectsButton;

        [Tooltip( "Reference to our GlobalEffectsPanel" )]
        [SerializeField] private GlobalEffectsPanel mGlobalEffectsPanel;

        [Tooltip( "Button to toggle the MeasureEditorPanel" )]
        [SerializeField] private Button mMeasureEditorButton;

        [Tooltip( "Button to toggle the Leitmotif Editor panel" )]
        [SerializeField] private Button mLeitmotifEditorButton;

        [Tooltip( "Reference to our UILeitmotifEditor" )]
        [SerializeField] private UILeitmotifEditor mUILeitmotifEditor;

        [Tooltip( "Button to toggle the Percussion Editor" )]
        [SerializeField] private Button mPercussionEditorButton;

        [Tooltip( "Reference to our UIPercussionEditor" )]
        [SerializeField] private UIPercussionEditor mUIPercussionEditor;

        [Tooltip( "Button to toggle the UIEditor" )]
        [SerializeField] private Button mUIEditorSettingsButton;

        [Tooltip( "Reference to our UIEditor Panel" )]
        [SerializeField] private UIEditorSettings mUIEditorSettings;

        [Tooltip( "Button to toggle the Mods panel" )]
        [SerializeField] private Button mModsPanelButton;

        [Tooltip( "Reference to our Mods Panel" )]
        [SerializeField] private UIModsEditor mUIModsEditor;

        [Tooltip( "Button to toggle the UI Waveform panel" )]
        [SerializeField] private Button mUIWaveformEditorButton;

        [Tooltip( "Reference to our WaveformEditor Panel" )]
        [SerializeField] private UIWaveformEditor mUIWaveformEditor;

        [Tooltip( "Reference to our Quit Button" )]
        [SerializeField] private Button mQuitButton;

        [Tooltip( "Dropdown to hold our presets" )]
        [SerializeField] public UITMPDropdown mPreset;

        /// <summary>
        /// Container of our existing preset file names
        /// </summary>
        private readonly List<string> mPresetFileNames = new List<string>();

        /// <summary>
        /// Name for 'new' configuration preset option
        /// </summary>
        private const string cNewConfigOption = "New";

        /// <summary>
        /// Name for temporary presets when using 'new' preset, so as to be able to display their current ( and  yet unnamed) configuration
        /// </summary>
        private const string cNewConfigName = "NewConfiguration";

        private static readonly string[] mExcludedPresetNames = new[] {"GeneratorDemo", "Tutorial"};
        private static readonly string[] mExcludedFMODPresets = new[] {"w_", "SpaceLair"};

        /// <summary>
        /// Adds presets to our dropdown from the persistent and streaming data directories
        /// </summary>
        /// <returns></returns>
        private IEnumerator AddPresets()
        {
            var completed = false;
            LoadPresetsFromAddressables( ( presets ) =>
            {
                
#if UNITY_EDITOR
                LoadPresetsFromDirectory( presets, MusicConstants.ConfigurationPersistentDataPath );
#endif
                presets.Sort();
                AddPresetOptions( presets );
                completed = true;
            } );
            yield return new WaitUntil( () => completed );
        }

        private void AddPresetOptions( IEnumerable<string> presets )
        {
            foreach ( var preset in presets.Where( preset => !mExcludedPresetNames.Contains( preset ) ) )
            {
#if FMOD_ENABLED
				var isFmodExcluded = false;
				foreach ( var excludedFmodPreset in mExcludedFMODPresets )
				{
					if ( preset.Contains( excludedFmodPreset ) )
					{
						isFmodExcluded = true;
						break;
					}
				}

				if ( isFmodExcluded )
				{
					continue;
				}
#endif
                AddPresetOption( preset );
            }
        }

        /// <summary>
        /// Adds all presets under addressable label
        /// </summary>
        /// <param name="onComplete"></param>
        private void LoadPresetsFromAddressables( Action<List<string>> onComplete )
        {
            var presets = new List<string>();
            var labelOperation = Addressables.LoadResourceLocationsAsync( MusicConstants.ConfigurationDataAddressableKey );
            labelOperation.Completed += ( labelResponse ) =>
            {
                var count = 0;
                foreach ( var item in labelResponse.Result )
                {
                    var resourceOperation = Addressables.LoadAssetAsync<TextAsset>( item.PrimaryKey );
                    resourceOperation.Completed += ( result ) =>
                    {
                        if ( result.Status == AsyncOperationStatus.Succeeded )
                        {
                            presets.Add( Path.GetFileNameWithoutExtension( result.Result.name ) );
                        }

                        count++;
                        if ( count >= labelResponse.Result.Count )
                        {
                            onComplete.Invoke( presets );
                        }
                    };
                }
            };
        }

        private void LoadPresetsFromDirectory( List<string> presets, string directory )
        {
            if ( Directory.Exists( directory ) == false )
            {
                return;
            }

            foreach ( var fileName in Directory.GetFiles( directory ) )
            {
                if ( Path.GetExtension( fileName ).Contains( "meta" ) == false &&
                     mPresetFileNames.Contains( fileName ) == false )
                {
                    presets.Add( Path.GetFileNameWithoutExtension( fileName ) );
                }
            }
        }

        /// <summary>
        /// returns the index for our default configuration.
        /// This is purely used to gimmick our 'selection' of it on scene start.
        /// </summary>
        /// <returns></returns>
        private int GetDefaultConfigIndex()
        {
            var index = 0;
            foreach ( var option in mPreset.Option.options )
            {
                if ( option.text.Equals( mMusicGenerator.DefaultConfigurationName ) )
                {
                    return index;
                }

                index++;
            }

            return index;
        }

        #endregion private
    }
}
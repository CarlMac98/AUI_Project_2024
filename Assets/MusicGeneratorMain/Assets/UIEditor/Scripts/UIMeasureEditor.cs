using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI Measure Editor handles the loading/saving of the configuration and general settings 
	/// </summary>
	public class UIMeasureEditor : UIDisplayEditor
	{
		#region public

		/// <summary>
		/// Returns state of currently loaded clip
		/// </summary>
		public ClipState ClipState => mLoadedClip != false ? mLoadedClip.ClipState : ClipState.Stop;

		/// <summary>
		/// returns the repeat count of the loaded clip
		/// </summary>
		public int RepeatCount
		{
			get
			{
				var hasData = mLoadedClip && mLoadedClip.ConfigurationData != null;
				return hasData ? mLoadedClip.InstrumentSet.RepeatCount : 0;
			}
		}

		/// <summary>
		/// Returns the number of measures for the ui measure editor
		/// </summary>
		public int NumberOfMeasures
		{
			get
			{
				var hasData = mLoadedClip && mLoadedClip.ConfigurationData != null;
				return hasData ? mLoadedClip.ConfigurationData.RepeatMeasuresNum : 1;
			}
		}

		///<inheritdoc/>
		public override Key Key => ( Key )mKey.Option.value;

		///<inheritdoc/>
		public override Scale Scale => ( Scale )mScale.Option.value;

		///<inheritdoc/>
		public override Mode Mode => ( Mode )mMode.Option.value;

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			LoadPresets();
			base.Initialize( uiManager, isEnabled );
			mMeasureEditor = mUIManager.MeasureEditor;
		}

		///<inheritdoc/>
		public override void SetPanelActive( bool isActive )
		{
			if ( isActive )
			{
				mIsInitializing = true;
				mUIManager.SetCachedConfigurationData( mMusicGenerator.ConfigurationData.Clone() );
				mPreset.Option.value = GetDefaultConfigurationIndex();
				mState = DisplayEditorState.Stopped;
			}
			else
			{
				Stop();
				StartCoroutine( mUIManager.EnableMeasureEditor( false ) );
				mState = DisplayEditorState.Inactive;
			}

			mUIManager.UIKeyboard.Stop( fadeLight: isActive == false );

			base.SetPanelActive( isActive );
		}

		/// <summary>
		/// Loads a configuration
		/// </summary>
		/// <param name="configurationName"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		public IEnumerator LoadConfiguration( string configurationName = NEW_FILE_NAME, Action onComplete = null )
		{
			// This is...a very inelegant method. A lot is going on when loading a new configuration and ordering of things during the coroutines is essential.
			// The only upside is the dirtiness is all in one place? :P 
			// Tread carefully here:

			mIsInitializing = true;
			mMeasureEditor.SetIsInitializing( true );

			RemoveNewConfigurationEntry();
			mUIManager.UIKeyboard.Stop( fadeLight: false );
			mMeasureEditor.Reset();
			mMeasureEditor.SetIsInitializing( true );
			mLoadedClip = gameObject.AddComponent<SingleClip>();
			mUIManager.UIKeyboard.ClearPercussionInstruments();
			mState = DisplayEditorState.Initializing;
			var instrumentsLoaded = false;
			if ( mInstrumentSet != null )
			{
				mMusicGenerator.UnregisterInstrumentSet( mInstrumentSet );
				mInstrumentSet = null;
			}

			mMusicGenerator.CleanupUnusedInstruments();
			yield return mLoadedClip.LoadConfiguration( configurationName,
				mMusicGenerator,
				() =>
				{
					// for editing ease, we InstrumentSet our music generator configuration data to our single clip. Allows
					// for not refactoring all of our UI values to handle separate configurations
					mMusicGenerator.SetConfigurationData( mLoadedClip.ConfigurationData );
					mInstrumentSet = mLoadedClip.InstrumentSet;
					instrumentsLoaded = true;
				} );

			// Once all instruments are loaded, we can initialize their notes in the editor: 
			yield return new WaitUntil( () => instrumentsLoaded );
			yield return mMeasureEditor.InitializeInstruments( InstrumentSet.Instruments );
			mIsInitializing = false;
			mState = DisplayEditorState.Stopped;
			yield return mUIManager.ReloadUIConfiguration( null, false, GeneratorState.Stopped );
			mMeasureEditor.SetIsInitializing( false );
			onComplete?.Invoke();
		}

		/// <summary>
		/// Sets our initialization state
		/// </summary>
		/// <param name="isInitializing"></param>
		public void SetIsInitializing( bool isInitializing )
		{
			mIsInitializing = isInitializing;
		}

		/// <summary>
		/// Exports our measure editor config
		/// </summary>
		/// <param name="configName"></param>
		public void ExportFile( string configName )
		{
			if ( string.IsNullOrEmpty( configName ) )
			{
				return;
			}

			Save( configName );
			AddPresetOption( configName );
		}

		///<inheritdoc/>
		public override void Reset()
		{
			if ( mLoadedClip )
			{
				mLoadedClip.Reset();
			}

			mCurrentMeasure.Option.value = 0;
		}

		///<inheritdoc/>
		public override void Play()
		{
			mUIManager.UIKeyboard.PlayClip( isPercussion: mUIManager.InstrumentListPanelUI.PercussionIsSelected );
			mLoadedClip.Play();
			mMeasureEditor.Play();
			mState = DisplayEditorState.Playing;
		}

		///<inheritdoc/>
		public override void Pause()
		{
			mState = DisplayEditorState.Paused;
			mMeasureEditor.Pause();
			mLoadedClip.Pause();
		}

		///<inheritdoc/>
		public override void Stop()
		{
			if ( mState == DisplayEditorState.Stopped )
			{
				return;
			}

			base.Stop();
			mState = DisplayEditorState.Stopped;
			mMeasureEditor.Stop();
			if ( mLoadedClip && mLoadedClip.ClipState != ClipState.Stop )
			{
				mLoadedClip.Stop();
			}
		}

		///<inheritdoc/>
		public override void Save( string filename )
		{
			// for clips, we always want this InstrumentSet, whether they're repeating or not. It allows multiple measures to not get cleared.
			mLoadedClip.ConfigurationData.ThemeRepeatOptions = ThemeRepeatOptions.Repeat;
			mLoadedClip.ConfigurationData.IsSingleClip = true;
			mLoadedClip.SaveConfiguration( filename );
		}

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			if ( mIsInitializing )
			{
				return;
			}

			base.UpdateUIElementValues();
			if ( mInstrumentSet == null )
			{
				return;
			}

			mKey.Option.value = ( int )mMusicGenerator.ConfigurationData.Key;
			mMode.Option.value = ( int )mMusicGenerator.ConfigurationData.Mode;
			mScale.Option.value = ( int )mMusicGenerator.ConfigurationData.Scale;

			var hasData = mLoadedClip && mLoadedClip.ConfigurationData != null;
			mCurrentMeasure.Option.value = mLoadedClip.InstrumentSet.RepeatCount;
			mNumberOfMeasures.Option.value = hasData ? mLoadedClip.ConfigurationData.RepeatMeasuresNum : 0;
			mClipIsRepeating.Option.isOn = hasData && mLoadedClip.ConfigurationData.SingleClipIsRepeating;
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			base.InitializeListeners();

			mPreset.Initialize( ( value ) =>
				{
					if ( IsEnabled == false )
					{
						return;
					}

					var filename = mPreset.Option.options[value].text;
					if ( filename.Equals( NEW_CONFIGURATION_NAME ) )
					{
						return;
					}

					var index = value;
					StartCoroutine( LoadConfiguration( filename,
						() =>
						{
							if ( index == 0 )
							{
								/* We use this index as the 'new' button, and don't ever want to 'select' it, so
								 * as to allow re-selection to load the new profile again.*/
								AddPresetOption( NEW_CONFIGURATION_NAME );
								mLoadedClip.ConfigurationData.ConfigurationName = NEW_CONFIGURATION_NAME;
								mPreset.Option.value = GetDefaultConfigIndex();
							}
						} ) );
				},
				initialValue: GetDefaultConfigurationIndex() );

			mClipIsRepeating.Initialize( value =>
				{
					if ( mLoadedClip )
					{
						mLoadedClip.ConfigurationData.SingleClipIsRepeating = value;
					}
				},
				initialValue: true );

			mCurrentMeasure.Initialize( ( value ) =>
				{
					value++;
					mCurrentMeasure.Text.text = $"{value}";

					if ( mNumberOfMeasures.Option.value < value )
					{
						mNumberOfMeasures.Option.value = value;
					}

					mDisplayIsDirty = true;
				},
				initialValue: 0 );

			mNumberOfMeasures.Initialize( ( value ) =>
				{
					if ( mInstrumentSet != null )
					{
						mInstrumentSet.Data.RepeatMeasuresNum = ( int )value;

						if ( mCurrentMeasure.Option.value >= value )
						{
							mCurrentMeasure.Option.value = value - 1;
							mDisplayIsDirty = true;
						}
					}

					mNumberOfMeasures.Text.text = $"{value}";
				},
				initialValue: mInstrumentSet?.Data.RepeatMeasuresNum ?? 1 );
		}

		///<inheritdoc/>
		protected override IEnumerator LoadPanel( bool isActive )
		{
			// We don't use this for the measure editor
			yield break;
		}

		#endregion protected

		[SerializeField, Tooltip( "Reference to our Clip Repeating Toggle" )]
		private UIToggle mClipIsRepeating;

		[SerializeField, Tooltip( "Reference to our Presets Dropdown" )]
		private UITMPDropdown mPreset;

		/// <summary>
		/// Reference to the measure editor
		/// </summary>
		private MeasureEditor mMeasureEditor;

		/// <summary>
		/// Reference to our currently loaded clip
		/// </summary>
		private SingleClip mLoadedClip;

		/// <summary>
		/// String used for 'New' Display option in the preset dropdown
		/// </summary>
		private const string NEW_FILE_NAME = "New";

		/// <summary>
		/// Name used for configuration created with 'New' dropdown until exported otherwise
		/// </summary>
		private const string NEW_CONFIGURATION_NAME = "NewConfiguration";

		/// <summary>
		/// Whether we're in the process of initializing. Used to prevent repeated cleaning of the displays during coroutines
		/// </summary>
		private bool mIsInitializing;

		/// <summary>
		/// Loads our presets from their directories
		/// </summary>
		private void LoadPresets()
		{
			var fileNames = new List<string>();
#if UNITY_EDITOR
			GetFilenamesFromDirectory( MusicConstants.PersistentClipsPath, fileNames );
			GetFilenamesFromDirectory( MusicConstants.BaseInstrumentClipsPath, fileNames );
			foreach ( var filename in fileNames )
			{
				AddPresetOption( Path.GetFileNameWithoutExtension( filename ) );
			}

#else
            LoadPresetsFromAddressables();
#endif
		}

		/// <summary>
		/// Adds all presets under addressable label
		/// </summary>
		[UsedImplicitly]
		private void LoadPresetsFromAddressables()
		{
			var labelOperation = Addressables.LoadResourceLocationsAsync( MusicConstants.InstrumentClipsAddressableKey );
			labelOperation.Completed += ( labelResponse ) =>
			{
				foreach ( var item in labelResponse.Result )
				{
					var resourceOperation = Addressables.LoadAssetAsync<TextAsset>( item.PrimaryKey );
					resourceOperation.Completed += ( result ) =>
					{
						if ( result.Status == AsyncOperationStatus.Succeeded )
						{
							AddPresetOption( Path.GetFileNameWithoutExtension( result.Result.name ) );
						}
					};
				}
			};
		}

		/// <summary>
		/// Returns configuration names from a directory
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="fileNames"></param>
		private static void GetFilenamesFromDirectory( string directory, List<string> fileNames )
		{
			//TODO: load without directory parsing
			if ( Directory.Exists( directory ) == false )
			{
				return;
			}

			foreach ( var filename in Directory.GetFiles( directory ) )
			{
				if ( filename.Contains( ".meta" ) || filename.Contains( "presets" ) )
				{
					continue;
				}

				fileNames.Add( Path.GetFileNameWithoutExtension( filename ) );
			}
		}

		/// <summary>
		/// Adds a preset option to our dropdown
		/// </summary>
		/// <param name="filename"></param>
		private void AddPresetOption( string filename )
		{
			foreach ( var optionData in mPreset.Option.options )
			{
				if ( optionData.text.Equals( filename ) )
				{
					return;
				}
			}

			var newOption = new TMP_Dropdown.OptionData
			{
				text = filename
			};

			if ( filename.Equals( NEW_FILE_NAME ) )
			{
				mPreset.Option.options.Insert( 0, newOption );
			}
			else
			{
				mPreset.Option.options.Add( newOption );
			}
		}

		/// <summary>
		/// Returns the index of our default configuration
		/// </summary>
		/// <returns></returns>
		private int GetDefaultConfigurationIndex()
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

		/// <summary>
		/// Removes the temporary 'newConfiguration' entry from our presets dropdown
		/// </summary>
		private void RemoveNewConfigurationEntry()
		{
			if ( mPreset.Option.options[mPreset.Option.options.Count - 1].text.Equals( NEW_CONFIGURATION_NAME ) )
			{
				mPreset.Option.options.Remove( mPreset.Option.options[mPreset.Option.options.Count - 1] );
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
				if ( option.text.Equals( NEW_CONFIGURATION_NAME ) )
				{
					return index;
				}

				index++;
			}

			return index;
		}
	}
}
using System.Collections;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	#region public

	/// <summary>
	/// Display Editor State
	/// </summary>
	public enum DisplayEditorState
	{
		Inactive = 0,
		Initializing = 1,
		Playing = 2,
		Paused = 3,
		Stopped = 4
	}

	/// <summary>
	/// Display Editors are UI Components that handle visual editing of measures (leitmotif, percussion, measure editor, etc).
	/// </summary>
	public abstract class UIDisplayEditor : UIPanel
	{
		/// <summary>
		/// Whether the display is dirty. Dirty displays generally have their data refreshed in child overrides. 
		/// </summary>
		public bool DisplayIsDirty => mDisplayIsDirty;

		/// <summary>
		/// Whether the display is broken. Broken displays generally have their data _and_ ui elements completely rebuilt as new.
		/// This typically happens when a large data change has happened (new instrument, time signature change) 
		/// </summary>
		public bool DisplayIsBroken => mDisplayIsBroken;

		/// <summary>
		/// Reference to our instrument InstrumentSet
		/// </summary>
		public InstrumentSet InstrumentSet => mInstrumentSet;

		/// <summary>
		/// Current state of the Display Editor
		/// </summary>
		public DisplayEditorState State => mState;

		/// <summary>
		/// Whether the display editor is currently showing all instruments (as opposed to only the selected instrument)
		/// </summary>
		public bool ShouldShowAllInstruments => mShowAllInstruments != false && mShowAllInstruments.Option.isOn;

		/// <summary>
		/// Currently displayed measure of the display editor
		/// </summary>
		public virtual int CurrentMeasure => ( int )mCurrentMeasure.Option.value;

		/// <summary>
		/// Inverse progression rate of the current instrument InstrumentSet.
		/// </summary>
		public int InverseProgressionRate => mInstrumentSet.GetInverseProgressionRate( ( int )mProgressionRate.Option.value );

		/// <summary>
		/// Whether we're showing editor hints
		/// </summary>
		public bool IsShowingEditorHints => mShowEditorHints.Option.isOn;

		/// <summary>
		/// Current key
		/// </summary>
		public abstract Key Key { get; }

		/// <summary>
		/// Current scale (references UI value, not instrument value)
		/// </summary>
		public abstract Scale Scale { get; }

		/// <summary>
		/// Current mode (references UI value, not instrument value)
		/// </summary>
		public abstract Mode Mode { get; }

		/// <summary>
		/// Plays the current display editor type (leitmotif, percussion, etc)
		/// </summary>
		public abstract void Play();

		/// <summary>
		/// Resets the player
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// Pauses the Player
		/// </summary>
		public abstract void Pause();

		/// <summary>
		/// Saves needed data
		/// </summary>
		/// <param name="filename"></param>
		public abstract void Save( string filename );

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			base.Initialize( uiManager, isEnabled );

			mUIManager.InstrumentListPanelUI.OnInstrumentSelected.AddListener( OnInstrumentSelected );
		}

		/// <summary>
		/// Clears the dirty flag
		/// </summary>
		public void CleanDisplay()
		{
			mDisplayIsDirty = false;
		}

		/// <summary>
		/// sets the dirty flag as true 
		/// </summary>
		public void DirtyDisplay()
		{
			mDisplayIsDirty = true;
		}

		/// <summary>
		/// clears broken flag
		/// </summary>
		public void RepairDisplay()
		{
			mDisplayIsBroken = false;
		}

		/// <summary>
		/// Sets broken flag as true
		/// </summary>
		public void BreakDisplay()
		{
			mDisplayIsBroken = true;
		}

		/// <summary>
		/// Handles stopping the player
		/// </summary>
		public virtual void Stop()
		{
			Reset();
			ToggleHelperNotes();
		}

		/// <summary>
		/// Updates the helper notes showing/not showing, whether mShowEditorHints.isOn.
		/// </summary>
		public void ToggleHelperNotes()
		{
			mDisplayIsDirty = true;
		}

		///<inheritdoc/>
		public override void SetPanelActive( bool isActive )
		{
			base.SetPanelActive( isActive );
			mState = isActive ? DisplayEditorState.Initializing : DisplayEditorState.Inactive;
		}

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			if ( mInstrumentSet == null || IsEnabled == false )
			{
				return;
			}

			mCurrentMeasure.Option.value = 0;
			mProgressionRate.Option.value =
				mMusicGenerator.InstrumentSet.GetProgressionRateIndex( ( mMusicGenerator.InstrumentSet.Data.ProgressionRate ) );
			mProgressionRate.Text.text = $"{mMusicGenerator.InstrumentSet.Data.ProgressionRate}";

			mTempo.Option.value = mInstrumentSet.Data.Tempo;

			if ( mScale )
			{
				mScale.Option.value = ( int )mMusicGenerator.ConfigurationData.Scale;
			}

			if ( mKey )
			{
				mKey.Option.value = ( int )mMusicGenerator.ConfigurationData.Key;
			}

			if ( mMode )
			{
				mMode.Option.value = ( int )mMusicGenerator.ConfigurationData.Mode;
			}

#if FMOD_ENABLED
			if ( mFmodMasterFader )
			{
				mFmodMasterFader.Option.value = mMusicGenerator.ConfigurationData.FmodMasterFader;
			}

			if ( mFmodMasterGain )
			{
				mFmodMasterGain.Option.value = mMusicGenerator.ConfigurationData.FmodMasterGain;
			}
#else
			if ( mMasterVolume )
			{
				mMasterVolume.Option.value = mMusicGenerator.ConfigurationData.MasterVolume;
			}
#endif
		}

		#endregion public

		#region protected

		[SerializeField, Tooltip( "Reference to our number of measures slider" )]
		protected UISlider mNumberOfMeasures;

		[SerializeField, Tooltip( "Reference to our current measure slider" )]
		protected UISlider mCurrentMeasure;

		[SerializeField, Tooltip( "Reference to our progression rate slider" )]
		protected UISlider mProgressionRate;

		[SerializeField, Tooltip( "Reference to our show hints toggle" )]
		protected UIToggle mShowEditorHints;

		[SerializeField, Tooltip( "Reference to our show all instruments toggle" )]
		protected UIToggle mShowAllInstruments;

		[SerializeField, Tooltip( "Reference to our Key slider" )]
		protected UISlider mKey;

		[SerializeField, Tooltip( "Reference to our scale slider" )]
		protected UISlider mScale;

		[SerializeField, Tooltip( "Reference to our tempo slider" )]
		protected UISlider mTempo;

		[SerializeField, Tooltip( "Reference to our mode slider" )]
		protected UISlider mMode;

		[SerializeField, Tooltip( "Reference to our Volume slider" )]
		protected UISlider mMasterVolume;

		[SerializeField, Tooltip( "Reference to our FMOD Volume slider" )]
		private UISlider mFmodMasterFader;

		[SerializeField, Tooltip( "Reference to our FMOD Gain slider" )]
		private UISlider mFmodMasterGain;

		/// <summary>
		/// In order not to force a rebuild of notes multiple times in a single frame, we just mark dirty and the
		/// measure editor rebuilds
		/// </summary>
		protected bool mDisplayIsDirty;

		/// <summary>
		/// Reference to our current instrument InstrumentSet (this may not be the Music generator's InstrumentSet)
		/// </summary>
		protected InstrumentSet mInstrumentSet;

		/// <summary>
		/// Our current state
		/// </summary>
		protected DisplayEditorState mState;

		/// <summary>
		/// Loads our UI panel
		/// </summary>
		/// <param name="isActive"></param>
		/// <returns></returns>
		protected abstract IEnumerator LoadPanel( bool isActive );

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			if ( mShowAllInstruments )
			{
				mShowAllInstruments.Initialize( ( value ) =>
					{
						mDisplayIsDirty = true;
						mShowAllInstruments.Text.text = value ? "Showing All Instruments" : "Showing Selected Inst.";
					},
					initialValue: false );
			}

			mProgressionRate.Initialize( ( value ) =>
				{
					mMusicGenerator.InstrumentSet.SetInverseProgressionRate( ( int )value );
					mProgressionRate.Text.text = $"{mMusicGenerator.InstrumentSet.Data.ProgressionRate}";
					mDisplayIsDirty = true;
				},
				initialValue: mMusicGenerator.InstrumentSet.GetProgressionRateIndex( mMusicGenerator.InstrumentSet.Data.ProgressionRate ) );

			if ( mShowEditorHints )
			{
				mShowEditorHints.Initialize( ( value ) =>
					{
						if ( mMusicGenerator.GeneratorState == GeneratorState.Stopped )
						{
							mDisplayIsDirty = true;
						}

						mShowEditorHints.Text.text = value ? "Hints Enabled" : "Hints Disabled";
					},
					initialValue: false );
			}

			mTempo.Initialize( ( value ) =>
				{
					mTempo.Option.maxValue = MusicConstants.MaxTempo;
					mTempo.Option.minValue = MusicConstants.MinTempo;

					if ( mInstrumentSet != null )
					{
						mInstrumentSet.Data.Tempo = value;
						mMusicGenerator.InstrumentSet.UpdateTempo();
						mTempo.Text.text = $"{60f / mMusicGenerator.InstrumentSet.BeatLength:0.00} bpm";
					}
				},
				initialValue: 60 );

			if ( mScale )
			{
				mScale.Initialize( ( value ) =>
					{
						mMusicGenerator.ConfigurationData.Scale = ( Scale )value;
						mScale.Text.text = $"{mMusicGenerator.ConfigurationData.Scale}";
						mDisplayIsBroken = true;
					},
					initialValue: ( int )mMusicGenerator.ConfigurationData.Scale );
			}

			if ( mMode )
			{
				mMode.Initialize( ( value ) =>
					{
						mMusicGenerator.ConfigurationData.Mode = ( Mode )value;
						mMode.Text.text = $"{mMusicGenerator.ConfigurationData.Mode}";
						mDisplayIsBroken = true;
					},
					initialValue: ( int )mMusicGenerator.ConfigurationData.Mode );
			}

			if ( mKey )
			{
				mKey.Initialize( ( value ) =>
					{
						mMusicGenerator.ConfigurationData.Key = ( Key )value;
						mKey.Text.text = $"{mMusicGenerator.ConfigurationData.Key}";
						mDisplayIsBroken = true;
					},
					initialValue: ( int )mMusicGenerator.ConfigurationData.Key );
			}
#if FMOD_ENABLED
			mFmodMasterGain.VisibleObject.SetActive( true );
			mFmodMasterFader.VisibleObject.SetActive( true );
			mMasterVolume.VisibleObject.SetActive( false );

			mFmodMasterFader.Initialize( ( value ) =>
			{
				mMusicGenerator.SetVolume( value );
				mFmodMasterFader.Text.text = $"{value}%";
			}, initialValue: mMusicGenerator.ConfigurationData.FmodMasterFader );

			mFmodMasterGain.Initialize( ( value ) =>
			{
				mMusicGenerator.ConfigurationData.FmodMasterGain = value;
				mFmodMasterGain.Text.text = $"{value}%";
			}, initialValue: mMusicGenerator.ConfigurationData.FmodMasterGain );
#else
			mFmodMasterGain.VisibleObject.SetActive( false );
			mFmodMasterFader.VisibleObject.SetActive( false );
			mMasterVolume.VisibleObject.SetActive( true );
			mMasterVolume.Initialize( ( value ) =>
				{
					mMusicGenerator.SetVolume( value );
					mMasterVolume.Text.text = $"{value}";
				},
				initialValue: mMusicGenerator.ConfigurationData.MasterVolume );
#endif
		}

		///<inheritdoc/>
		protected override void OnDestroy()
		{
			if ( mUIManager )
			{
				mUIManager.InstrumentListPanelUI.OnInstrumentSelected.RemoveListener( OnInstrumentSelected );
			}

			base.OnDestroy();
		}

		#endregion protected

		#region private

		/// <summary>
		/// Generally forces a rebuild of all data/ui elements in child classes
		/// </summary>
		private bool mDisplayIsBroken;

		/// <summary>
		/// Handles logic for updating things when an instrument is selected.
		/// </summary>
		/// <param name="instrument"></param>
		private void OnInstrumentSelected( Instrument instrument )
		{
			mDisplayIsDirty = true;
		}

		#endregion private
	}
}
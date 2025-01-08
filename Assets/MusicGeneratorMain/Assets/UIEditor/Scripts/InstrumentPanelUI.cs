using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Instrument Panel with full Instrument settings
	/// </summary>
	public class InstrumentPanelUI : UIPanel
	{
		#region public

		/// <summary>
		/// Reference to our instrument
		/// </summary>
		public Instrument Instrument { get; private set; }

		/// <summary>
		/// Clears our current instrument
		/// This leaves the panel empty
		/// </summary>
		public void ClearInstrument()
		{
			Instrument = null;
			HideAllPanels();
		}

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			Instrument = uiManager.MusicGenerator.InstrumentSet.Instruments.Count > 0 ? uiManager.MusicGenerator.InstrumentSet.Instruments[0] : null;
			base.Initialize( uiManager, isEnabled );
			HideAllPanels();
		}

		/// <summary>
		/// Sets our instrument and updates values.
		/// </summary>
		/// <param name="instrument"></param>
		public void SetInstrument( Instrument instrument )
		{
			Instrument = instrument;

			ResetTopToggles();

			UpdateUIElementValues();
		}

		/// <summary>
		/// Breaks our display (naming to match other panels that use this nomenclature. This just updates our ui elements)
		/// </summary>
		public void BreakDisplay()
		{
			UpdateUIElementValues();
		}

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			if ( mIsInitialized == false || Instrument == null )
			{
				return;
			}

			if ( mListenersHaveInitialized == false )
			{
				InitializeListeners();
				return;
			}

			var fxIsOn = mFXToggle.Option.isOn;
#if FMOD_ENABLED
			mFmodInstrumentEffectVisibleObject.SetActive( fxIsOn );
			mFmodFXPanel.UpdateUIElementValues( Instrument );
			mSynthPanelVisibleObject.SetActive( false );
			mSynthToggle.gameObject.SetActive( false );
#else
			mInstrumentEffectVisibleObject.SetActive( fxIsOn );

			mFXPanel.UpdateUIElementValues( Instrument );
			mSynthSettingsUIPanelPanel.UpdateUIElementValues( Instrument );
			mSynthToggle.gameObject.SetActive( Instrument.InstrumentData.IsSynth && mUIManager.UIWaveformEditor.IsEnabled == false );
			if ( Instrument.InstrumentData.IsSynth == false || mUIManager.UIWaveformEditor.IsEnabled )
			{
				mSynthToggle.Option.isOn = false;
				mSynthPanelVisibleObject.SetActive( false );
			}

			mInstrumentPanelVisibleObject.SetActive( fxIsOn == false );
#endif // FMOD_ENABLED

			SetLeadAvoidNoteListeners( initialize: false );

			mUseSevenths.Option.isOn = Instrument.InstrumentData.ChordSize == 4;

			mArpeggio.Option.isOn = Instrument.InstrumentData.Arpeggio && Instrument.InstrumentData.IsPercussion == false;
			mArpeggioRepeat.Option.value = Instrument.InstrumentData.ArpeggioRepeat;
			mNumStrumNotes.Option.value = Instrument.InstrumentData.NumStrumNotes;
			mKeepMelodicRhythm.Option.isOn = Instrument.InstrumentData.KeepMelodicRhythm;
			mPattern.Option.isOn = Instrument.InstrumentData.UsePattern;
			mStrumLength.Option.value = Instrument.InstrumentData.StrumLength;
			mReverseStrumToggle.Option.isOn = Instrument.InstrumentData.ReverseStrum;
			mStrumVariation.Option.maxValue = mMusicGenerator.InstrumentSet.BeatLength * mMusicGenerator.InstrumentSet.TimeSignature.StepsPerMeasure;
			mStrumVariation.Option.value = Instrument.InstrumentData.StrumVariation / mMusicGenerator.InstrumentSet.BeatLength;
			mOddsOfPlaying.Option.value = Instrument.InstrumentData.OddsOfPlaying;
			mPentatonic.Option.isOn = Instrument.InstrumentData.IsPentatonic;
			mPentatonicParent.SetActive( Instrument.InstrumentData.SuccessionType == SuccessionType.Lead && mPentatonic.Option.isOn == false );
			mTimestep.Option.value = Enum.GetNames( typeof( Timestep ) ).Length - ( int )Instrument.InstrumentData.TimeStep - 1;

			mMinMelodicRhythmTimestep.Option.value = Enum.GetNames( typeof( Timestep ) ).Length - ( int )Instrument.InstrumentData.MinMelodicRhythmTimeStep - 1;
			mPatternLength.Option.value = Instrument.InstrumentData.PatternLength;
			mPatternRepeat.Option.value = Instrument.InstrumentData.PatternRepeat;
			mPatternRelease.Option.value = Instrument.InstrumentData.PatternRelease;
			mLeadVariation.Option.value = Instrument.InstrumentData.AscendDescendInfluence;
			mLeadMaxSteps.Option.value = Instrument.InstrumentData.LeadMaxSteps;
			mSuccessivePlayOdds.Option.value = Instrument.InstrumentData.SuccessivePlayOdds;
			mManualBeat.Option.isOn = Instrument.InstrumentData.ForceBeat;

#if FMOD_ENABLED
			mVolume.Option.value = Instrument.InstrumentData.FmodFader;
			mGain.Option.value = Instrument.InstrumentData.FmodGain;
			mStereoPan.Option.value = Instrument.InstrumentData.FmodPanner * 100f;
			mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodPannerName, Instrument.InstrumentIndex, Instrument.InstrumentData.FmodPanner );
			mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodGainName, Instrument.InstrumentIndex, Instrument.InstrumentData.FmodGain );
			mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodFaderName, Instrument.InstrumentIndex, Instrument.InstrumentData.FmodFader );
#else
			mVolume.Option.value = Instrument.InstrumentData.Volume * 100f;
			mStereoPan.Option.value = Instrument.InstrumentData.StereoPan * 100f;
#endif //FMOD_ENABLED

			mOddsOfPlayingChordNote.Option.value = Instrument.InstrumentData.OddsOfUsingChordNotes;
			mOddsOfPlayingChordNote.VisibleObject.SetActive( Instrument.InstrumentData.IsPercussion == false );
			mMixerGroupVolume.Option.value = Instrument.InstrumentData.MixerGroupVolume;

			SetSuccessionType( ( int )Instrument.InstrumentData.SuccessionType );
			ToggleManualBeat( Instrument.InstrumentData.ForceBeat );

			//reverb:
			mFXPanel.UpdateUIElementValues( Instrument );

			mOctavesToUse.Clear();
			for ( var index = 0; index < Instrument.InstrumentData.OctavesToUse.Count; index++ )
			{
				mOctavesToUse.Add( Instrument.InstrumentData.OctavesToUse[index] );
			}

			mOctave1.Option.isOn = Instrument.InstrumentData.OctavesToUse.Contains( 0 );
			mOctave2.Option.isOn = Instrument.InstrumentData.OctavesToUse.Contains( 1 );
			mOctave3.Option.isOn = Instrument.InstrumentData.OctavesToUse.Contains( 2 );

			//update this last as it enables/disables other objects
			mSuccession.Option.value = ( int )Instrument.InstrumentData.SuccessionType;
			UpdateTitleColor();
		}

		public void ToggleFXPanel()
		{
			mFXToggle.Option.isOn = mFXToggle.Option.isOn == false;
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			if ( Instrument == null )
			{
				return;
			}

			SetLeadAvoidNoteListeners( initialize: true );

			mInstrumentTitleText.OnTextWasSet.AddListener( OnTitleChanged );

			mUseSevenths.Initialize( ( value ) =>
				{
					var chordSize = value ? 4 : 3;
					mUseSevenths.Text.text = value ? "Dominant 7ths Enabled" : "Dominant 7ths Disabled";
					Instrument.InstrumentData.ChordSize = chordSize;
				},
				Instrument.InstrumentData.ChordSize == 4 );

			mArpeggio.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.Arpeggio = value;
					mArpeggio.Text.text = value ? "Arpeggio Enabled" : "Arpeggio Disabled";
					SetSuccessionType( ( int )mSuccession.Option.value );
				},
				Instrument.InstrumentData.Arpeggio && Instrument.InstrumentData.IsPercussion == false );

			mArpeggioRepeat.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.ArpeggioRepeat = ( int )value;
					mArpeggioRepeat.Text.text = $"{value}";
				},
				Instrument.InstrumentData.ArpeggioRepeat );

			mNumStrumNotes.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.NumStrumNotes = ( int )value;
					Instrument.GenerateArpeggioPattern();
					SetSuccessionType( ( int )mSuccession.Option.value );
					mNumStrumNotes.Text.text = $"{value}";
				},
				Instrument.InstrumentData.NumStrumNotes,
				createDividers: true );

			mKeepMelodicRhythm.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.KeepMelodicRhythm = value;
					SetSuccessionType( ( int )mSuccession.Option.value );
				},
				Instrument.InstrumentData.Arpeggio );

			mPattern.Initialize( ( value ) =>
				{
					mPattern.Option.isOn = value;
					mPattern.Text.text = value ? "Pattern Enabled" : "Pattern Disabled";
					Instrument.InstrumentData.UsePattern = value;
					mPatternRelease.VisibleObject.SetActive( value );
					mPatternLength.VisibleObject.SetActive( value );
					mPatternRepeat.VisibleObject.SetActive( value );

					var activeElements = value ? 4 : 1;
					var padding = mPatternGroup.padding;
					mPatternRect.SetSizeWithCurrentAnchors(
						RectTransform.Axis.Vertical,
						( activeElements - 1 ) * mPatternGroup.spacing +
						activeElements * mElementHeight +
						padding.top +
						padding.bottom
					);
				},
				Instrument.InstrumentData.UsePattern && Instrument.InstrumentData.SuccessionType != SuccessionType.Lead );

			mStrumLength.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.StrumLength = value;
					mStrumLength.Text.text = $"{value:0.00}";
				},
				Instrument.InstrumentData.StrumLength );

			mReverseStrumToggle.Initialize( ( value ) => { Instrument.InstrumentData.ReverseStrum = value; }, Instrument.InstrumentData.ReverseStrum );

			mStrumVariation.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.StrumVariation = value * mMusicGenerator.InstrumentSet.BeatLength;
					mStrumVariation.Text.text = $"{value:0.00}";
				},
				mMusicGenerator.InstrumentSet.BeatLength == 0 ? 0 : Instrument.InstrumentData.StrumVariation / mMusicGenerator.InstrumentSet.BeatLength );

			mOddsOfPlaying.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.OddsOfPlaying = ( int )value;
					mOddsOfPlaying.Text.text = $"{value}%";
				},
				Instrument.InstrumentData.OddsOfPlaying );

			mPentatonic.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.IsPentatonic = value;
					// all avoid notes share a visible object
					OnPentatonicStateChanged();
					SetSuccessionType( ( int )mSuccession.Option.value );
				},
				Instrument.InstrumentData.IsPentatonic );
			mPentatonic.Option.onValueChanged.AddListener( _ => mUIManager.DirtyEditorDisplays() );

			mManualBeat.Initialize( ( isOn ) =>
				{
					Instrument.InstrumentData.ForceBeat = isOn;
					if ( isOn )
					{
						mKeepMelodicRhythm.Option.isOn = false;
					}

					ToggleManualBeat( Instrument.InstrumentData.ForceBeat );
				},
				Instrument.InstrumentData.ForceBeat );

			mTimestep.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.TimeStep = ( Timestep )( Enum.GetNames( typeof( Timestep ) ).Length - value - 1 );
					mTimestep.Text.text = $"{Instrument.InstrumentData.TimeStep}";
				},
				Enum.GetNames( typeof( Timestep ) ).Length - ( int )Instrument.InstrumentData.TimeStep - 1,
				resetValue: null,
				createDividers: true );
			mTimestep.Option.onValueChanged.AddListener( _ => mUIManager.DirtyEditorDisplays() );

			InitializeManualBeatToggles();

			mMinMelodicRhythmTimestep.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.MinMelodicRhythmTimeStep = ( Timestep )( Enum.GetNames( typeof( Timestep ) ).Length - value - 1 );
					mMinMelodicRhythmTimestep.Text.text = $"{Instrument.InstrumentData.MinMelodicRhythmTimeStep}";
				},
				Enum.GetNames( typeof( Timestep ) ).Length - ( int )Instrument.InstrumentData.MinMelodicRhythmTimeStep - 1,
				resetValue: null,
				createDividers: true );
			mMinMelodicRhythmTimestep.Option.onValueChanged.AddListener( _ => mUIManager.DirtyEditorDisplays() );

			mStereoPan.Initialize( ( value ) =>
				{
#if FMOD_ENABLED
					Instrument.InstrumentData.FmodPanner = value / 100f;
					mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodPannerName, Instrument.InstrumentIndex, Instrument.InstrumentData.FmodPanner );
#else
					Instrument.InstrumentData.StereoPan = value / 100f;
#endif //FMOD_ENABLED

					mStereoPan.Text.text = $"{value}%";
				},
#if FMOD_ENABLED
				Instrument.InstrumentData.FmodPanner * 100f );
#else
				Instrument.InstrumentData.StereoPan * 100f );
#endif //FMOD_ENABLED

			mPatternRepeat.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.PatternRepeat = ( int )value;
					mPatternRepeat.Text.text = $"{value}";
				},
				Instrument.InstrumentData.PatternRepeat );

			mPatternRelease.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.PatternRelease = ( int )mPatternRelease.Option.value;
					mPatternRelease.Text.text = $"{value}";
				},
				Instrument.InstrumentData.PatternRelease );

			mPatternLength.Initialize( ( value ) =>
				{
					mPatternLength.Text.text = $"{value}";
					Instrument.InstrumentData.PatternLength = ( int )value;
					mPatternRelease.Option.maxValue = value - 1;
				},
				Instrument.InstrumentData.PatternLength );

			mLeadVariation.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.AscendDescendInfluence = value;
					mLeadVariation.Text.text = $"{value}";
				},
				Instrument.InstrumentData.AscendDescendInfluence );

			mLeadMaxSteps.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.LeadMaxSteps = ( int )value;
					mLeadMaxSteps.Text.text = $"{( int )value}";
				},
				Instrument.InstrumentData.LeadMaxSteps,
				resetValue: null,
				createDividers: true );

			mSuccessivePlayOdds.Initialize( ( value ) =>
				{
					mSuccessivePlayOdds.Text.text = $"{( int )value}";
					Instrument.InstrumentData.SuccessivePlayOdds = value;
				},
				Instrument.InstrumentData.SuccessivePlayOdds );

#if FMOD_ENABLED
			mGain.VisibleObject.SetActive( true );
			mGain.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.FmodGain = value;
					mGain.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodGainName, Instrument.InstrumentIndex, value );
				},
				initialValue: Instrument.InstrumentData.FmodGain );

			mVolume.Initialize( ( value ) =>
				{
					mVolume.Option.minValue = 0f;
					mVolume.Option.maxValue = 1f;
					mVolume.Option.wholeNumbers = false;
					Instrument.InstrumentData.FmodFader = value;
					mVolume.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodFaderName, Instrument.InstrumentIndex, value );
				},
				Instrument.InstrumentData.FmodFader );

			mMixerGroupVolume.VisibleObject.SetActive( false );
			mFmodFXPanel.InitializeFXPanel( Instrument );
#else
			mGain.VisibleObject.SetActive( false );

			mVolume.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.Volume = value / 100f;
					mVolume.Text.text = $"{value}%";
				},
				Instrument.InstrumentData.Volume * 100f );

			mMixerGroupVolume.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.MixerGroupVolume = value;
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerVolumeName}{Instrument.InstrumentIndex}", parameterValue: value );
					mMixerGroupVolume.Text.text = $"{value:0.00}dB";
				},
				Instrument.InstrumentData.MixerGroupVolume );

			mMixerGroupVolume.VisibleObject.SetActive( true );
			mFXPanel.InitializeFXPanel( Instrument );
			mSynthSettingsUIPanelPanel.Initialize( Instrument, mUIManager );
#endif //FMOD_ENABLED

			mOddsOfPlayingChordNote.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.OddsOfUsingChordNotes = value;
					mOddsOfPlayingChordNote.Text.text = $"{value}%";
				},
				Instrument.InstrumentData.OddsOfUsingChordNotes );

			mFXToggle.Initialize( ( value ) =>
				{
					//mInstrumentPanelVisibleObject.SetActive( value == false );
#if FMOD_ENABLED
					mFmodInstrumentEffectVisibleObject.SetActive( value );
					mSynthToggle.Option.isOn = false;
					mSynthToggle.gameObject.SetActive( false );
#else
					mInstrumentEffectVisibleObject.SetActive( value );
					if ( value )
					{
						mSynthToggle.Option.isOn = false;
					}
					else
					{
						mInstrumentPanelVisibleObject.SetActive( true );
					}
#endif // FMOD_ENABLED
				},
				initialValue: false );

			mSynthToggle.Initialize( value =>
				{
					mSynthPanelVisibleObject.SetActive( value );
					if ( value )
					{
						mFXToggle.Option.isOn = false;
					}
				},
				initialValue: false );

			mOctavesToUse.Clear();
			AddOctaveListener( ref mOctave1, 0 );
			AddOctaveListener( ref mOctave2, 1 );
			AddOctaveListener( ref mOctave3, 2 );

			//update this last as it enables/disables other objects
			mSuccession.Initialize( ( value ) =>
				{
					var successionType = ( SuccessionType )value;
					SetSuccessionType( ( int )value );
					LeadOctaveContiguousCheck( successionType == SuccessionType.Lead );
					if ( successionType == SuccessionType.Lead )
					{
						mStrumLength.Option.value = 0;
					}

					Instrument.InstrumentData.SuccessionType = ( SuccessionType )value;
					mSuccession.Text.text = $"{Instrument.InstrumentData.SuccessionType}";
				},
				( int )Instrument.InstrumentData.SuccessionType,
				resetValue: null,
				createDividers: true );
			mSuccession.Option.onValueChanged.AddListener( _ => mUIManager.DirtyEditorDisplays() );

			UpdateTitleColor();

			mListenersHaveInitialized = true;
		}

		#endregion protected

		#region private

		/// <summary>
		/// List of octaves we're currently using
		/// </summary>
		private readonly List<int> mOctavesToUse = new List<int>();

		[SerializeField, Tooltip( "Reference to our FX Panel" )]
		private InstrumentFXPanelUI mFXPanel;

		[SerializeField, Tooltip( "Reference to our synth settings panel" )]
		private SynthSettingsUIPanel mSynthSettingsUIPanelPanel;

		[SerializeField, Tooltip( "Reference to our FMOD FX Panel" )]
		private FmodInstrumentFXPanelUI mFmodFXPanel;

		[SerializeField, Tooltip( "Reference to our octaves game object" )]
		private GameObject mOctavesParent;

		/// <summary>
		/// Whether our listeners have initialized (to avoid duplicated calls)
		/// </summary>
		private bool mListenersHaveInitialized;

		[SerializeField, Tooltip( "Reference to our FX Toggle" )]
		private UIToggle mFXToggle;

		[SerializeField, Tooltip( "Reference to our Synth Toggle" )]
		private UIToggle mSynthToggle;

		[SerializeField, Tooltip( "Reference to our Synth Panel Object" )]
		private GameObject mSynthPanelVisibleObject;

		[SerializeField, Tooltip( "Reference to our Instrument Panel Object" )]
		private GameObject mInstrumentPanelVisibleObject;

		[SerializeField, Tooltip( "Reference to our FX Panel Object" )]
		private GameObject mInstrumentEffectVisibleObject;

		[SerializeField, Tooltip( "Reference to our FX Panel Object" )]
		private GameObject mFmodInstrumentEffectVisibleObject;

		[SerializeField, Tooltip( "Reference to our Title image" )]
		private Image mInstrumentTitleImage;

		[SerializeField, Tooltip( "Reference to our Title text" )]
		private UITextField mInstrumentTitleText;

		[SerializeField, Tooltip( "Reference to our odds of playing slider" )]
		private UISlider mOddsOfPlaying;

		[SerializeField, Tooltip( "Reference to our Volume slider" )]
		private UISlider mVolume;

		[SerializeField, Tooltip( "Reference to our Volume slider" )]
		private UISlider mGain;

		[SerializeField, Tooltip( "Reference to our pattern length slider" )]
		private UISlider mPatternLength;

		[SerializeField, Tooltip( "Reference to our pattern repeat slider" )]
		private UISlider mPatternRepeat;

		[SerializeField, Tooltip( "Reference to our strum variation slider" )]
		private UISlider mStrumVariation;

		[SerializeField, Tooltip( "Reference to our odds of playing chord slider" )]
		private UISlider mOddsOfPlayingChordNote;

		[SerializeField, Tooltip( "Reference to our successive play odds slider" )]
		private UISlider mSuccessivePlayOdds;

		[SerializeField, Tooltip( "Reference to our lead mad steps slider" )]
		private UISlider mLeadMaxSteps;

		[SerializeField, Tooltip( "Reference to our succession type slider" )]
		private UISlider mSuccession;

		[SerializeField, Tooltip( "Reference to our manual beat toggle" )]
		private UIToggle mManualBeat;

		[SerializeField, Tooltip( "Reference to our timestep slider" )]
		private UISlider mTimestep;

		[SerializeField, Tooltip( "Reference to our manual beat object" )]
		private GameObject mManualBeatVisibleObject;

		[SerializeField, Tooltip( "Reference to our list of manual beat toggles" )]
		private List<UIToggle> mManualBeatToggles;

		[SerializeField, Tooltip( "Reference to our Manual Beat Toggle Grid" )]
		private GridLayoutGroup mManualGridLayout;

		[SerializeField, Tooltip( "Reference to our minimum melodic rhythm slider" )]
		private UISlider mMinMelodicRhythmTimestep;

		[SerializeField, Tooltip( "Reference to our pattern toggle" )]
		private UIToggle mPattern;

		[SerializeField, Tooltip( "Reference to the pattern group game object" )]
		private GameObject mPatternParent;

		[SerializeField, Tooltip( "Reference to our stereo pan slider" )]
		private UISlider mStereoPan;

		[SerializeField, Tooltip( "Reference to our mixer group Volume slider" )]
		private UISlider mMixerGroupVolume;

		[SerializeField, Tooltip( "Reference to our pattern release slider" )]
		private UISlider mPatternRelease;

		[SerializeField, Tooltip( "Reference to our Sevenths slider" )]
		private UIToggle mUseSevenths;

		[SerializeField, Tooltip( "Reference to our Strum Length slider" )]
		private UISlider mStrumLength;

		[SerializeField, Tooltip( "Reference to our Reverse Strum Toggle" )]
		private UIToggle mReverseStrumToggle;

		[SerializeField, Tooltip( "Reference to our lead variation slider" )]
		private UISlider mLeadVariation;

		[SerializeField, Tooltip( "Reference to our first octave Toggle" )]
		private UIToggle mOctave1;

		[SerializeField, Tooltip( "Reference to our second octave Toggle" )]
		private UIToggle mOctave2;

		[SerializeField, Tooltip( "Reference to our third octave Toggle" )]
		private UIToggle mOctave3;

		[SerializeField, Tooltip( "Reference to our arpeggio Toggle" )]
		private UIToggle mArpeggio;

		[SerializeField, Tooltip( "Reference to our arpeggio repeat slider" )]
		private UISlider mArpeggioRepeat;

		[SerializeField, Tooltip( "Reference to our arpeggio length slider" )]
		private UISlider mNumStrumNotes;

		[SerializeField, Tooltip( "Reference to our pentatonic Toggle" )]
		private UIToggle mPentatonic;

		[SerializeField, Tooltip( "Reference to our keep melodic rhythm Toggle" )]
		private UIToggle mKeepMelodicRhythm;

		[SerializeField, Tooltip( "Reference to our lead avoid step Toggles" )]
		private UIToggle[] mLeadAvoidSteps;

		[SerializeField, Tooltip( "Parent game object to pentatonic avoid toggles" )]
		private GameObject mPentatonicParent;

		[SerializeField, Tooltip( "Reference to our Vertical layout group for succession groups" )]
		private VerticalLayoutGroup mSuccessionGroup;

		[SerializeField, Tooltip( "Reference to our rect transform for succession" )]
		private RectTransform mSuccessionRect;

		[SerializeField, Tooltip( "Reference to our vertical layout group for pattern variables" )]
		private VerticalLayoutGroup mPatternGroup;

		[SerializeField, Tooltip( "Reference to our rect transform for pattern variables" )]
		private RectTransform mPatternRect;

		[SerializeField, Tooltip( "Height of our elements" )]
		private float mElementHeight = 40f;

		/// <summary>
		/// Updates our title color
		/// </summary>
		private void UpdateTitleColor()
		{
			var titleColor = mUIManager.Colors[( int )Instrument.InstrumentData.StaffPlayerColor];
			mInstrumentTitleText.SetTextAndColor( Instrument.InstrumentData.InstrumentName, MusicConstants.InvertTextColor( titleColor ) );

			mInstrumentTitleImage.color = titleColor;
		}

		/// <summary>
		/// Sets our lead avoid note listeners. 
		/// </summary>
		/// <param name="initialize"></param>
		private void SetLeadAvoidNoteListeners( bool initialize )
		{
			var avoidNotes = Instrument.InstrumentData.LeadAvoidNotes;
			var firstAvoid = avoidNotes[0] >= 0 ? MusicConstants.SafeLoop( avoidNotes[0], 0, MusicConstants.ScaleLength ) : -1;
			var secondAvoid = avoidNotes[1] >= 0 ? MusicConstants.SafeLoop( avoidNotes[1], 0, MusicConstants.ScaleLength ) : -1;

			for ( var avoidIndex = 0; avoidIndex < mLeadAvoidSteps.Length; avoidIndex++ )
			{
				var isOn = avoidIndex == firstAvoid || avoidIndex == secondAvoid;
				var toggleIndex = avoidIndex;
				if ( initialize )
				{
					mLeadAvoidSteps[avoidIndex].Initialize( ( value ) => { OnLeadAvoidValueChanged( value, toggleIndex ); }, initialValue: isOn );
					mLeadAvoidSteps[avoidIndex].Option.onValueChanged.AddListener( _ => mUIManager.DirtyEditorDisplays() );
				}
				else
				{
					mLeadAvoidSteps[avoidIndex].Option.isOn = isOn;
				}
			}
		}

		/// <summary>
		/// Adds an octave listener. We want to enforce various conditions on which octaves can be enabled/disabled
		/// </summary>
		/// <param name="toggle"></param>
		/// <param name="index"></param>
		private void AddOctaveListener( ref UIToggle toggle, int index )
		{
			toggle.Initialize( ( value ) =>
				{
					var contains = mOctavesToUse.Contains( index );
					switch ( value )
					{
						case false when contains:
							mOctavesToUse.Remove( index );
							break;
						case true when contains == false:
							mOctavesToUse.Add( index );
							break;
					}

					// safety check. We have to use at least one octave
					if ( mOctavesToUse.Count == 0 )
					{
						mOctavesToUse.Add( 0 );
						mOctave1.Option.isOn = true;
					}

					LeadOctaveContiguousCheck( Instrument.InstrumentData.SuccessionType == SuccessionType.Lead );

					Instrument.InstrumentData.OctavesToUse.Clear();
					foreach ( var octave in mOctavesToUse )
					{
						Instrument.InstrumentData.OctavesToUse.Add( octave );
					}
				},
				Instrument.InstrumentData.OctavesToUse.Contains( index ) );
		}

		/// <summary>
		/// Enforces contiguity for lead instruments 
		/// </summary>
		private void LeadOctaveContiguousCheck( bool isLead )
		{
			if ( isLead == false )
			{
				return;
			}

			if ( mOctavesToUse.Contains( 0 ) && mOctavesToUse.Contains( 2 ) )
			{
				mOctave2.Option.isOn = true;
			}
		}

		/// <summary>
		/// Invoked when lead avoid changes, to handle changes to other relevant values 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="avoidIndex"></param>
		private void OnLeadAvoidValueChanged( bool value, int avoidIndex )
		{
			if ( value )
			{
				// We only want 2 avoid notes for any pentatonic scale.
				// we're enforcing here by just grabbing the first two.
				// TODO: Something more elegant? This is pretty bad
				var dataIndex = 0;
				for ( var avoidStepIndex = 0; avoidStepIndex < mLeadAvoidSteps.Length; avoidStepIndex++ )
				{
					if ( mLeadAvoidSteps[avoidStepIndex].Option.isOn == false )
					{
						continue;
					}

					if ( dataIndex < MusicConstants.NumPentatonicAvoids )
					{
						var avoidNote = avoidStepIndex;
						Instrument.InstrumentData.LeadAvoidNotes[dataIndex] = avoidNote;
						dataIndex++;
					}
					else
					{
						mLeadAvoidSteps[avoidStepIndex].Option.isOn = false;
					}
				}
			}
			else
			{
				if ( Instrument.InstrumentData.LeadAvoidNotes[0] == avoidIndex )
				{
					Instrument.InstrumentData.LeadAvoidNotes[0] = -1;
				}
				else if ( Instrument.InstrumentData.LeadAvoidNotes[1] == avoidIndex )
				{
					Instrument.InstrumentData.LeadAvoidNotes[1] = -1;
				}
			}
		}

		/// <summary>
		/// Invoked when pentatonic state changes to handle relevant changes
		/// </summary>
		private void OnPentatonicStateChanged()
		{
			if ( Instrument.InstrumentData.IsPentatonic == false )
			{
				return;
			}

			foreach ( var toggle in mLeadAvoidSteps )
			{
				toggle.Option.isOn = false;
			}

			// If our 'standard' pentatonic is enabled, force values to generic pentatonic.
			var newPentatonic = mMusicGenerator.ConfigurationData.Scale == Scale.Major ||
			                    mMusicGenerator.ConfigurationData.Scale == Scale.HarmonicMajor ?
				MusicConstants.MajorPentatonicAvoid :
				MusicConstants.MinorPentatonicAvoid;

			for ( var index = 0; index < mLeadAvoidSteps.Length; index++ )
			{
				var isOn =
					index == MusicConstants.SafeLoop( newPentatonic[0], 0, MusicConstants.ScaleLength ) ||
					index == MusicConstants.SafeLoop( newPentatonic[1], 0, MusicConstants.ScaleLength );

				mLeadAvoidSteps[index].Option.isOn = isOn;
			}
		}

		/// <summary>
		/// Sets our succession type. We enabled/disable a ton of dependent elements conditionally here
		/// </summary>
		/// <param name="successionType"></param>
		private void SetSuccessionType( int successionType )
		{
			var isRhythm = successionType == ( int )SuccessionType.Rhythm;
			var isLead = successionType == ( int )SuccessionType.Lead;
			var isPercussion = Instrument.InstrumentData.IsPercussion;
			var forcedBeats = Instrument.InstrumentData.ForceBeat;
			var activeElements = 1;

			mOddsOfPlaying.VisibleObject.SetActive( isRhythm == false );
			mSuccessivePlayOdds.VisibleObject.SetActive( isRhythm == false );
			activeElements += isRhythm == false ? 2 : 0;

			var keepMelodicRhythm = isRhythm == false && forcedBeats == false;
			mKeepMelodicRhythm.VisibleObject.SetActive( keepMelodicRhythm );
			activeElements += keepMelodicRhythm ? 1 : 0;

			mManualBeat.VisibleObject.SetActive( isRhythm || Instrument.InstrumentData.KeepMelodicRhythm == false );
			activeElements += isRhythm || Instrument.InstrumentData.KeepMelodicRhythm == false ? 1 : 0;

			mManualBeatVisibleObject.SetActive( ( isRhythm || keepMelodicRhythm == false ) && forcedBeats );
			activeElements += ( isRhythm || keepMelodicRhythm == false ) && forcedBeats ? 1 : 0;

			mMinMelodicRhythmTimestep.VisibleObject.SetActive( isRhythm == false && mKeepMelodicRhythm.Option.isOn && forcedBeats == false );
			activeElements += isRhythm == false && mKeepMelodicRhythm.Option.isOn && forcedBeats == false ? 1 : 0;

			mPentatonic.VisibleObject.SetActive( isLead );
			activeElements += isLead ? 1 : 0;

			mPentatonicParent.SetActive( isLead && mPentatonic.Option.isOn == false );
			activeElements += isLead && mPentatonic.Option.isOn == false ? 1 : 0;

			mLeadMaxSteps.VisibleObject.SetActive( isLead && isPercussion == false );
			mLeadVariation.VisibleObject.SetActive( isLead && isPercussion == false );
			activeElements += isLead && isPercussion == false ? 2 : 0;

			mOddsOfPlayingChordNote.VisibleObject.SetActive( Instrument.InstrumentData.IsPercussion == false );
			activeElements += Instrument.InstrumentData.IsPercussion == false ? 1 : 0;

			mNumStrumNotes.VisibleObject.SetActive( isLead == false );
			mStrumLength.VisibleObject.SetActive( isLead == false );
			mStrumVariation.VisibleObject.SetActive( isLead == false );
			activeElements += isLead == false ? 3 : 0;

			mReverseStrumToggle.VisibleObject.SetActive( isLead == false && isPercussion == false );
			activeElements += isLead == false && isPercussion == false ? 1 : 0;

			mArpeggio.VisibleObject.SetActive( isLead == false && isPercussion == false );
			activeElements += isLead == false && isPercussion == false ? 1 : 0;

			mArpeggioRepeat.VisibleObject.SetActive( isLead == false && mArpeggio.Option.isOn && isPercussion == false );
			activeElements += isLead == false && mArpeggio.Option.isOn && isPercussion == false ? 1 : 0;

			mOctavesParent.SetActive( isPercussion == false );

			var padding = mSuccessionGroup.padding;
			mSuccessionRect.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Vertical,
				( activeElements - 1 ) * mSuccessionGroup.spacing +
				activeElements * mElementHeight +
				padding.top +
				padding.bottom
			);

			if ( mUIManager.MeasureEditorIsActive )
			{
				mUIManager.UIMeasureEditor.ToggleHelperNotes();
			}

			mPatternParent.SetActive( isLead == false && isPercussion == false );
		}

		private void InitializeManualBeatToggles()
		{
			for ( var index = 0; index < mManualBeatToggles.Count; index++ )
			{
				var beatIndex = index;
				mManualBeatToggles[index].Initialize( ( value ) => { Instrument.InstrumentData.ForcedBeats[beatIndex] = value; },
					Instrument.InstrumentData.ForcedBeats[beatIndex] );
			}
		}

		private void ToggleManualBeat( bool isEnabled )
		{
			Instrument.InstrumentData.ForceBeat = isEnabled;
			if ( isEnabled == false )
			{
				SetSuccessionType( ( int )Instrument.InstrumentData.SuccessionType );
				return;
			}

			var numBeats = 0;
			switch ( mMusicGenerator.InstrumentSet.TimeSignature.Signature )
			{
				case TimeSignatures.ThreeFour:
					numBeats = 12;
					break;
				case TimeSignatures.FourFour:
					numBeats = 16;
					break;
				case TimeSignatures.FiveFour:
					numBeats = 20;
					break;
			}

			mManualGridLayout.constraintCount = numBeats / 2;

			for ( var index = 0; index < 20; index++ )
			{
				mManualBeatToggles[index].Option.isOn = Instrument.InstrumentData.ForcedBeats[index];
				mManualBeatToggles[index].gameObject.SetActive( index < numBeats );
			}

			SetSuccessionType( ( int )Instrument.InstrumentData.SuccessionType );
		}

		private void ResetTopToggles()
		{
			mSynthToggle.Option.isOn = false;
			mFXToggle.Option.isOn = false;
			mInstrumentEffectVisibleObject.SetActive( false );
			mSynthPanelVisibleObject.SetActive( false );
			mInstrumentPanelVisibleObject.SetActive( true );
		}

		private void HideAllPanels()
		{
			mInstrumentPanelVisibleObject.SetActive( false );
			mInstrumentEffectVisibleObject.SetActive( false );
			mSynthPanelVisibleObject.SetActive( false );
			mFmodInstrumentEffectVisibleObject.SetActive( false );
		}

		private void OnTitleChanged( string title )
		{
			Instrument.InstrumentData.InstrumentName = title;
			mUIManager.DirtyEditorDisplays();
		}

		#endregion private
	}
}
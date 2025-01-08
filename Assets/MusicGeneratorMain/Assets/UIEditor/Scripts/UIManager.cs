using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Global UI manager
	/// </summary>
	public class UIManager : MonoBehaviour
	{
		/// <summary>
		/// Reference to the Music Generator
		/// </summary>
		public MusicGenerator MusicGenerator => mMusicGenerator;

		/// <summary>
		/// Reference to the UI Measure Editor
		/// </summary>
		public UIMeasureEditor UIMeasureEditor => mUIMeasureEditor;

		/// <summary>
		/// Reference to the Tooltip Manager 
		/// </summary>
		public TooltipManager TooltipManager => mTooltipManager;

		/// <summary>
		/// Reference to the InstrumentListPanelUI
		/// </summary>
		public InstrumentListPanelUI InstrumentListPanelUI => mInstrumentListPanelUI;

		/// <summary>
		/// Reference to the Instrument Panel UI
		/// </summary>
		public InstrumentPanelUI InstrumentPanelUI => mInstrumentPanelUI;

		/// <summary>
		/// Reference to the UIKeyboard
		/// </summary>
		public UIKeyboard UIKeyboard => mUIKeyboard;

		/// <summary>
		/// Reference to the Measure Editor
		/// </summary>
		public MeasureEditor MeasureEditor => mMeasureEditor;

		/// <summary>
		/// Reference to the standard list of colors (not theme based)
		/// </summary>
		public List<Color> Colors => mColors;

		/// <summary>
		/// Reference to the leitmotif editor
		/// </summary>
		public LeitmotifEditor LeitmotifEditor => mLeitmotifEditor;

		/// <summary>
		/// Reference to the UI Leitmotif Editor
		/// </summary>
		public UILeitmotifEditor UILeitmotifEditor => mUILeitmotifEditor;

		/// <summary>
		/// Reference to our Waveform Editor
		/// </summary>
		public WaveformEditor WaveformEditor => mWaveformEditor;

		/// <summary>
		/// Reference to our Envelope Editor
		/// </summary>
		public EnvelopeEditor EnvelopeEditor => mEnvelopeEditor;

		/// <summary>
		/// Reference to our Panvelope Editor
		/// </summary>
		public PanvelopeEditor PanvelopeEditor => mPanvelopeEditor;

		/// <summary>
		/// Reference to our UIWaveformEditor
		/// </summary>
		public UIWaveformEditor UIWaveformEditor => mUIWaveformEditor;

		/// <summary>
		/// Reference to our bezier editor panel
		/// </summary>
		public BezierEditorPanel BezierEditorPanel => mBezierEditorPanel;

		/// <summary>
		/// Reference to the Percussion Editor
		/// </summary>
		public PercussionEditor PercussionEditor => mPercussionEditor;

		/// <summary>
		/// Reference to the UI Percussion Editor
		/// </summary>
		public UIPercussionEditor UIPercussionEditor => mUIPercussionEditor;

		/// <summary>
		/// Reference to the General Menu Panel
		/// </summary>
		public GeneralMenuPanel GeneralMenuPanel => mGeneralMenuPanel;

		/// <summary>
		/// Reference to the UI Editor Settings
		/// </summary>
		public UIEditorSettings UIEditorSettings => mUIEditorSettings;

		/// <summary>
		/// Reference to the UIModsEditor
		/// </summary>
		public UIModsEditor UIModsEditor => mUIModsEditor;

		/// <summary>
		/// Reference to the FX Settings
		/// </summary>
		public UIEditorFXSettings FXSettings => mFXSettings;

		/// <summary>
		/// Current MouseWorld point (int 2D space)
		/// </summary>
		public Vector2 MouseWorldPoint { get; private set; }

		/// <summary>
		/// Current mouse screen point
		/// </summary>
		public Vector2 MouseScreenPoint { get; private set; }

		/// <summary>
		/// Current Active Time Signature (depending on open display editor may refer to different instrument sets.
		/// </summary>
		public TimeSignatures ActiveTimeSignature => CurrentInstrumentSet.TimeSignature.Signature;

		/// <summary>
		/// Whether the Measure Editor is active
		/// </summary>
		public bool MeasureEditorIsActive => mUIMeasureEditor.State > DisplayEditorState.Inactive;

		public bool PercussionEditorIsActive => mUIPercussionEditor.State > DisplayEditorState.Inactive;

		/// <summary>
		/// Reference to our current Palette's color fields
		/// </summary>
		public IReadOnlyList<ColorPalette.UIColorField> CurrentColors => mUIEditorSettings.CurrentPalette.ColorFields;

		/// <summary>
		/// Whether left shift is held down (referenced here to avoid repeated calls to the input manager
		/// </summary>
		public bool LeftShiftIsDown { get; private set; }

		/// <summary>
		/// Left Click Down Event
		/// </summary>
		public UnityEvent OnLeftClickDown { get; } = new UnityEvent();

		/// <summary>
		/// Left Click Up event
		/// </summary>
		public UnityEvent OnLeftClickUp { get; } = new UnityEvent();

		/// <summary>
		/// Left Shift Down Event
		/// </summary>
		public UnityEvent OnLeftShiftDown { get; } = new UnityEvent();

		/// <summary>
		/// Left Shift Up Event
		/// </summary>
		public UnityEvent OnLeftShiftUp { get; } = new UnityEvent();

		/// <summary>
		/// Reference to our current InstrumentSet (depending on open display editor may refer to various instrument sets
		/// </summary>
		public InstrumentSet CurrentInstrumentSet => mIsUsingMeasureEditor ? mUIMeasureEditor.InstrumentSet : mMusicGenerator.InstrumentSet;

		public bool ExportIsFocused => mPlayerController.ExportIsHovered ||
		                               mPercussionEditorController.ExportIsHovered ||
		                               mLeitmotifController.ExportIsHovered ||
		                               mMeasureEditorController.ExportIsHovered;

		/// <summary>
		/// Whether we're currently editing a slider value. I hate that I'm using a global static here...
		/// </summary>
		private static bool mIsEditingSlider;

		private static bool mUnlockSlider;

		public static void LockSliderInput()
		{
			mIsEditingSlider = true;
		}

		public static void UnlockSlider()
		{
			mUnlockSlider = true;
		}

		/// <summary>
		/// Toggles post processing on/off
		/// </summary>
		/// <param name="isEnabled"></param>
		public void SetPostProcessingEnabled( bool isEnabled )
		{
			mPostProcessingVolume.gameObject.SetActive( isEnabled );
			mPostProcessLayer.enabled = isEnabled;
		}

		/// <summary>
		/// Sets displays as dirty (each will update relevant data next tick)
		/// </summary>
		public void DirtyEditorDisplays()
		{
			mIsDirty = true;
		}

		/// <summary>
		/// Breaks the editor displays (displays will regenerate all needed data)
		/// </summary>
		public void BreakEditorDisplays()
		{
			mIsBroken = true;
		}

		/// <summary>
		/// Enables the leitmotif Editor
		/// </summary>
		/// <param name="isActive"></param>
		/// <returns></returns>
		public IEnumerator EnableLeitmotifEditor( bool isActive )
		{
			mMusicGenerator.Stop();
			mPlayerController.SetPanelActive( isActive == false );
			mLeitmotifController.SetPanelActive( isActive );
			LeitmotifEditor.SetPanelActive( isActive );

			if ( isActive )
			{
				yield return mLeitmotifEditor.InitializeInstruments( CurrentInstrumentSet.Instruments );
			}
		}

		public void EnableWaveformEditor( bool isActive )
		{
#if FMOD_ENABLED == false
			mMusicGenerator.Stop();
			mUIKeyboard.Stop();
			mUIWaveformEditor.SetPanelActive( isActive );
			DirtyEditorDisplays();
#endif // FMOD_ENABLED == false
		}

		/// <summary>
		/// Enables the Percussion Editor
		/// </summary>
		/// <param name="isActive"></param>
		/// <returns></returns>
		public IEnumerator EnablePercussionEditor( bool isActive )
		{
			mMusicGenerator.Stop();
			mPlayerController.SetPanelActive( isActive == false );
			mPercussionEditorController.SetPanelActive( isActive );
			PercussionEditor.SetPanelActive( isActive );

			if ( isActive )
			{
				yield return mPercussionEditor.InitializeInstruments( CurrentInstrumentSet.Instruments );
			}
			else
			{
				mPercussionEditor.DestroyInstruments();
			}
		}

		/// <summary>
		/// Enables the Measure Editor
		/// </summary>
		/// <param name="isActive"></param>
		/// <param name="onComplete"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public IEnumerator EnableMeasureEditor( bool isActive, Action onComplete = null, string configuration = "Default" )
		{
			mMusicGenerator.Stop();
			mPlayerController.SetPanelActive( isActive == false );
			mMeasureEditorController.SetPanelActive( isActive );
			MeasureEditor.SetPanelActive( isActive );
			mIsUsingMeasureEditor = isActive;

			if ( isActive )
			{
				mUIMeasureEditor.SetPanelActive( true );
				yield return mUIMeasureEditor.LoadConfiguration( configuration, () => { } );
			}
			else
			{
				mGeneralMenuPanel.TogglePanel();
				yield return mMusicGenerator.RestoreConfiguration( mCachedConfigurationData );
				yield return ReloadUIConfiguration();
			}


			onComplete?.Invoke();
		}

		/// <summary>
		/// Reloads the UI Configuration (this will update most panel's ui elements
		/// </summary>
		/// <param name="configurationName"></param>
		/// <param name="loadNewConfig"></param>
		/// <param name="onCompleteState"></param>
		/// <returns></returns>
		public IEnumerator ReloadUIConfiguration( string configurationName = null,
			bool loadNewConfig = false,
			GeneratorState onCompleteState = GeneratorState.Ready )
		{
			if ( loadNewConfig )
			{
				mMusicGenerator.ClearInstruments( mMusicGenerator.InstrumentSet );
				yield return mMusicGenerator.LoadConfiguration( configurationName, GeneratorState.Initializing );
			}
			else
			{
				mMusicGenerator.ResetPlayer();
			}

			mInstrumentListPanelUI.ReloadInstruments();
			mPlayerController.UpdateUIElementValues();
			mInstrumentListPanelUI.SelectBestInstrument();
			mGeneratorUIPanel.UpdateUIElementValues();
			mUILeitmotifEditor.UpdateUIElementValues();
			mUIMeasureEditor.UpdateUIElementValues();
			mLeitmotifController.UpdateUIElementValues();
			mMeasureEditorController.UpdateUIElementValues();
			mMusicGenerator.SetState( onCompleteState );
			yield return null;
		}

		/// <summary>
		/// Caches the configuration data
		/// </summary>
		/// <param name="data"></param>
		public void SetCachedConfigurationData( ConfigurationData data )
		{
			mCachedConfigurationData = data;
		}

		[SerializeField, Tooltip( "Reference to our MusicGenerator" )]
		private MusicGenerator mMusicGenerator;

		[SerializeField, Tooltip( "Reference to our GeneralMenuPanel" )]
		private GeneralMenuPanel mGeneralMenuPanel;

		[SerializeField, Tooltip( "Reference to our UIMeasureEditor" )]
		private UIMeasureEditor mUIMeasureEditor;

		[SerializeField, Tooltip( "Reference to our MeasureEditor" )]
		private MeasureEditor mMeasureEditor;

		[SerializeField, Tooltip( "Reference to our GeneralUIPanel" )]
		private MusicGeneratorUIPanel mGeneratorUIPanel;

		[SerializeField, Tooltip( "Reference to our TooltipManager" )]
		private TooltipManager mTooltipManager;

		[SerializeField, Tooltip( "Reference to our AdvancedSettingsPanel" )]
		private AdvancedSettingsPanel mAdvancedSettingsPanel;

		[SerializeField, Tooltip( "Reference to our GlobalEffectsPanel" )]
		private GlobalEffectsPanel mGlobalEffectsPanel;

		[SerializeField, Tooltip( "Reference to our InstrumentListPanelUI" )]
		private InstrumentListPanelUI mInstrumentListPanelUI;

		[SerializeField, Tooltip( "Reference to our InstrumentPanelUI" )]
		private InstrumentPanelUI mInstrumentPanelUI;

		[SerializeField, Tooltip( "Reference to our PlayerController" )]
		private PlayerController mPlayerController;

		[SerializeField, Tooltip( "Reference to our MeasureEditorController" )]
		private MeasureEditorController mMeasureEditorController;

		[SerializeField, Tooltip( "Reference to our UIKeyboard" )]
		private UIKeyboard mUIKeyboard;

		[SerializeField, Tooltip( "Reference to our List of ui colors" )]
		private List<Color> mColors = new List<Color>();

		[SerializeField, Tooltip( "Reference to our PanelActivator" )]
		private PanelActivator mPanelActivator;

		[SerializeField, Tooltip( "Reference to our Main Camera" )]
		private Camera mMainCamera;

		[SerializeField, Tooltip( "Reference to our LeitmotifEditor" )]
		private LeitmotifEditor mLeitmotifEditor;

		[SerializeField, Tooltip( "Reference to our UILeitmotifEditor" )]
		private UILeitmotifEditor mUILeitmotifEditor;

		[SerializeField, Tooltip( "Reference to our Waveform Editor" )]
		private WaveformEditor mWaveformEditor;

		[SerializeField, Tooltip( "Reference to our UI Waveform Editor" )]
		private UIWaveformEditor mUIWaveformEditor;

		[SerializeField]
		private BezierEditorPanel mBezierEditorPanel;

		[SerializeField, Tooltip( "Reference to our Envelope Editor" )]
		private EnvelopeEditor mEnvelopeEditor;

		[SerializeField, Tooltip( "Reference to our PanvelopeEditor" )]
		private PanvelopeEditor mPanvelopeEditor;

		[SerializeField, Tooltip( "Reference to our PercussionEditor" )]
		private PercussionEditor mPercussionEditor;

		[SerializeField, Tooltip( "Reference to our UIPercussionEditor" )]
		private UIPercussionEditor mUIPercussionEditor;

		[SerializeField, Tooltip( "Reference to our PercussionEditorController" )]
		private PercussionEditorController mPercussionEditorController;

		[SerializeField, Tooltip( "Reference to our LeitmotifController" )]
		private LeitmotifController mLeitmotifController;

		[SerializeField, Tooltip( "Reference to our UIEditorSettings" )]
		private UIEditorSettings mUIEditorSettings;

		[SerializeField, Tooltip( "Reference to our Mods Panel" )]
		private UIModsEditor mUIModsEditor;

		[SerializeField, Tooltip( "Reference to our UIEditorFXSettings" )]
		private UIEditorFXSettings mFXSettings;

		[SerializeField, Tooltip( "Reference to our PostProcessLayer" )]
		private PostProcessLayer mPostProcessLayer;

		[SerializeField, Tooltip( "Reference to our PostProcessing Volume" )]
		private GameObject mPostProcessingVolume;

		[SerializeField, Tooltip( "Reference to our keybinds" )]
		private UIKeybinds mKeybinds;

		/// <summary>
		/// Cached Configuration Data (we cache this as certain editor displays will need to restore the configuration data upon exit
		/// </summary>
		private ConfigurationData mCachedConfigurationData;

		/// <summary>
		/// Whether displays are currently dirty
		/// </summary>
		private bool mIsDirty;

		/// <summary>
		/// Whether displays are currently broken
		/// </summary>
		private bool mIsBroken;

		/// <summary>
		/// Whether we're currently using the measure editor
		/// </summary>
		private bool mIsUsingMeasureEditor;

		/// <summary>
		/// Awake
		/// </summary>
		private void Awake()
		{
			mMusicGenerator.Started.AddListener( OnStarted );
			mMusicGenerator.HasVisiblePlayer += OnHasVisiblePlayer;
			mIsEditingSlider = false;
			mUnlockSlider = false;
		}

		/// <summary>
		/// OnStarted
		/// </summary>
		private void OnStarted()
		{
			mMusicGenerator.VolumeFaded.AddListener( OnVolumeFaded );
			mMusicGenerator.InstrumentsCleared.AddListener( OnInstrumentsCleared );
			mMusicGenerator.KeyChanged.AddListener( OnKeyChanged );
			StartCoroutine( Initialize() );
		}

		/// <summary>
		/// OnDestroy
		/// </summary>
		private void OnDestroy()
		{
			if ( mMusicGenerator == false )
			{
				return;
			}

			mMusicGenerator.VolumeFaded.RemoveListener( OnVolumeFaded );
			mMusicGenerator.InstrumentsCleared.RemoveListener( OnInstrumentsCleared );
			mMusicGenerator.KeyChanged.RemoveListener( OnKeyChanged );
		}

		private void FixedUpdate()
		{
			UIKeyboard.DoUpdate( Time.deltaTime );
		}

		/// <summary>
		/// Update loop
		/// </summary>
		private void Update()
		{
			CheckInput();

			if ( mIsBroken )
			{
				mUILeitmotifEditor.BreakDisplay();
				mUIPercussionEditor.BreakDisplay();
				mUIMeasureEditor.BreakDisplay();
				mGeneratorUIPanel.BreakDisplay();
				mInstrumentPanelUI.BreakDisplay();
			}

			if ( mIsDirty )
			{
				mInstrumentPanelUI.UpdateUIElementValues();
				mInstrumentListPanelUI.UpdateUIElementValues();
				mUIEditorSettings.UpdateUIElementValues();
				mUILeitmotifEditor.DirtyDisplay();
				mUIPercussionEditor.DirtyDisplay();
				mUIMeasureEditor.DirtyDisplay();
				mUIKeyboard.DirtyDisplay();
				mUIModsEditor.UpdateUIElementValues();

#if FMOD_ENABLED == false
				mUIWaveformEditor.UpdateUIElementValues();
#endif //FMOD_ENABLED == false
			}

			mIsDirty = false;
			mIsBroken = false;


			if ( mMusicGenerator.GeneratorState != GeneratorState.Initializing &&
			     mUIMeasureEditor.State != DisplayEditorState.Initializing )
			{
				mPanelActivator.DoUpdate();
			}

			if ( mUIMeasureEditor.State >= DisplayEditorState.Initializing )
			{
				switch ( mUIMeasureEditor.State )
				{
					case DisplayEditorState.Playing:
					case DisplayEditorState.Paused:
					case DisplayEditorState.Stopped:
						mMeasureEditor.DoUpdate();
						break;
				}
			}
			else if ( mUILeitmotifEditor.State >= DisplayEditorState.Initializing )
			{
				switch ( mUILeitmotifEditor.State )
				{
					case DisplayEditorState.Playing:
					case DisplayEditorState.Paused:
					case DisplayEditorState.Stopped:
						mLeitmotifEditor.DoUpdate();
						break;
				}
			}
			else if ( mUIPercussionEditor.State >= DisplayEditorState.Initializing )
			{
				switch ( mUIPercussionEditor.State )
				{
					case DisplayEditorState.Playing:
					case DisplayEditorState.Paused:
					case DisplayEditorState.Stopped:
						mPercussionEditor.DoUpdate();
						break;
				}
			}
			else
			{
				switch ( mMusicGenerator.GeneratorState )
				{
					case GeneratorState.Playing:
					case GeneratorState.Repeating:
					case GeneratorState.Stopped:
					case GeneratorState.Paused:
						mPlayerController.DoUpdate();
						break;
				}
			}
		}

		/// <summary>
		/// Initialization
		/// </summary>
		/// <returns></returns>
		private IEnumerator Initialize()
		{
			mTooltipManager.Initialize( this );
			yield return mMusicGenerator.LoadConfiguration( mMusicGenerator.DefaultConfigurationName, GeneratorState.Initializing );
			yield return mGeneralMenuPanel.InitializeRoutine( this );
			mPlayerController.Initialize( this );
			mInstrumentPanelUI.Initialize( this );
			mAdvancedSettingsPanel.Initialize( this );
			mGlobalEffectsPanel.Initialize( this );
			mGeneratorUIPanel.Initialize( this );
			yield return mInstrumentListPanelUI.InitializeRoutine( this );
			mMeasureEditor.Initialize( this );
			mLeitmotifEditor.Initialize( this );
			mPercussionEditor.Initialize( this );
			mUIMeasureEditor.Initialize( this, false );
			mUILeitmotifEditor.Initialize( this, false );
			mUIPercussionEditor.Initialize( this, false );
			mMeasureEditorController.Initialize( this, false );
			mLeitmotifController.Initialize( this, false );
			mPercussionEditorController.Initialize( this, false );
			mUIKeyboard.Initialize( this );
			mPanelActivator.Initialize( this );
			mUIEditorSettings.Initialize( this, false );
			mUIModsEditor.Initialize( this, false );

#if FMOD_ENABLED == false
			mUIWaveformEditor.Initialize( this, false );
#endif //FMOD_ENABLED == false

			yield return ReloadUIConfiguration( mMusicGenerator.DefaultConfigurationName,
				onCompleteState: mMusicGenerator.AutoPlay ? GeneratorState.Playing : GeneratorState.Ready );
		}

		/// <summary>
		/// Returning true here will prevent the MusicGenerator from initializing certain things as it assumes we will
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private static bool OnHasVisiblePlayer( object source, EventArgs args )
		{
			return true;
		}

		/// <summary>
		/// Volume faded event handler
		/// </summary>
		/// <param name="volume"></param>
		private void OnVolumeFaded( float volume )
		{
			mGeneratorUIPanel.FadeVolume( volume );
		}

		/// <summary>
		/// Input updater
		/// </summary>
		private void CheckInput()
		{
			if ( mUnlockSlider )
			{
				// pretty kludgey :/
				// to avoid allowing input on the same frame we unlock the slider input:
				mIsEditingSlider = false;
				mUnlockSlider = false;
				return;
			}

			if ( ExportIsFocused || mIsEditingSlider )
			{
				return;
			}

			LeftShiftIsDown = Input.GetKey( KeyCode.LeftShift );
			MouseScreenPoint = Input.mousePosition;
			MouseWorldPoint = mMainCamera.ScreenToWorldPoint( MouseScreenPoint );

			if ( Input.GetKeyUp( KeyCode.LeftShift ) )
			{
				OnLeftShiftUp.Invoke();
			}

			if ( Input.GetKeyDown( KeyCode.LeftShift ) )
			{
				OnLeftShiftDown.Invoke();
			}

			if ( Input.GetKeyDown( KeyCode.Mouse0 ) )
			{
				OnLeftClickDown.Invoke();
			}

			if ( Input.GetKeyUp( KeyCode.Mouse0 ) )
			{
				OnLeftClickUp.Invoke();
			}

			if ( Input.GetKeyDown( mKeybinds.PlayPause ) )
			{
				if ( mMusicGenerator.GeneratorState == GeneratorState.Paused ||
				     mMusicGenerator.GeneratorState == GeneratorState.Stopped ||
				     mMusicGenerator.GeneratorState == GeneratorState.Ready )
				{
					UIKeyboard.Play();
				}
				else if ( mMusicGenerator.GeneratorState == GeneratorState.Playing ||
				          mMusicGenerator.GeneratorState == GeneratorState.Repeating )
				{
					UIKeyboard.Pause();
				}
			}

			if ( Input.GetKeyDown( mKeybinds.Stop ) &&
			     ( mMusicGenerator.GeneratorState == GeneratorState.Playing ||
			       mMusicGenerator.GeneratorState == GeneratorState.Repeating ) )
			{
				UIKeyboard.Reset();
			}

			if ( Input.GetKeyDown( mKeybinds.MuteSelected ) )
			{
				mInstrumentListPanelUI.SelectedInstrument.InstrumentData.IsMuted = mInstrumentListPanelUI.SelectedInstrument.InstrumentData.IsMuted == false;
				mInstrumentListPanelUI.ToggleMute();
				DirtyEditorDisplays();
			}

			if ( Input.GetKeyDown( mKeybinds.ToggleFX ) )
			{
				mInstrumentPanelUI.ToggleFXPanel();
			}

			if ( Input.GetKeyDown( mKeybinds.SoloSelected ) )
			{
				mInstrumentListPanelUI.SelectedInstrument.InstrumentData.IsSolo = mInstrumentListPanelUI.SelectedInstrument.InstrumentData.IsSolo == false;
				DirtyEditorDisplays();
			}

			if ( Input.GetKeyDown( mKeybinds.ToggleInstrumentPercussion ) )
			{
				mInstrumentListPanelUI.ToggleInstrumentPercussion();
			}

			for ( var index = 0; index < mKeybinds.Instruments.Length; index++ )
			{
				if ( Input.GetKeyDown( mKeybinds.Instruments[index] ) && CurrentInstrumentSet.Instruments.Count > index )
				{
					var instrument = mInstrumentListPanelUI.GetInstrument( index );
					mInstrumentListPanelUI.SelectInstrument( instrument );
					return;
				}
			}
		}

		/// <summary>
		/// Instruments cleared event handler
		/// </summary>
		private void OnInstrumentsCleared()
		{
			if ( mInstrumentListPanelUI )
			{
				mInstrumentListPanelUI.ClearInstruments();
			}
		}

		/// <summary>
		/// Key Changed event handler
		/// </summary>
		/// <param name="key"></param>
		private void OnKeyChanged( int key )
		{
			mGeneratorUIPanel.SetKey( key );
		}
	}
}
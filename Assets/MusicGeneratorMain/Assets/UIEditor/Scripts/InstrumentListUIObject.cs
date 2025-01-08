using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Visual UI/UX for an instrument object appearing in the Instrument List panel. Handles basic functionality: selection, color, mute, solo, etc.
	/// </summary>
	public class InstrumentListUIObject : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Getter for our instrument
		/// </summary>
		public Instrument Instrument { get; private set; }

		/// <summary>
		/// Width of our rect transform for the instrument
		/// </summary>
		public float Width => mRectTransform.rect.width;

		public void Select()
		{
			mIsSelected = true;
			mSelectButton.image.color = mSelectButtonColor;
			mInstrumentListPanelUI.SelectInstrument( Instrument );
			SetSelectionColor( mUIManager.CurrentColors[( int )mSelectedColor].Color );
		}

		public void Deselect()
		{
			mIsSelected = false;
			mSelectButton.image.color = mUnselectButtonColor;
			mInstrumentListPanelUI.DeselectInstrument( Instrument );
			SetSelectionColor( mUIManager.CurrentColors[( int )mUnselectedColor].Color );
		}

		/// <summary>
		/// Updates UI Elements (that could be dependent on changes)
		/// </summary>
		public void UpdateUIElementValues()
		{
			SetSelectionColor( mIsSelected ? mUIManager.CurrentColors[( int )mSelectedColor].Color : mUIManager.CurrentColors[( int )mUnselectedColor].Color );

			mMuteToggle.Option.isOn = Instrument.InstrumentData.IsMuted;
			mLeitmotif.Option.isOn = Instrument.InstrumentData.Leitmotif.IsEnabled;
			mSoloToggle.Option.isOn = Instrument.InstrumentData.IsSolo;
			mColor.Option.Value = mUIManager.Colors[( int )Instrument.InstrumentData.StaffPlayerColor];
			mInstrumentDropdown.Text.SetText( Instrument.InstrumentData.InstrumentName );
		}

		/// <summary>
		/// Toggles mute on/off
		/// </summary>
		/// <param name="isOn"></param>
		public void SetMute( bool isOn )
		{
			mMuteToggle.Option.isOn = isOn;
		}

		/// <summary>
		/// Toggles solo on/off
		/// </summary>
		/// <param name="isOn"></param>
		public void SetSolo( bool isOn )
		{
			mSoloToggle.Option.isOn = isOn;
		}

		/// <summary>
		/// Initializes the instrument object
		/// </summary>
		/// <param name="uIManager"></param>
		/// <param name="instrument"></param>
		public void Initialize( UIManager uIManager, Instrument instrument )
		{
			mUIManager = uIManager;
			mMusicGenerator = mUIManager.MusicGenerator;
			mInstrumentListPanelUI = mUIManager.InstrumentListPanelUI;
			Instrument = instrument;
			mRectTransform.localScale = Vector3.one;
			InitializeListeners();
			AddTooltips();
			SetSelectionColor( mIsSelected ? mUIManager.CurrentColors[( int )mSelectedColor].Color : mUIManager.CurrentColors[( int )mUnselectedColor].Color );
		}

		#endregion public

		#region private

		[Tooltip( "Reference to our mute toggle" )]
		[SerializeField] private UIToggle mMuteToggle;

		[Tooltip( "Reference to our selection background" )]
		[SerializeField] private Image mInstrumentSelectionBackground;

		[Tooltip( "Reference to our instrument background" )]
		[SerializeField] private Image mInstrumentBackground;

		[Tooltip( "Reference to our solo toggle" )]
		[SerializeField] private UIToggle mSoloToggle;

		[Tooltip( "Reference to our instrument dropdown panel" )]
		[SerializeField] private UITMPDropdown mInstrumentDropdown;

		[Tooltip( "Reference to our select toggle" )]
		[SerializeField] private Button mSelectButton;

		[Tooltip( "Reference to our rect transform" )]
		[SerializeField] private RectTransform mRectTransform;

		[Tooltip( "Reference to our color scroller" )]
		[SerializeField] private UIColorScroller mColor;

		[Tooltip( "Color for selected button" )]
		[SerializeField] private Color mSelectButtonColor = Color.red;

		[Tooltip( "Color for unselected button" )]
		[SerializeField] private Color mUnselectButtonColor = Color.gray;

		[Tooltip( "Reference to our UI Drag element for the group" )]
		[SerializeField] private UIInstrumentDragElement mGroup;

		[Tooltip( "Reference to our leitmotif toggle" )]
		[SerializeField] private UIToggle mLeitmotif;

		[Tooltip( "Reference to our forced percussion toggle" )]
		[SerializeField] private UIToggle mForcedPercussion;

		[Tooltip( "Reference to our color field type for selected color" )]
		[SerializeField] private ColorFieldType mSelectedColor = ColorFieldType.UI_3;

		[Tooltip( "Reference to our unselected color field type" )]
		[SerializeField] private ColorFieldType mUnselectedColor = ColorFieldType.Background_3;

		private MusicGenerator mMusicGenerator;
		private InstrumentListPanelUI mInstrumentListPanelUI;
		private UIManager mUIManager;
		private bool mIsSelected;

		/// <summary>
		/// Initializes our ui elements
		/// </summary>
		private void InitializeListeners()
		{
			SetDropdown( Instrument.InstrumentData.IsPercussion );

			mGroup.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.Group = ( value );
					mGroup.Text.text = $"{value + 1}";
					mInstrumentListPanelUI.MoveInstrument( this );
				},
				mUIManager,
				instrumentObject: this,
				scrollView: Instrument.InstrumentData.IsPercussion ? mInstrumentListPanelUI.PercussionScrollView : mInstrumentListPanelUI.InstrumentScrollView );

			mSelectButton.onClick.AddListener( Select );

			mMuteToggle.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.IsMuted = value;
					mUIManager.InstrumentListPanelUI.ToggleMute();
				},
				initialValue: Instrument.InstrumentData.IsMuted );

			mSoloToggle.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.IsSolo = value;
					mUIManager.InstrumentListPanelUI.ToggleSolo( this, value );
				},
				initialValue: Instrument.InstrumentData.IsSolo );

			mColor.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.StaffPlayerColor = ( StaffPlayerColors )mColor.Option.SelectedIndex;
					SetColor( value );
				},
				( int )Instrument.InstrumentData.StaffPlayerColor,
				mUIManager.Colors );
			mColor.Option.OnValueChanged.AddListener( _ => mUIManager.DirtyEditorDisplays() );

			if ( mInstrumentDropdown.gameObject.activeInHierarchy )
			{
				mInstrumentDropdown.Initialize( _ => { StartCoroutine( ChangeInstrument() ); }, initialValue: mInstrumentDropdown.Option.value );
			}

			mLeitmotif.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.Leitmotif.IsEnabled = value;
					mUIManager.DirtyEditorDisplays();
				},
				initialValue: Instrument.InstrumentData.Leitmotif.IsEnabled );

			mForcedPercussion.Initialize( ( value ) =>
				{
					Instrument.InstrumentData.UseForcedPercussion = value;
					mUIManager.DirtyEditorDisplays();
				},
				initialValue: Instrument.InstrumentData.UseForcedPercussion );
			mForcedPercussion.VisibleObject.SetActive( Instrument.InstrumentData.IsPercussion );
		}

		/// <summary>
		/// Adds our tooltips
		/// </summary>
		private void AddTooltips()
		{
			foreach ( var tooltip in GetComponentsInChildren<Tooltip>() )
			{
				tooltip.Initialize( mUIManager );
			}
		}

		/// <summary>
		/// Set our dropdown instrument type
		/// </summary>
		/// <param name="isPercussion"></param>
		private void SetDropdown( bool isPercussion = false )
		{
#if FMOD_ENABLED == false
			mInstrumentDropdown.Option.options.Add( new TMP_Dropdown.OptionData {text = isPercussion ? "P_SYNTH" : MusicConstants.SynthID} );
#endif

			foreach ( var instrumentPath in mMusicGenerator.BaseInstrumentPaths )
			{
				if ( instrumentPath.Contains( "P_" ) != isPercussion )
				{
					continue;
				}

				var data = new TMP_Dropdown.OptionData {text = instrumentPath};
				mInstrumentDropdown.Option.options.Add( data );
			}

			mInstrumentDropdown.Option.value = -1;
			for ( var i = 0; i < mInstrumentDropdown.Option.options.Count; i++ )
			{
				if ( mInstrumentDropdown.Option.options[i].text == Instrument.InstrumentData.InstrumentType )
				{
					mInstrumentDropdown.Option.value = i;
				}
			}

			mInstrumentDropdown.Option.options[mInstrumentDropdown.Option.value].text = Instrument.InstrumentData.InstrumentType;
			mInstrumentDropdown.Text.SetText( Instrument.InstrumentData.InstrumentName );
		}

		/// <summary>
		/// Asynchronously changes the instrument (new music assets may need to be loaded).
		/// </summary>
		/// <returns></returns>
		private IEnumerator ChangeInstrument()
		{
			if ( mMusicGenerator.GeneratorState < GeneratorState.Ready )
			{
				yield break;
			}

			var instrumentType = mInstrumentDropdown.Option.options[mInstrumentDropdown.Option.value].text;
			if ( mMusicGenerator.BaseInstrumentPaths.Contains( instrumentType ) == false )
			{
				instrumentType = MusicConstants.SynthID;
			}

			var instruments = mUIManager.CurrentInstrumentSet.Instruments;
			if ( Instrument != null )
			{
				instruments[Instrument.InstrumentIndex].InstrumentData.InstrumentType = instrumentType;
				mMuteToggle.Option.isOn = instruments[Instrument.InstrumentIndex].InstrumentData.IsMuted;
				mInstrumentDropdown.Title.SetText( Instrument.InstrumentData.InstrumentName );
			}

			if ( mMusicGenerator.HasLoadedInstrument( instrumentType ) == false )
			{
				yield return mMusicGenerator.LoadBaseClips( Instrument );
			}

			mMusicGenerator.RemoveBaseClip( instrumentType );
			mUIManager.DirtyEditorDisplays();
		}

		/// <summary>
		/// Sets our color
		/// </summary>
		/// <param name="color"></param>
		private void SetColor( Color color )
		{
			mInstrumentBackground.color = color;
			mInstrumentDropdown.Text.color = MusicConstants.InvertTextColor( color );
		}

		/// <summary>
		/// Sets our selection background color (selected highlight) 
		/// </summary>
		/// <param name="color"></param>
		private void SetSelectionColor( Color color )
		{
			mInstrumentSelectionBackground.color = color;
		}

		/// <summary>
		/// OnDestroy
		/// </summary>
		private void OnDestroy()
		{
			mColor.Option.OnValueChanged.RemoveAllListeners();
		}

		#endregion private
	}
}
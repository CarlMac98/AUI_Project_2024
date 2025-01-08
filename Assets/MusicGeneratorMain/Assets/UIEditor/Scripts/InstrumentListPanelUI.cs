using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI panel containing a list of instrument objects (ui element with basic controls: mute, solo, instrument type, color, but not the full
	/// list of instrument settings contained in the Instrument Panel ). Handles UX for these instrument objects
	/// 
	/// </summary>
	public class InstrumentListPanelUI : UIPanel
	{
		#region public

		/// <summary>
		/// Event for when percussion is toggled
		/// </summary>
		public class PercussionDidToggle : UnityEvent<bool> { }

		/// <summary>
		/// Invoked upon percussion being toggled
		/// </summary>
		public PercussionDidToggle OnPercussionToggled { get; } = new PercussionDidToggle();

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			foreach ( var instrument in mInstrumentIcons )
			{
				instrument.Value.UpdateUIElementValues();
				if ( instrument.Value.Instrument.InstrumentData.IsSolo )
				{
					ToggleSolo( instrument.Value, true );
				}
			}

			OnGroupsWereChosen();
		}

		///<inheritdoc/>
		public override IEnumerator InitializeRoutine( UIManager uiManager, bool isEnabled = true )
		{
			yield return base.InitializeRoutine( uiManager, isEnabled );
			//yield return StartCoroutine(CreateInstrumentUIObjectBase());
			mInstrumentScrollView.Initialize( mUIManager );
			mPercussionScrollView.Initialize( mUIManager );
			mMusicGenerator.GroupsWereChosen.AddListener( OnGroupsWereChosen );
			mMusicGenerator.StopGenerator.AddListener( OnGroupsWereChosen );
			mMusicGenerator.PlayGenerator.AddListener( OnGroupsWereChosen );
			OnGroupsWereChosen();
		}

		/// <summary>
		/// Getter for our instrument scroll view
		/// </summary>
		public UIInstrumentListScrollView InstrumentScrollView => mInstrumentScrollView;

		/// <summary>
		/// Getter for our percussion scroll view
		/// </summary>
		public UIInstrumentListScrollView PercussionScrollView => mPercussionScrollView;

		/// <summary>
		/// Getter for our dictionary of instrument icons
		/// </summary>
		public IReadOnlyDictionary<Instrument, InstrumentListUIObject> InstrumentIcons => mInstrumentIcons;

		/// <summary>
		/// Getter for our currently selected instrument
		/// </summary>
		public Instrument SelectedInstrument => mSelectedInstrument;

		/// <summary>
		/// Event invoked when instrument is selected
		/// </summary>
		public InstrumentSelectedEvent OnInstrumentSelected { get; } = new InstrumentSelectedEvent();

		/// <summary>
		/// Event for selecting instrument
		/// </summary>
		public class InstrumentSelectedEvent : UnityEvent<Instrument> { };

		/// <summary>
		/// Getter for whether percussion is currently selected
		/// </summary>
		public bool PercussionIsSelected { get; private set; }

		/// <summary>
		/// Set list panel to be percussion only
		/// </summary>
		/// <param name="isPercussionOnly"></param>
		public void SetPercussionOnly( bool isPercussionOnly )
		{
			mSelectPercussionToggle.Option.isOn = isPercussionOnly;
			mSelectPercussionToggle.Option.interactable = isPercussionOnly == false;
			mSelectInstrumentsToggle.Option.isOn = isPercussionOnly == false;
			mSelectInstrumentsToggle.Option.interactable = isPercussionOnly == false;
			SelectBestInstrument();
		}

		public void ToggleInstrumentPercussion()
		{
			mSelectPercussionToggle.Option.isOn = mSelectPercussionToggle.Option.isOn == false;
		}

		/// <summary>
		/// Deselects an instrument
		/// </summary>
		/// <param name="instrument"></param>
		public void DeselectInstrument( Instrument instrument )
		{
			if ( instrument != mSelectedInstrument )
			{
				return;
			}

			if ( mInstrumentIcons.Count == 0 )
			{
				mInstrumentPanel.ClearInstrument();
			}
			else
			{
				SelectBestInstrument();
			}
		}

		/// <summary>
		/// Selects an instrument
		/// </summary>
		/// <param name="instrument"></param>
		public void SelectInstrument( Instrument instrument )
		{
			if ( instrument == null )
			{
				mInstrumentPanel.ClearInstrument();
				OnInstrumentSelected.Invoke( null );
				return;
			}

			if ( mSelectedInstrument == instrument )
			{
				mInstrumentPanel.SetInstrument( mSelectedInstrument );
				OnInstrumentSelected.Invoke( mSelectedInstrument );
				return;
			}

			mSelectedInstrument = instrument;

			foreach ( var instrumentIcon in mInstrumentIcons )
			{
				if ( instrumentIcon.Key == mSelectedInstrument )
				{
					instrumentIcon.Value.Select();
				}
				else
				{
					instrumentIcon.Value.Deselect();
				}
			}

			mInstrumentPanel.SetInstrument( mSelectedInstrument );
			OnInstrumentSelected.Invoke( mSelectedInstrument );
			mUIManager.DirtyEditorDisplays();
		}

		/// <summary>
		/// Returns width of our icon
		/// </summary>
		/// <returns></returns>
		public float GetIconWidth()
		{
			if ( mSelectedInstrument != null && mInstrumentIcons.TryGetValue( mSelectedInstrument, out var instrument ) )
			{
				return instrument.Width;
			}

			return 0f;
		}

		/// <summary>
		/// Toggles mute for an instrument. Handled at list panel level as we need to handle potential issues with the solo button
		/// </summary>
		public void ToggleMute()
		{
			if ( mSoloAndMuteAreLocked )
			{
				return;
			}

			mSoloAndMuteAreLocked = true;

			foreach ( var instrument in mInstrumentIcons )
			{
				instrument.Value.SetSolo( false );
			}

			mSoloAndMuteAreLocked = false;
		}

		/// <summary>
		/// Toggles instrument solo. Handled at the panel level as we need to handle other solo/muted instruments
		/// </summary>
		/// <param name="icon"></param>
		/// <param name="isOn"></param>
		public void ToggleSolo( InstrumentListUIObject icon, bool isOn )
		{
			if ( mSoloAndMuteAreLocked )
			{
				return;
			}

			mSoloAndMuteAreLocked = true;
			icon.SetMute( false );

			var othersAreMuted = isOn;

			foreach ( var instrument in mInstrumentIcons )
			{
				if ( instrument.Value == icon )
				{
					continue;
				}

				instrument.Value.SetSolo( false );
				instrument.Value.SetMute( othersAreMuted );
			}

			mSoloAndMuteAreLocked = false;
		}

		/// <summary>
		/// Starts instrument drag (re-parents to avoid issues).
		/// </summary>
		/// <param name="icon"></param>
		public void StartDragInstrument( InstrumentListUIObject icon )
		{
			icon.transform.SetParent( mRectTransform );
		}

		/// <summary>
		/// Moves the instrument icon, handling group parenting
		/// </summary>
		/// <param name="icon"></param>
		public void MoveInstrument( InstrumentListUIObject icon )
		{
			icon.transform.SetParent( GetGroupParent( icon.Instrument ) );
			mShouldResetRectGroups = true;
		}

		/// <summary>
		/// Clears and destroys all instruments in the list panel
		/// </summary>
		public void ClearInstruments()
		{
			foreach ( var instrumentIcon in mInstrumentIcons )
			{
				Destroy( instrumentIcon.Value.gameObject );
			}

			mInstrumentIcons.Clear();
		}

		/// <summary>
		/// Selects instrument at index of current scroll view
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Instrument GetInstrument( int index )
		{
			return mCurrentScrollView.GetInstrument( index );
		}

		/// <summary>
		/// Selects the next 'best' instrument. This handles keeping instrument/percussion selection separated.
		/// </summary>
		public void SelectBestInstrument()
		{
			var instrument = mCurrentScrollView.GetInstrument( 0 );
			if ( instrument != null )
			{
				SelectInstrument( instrument );
			}
			else
			{
				// we found nothing. was our last instrument.
				mInstrumentPanel.ClearInstrument();
				mDuplicateInstrumentButton.gameObject.SetActive( false );
			}
		}

		/// <summary>
		/// Reloads the panel, adding instruments and parenting them
		/// </summary>
		/// <returns></returns>
		public void ReloadInstruments()
		{
			ClearInstruments();
			foreach ( var instrument in mUIManager.CurrentInstrumentSet.Instruments )
			{
				AddInstrument( instrument );
			}

			mShouldResetRectGroups = true;
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			mAddInstrument.onClick.AddListener( () => { CreateInstrument( onComplete: SelectInstrument ); } );
			mRemoveInstrument.onClick.AddListener( DeleteSelectedInstrument );
			mDuplicateInstrumentButton.onClick.AddListener( DuplicateInstrument );

			// a bit hacky. Please ensure the startup toggles are InstrumentSet correctly.
			mCurrentScrollView = mInstrumentScrollView;
			mPercussionScrollView.SetVisibility( false, mUIManager.CurrentInstrumentSet.GroupIsPlaying );
			mInstrumentScrollView.SetVisibility( true, mUIManager.CurrentInstrumentSet.GroupIsPlaying );

#if FMOD_ENABLED
			mWaveformVisualizerToggle.gameObject.SetActive(false);
			mWaveformUIPanel.gameObject.SetActive(false);
#else
			mWaveformUIPanel.Initialize( mUIManager );

			mWaveformVisualizerToggle.Initialize( isOn =>
				{
					mOutputVisualizer.gameObject.SetActive( isOn );
					mVisibleObject.SetActive( isOn == false );
					mOutputVisualizer.ToggleFadeState( isOn );
					mAudioVisualizer.ToggleWaveformVisibility( isOn );
					mWaveformUIPanel.SetVisibility( isOn );

					if ( isOn == false )
					{
						mPercussionScrollView.SetVisibility( PercussionIsSelected, mUIManager.CurrentInstrumentSet.GroupIsPlaying );
						mInstrumentScrollView.SetVisibility( PercussionIsSelected == false, mUIManager.CurrentInstrumentSet.GroupIsPlaying );
						mShouldResetRectGroups = true;
					}
				},
				false );
#endif //#if FMOD_ENABLED == false

			mSelectInstrumentsToggle.Initialize( ( isOn ) =>
				{
					PercussionIsSelected = isOn == false;
					OnPercussionToggled.Invoke( isOn == false );
					if ( isOn )
					{
						mCurrentScrollView = mInstrumentScrollView;
						mPercussionScrollView.SetVisibility( false, mUIManager.CurrentInstrumentSet.GroupIsPlaying );
						mInstrumentScrollView.SetVisibility( true, mUIManager.CurrentInstrumentSet.GroupIsPlaying );
						mShouldResetRectGroups = true;
					}

					mSelectPercussionToggle.Option.isOn = !isOn;
					SelectBestInstrument();
				},
				initialValue: true );

			mSelectPercussionToggle.Initialize( ( isOn ) =>
				{
					PercussionIsSelected = isOn;
					OnPercussionToggled.Invoke( isOn );
					if ( isOn )
					{
						mCurrentScrollView = mPercussionScrollView;
						mInstrumentScrollView.SetVisibility( false, mUIManager.CurrentInstrumentSet.GroupIsPlaying );
						mPercussionScrollView.SetVisibility( true, mUIManager.CurrentInstrumentSet.GroupIsPlaying );
						mShouldResetRectGroups = true;
					}

					mSelectInstrumentsToggle.Option.isOn = !isOn;
					SelectBestInstrument();
				},
				initialValue: false );
		}

		///<inheritdoc/>
		protected override void OnDestroy()
		{
			base.OnDestroy();
			mAddInstrument.onClick.RemoveAllListeners();
			mRemoveInstrument.onClick.RemoveAllListeners();
			OnInstrumentSelected.RemoveAllListeners();
			mDuplicateInstrumentButton.onClick.RemoveAllListeners();

			if ( mMusicGenerator != false )
			{
				mMusicGenerator.GroupsWereChosen.RemoveListener( OnGroupsWereChosen );
				mMusicGenerator.PlayGenerator.RemoveListener( OnGroupsWereChosen );
				mMusicGenerator.StopGenerator.RemoveListener( OnGroupsWereChosen );
			}
		}

		#endregion protected

		#region private

		[SerializeField, Tooltip( "Reference to our visible object" )]
		private GameObject mVisibleObject;

		[SerializeField, Tooltip( "Reference to our waveform visualizer toggle" )]
		private UIToggle mWaveformVisualizerToggle;

		[SerializeField]
		private WaveformUIPanel mWaveformUIPanel;

		[SerializeField, Tooltip( "Reference to our output visualizer" )]
		private OutputVisualizer mOutputVisualizer;

		[SerializeField]
		private AudioVisualizer mAudioVisualizer;

		[SerializeField] private Instrument mSelectedInstrument;

		[Tooltip( "Reference to our panel's rect transform" )]
		[SerializeField] private RectTransform mRectTransform;

		[Tooltip( "Reference to our Instrument Panel" )]
		[SerializeField] private InstrumentPanelUI mInstrumentPanel;

		[Tooltip( "Reference to our Add Instrument button" )]
		[SerializeField] private Button mAddInstrument;

		[Tooltip( "Reference to our Remove Instrument button" )]
		[SerializeField] private Button mRemoveInstrument;

		[Tooltip( "Reference to our Instrument scroll view" )]
		[SerializeField] private UIInstrumentListScrollView mInstrumentScrollView;

		[Tooltip( "Reference to our Percussion scroll view" )]
		[SerializeField] private UIInstrumentListScrollView mPercussionScrollView;

		[Tooltip( "Reference to our Select Instruments Toggle" )]
		[SerializeField] private UIToggle mSelectInstrumentsToggle;

		[Tooltip( "Reference to our Select Percussion Toggle" )]
		[SerializeField] private UIToggle mSelectPercussionToggle;

		[Tooltip( "Reference to our base Instrument UIObject" )]
		[SerializeField] private InstrumentListUIObject mInstrumentUIObjectBase;

		[Tooltip( "Reference to our Group Backgrounds" )]
		[SerializeField] private Image[] mGroupBackgrounds;

		[Tooltip( "Reference to our Group Tabs" )]
		[SerializeField] private Image[] mGroupTabs;

		[Tooltip( "Reference to our Groups Texts" )]
		[SerializeField] private TMP_Text[] mGroupText;

		[Tooltip( "Reference to our Percussion Group Backgrounds" )]
		[SerializeField] private Image[] mPercussionGroupBackgrounds;

		[Tooltip( "Reference to our Percussion Group Tabs" )]
		[SerializeField] private Image[] mPercussionGroupTabs;

		[Tooltip( "Reference to our Percussion Group Texts" )]
		[SerializeField] private TMP_Text[] mPercussionGroupText;

		[Tooltip( "Reference to our Color field type" )]
		[SerializeField] private ColorFieldType mEnabledGroupMaterial = ColorFieldType.UI_1;

		[Tooltip( "Reference to our disabled group material" )]
		[SerializeField] private ColorFieldType mDisabledGroupMaterial = ColorFieldType.Background_2;

		[Tooltip( "Reference to our duplicate instrument button" )]
		[SerializeField] private Button mDuplicateInstrumentButton;

		/// <summary>
		/// Returns the parent group rect of an instrument (percussion/instrument groups)
		/// </summary>
		/// <param name="instrument"></param>
		/// <returns></returns>
		private RectTransform GetGroupParent( Instrument instrument )
		{
			var group = instrument.InstrumentData.Group;
			return instrument.InstrumentData.IsPercussion ? mPercussionScrollView.GetGroupRect( group ) : mInstrumentScrollView.GetGroupRect( group );
		}

		/// <summary>
		/// Our current scroll view
		/// </summary>
		private UIInstrumentListScrollView mCurrentScrollView;

		/// <summary>
		/// Our dictionary of instrument icons
		/// </summary>
		private readonly Dictionary<Instrument, InstrumentListUIObject> mInstrumentIcons = new Dictionary<Instrument, InstrumentListUIObject>();

		/// <summary>
		/// Whether solo and mute are locked
		/// </summary>
		private bool mSoloAndMuteAreLocked;

		private bool mShouldResetRectGroups;

		private void Update()
		{
			if ( mShouldResetRectGroups )
			{
				mShouldResetRectGroups = false;

				StartCoroutine( ResetRectGroups( mInstrumentScrollView ) );
				StartCoroutine( ResetRectGroups( mPercussionScrollView ) );
			}
		}

		/// <summary>
		/// Creates an instrument 
		/// </summary>
		/// <param name="onComplete"></param>
		private void CreateInstrument( Action<Instrument> onComplete = null )
		{
			var set = mUIManager.CurrentInstrumentSet;
			if ( set.Instruments.Count < MusicConstants.MaxInstruments )
			{
				StartCoroutine( mMusicGenerator.AddInstrument( set,
					null,
					( instrument ) =>
					{
						if ( instrument == null )
						{
							return;
						}

						instrument.InstrumentData.StaffPlayerColor =
							( StaffPlayerColors )( UnityEngine.Random.Range( 0, Enum.GetNames( typeof( StaffPlayerColors ) ).Length ) );
						AddInstrument( instrument, onComplete );
					},
					isPercussion: PercussionIsSelected ) );
			}
		}

		/// <summary>
		/// Deletes our currently selected instrument
		/// </summary>
		private void DeleteSelectedInstrument()
		{
			StartCoroutine( RemoveInstrument( mSelectedInstrument ) );
		}

		/// <summary>
		/// Resets group rects for a scroll view
		/// </summary>
		/// <param name="scrollView"></param>
		/// <returns></returns>
		private IEnumerator ResetRectGroups( UIInstrumentListScrollView scrollView )
		{
			if ( mUIManager.CurrentInstrumentSet.Instruments.Contains( mSelectedInstrument ) == false &&
			     mUIManager.CurrentInstrumentSet.Instruments.Count > 0 )
			{
				SelectBestInstrument();
			}

			// we've removed a ui element, need to wait for the scene to catch up before resizing
			yield return new WaitForEndOfFrame();
			scrollView.ResetRectGroups();
		}

		/// <summary>
		/// Adds an instrument to our panel
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		private void AddInstrument( Instrument instrument, Action<Instrument> onComplete = null )
		{
			var instruments = mUIManager.CurrentInstrumentSet.Instruments;
			if ( mInstrumentUIObjectBase == null || instruments.Count > MusicConstants.MaxInstruments )
			{
				return;
			}

			var result = Instantiate( mInstrumentUIObjectBase, Vector3.zero, Quaternion.identity, GetGroupParent( instrument ) );
			if ( result != null )
			{
				var icon = result.GetComponent<InstrumentListUIObject>();
				mInstrumentIcons.Add( instrument, icon );
				icon.Initialize( mUIManager, instrument );
				mUIManager.UIKeyboard.AddInstrument( instrument );
				mShouldResetRectGroups = true;
			}
			else
			{
				Debug.LogError( "Instrument UI Object was not loaded" );
			}

			mDuplicateInstrumentButton.gameObject.SetActive( true );
			onComplete?.Invoke( instrument );
		}

		/// <summary>
		/// Invoked when groups are chosen
		/// </summary>
		private void OnGroupsWereChosen()
		{
			for ( var index = 0; index < mGroupBackgrounds.Length; index++ )
			{
				var color = mMusicGenerator.InstrumentSet.GroupIsPlaying[index] ?
					mUIManager.CurrentColors[( int )mEnabledGroupMaterial].Color :
					mUIManager.CurrentColors[( int )mDisabledGroupMaterial].Color;
				mGroupBackgrounds[index].color = color;
				mGroupTabs[index].color = color;
				mGroupText[index].color = MusicConstants.InvertTextColor( color );

				mPercussionGroupBackgrounds[index].color = color;
				mPercussionGroupTabs[index].color = color;
				mPercussionGroupText[index].color = MusicConstants.InvertTextColor( color );
			}

			mMusicGenerator.GroupsAreTemporarilyOverriden = mCurrentScrollView.HasManualGroupOverride();
		}

		/// <summary>
		/// Removes an instrument from the list panel
		/// </summary>
		/// <param name="instrument"></param>
		private IEnumerator RemoveInstrument( Instrument instrument )
		{
			if ( mInstrumentIcons.TryGetValue( instrument, out var icon ) )
			{
				mMusicGenerator.RemoveInstrument( instrument.InstrumentIndex, mUIManager.CurrentInstrumentSet );
				Destroy( icon.gameObject );
				mInstrumentIcons.Remove( instrument );
				yield return new WaitUntil( () => icon == null );
				SelectBestInstrument();
				mShouldResetRectGroups = true;
			}
		}

		private void DuplicateInstrument()
		{
			if ( mSelectedInstrument == null || mUIManager.CurrentInstrumentSet.Instruments.Count >= 20 )
			{
				return;
			}

			var instrumentData = mSelectedInstrument.InstrumentData.Clone();
			StartCoroutine( mMusicGenerator.AddInstrument( mUIManager.CurrentInstrumentSet,
				instrumentData,
				( instrument ) =>
				{
					if ( instrument == null )
					{
						return;
					}

					mMusicGenerator.ConfigurationData.Instruments.Add( instrumentData );
					AddInstrument( instrument,
						( _ ) =>
						{
							SelectInstrument( instrument );
							mUIManager.BreakEditorDisplays();
						} );
				} ) );
		}

		#endregion private
	}
}
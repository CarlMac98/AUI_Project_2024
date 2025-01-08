using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Manager for the Leitmotif Editor functionality
	/// </summary>
	public class LeitmotifEditor : MonoBehaviour
	{
		/// <summary>
		/// Reference to the percussion editor
		/// </summary>
		public LeitmotifPercussionEditor PercussionEditor => mPercussionEditor;

		/// <summary>
		/// Initializes the Leitmotif Editor
		/// </summary>
		/// <param name="uiManager"></param>
		public void Initialize( UIManager uiManager )
		{
			mUIManager = uiManager;
			mUIManager.InstrumentListPanelUI.OnPercussionToggled.AddListener( OnPercussionToggled );
			mUIManager.MusicGenerator.NormalMeasureExited.AddListener( OnMeasureExited );
			mInstrumentEditor.Initialize( uiManager );
			mPercussionEditor.Initialize( uiManager );
		}

		/// <summary>
		/// Sets the Leitmotif Editor panel active or inactive
		/// </summary>
		/// <param name="isActive"></param>
		public void SetPanelActive( bool isActive )
		{
			mUIManager.UIKeyboard.Stop( fadeLight: isActive == false );
			// ensure these are clean.
			mUIManager.UILeitmotifEditor.CleanDisplay();
			mUIManager.UILeitmotifEditor.RepairDisplay();

			mIsEnabled = isActive;
			var isPercussion = mUIManager.InstrumentListPanelUI.PercussionIsSelected;
			mInstrumentEditor.SetPanelActive( mIsEnabled && isPercussion == false );
			mPercussionEditor.SetPanelActive( mIsEnabled && isPercussion );
			if ( mIsEnabled == false )
			{
				mPercussionEditor.DestroyInstruments();
			}
		}

		/// <summary>
		/// Manual Update Loop.
		/// </summary>
		public void DoUpdate()
		{
			if ( mIsInitializing )
			{
				return;
			}

			if ( mUIManager.UILeitmotifEditor.DisplayIsBroken )
			{
				mUIManager.LeitmotifEditor.Stop();
			}

			mInstrumentEditor.DoUpdate( );
			mPercussionEditor.DoUpdate( );

			if ( mUIManager.UILeitmotifEditor.DisplayIsDirty )
			{
				mUIManager.UILeitmotifEditor.CleanDisplay();
			}

			if ( mUIManager.UILeitmotifEditor.DisplayIsBroken )
			{
				mUIManager.UILeitmotifEditor.RepairDisplay();
			}
		}

		/// <summary>
		/// Pauses the Leitmotif Editor
		/// </summary>
		public void Pause()
		{
			mInstrumentEditor.Pause();
			mPercussionEditor.Pause();
		}

		/// <summary>
		/// Stops the Leitmotif Editor
		/// </summary>
		public void Stop()
		{
			if ( mUIManager.InstrumentListPanelUI.PercussionIsSelected )
			{
				mPercussionEditor.Stop();
			}
			else
			{
				mInstrumentEditor.Stop();
			}
		}

		/// <summary>
		/// Plays the Leitmotif Editor
		/// </summary>
		public void Play()
		{
			if ( mUIManager.InstrumentListPanelUI.PercussionIsSelected )
			{
				mInstrumentEditor.Hide();
				mUIManager.UIKeyboard.Play( playMode: UIKeyboard.PlayMode.LeitmotifPercussion );
			}
			else
			{
				mInstrumentEditor.Play();
			}
		}

		/// <summary>
		/// Initializes the leitmotif editor instruments
		/// </summary>
		/// <param name="instruments"></param>
		/// <returns></returns>
		public IEnumerator InitializeInstruments( List<Instrument> instruments )
		{
			mIsInitializing = true;
			yield return mInstrumentEditor.InitializeInstruments( instruments );
			yield return mPercussionEditor.InitializeInstruments( instruments );
			OnPercussionToggled( mUIManager.InstrumentListPanelUI.PercussionIsSelected );
			yield return new WaitForEndOfFrame();
			mUIManager.UILeitmotifEditor.CleanDisplay();
			mUIManager.UILeitmotifEditor.RepairDisplay();
			mIsInitializing = false;
		}

		/// <summary>
		/// Sets the leitmotif into an initializing state (prevents reloading ui elements during initialization)
		/// </summary>
		/// <param name="isInitializing"></param>
		public void SetIsInitializing( bool isInitializing )
		{
			mIsInitializing = isInitializing;
		}

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Whether we're currently enabled or not
		/// </summary>
		private bool mIsEnabled;

		/// <summary>
		/// Whether we're currently initializing
		/// </summary>
		private bool mIsInitializing;

		[SerializeField, Tooltip( "Reference to our Instrument Editor" )]
		private LeitmotifInstrumentEditor mInstrumentEditor;

		[SerializeField, Tooltip( "Reference to our Percussion Editor" )]
		private LeitmotifPercussionEditor mPercussionEditor;

		[SerializeField, Tooltip( "Reference to our Instrument Object" )]
		private GameObject mInstrumentObject;

		[SerializeField, Tooltip( "Reference to our Percussion Object " )]
		private GameObject mPercussionObject;

		[SerializeField, Tooltip( "Reference to our Hint Display" )]
		private LeitmotifHintDisplay mHintDisplay;

		/// <summary>
		/// Invoked when percussion is toggled on/off. There are
		/// various panels and objects that need to be enabled/disabled
		/// </summary>
		/// <param name="isPercussion"></param>
		private void OnPercussionToggled( bool isPercussion )
		{
			if ( !mIsEnabled )
			{
				return;
			}

			mUIManager.UIKeyboard.Stop( fadeLight: isPercussion );
			mInstrumentEditor.SetPanelActive( isPercussion == false );
			mPercussionEditor.SetPanelActive( isPercussion );
			if ( isPercussion )
			{
				mInstrumentEditor.Hide();
			}
			else
			{
				mInstrumentEditor.Show();
			}

			mInstrumentObject.SetActive( isPercussion == false );
			mPercussionObject.SetActive( isPercussion );
			mHintDisplay.gameObject.SetActive( isPercussion == false );
			mUIManager.UILeitmotifEditor.BreakDisplay();
		}

		/// <summary>
		/// Destroy/clean up listeners
		/// </summary>
		private void OnDisable()
		{
			if ( mUIManager )
			{
				mUIManager.InstrumentListPanelUI.OnPercussionToggled.RemoveListener( OnPercussionToggled );
				mUIManager.MusicGenerator.NormalMeasureExited.RemoveListener( OnMeasureExited );
			}
		}

		/// <summary>
		/// Invoked when measures are exited. (we dirty the editor display so it will generate the new measure)
		/// </summary>
		private void OnMeasureExited()
		{
			if ( mUIManager.InstrumentListPanelUI.PercussionIsSelected )
			{
				mUIManager.DirtyEditorDisplays();
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Manager for the Measure Editor
	/// </summary>
	public class MeasureEditor : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Reference to our Percussion Editor
		/// </summary>
		public MeasurePercussionEditor PercussionEditor => mPercussionEditor;

		/// <summary>
		/// Initializes the Measure Editor
		/// </summary>
		/// <param name="uiManager"></param>
		public void Initialize( UIManager uiManager )
		{
			mUIManager = uiManager;
			mUIManager.InstrumentListPanelUI.OnPercussionToggled.AddListener( OnPercussionToggled );
			mUIManager.MusicGenerator.RepeatedMeasureExited.AddListener( OnMeasureExited );
			mUIManager.MusicGenerator.InstrumentWillBeRemoved.AddListener( OnInstrumentRemoved );
			mInstrumentEditor.Initialize( uiManager );
			mPercussionEditor.Initialize( uiManager );
		}

		/// <summary>
		/// Sets the panel active
		/// </summary>
		/// <param name="isActive"></param>
		public void SetPanelActive( bool isActive )
		{
			// ensure these are clean.
			mUIManager.UIMeasureEditor.CleanDisplay();
			mUIManager.UIMeasureEditor.RepairDisplay();

			mIsEnabled = isActive;
			mInstrumentEditor.SetPanelActive( isActive );
			mPercussionEditor.SetPanelActive( isActive );

			if ( mIsEnabled == false )
			{
				mPercussionEditor.DestroyInstruments();
			}
		}

		/// <summary>
		/// Manual Update loop
		/// </summary>
		public void DoUpdate()
		{
			if ( mIsInitializing )
			{
				return;
			}

			if ( mUIManager.UIMeasureEditor.DisplayIsBroken )
			{
				mUIManager.MeasureEditor.Stop();
			}

			if ( mUIManager.InstrumentListPanelUI.PercussionIsSelected )
			{
				mPercussionEditor.DoUpdate();
			}
			else if ( mUIManager.UIMeasureEditor.State == DisplayEditorState.Stopped )
			{
				// we only want to update this if we're not playing
				mInstrumentEditor.DoUpdate();
			}

			if ( mUIManager.UIMeasureEditor.DisplayIsDirty )
			{
				mUIManager.UIMeasureEditor.CleanDisplay();
			}

			if ( mUIManager.UIMeasureEditor.DisplayIsBroken )
			{
				mUIManager.UIMeasureEditor.RepairDisplay();
			}
		}

		/// <summary>
		/// Pauses the Measure Editor
		/// </summary>
		public void Pause()
		{
			mInstrumentEditor.Pause();
			mPercussionEditor.Pause();
		}

		/// <summary>
		/// Stops the Measure Editor
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
		/// Plays the Measure Editor
		/// </summary>
		public void Play()
		{
			if ( mUIManager.InstrumentListPanelUI.PercussionIsSelected )
			{
				mInstrumentEditor.Hide();
			}
			else
			{
				mInstrumentEditor.Play();
			}
		}

		/// <summary>
		/// Resets the Measure Editor
		/// </summary>
		public void Reset()
		{
			mInstrumentEditor.Reset();
		}

		/// <summary>
		/// Initializes instruments
		/// </summary>
		/// <param name="instruments"></param>
		/// <returns></returns>
		public IEnumerator InitializeInstruments( List<Instrument> instruments )
		{
			mIsInitializing = true;
			mInstrumentEditor.Reset();
			mPercussionEditor.DestroyInstruments();
			yield return mInstrumentEditor.InitializeInstruments( instruments );
			yield return mPercussionEditor.InitializeInstruments( instruments );
			OnPercussionToggled( mUIManager.InstrumentListPanelUI.PercussionIsSelected );
			mUIManager.UIMeasureEditor.CleanDisplay();
			mUIManager.UIMeasureEditor.RepairDisplay();
		}

		/// <summary>
		/// sets the measure in an 'initializing' state.
		/// This prevents refreshing values while async and coroutine operations are finishing
		/// </summary>
		/// <param name="isInitializing"></param>
		public void SetIsInitializing( bool isInitializing )
		{
			mIsInitializing = isInitializing;
		}

		#endregion public

		#region private

		/// <summary>
		/// Reference to our UI Manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Whether the Measure Editor is Enabled
		/// </summary>
		private bool mIsEnabled;

		/// <summary>
		/// Whether the Measure Editor is initializing
		/// </summary>
		private bool mIsInitializing;

		[SerializeField, Tooltip( "Reference to our Measure Instrument Editor" )]
		private MeasureInstrumentEditor mInstrumentEditor;

		[SerializeField, Tooltip( "Reference to our Measure Percussion Editor" )]
		private MeasurePercussionEditor mPercussionEditor;

		[SerializeField, Tooltip( "Reference to our Instrument scroll view object" )]
		private GameObject mInstrumentObject;

		[SerializeField, Tooltip( "Reference to our Percussion scroll view object" )]
		private GameObject mPercussionObject;

		[SerializeField, Tooltip( "Reference to our Hint Display" )]
		private EditorHintDisplay mHintDisplay;

		/// <summary>
		/// On Destroy
		/// </summary>
		private void OnDestroy()
		{
			if ( mUIManager )
			{
				mUIManager.InstrumentListPanelUI.OnPercussionToggled.RemoveListener( OnPercussionToggled );
				mUIManager.MusicGenerator.InstrumentWillBeRemoved.RemoveListener( OnInstrumentRemoved );
			}
		}

		/// <summary>
		/// Invoked when percussion is toggled. Updates dependent values
		/// </summary>
		/// <param name="isPercussion"></param>
		private void OnPercussionToggled( bool isPercussion )
		{
			if ( !mIsEnabled )
			{
				return;
			}

			if ( isPercussion )
			{
				mInstrumentEditor.Hide();
			}
			else
			{
				mInstrumentEditor.Show();
			}

			mUIManager.UIKeyboard.Stop( fadeLight: isPercussion );
			mInstrumentObject.SetActive( isPercussion == false );
			mPercussionObject.SetActive( isPercussion );
			mHintDisplay.gameObject.SetActive( isPercussion == false );
		}

		/// <summary>
		/// Invoked when instrument is removed
		/// </summary>
		/// <param name="instrument"></param>
		private void OnInstrumentRemoved( Instrument instrument )
		{
			mUIManager.BreakEditorDisplays();
		}

		/// <summary>
		/// Invoked when measures are exited. (we dirty the editor display so it will generate the new measure)
		/// </summary>
		private void OnMeasureExited( GeneratorState state )
		{
			if ( mUIManager.InstrumentListPanelUI.PercussionIsSelected )
			{
				mUIManager.DirtyEditorDisplays();
			}
		}

		private void OnDisable()
		{
			if ( mUIManager != false && mUIManager.MusicGenerator != false )
			{
				mUIManager.MusicGenerator.RepeatedMeasureExited.RemoveListener( OnMeasureExited );
			}
		}

		#endregion private
	}
}
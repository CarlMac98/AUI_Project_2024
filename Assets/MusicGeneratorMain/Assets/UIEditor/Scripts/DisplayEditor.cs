using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Display editors are the visible editor UI interfaces (leitmotif editors, measure editors, etc).
	/// </summary>
	public abstract class DisplayEditor : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Reference to our current time signature
		/// </summary>
		public MeasureTimeSignature CurrentTimeSignature => mMeasureTimeSignatures[( int )mUIManager.ActiveTimeSignature];

		/// <summary>
		/// Non-unity Update method. Must be manually invoked.
		/// </summary>
		public abstract void DoUpdate();

		/// <summary>
		/// Resets our measures. This typically happens when stopping the player,resetting the player, etc.  
		/// </summary>
		public void Reset()
		{
			if ( ( mInstrumentDisplay is null ) == false )
			{
				mInstrumentDisplay.ResetMeasures();
			}

			if ( ( mHintDisplay is null ) == false )
			{
				mHintDisplay.ResetMeasures();
			}
		}

		/// <summary>
		/// Invoked when editor is playing (typically hides the editor).
		/// </summary>
		public virtual void Play()
		{
			FadeOut();
		}

		/// <summary>
		/// Pauses the display editor
		/// </summary>
		public void Pause()
		{
			mUIManager.UIKeyboard.Pause();
		}

		/// <summary>
		/// Stops the display editor (typically fades the editor back in.
		/// </summary>
		public void Stop()
		{
			mUIManager.UIKeyboard.Stop( fadeLight: false );
			FadeIn();
		}

		/// <summary>
		/// Hides the display editor
		/// </summary>
		public void Hide()
		{
			if ( CurrentTimeSignature is null == false )
			{
				CurrentTimeSignature.Hide();
			}

			if ( mInstrumentDisplay is null == false )
			{
				mInstrumentDisplay.Hide();
			}

			if ( mHintDisplay is null == false )
			{
				mHintDisplay.Hide();
			}
		}

		/// <summary>
		/// Shows the display editor
		/// </summary>
		public void Show()
		{
			if ( CurrentTimeSignature is null == false )
			{
				CurrentTimeSignature.Show();
			}

			if ( mInstrumentDisplay is null == false )
			{
				mInstrumentDisplay.Show();
			}

			if ( mHintDisplay is null == false )
			{
				mHintDisplay.Show();
			}
		}

		/// <summary>
		/// Initializes the Display Editor
		/// </summary>
		/// <param name="uiManager"></param>
		public void Initialize( UIManager uiManager )
		{
			mUIManager = uiManager;
			if ( mInstrumentDisplay is null == false )
			{
				mInstrumentDisplay.Initialize( uiManager );
			}

			if ( mHintDisplay is null == false )
			{
				mHintDisplay.Initialize( uiManager );
			}

			EnableMeasures( false );
		}

		/// <summary>
		/// Sets the panel active or inactive
		/// </summary>
		/// <param name="isEnabled"></param>
		public void SetPanelActive( bool isEnabled )
		{
			if ( mIsEnabled == isEnabled )
			{
				return;
			}

			mIsEnabled = isEnabled;

			if ( isEnabled )
			{
				if ( mInstrumentDisplay is null == false )
				{
					mInstrumentDisplay.InitializeMeasures();
				}

				if ( mHintDisplay is null == false )
				{
					mHintDisplay.InitializeMeasures();
				}
			}
			else
			{
				Reset();
			}

			EnableMeasures( isEnabled );
		}

		/// <summary>
		/// Initializes the instruments for a configuration for this display panel
		/// </summary>
		/// <param name="instruments"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public IEnumerator InitializeInstruments( List<Instrument> instruments, Action callback = null )
		{
			yield return new WaitUntil( () => mUIManager.MusicGenerator.GeneratorState != GeneratorState.Initializing );

			foreach ( var instrument in instruments )
			{
				// Percussion initialization is handled by the percussionEditorBase class
				if ( instrument.InstrumentData.IsPercussion )
				{
					callback?.Invoke();
					continue;
				}

				var isComplete = false;
				InitializeInstrument( instrument, ( completed ) => { isComplete = completed; } );
				yield return new WaitUntil( () => isComplete );
			}

			if ( mUIManager.InstrumentListPanelUI.PercussionIsSelected == false )
			{
				RefreshDisplay( CurrentMeasure );
			}

			callback?.Invoke();
		}

		#endregion public

		#region protected

		/// <summary>
		/// Reference to our instrument display
		/// </summary>
		[SerializeField] protected EditorDisplayOverlay mInstrumentDisplay;

		/// <summary>
		/// Reference to our Hint Display
		/// </summary>
		[SerializeField] protected EditorDisplayOverlay mHintDisplay;

		/// <summary>
		/// Reference to our collider
		/// </summary>
		[SerializeField] protected Collider2D mCollider;

		/// <summary>
		/// Reference to our UIManager
		/// </summary>
		protected UIManager mUIManager;

		/// <summary>
		/// Whether this Display Editor is currently enabled
		/// </summary>
		protected bool mIsEnabled;

		/// <summary>
		/// Just a shortened version of the music constant unplayed note (-1)
		/// </summary>
		protected const int UNPLAYED = MusicConstants.UnplayedNote;

		/// <summary>
		/// Updates our clip note
		/// </summary>
		/// <param name="noteData"></param>
		/// <param name="wasAdded"></param>
		/// <param name="instrument"></param>
		protected abstract void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument );

		/// <summary>
		/// Initializes an instrument for this editor display
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="callback"></param>
		protected abstract void InitializeInstrument( Instrument instrument, Action<bool> callback = null );

		/// <summary>
		/// Returns the current measure being edited.
		/// Note, this may point to different measures depending upon the ui manager for this display
		/// </summary>
		protected abstract int CurrentMeasure { get; }

		/// <summary>
		/// Refreshes the display
		/// </summary>
		/// <param name="measureIndex"></param>
		protected virtual void RefreshDisplay( int measureIndex )
		{
			if ( mInstrumentDisplay is null == false )
			{
				mInstrumentDisplay.ShowNotes( measureIndex );
			}

			if ( mHintDisplay is null == false )
			{
				mHintDisplay.ShowNotes( measureIndex );
			}
		}

		/// <summary>
		/// Rebuilds the display (note, this  mostly toggles the display inactive/active
		/// </summary>
		protected void RebuildDisplay()
		{
			if ( mIsEnabled )
			{
				SetPanelActive( false );
				SetPanelActive( true );
			}
		}

		/// <summary>
		/// Handles logic for clicking a note in the display editor
		/// </summary>
		/// <param name="measureIndex"></param>
		protected virtual void ClickNote( int measureIndex )
		{
			if ( mUIManager.InstrumentPanelUI.Instrument == null )
			{
				return;
			}

			mInstrumentDisplay.GetOffsetAndNoteIndex( measureIndex, mUIManager.MouseWorldPoint, out var noteData );

			if ( noteData.OffsetPosition.x != 0 && noteData.OffsetPosition.y != 0 )
			{
				var wasAdded = mInstrumentDisplay.AddOrRemoveNote( measureIndex,
					CurrentMeasure,
					mUIManager.InstrumentListPanelUI.SelectedInstrument,
					noteData );
				UpdateClipNote( noteData, wasAdded, mUIManager.InstrumentListPanelUI.SelectedInstrument );
			}
		}

		#endregion protected

		#region private

		[Tooltip( "Reference to our MeasureTimeSignatures" )]
		[SerializeField] private MeasureTimeSignature[] mMeasureTimeSignatures;

		/// <summary>
		/// Enables our measure displays within the editor
		/// </summary>
		/// <param name="isEnabled"></param>
		private void EnableMeasures( bool isEnabled )
		{
			if ( isEnabled )
			{
				for ( var index = 0; index < mMeasureTimeSignatures.Length; index++ )
				{
					var isActive = index == ( int )mUIManager.ActiveTimeSignature;
					mMeasureTimeSignatures[index].gameObject.SetActive( isActive );
					if ( isActive )
					{
						mMeasureTimeSignatures[index].FadeIn();
					}
					else
					{
						mMeasureTimeSignatures[index].FadeOut();
					}
				}
			}
			else
			{
				foreach ( var measure in mMeasureTimeSignatures )
				{
					measure.gameObject.SetActive( false );
				}
			}
		}

		/// <summary>
		/// Fades out the display over time
		/// </summary>
		private void FadeOut()
		{
			if ( CurrentTimeSignature is null == false )
			{
				CurrentTimeSignature.FadeOut();
			}

			if ( mInstrumentDisplay is null == false )
			{
				mInstrumentDisplay.FadeOut();
			}

			if ( mHintDisplay is null == false )
			{
				mHintDisplay.FadeOut();
			}
		}

		/// <summary>
		/// Fades in the display over time
		/// </summary>
		private void FadeIn()
		{
			if ( CurrentTimeSignature is null == false )
			{
				CurrentTimeSignature.FadeIn();
			}

			if ( mInstrumentDisplay is null == false )
			{
				mInstrumentDisplay.FadeIn();
			}

			if ( mHintDisplay is null == false )
			{
				mHintDisplay.FadeIn();
			}
		}

		#endregion private
	}
}
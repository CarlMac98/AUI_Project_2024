using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Instrument handler for measure display editor, handles differences between percussion/instrument ui.
	/// </summary>
	public class MeasureInstrumentEditor : DisplayEditor
	{
		#region public

		///<inheritdoc/>
		public override void DoUpdate()
		{
			if ( !mIsEnabled || mUIManager.MusicGenerator.GeneratorState != GeneratorState.Stopped )
			{
				return;
			}

			if ( mUIManager.UIMeasureEditor.DisplayIsBroken )
			{
				mUIManager.MeasureEditor.SetIsInitializing( true );
				RebuildDisplay();
				StartCoroutine( InitializeInstruments( mUIManager.UIMeasureEditor.InstrumentSet.Instruments,
					() => { mUIManager.MeasureEditor.SetIsInitializing( false ); } ) );
			}
			else if ( mUIManager.UIMeasureEditor.DisplayIsDirty )
			{
				RefreshDisplay( CurrentMeasure );
			}

			// Highlight
			mUIManager.UIKeyboard.GetKeyHorizontalOffset( mUIManager.MouseWorldPoint, out var noteIndex );
			mUIManager.UIKeyboard.PlayKeyLight( noteIndex,
				mUIManager.Colors[( int )mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.StaffPlayerColor] );

			if ( Input.GetMouseButtonDown( 0 ) && mCollider.bounds.Contains( mUIManager.MouseWorldPoint ) )
			{
				ClickNote( CurrentMeasure );
			}
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override int CurrentMeasure => mUIManager.UIMeasureEditor.CurrentMeasure;

		///<inheritdoc/>
		protected override void InitializeInstrument( Instrument instrument, Action<bool> callback = null )
		{
			if ( instrument.InstrumentData.IsPercussion )
			{
				callback?.Invoke( true );
				return;
			}

			var clipNotes = instrument.InstrumentData.ClipNotes;
			for ( var measureIdx = 0; measureIdx < clipNotes.Count; measureIdx++ )
			{
				for ( var beatIdx = 0; beatIdx < clipNotes[measureIdx].Beats.Count; beatIdx++ )
				{
					for ( var subBeatIdx = 0; subBeatIdx < clipNotes[measureIdx].Beats[beatIdx].Steps.Count; subBeatIdx++ )
					{
						foreach ( var note in clipNotes[measureIdx].Beats[beatIdx].Steps[subBeatIdx].Notes )
						{
							if ( note == MusicConstants.UnplayedNote )
							{
								continue;
							}

							var beatInfo = new Vector2Int( beatIdx, subBeatIdx );

							mInstrumentDisplay.GetOffsetAndNoteIndex(
								measureIdx,
								beatInfo,
								note,
								out var offsetPosition );
							if ( offsetPosition.y > 0 )
							{
								mInstrumentDisplay.AddOrRemoveNote( measureIdx,
									CurrentMeasure,
									instrument,
									new MeasureEditorNoteData( beatInfo, note, mUIManager.UIMeasureEditor.CurrentMeasure, offsetPosition ) );
							}
						}
					}
				}
			}

			callback?.Invoke( true );
		}

		///<inheritdoc/>
		protected override void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument )
		{
			if ( wasAdded )
			{
				mNotes[noteData.Measure].Beats[noteData.Beat.x].Steps[noteData.Beat.y].Notes.Add( noteData.NoteIndex );
			}
			else
			{
				mNotes[noteData.Measure].Beats[noteData.Beat.x].Steps[noteData.Beat.y].Notes.Remove( noteData.NoteIndex );
			}

			mUIManager.MusicGenerator.PlayNote( mUIManager.CurrentInstrumentSet,
				instrument.InstrumentData.Volume,
				instrument.InstrumentData.InstrumentType,
				noteData.NoteIndex,
				instrument.InstrumentIndex );
		}

		#endregion protected

		#region private

		/// <summary>
		/// Our clip notes for the measure instrument editor
		/// </summary>
		private IReadOnlyList<ClipNotesMeasure> mNotes => mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.ClipNotes;

		/// <summary>
		/// Adds a note to our clip notes data
		/// </summary>
		/// <param name="beat"></param>
		/// <param name="subBeat"></param>
		/// <param name="note"></param>
		/// <param name="measure"></param>
		private void AddNote( int beat, int subBeat, int note, int measure = 0 )
		{
			mNotes[measure].Beats[beat].Steps[subBeat].Notes.Add( note );
		}

		/// <summary>
		/// Removes a note from our clip notes data
		/// </summary>
		/// <param name="beat"></param>
		/// <param name="subBeat"></param>
		/// <param name="note"></param>
		/// <param name="measure"></param>
		private void RemoveNote( int beat, int subBeat, int note, int measure = 0 )
		{
			for ( var index = 0; index < mNotes[measure].Beats[beat].Steps[subBeat].Notes.Count; index++ )
			{
				if ( mNotes[measure].Beats[beat].Steps[subBeat].Notes[index] == note )
				{
					mNotes[measure].Beats[beat].Steps[subBeat].Notes[index] = UNPLAYED;
				}
			}
		}

		#endregion private
	}
}
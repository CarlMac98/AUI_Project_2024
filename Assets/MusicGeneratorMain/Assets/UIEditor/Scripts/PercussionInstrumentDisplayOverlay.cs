using System;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Editor Display Overlay for percussion instruments
	/// </summary>
	public class PercussionInstrumentDisplayOverlay : EditorDisplayOverlay
	{
		///<inheritdoc/>
		protected override float NoteColorAlpha => 1f;

		///<inheritdoc/>
		public override bool AddOrRemoveNote( int measureIdx,
			int currentMeasure,
			Instrument instrument,
			MeasureEditorNoteData noteData,
			float colorAlpha = 1f,
			Action callback = null )
		{
			var didAddNote = false;
			var isActive = measureIdx == currentMeasure && mUIManager.InstrumentListPanelUI.PercussionIsSelected;
			if ( mMeasures[measureIdx].TryGetNotes( noteData.NoteInfo, out var notes ) )
			{
				if ( notes.mInstrumentNotes.ContainsKey( instrument ) )
				{
					RemoveNote( measureIdx, noteData.NoteInfo, instrument, notes );
				}
				else
				{
					SpawnNote( noteData.NoteInfo, noteData.OffsetPosition, transform, instrument, measureIdx, isActive );
					didAddNote = true;
				}
			}
			else
			{
				SpawnNote( noteData.NoteInfo, noteData.OffsetPosition, transform, instrument, measureIdx, isActive );
				didAddNote = true;
			}

			return didAddNote;
		}

		///<inheritdoc/>
		public override void GetOffsetAndNoteIndex( int measureIdx, Vector2 position, out MeasureEditorNoteData noteData )
		{
			//bypassing base
			var offsetPosition = new Vector3(
				transform.position.x,
				mDisplayEditor.CurrentTimeSignature.GetStepVerticalOffset( position, out var beat ),
				-1
			);

			offsetPosition = UpdateZPosition( measureIdx, offsetPosition, new NoteData( beat, noteIndex: 0 ) );

			noteData = new MeasureEditorNoteData( beat, noteIndex: 0, mUIDisplayEditor.CurrentMeasure, offsetPosition );
		}

		///<inheritdoc/>
		public override void GetOffsetAndNoteIndex( int measureIdx, Vector2Int beat, int note, out Vector3 offsetPosition )
		{
			//bypassing base
			offsetPosition = new Vector3(
				transform.position.x,
				mDisplayEditor.CurrentTimeSignature.GetStepVerticalOffset( beat ),
				-1 );

			offsetPosition = UpdateZPosition( measureIdx, offsetPosition, new NoteData( beat, note ) );
		}

		///<inheritdoc/>
		public override void ShowNotes( int measureIndex )
		{
			var index = 0;
			foreach ( var measure in mMeasures )
			{
				foreach ( var measureNotes in measure.MeasureNotes )
				{
					foreach ( var instrumentNote in measureNotes.Value.mInstrumentNotes )
					{
						instrumentNote.Value.gameObject.SetActive( index == mUIDisplayEditor.CurrentMeasure );
						instrumentNote.Value.SetColor( mUIManager.Colors[( int )instrumentNote.Key.InstrumentData.StaffPlayerColor] );
					}

					ResizeNotes( measureNotes.Value.mInstrumentNotes );
				}

				index++;
			}
		}
	}
}
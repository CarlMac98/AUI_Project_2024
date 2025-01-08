using System;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Display Overlay for Instruments (as opposed to Percussion Overlays, which have very different visual representation/UX)
	/// </summary>
	public class EditorInstrumentDisplay : EditorDisplayOverlay
	{
		///<inheritdoc/>
		public override bool AddOrRemoveNote( int measureIdx,
			int currentMeasure,
			Instrument instrument,
			MeasureEditorNoteData noteData,
			float colorAlpha = 1f,
			Action callback = null )
		{
			var displayNote = currentMeasure == measureIdx && instrument == mUIManager.InstrumentListPanelUI.SelectedInstrument &&
			                  instrument.InstrumentData.IsPercussion == mUIManager.InstrumentListPanelUI.PercussionIsSelected;

			if ( mMeasures[measureIdx].TryGetNotes( noteData.NoteInfo, out var notes ) )
			{
				if ( notes.mInstrumentNotes.ContainsKey( instrument ) )
				{
					RemoveNote( measureIdx, noteData.NoteInfo, instrument, notes );
					return false;
				}
			}

			SpawnNote( noteData.NoteInfo, noteData.OffsetPosition, transform, instrument, measureIdx, displayNote, callback: callback );
			return true;
		}

		///<inheritdoc/>
		public override void ShowNotes( int measureIdx )
		{
			ResetNotes();
			var showAllInstruments = mUIDisplayEditor.ShouldShowAllInstruments;

			foreach ( var measureNotes in mMeasures[measureIdx].MeasureNotes )
			{
				foreach ( var instrumentNote in measureNotes.Value.mInstrumentNotes )
				{
					instrumentNote.Value.gameObject.SetActive( measureIdx == mUIDisplayEditor.CurrentMeasure &&
					                                           ( showAllInstruments || instrumentNote.Key == CurrentInstrument ) );
					instrumentNote.Value.SetColor( mUIManager.Colors[( int )instrumentNote.Key.InstrumentData.StaffPlayerColor] );
				}

				ResizeNotes( measureNotes.Value.mInstrumentNotes );
			}
		}

		///<inheritdoc/>
		protected override float NoteColorAlpha => 1f;

		/// <summary>
		/// Reference to the instrument list panel's currently selected instrument
		/// </summary>
		private Instrument CurrentInstrument => mUIManager.InstrumentListPanelUI.SelectedInstrument;

		/// <summary>
		/// Resets our notes (sets them all inactive).
		/// </summary>
		private void ResetNotes()
		{
			foreach ( var measure in mMeasures )
			{
				foreach ( var measureNotes in measure.MeasureNotes )
				{
					foreach ( var instrumentNote in measureNotes.Value.mInstrumentNotes )
					{
						instrumentNote.Value.gameObject.SetActive( false );
					}
				}
			}
		}
	}
}
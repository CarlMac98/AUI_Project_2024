using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Percussion Editor Instrument for the Leitmotif Editor
	/// </summary>
	public class LeitmotifPercussionEditorInstrument : PercussionEditorInstrument
	{
		protected override int RepeatCount => mUIManager == null ? 0 : mUIManager.CurrentInstrumentSet.RepeatCount;
		protected override int NumMeasures => mUIManager == null ? 0 : mUIManager.MusicGenerator.ConfigurationData.NumLeitmotifMeasures;

		///<inheritdoc/>
		protected override void InitializeInstrument( Instrument instrument, Action<bool> callback = null )
		{
			mInstrument = instrument;
			IReadOnlyList<Leitmotif.LeitmotifMeasure> leitmotif = instrument.InstrumentData.Leitmotif.Notes;
			for ( var measureIdx = 0; measureIdx < leitmotif.Count; measureIdx++ )
			{
				for ( var timestepIdx = 0; timestepIdx < leitmotif[measureIdx].Beat.Count; timestepIdx++ )
				{
					for ( var noteIdx = 0; noteIdx < leitmotif[measureIdx].Beat[timestepIdx].SubBeat.Count; noteIdx++ )
					{
						for ( var note = 0; note < leitmotif[measureIdx].Beat[timestepIdx].SubBeat[noteIdx].notes.Count; note++ )
						{
							AddNote( timestepIdx, noteIdx, measureIdx );
						}
					}
				}
			}

			mDragElement.Initialize( () => { mOnInstrumentMoved.Invoke(); },
				mUIManager );
			mInstrument = instrument;
			RefreshInstrument();
			callback?.Invoke( true );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timestepIdx"></param>
		/// <param name="noteIdx"></param>
		/// <param name="measureIdx"></param>
		private void AddNote( int timestepIdx, int noteIdx, int measureIdx )
		{
			var timestep = new Vector2Int( timestepIdx, noteIdx );
			mInstrumentDisplay.GetOffsetAndNoteIndex(
				measureIdx,
				timestep,
				note: 0,
				out var offsetPosition );
			if ( offsetPosition.y > 0 )
			{
				mInstrumentDisplay.AddOrRemoveNote(
					measureIdx,
					CurrentMeasure,
					mInstrument,
					new MeasureEditorNoteData( timestep, noteIndex: 0, CurrentMeasure, offsetPosition )
				);
			}
		}
	}
}
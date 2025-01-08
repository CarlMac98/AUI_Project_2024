using System.Collections.Generic;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/* This class is a bit ugly. For percussion, each instrument will function as its own
	 * DisplayEditor, so, this acts more as a parent forwarding each command to the children
	 */
	public class LeitmotifPercussionEditor : PercussionEditorBase
	{
		///<inheritdoc/>
		public override void Initialize( UIManager uiManager )
		{
			mDisplayEditor = uiManager.UILeitmotifEditor;
			base.Initialize( uiManager );
		}

		///<inheritdoc/>
		protected override void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument )
		{
			var note = new LeitmotifNote( 0 );
			IReadOnlyList<Leitmotif.LeitmotifMeasure> notes = instrument.InstrumentData.Leitmotif.Notes;
			if ( wasAdded )
			{
				AddLeitmotifNote( instrument, noteData, note, noteData.Measure );
			}
			else
			{
				RemoveLeitmotifNote( instrument, noteData, note, noteData.Measure );
			}

			var index = Leitmotif.GetUnscaledNoteArray( notes[noteData.Measure].Beat[noteData.Beat.x].SubBeat[noteData.Beat.y].notes,
				mUIManager.MusicGenerator );
			foreach ( var timestepNotes in index )
			{
				mUIManager.MusicGenerator.PlayNote( mUIManager.CurrentInstrumentSet,
					instrument.InstrumentData.Volume,
					instrument.InstrumentData.InstrumentType,
					timestepNotes,
					instrument.InstrumentIndex );
			}
		}


		/// <summary>
		/// Adds a leitmotif note to the instrument's leitmotif container
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="noteData"></param>
		/// <param name="note"></param>
		/// <param name="measure"></param>
		private static void AddLeitmotifNote( Instrument instrument, MeasureEditorNoteData noteData, LeitmotifNote note, int measure = 0 )
		{
			instrument.InstrumentData.Leitmotif.Notes[measure].Beat[noteData.Beat.x].SubBeat[noteData.Beat.y].notes.Add( note );
		}

		/// <summary>
		/// removes a leitmotif note from the instrument's leitmotif container
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="noteData"></param>
		/// <param name="note"></param>
		/// <param name="measure"></param>
		private static void RemoveLeitmotifNote( Instrument instrument, MeasureEditorNoteData noteData, LeitmotifNote note, int measure = 0 )
		{
			var notes = instrument.InstrumentData.Leitmotif.Notes[measure].Beat[noteData.Beat.x].SubBeat[noteData.Beat.y].notes;
			for ( var index = 0; index < notes.Count; index++ )
			{
				if ( notes[index].Equals( note ) )
				{
					notes.Remove( note );
				}
			}
		}
	}
}
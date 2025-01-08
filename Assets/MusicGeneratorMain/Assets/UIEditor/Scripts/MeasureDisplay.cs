using System.Collections.Generic;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Visual representation of the measures for the measure editor (or editor variants, leitmotif, percussion, etc)
	/// </summary>
	public class MeasureDisplay
	{
		/// <summary>
		/// Getter for our note data
		/// </summary>
		public IReadOnlyDictionary<NoteData, InstrumentNotes> MeasureNotes => mMeasureNotes;

		/// <summary>
		/// Destroys all notes
		/// </summary>
		public void ClearMeasureNotes()
		{
			mMeasureNotes.Clear();
		}

		/// <summary>
		/// Returns note if it exists in our container
		/// </summary>
		/// <param name="key"></param>
		/// <param name="instrumentNotes"></param>
		/// <returns></returns>
		public bool TryGetNotes( NoteData key, out InstrumentNotes instrumentNotes )
		{
			return mMeasureNotes.TryGetValue( key, out instrumentNotes );
		}

		/// <summary>
		/// Adds a note to our instrument's container
		/// </summary>
		/// <param name="key"></param>
		/// <param name="instrument"></param>
		/// <param name="note"></param>
		public void Add( NoteData key, Instrument instrument, MeasureEditorNote note )
		{
			if ( mMeasureNotes.TryGetValue( key, out var measureNote ) )
			{
				measureNote.mInstrumentNotes[instrument] = note;
			}
			else
			{
				mMeasureNotes.Add( key, new InstrumentNotes() );
				mMeasureNotes[key].mInstrumentNotes[instrument] = note;
			}
		}

		/// <summary>
		/// Removes a note from our measure display
		/// </summary>
		/// <param name="key"></param>
		public void Remove( NoteData key )
		{
			mMeasureNotes.Remove( key );
		}

		/// <summary>
		/// Container of instrument notes
		/// </summary>
		public class InstrumentNotes
		{
			public readonly Dictionary<Instrument, MeasureEditorNote> mInstrumentNotes = new Dictionary<Instrument, MeasureEditorNote>();
		}

		/// <summary>
		/// Our container of note data
		/// </summary>
		private readonly Dictionary<NoteData, InstrumentNotes> mMeasureNotes = new Dictionary<NoteData, InstrumentNotes>();
	}
}
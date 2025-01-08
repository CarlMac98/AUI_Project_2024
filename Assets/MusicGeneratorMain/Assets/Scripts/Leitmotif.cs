using System;
using System.Collections.Generic;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Our Leitmotif. Handles preset notes to integrate into normal music generation.
	/// </summary>
	[Serializable]
	public class Leitmotif
	{
		public Leitmotif()
		{
			if ( Notes != null )
			{
				return;
			}

			Notes = new List<LeitmotifMeasure>();
			for ( var x = 0; x < MusicConstants.NumMeasures; x++ )
			{
				Notes.Add( new LeitmotifMeasure() );
				for ( var y = 0; y < MusicConstants.MaxFullstepsTaken; y++ )
				{
					Notes[x].Beat.Add( new LeitmotifTimeStep() );
					for ( var z = 0; z < MusicConstants.MaxStepsPerTimestep; z++ )
					{
						Notes[x].Beat[y].SubBeat.Add( new LeitmotifNoteIndex() );
					}
				}
			}
		}

		public Leitmotif Clone()
		{
			var clone = (Leitmotif) MemberwiseClone();
			clone.Notes = new List<LeitmotifMeasure>();
			foreach ( var note in Notes )
			{
				var leitmotifMeasure = new LeitmotifMeasure();
				foreach ( var beat in note.Beat )
				{
					var cloneBeat = new LeitmotifTimeStep();
					foreach ( var _ in beat.SubBeat )
					{
						var cloneSubBeat = new LeitmotifNoteIndex();
						foreach ( var leitmotifNote in cloneSubBeat.notes )
						{
							var cloneLeitmotifNote = new LeitmotifNote();
							cloneLeitmotifNote.Accidental = leitmotifNote.Accidental;
							cloneLeitmotifNote.ScaledNote = leitmotifNote.ScaledNote;

							cloneSubBeat.notes.Add( leitmotifNote );
						}

						cloneBeat.SubBeat.Add( cloneSubBeat );
					}

					leitmotifMeasure.Beat.Add( cloneBeat );
				}

				clone.Notes.Add( leitmotifMeasure );
			}

			clone.IsEnabled = IsEnabled;
			return clone;
		}

		/// <summary>
		/// Whether this leitmotif is enabled
		/// </summary>
		public bool IsEnabled;

		[Serializable]
		public class LeitmotifMeasure
		{
			public List<LeitmotifTimeStep> Beat = new List<LeitmotifTimeStep>();
		}

		[Serializable]
		public class LeitmotifNoteIndex
		{
			public List<LeitmotifNote> notes = new List<LeitmotifNote>();
		}

		[Serializable]
		public class LeitmotifTimeStep
		{
			public List<LeitmotifNoteIndex> SubBeat = new List<LeitmotifNoteIndex>();
		}

		public List<LeitmotifMeasure> Notes;

		public List<LeitmotifNote> GetLeitmotifNotes( int measureIndex, int timestep, int noteIndex )
		{
			return Notes[measureIndex].Beat[timestep].SubBeat[noteIndex].notes;
		}

		public static int[] GetUnscaledNoteArray( List<LeitmotifNote> leitMotifnotes, MusicGenerator musicGenerator )
		{
			var notes = new int[leitMotifnotes.Count];
			for ( var index = 0; index < leitMotifnotes.Count; index++ )
			{
				notes[index] = GetUnscaledNoteIndex( leitMotifnotes[index], musicGenerator );
			}

			return notes;
		}

		public static int GetUnscaledNoteIndex( LeitmotifNote note, MusicGenerator musicGenerator )
		{
			if ( note.ScaledNote == MusicConstants.UnplayedNote )
			{
				return note.ScaledNote;
			}

			var scale = MusicConstants.GetScale( musicGenerator.ConfigurationData.Scale );
			var mode = (int) musicGenerator.ConfigurationData.Mode;
			var unscaledIndex = 0;

			for ( var index = 0; index < note.ScaledNote; index++ )
			{
				unscaledIndex += scale[MusicConstants.SafeLoop( mode + index, 0, MusicConstants.ScaleLength )];
			}

			unscaledIndex += (int) musicGenerator.ConfigurationData.Key;
			unscaledIndex += note.Accidental;

			return MusicConstants.SafeLoop( unscaledIndex, 0, MusicConstants.MaxInstrumentNotes );
		}
	}
}

using System;
using Random = UnityEngine.Random;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// This class generates notes for a rhythm instrument
	/// </summary>
	public class NoteGenerator_Rhythm : NoteGenerator
	{
		/// <summary>
		/// Clears any saved notes for this generator
		/// </summary>
		public override void ClearNotes()
		{
			for ( var index = 0; index < mNotes.Length; index++ )
			{
				mNotes[index] = -1;
			}
		}

		/// <summary>
		/// Generates notes for a rhythm instrument for a single step
		/// </summary>
		/// <returns></returns>
		public override int[] GenerateNotes( bool isMinRhythm )
		{
			ClearNotes();

			var useArpeggio = mInstrument.InstrumentData.Arpeggio;

			if ( mInstrument.IsRepeatingPattern && mInstrument.InstrumentData.UsePattern )
			{
				return AddRepeatNotes();
			}

			if ( mInstrument.InstrumentData.IsPercussion )
			{
				return GetPercussionNotes();
			}

			if ( useArpeggio )
			{
				GenerateArpeggio();
			}
			else if ( mInstrument.InstrumentData.StrumLength > 0 )
			{
				GenerateStrum();
			}
			else
			{
				GenerateNormalRhythm();
			}

			return mNotes;
		}

		/// <summary>
		/// Returns the repeating notes for this instrument.
		/// </summary>
		/// <returns></returns>
		protected override int[] AddRepeatNotes()
		{
			var isStrumming = mInstrument.InstrumentData.StrumLength > 0f;
			for ( var index = 0; index < mInstrument.CurrentPatternNotes.Length; index++ )
			{
				var note = mInstrument.CurrentPatternNotes[index];
				var clamp = isStrumming ? 10 : -1;
				mNotes[index] = ( note != MusicConstants.UnplayedNote ) ?
					GetChordNote( note, index, mInstrument.CurrentPatternOctave[index], clamp ) :
					MusicConstants.UnplayedNote;
			}

			// ew. sorry. To keep the pattern whole, we allow overflowing above, and then shift things down until they fit :P
			// This ONLY works for strummed/arpeggio
			if ( isStrumming || mInstrument.InstrumentData.Arpeggio )
			{
				var needsClamping = true;
				while ( needsClamping )
				{
					needsClamping = ClampNotes();
				}
			}

			if ( mInstrument.InstrumentData.StrumLength > 0f && mInstrument.InstrumentData.Arpeggio == false )
			{
				Array.Sort( mNotes );
			}

			return mNotes;
		}

		/// <summary>
		/// For sorting our strum if needed.
		/// </summary>
		private readonly int[] mStrumTriad =
		{
			-1, -1, -1
		};

		/// <summary>
		/// Generates normal rhythm notes (non-strum, non-arpeggio)
		/// </summary>
		private void GenerateNormalRhythm()
		{
			// because we generally want to play at least one note for rhythm
			// we start backward and just play the root chord note if other chord notes don't play.
			var successfulNote = false;
			var isSettingPattern = mInstrument.IsSettingPattern && mInstrument.InstrumentData.UsePattern;
			var octave = GetOctaveIndex();
			for ( var index = mInstrument.InstrumentData.ChordSize - 1; index >= 0; index-- )
			{
				if ( Random.Range( 0, 100 ) < mInstrument.InstrumentData.OddsOfUsingChordNotes ||
				     ( index == 0 && successfulNote == false ) )
				{
					var chordNote = MusicConstants.SeventhChord[index];
					if ( isSettingPattern == false )
					{
						octave = GetOctaveIndex();
					}

					mNotes[index] = GetChordNote( chordNote, index, octave );
					mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep][index] = octave;
					mInstrument.PatternNoteOffset[mInstrument.CurrentPatternStep][index] = chordNote;
					successfulNote = true;
				}
				else
				{
					mNotes[index] = EmptyPatternedNote( index );
				}
			}
		}

		/// <summary>
		/// Generates strummed notes
		/// </summary>
		private void GenerateStrum()
		{
			var octave = mInstrument.InstrumentData.OctavesToUse[Random.Range( 0, mInstrument.InstrumentData.OctavesToUse.Count )];
			var isTriad = mInstrument.InstrumentData.NumStrumNotes <= MusicConstants.TriadCount;

			// because we'll be strumming out of our octave, we shift down one if we're at the highest octave
			if ( isTriad == false && octave == MusicConstants.TriadCount - 1 )
			{
				octave = 1;
			}

			// just clearing out our old array
			for ( var index = 0; index < mNotes.Length; index++ )
			{
				mNotes[index] = -1;
			}

			for ( var index = 0; index < mInstrument.InstrumentData.NumStrumNotes; index++ )
			{
				var chordNote = MusicConstants.SeventhChord[index % MusicConstants.TriadCount];
				if ( isTriad == false )
				{
					if ( index < MusicConstants.TriadCount )
					{
						mNotes[index] = GetChordNote( chordNote, index, octave, 10 );
						mStrumTriad[index] = mNotes[index];
					}
					else
					{
						// here we just continue playing the previously strummed triad, just an octave higher.
						Array.Sort( mStrumTriad );
						var triadIndex = index % MusicConstants.TriadCount;
						var note = mStrumTriad[triadIndex] + MusicConstants.OctaveSize;
						mInstrument.PatternNoteOffset[mInstrument.CurrentPatternStep][index] = chordNote;
						mNotes[index] = note;
					}
				}
				else
				{
					mNotes[index] = GetChordNote( chordNote, index, octave, 10 );
				}

				if ( mInstrument.IsSettingPattern )
				{
					mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep][index] = index < MusicConstants.TriadCount ? octave : octave + 1;
				}
			}

			// ew. sorry. To keep the pattern whole, we allow overflowing above, and then shift things down until they fit :P
			var needsClamping = true;
			while ( needsClamping )
			{
				needsClamping = ClampNotes();
			}

			Array.Sort( mNotes );
		}
	}
}

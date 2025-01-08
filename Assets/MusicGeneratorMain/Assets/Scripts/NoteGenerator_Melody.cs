using System;
using Random = UnityEngine.Random;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// This class generates notes for a melodic instrument
	/// </summary>
	public class NoteGenerator_Melody : NoteGenerator
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
		/// Generates notes for a step
		/// </summary>
		/// <returns></returns>
		public override int[] GenerateNotes( bool isMinRhythm )
		{
			ClearNotes();

			if ( mInstrument.IsRepeatingPattern && mInstrument.InstrumentData.UsePattern )
			{
				return AddRepeatNotes();
			}

			if ( mInstrument.NoteRoll( isMinRhythm ) )
			{
				if ( mInstrument.InstrumentData.IsPercussion )
				{
					return GetPercussionNotes();
				}

				if ( Random.Range( 0, 100 ) > mInstrument.InstrumentData.OddsOfUsingChordNotes )
				{
					GenerateNormalMelody();
				}
				else
				{
					return mFallback();
				}
			}
			else
			{
				AddEmptyNotes();
			}

			return mNotes;
		}

		/// <summary>
		/// Returns the repeating notes for this instrument.
		/// </summary>
		/// <returns></returns>
		protected override int[] AddRepeatNotes()
		{
			var isChord = false;
			for ( var index = 1; index < mInstrument.CurrentPatternNotes.Length; index++ )
			{
				if ( mInstrument.CurrentPatternNotes[index] != -1 )
				{
					isChord = true;
				}
			}

			var isStrumming = mInstrument.InstrumentData.StrumLength > 0f || isChord || mInstrument.InstrumentData.Arpeggio;
			for ( var index = 0; index < mInstrument.CurrentPatternNotes.Length; index++ )
			{
				var note = mInstrument.CurrentPatternNotes[index];
				var clamp = isStrumming ? 10 : -1;
				mNotes[index] = ( note != MusicConstants.UnplayedNote ) ?
					GetChordNote( note, index, mInstrument.CurrentPatternOctave[index], clamp ) :
					MusicConstants.UnplayedNote;
			}

			// ew. sorry. To keep the pattern whole, we allow overflowing above, and then shift things down until they fit :P
			if ( isStrumming )
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
		/// Generates our normal melodic notes
		/// </summary>
		private void GenerateNormalMelody()
		{
			var note = Random.Range( 0, mInstrument.InstrumentData.ChordSize );
			var octave = mInstrument.InstrumentData.OctavesToUse[Random.Range( 0, mInstrument.InstrumentData.OctavesToUse.Count )];

			if ( mInstrument.IsSettingPattern )
			{
				if ( mInstrument.CurrentPatternStep > 0 )
				{
					mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep][0] = mInstrument.PatternOctaveOffset[0][0];
					octave = mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep][0];
				}
				else
				{
					mInstrument.PatternOctaveOffset[0][0] = octave;
				}
			}

			AddSingleNote( GetChordNote( MusicConstants.SeventhChord[note], 0, octave ), true );
		}
	}
}

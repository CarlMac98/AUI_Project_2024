using UnityEngine;
using Random = UnityEngine.Random;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public abstract class NoteGenerator
	{
		#region public

		/// <summary>
		/// fallback function if the note fails a check (like, if a lead instrument plays a rhythm chord instead.)
		/// </summary>
		public delegate int[] Fallback( Fallback fallback = null );

		/// <summary>
		/// clears any saved notes for this instrument
		/// </summary>
		public abstract void ClearNotes();

		/// <summary>
		/// generates the next InstrumentSet of notes for this instrument.
		/// </summary>
		/// <returns></returns>
		public abstract int[] GenerateNotes( bool isMinRhythm );

		/// <summary>
		/// Initializes this note generator.
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="musicGenerator"></param>
		/// <param name="fallback"></param>
		public void Init( Instrument instrument, MusicGenerator musicGenerator, Fallback fallback )
		{
			mInstrument = instrument;
			mFallback = fallback;
			mMusicGenerator = musicGenerator;
		}

		public int GetSpecificNote( int index, int octave )
		{
			var note = GetChordNote( index, octave, octave );
			return note;
		}

		#endregion public

		#region protected

		/// <summary>
		/// container for the notes for a single step
		/// </summary>
		protected readonly int[] mNotes =
		{
			0, 0, 0, 0, 0
		};

		/// <summary>
		/// Reference for our instrument
		/// </summary>
		protected Instrument mInstrument;

		/// <summary>
		/// Reference for our music generator
		/// </summary>
		protected MusicGenerator mMusicGenerator;

		/// <summary>
		/// Reference for our fallback.
		/// </summary>
		protected Fallback mFallback;

		/// <summary>
		/// adds a single note for this instrument
		/// </summary>
		/// <param name="note"></param>
		/// <param name="addPattern"></param>
		protected void AddSingleNote( int note, bool addPattern = false )
		{
			mNotes[0] = note;
			mNotes[1] = ( addPattern ? EmptyPatternedNote( 1 ) : MusicConstants.UnplayedNote );
			mNotes[2] = ( addPattern ? EmptyPatternedNote( 2 ) : MusicConstants.UnplayedNote );
			mNotes[3] = ( addPattern ? EmptyPatternedNote( 3 ) : MusicConstants.UnplayedNote );
			mNotes[4] = ( addPattern ? EmptyPatternedNote( 3 ) : MusicConstants.UnplayedNote );
		}

		/// <summary>
		/// fills the current octaves and notes with non-played values.
		/// </summary>
		protected void AddEmptyNotes()
		{
			for ( var index = 0; index < mNotes.Length; index++ )
			{
				mNotes[index] = MusicConstants.UnplayedNote;
				mInstrument.CurrentPatternNotes[index] = MusicConstants.UnplayedNote;
				mInstrument.CurrentPatternOctave[index] =
					mInstrument.InstrumentData.OctavesToUse[Random.Range( 0, mInstrument.InstrumentData.OctavesToUse.Count )];
			}
		}

		/// <summary>
		/// Sets the empty patterned notes. Plays no regular notes:
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected int EmptyPatternedNote( int index )
		{
			mInstrument.CurrentPatternNotes[index] = MusicConstants.UnplayedNote;
			mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep][index] =
				mInstrument.InstrumentData.OctavesToUse[Random.Range( 0, mInstrument.InstrumentData.OctavesToUse.Count )];
			return MusicConstants.UnplayedNote;
		}

		/// <summary>
		/// Gets a note from a chord.
		/// </summary>
		/// <param name="chordNote"></param>
		/// <param name="chordIndex"></param>
		/// <param name="octaveOffset"></param>
		/// <param name="octaveClamp"></param>
		/// <returns></returns>
		protected int GetChordNote( int chordNote, int chordIndex, int octaveOffset, int octaveClamp = -1 )
		{
			var note = (int) mMusicGenerator.ConfigurationData.Key;

			//tri-tone check.
			var progressionStep = ( mInstrument.CurrentProgressionStep < 0 ) ? mInstrument.CurrentProgressionStep * -1 : mInstrument.CurrentProgressionStep;

			//add octave offset:
			octaveOffset *= MusicConstants.OctaveSize;
			var clamp = ( octaveClamp == -1 ) ? octaveOffset : octaveClamp * MusicConstants.OctaveSize;
			note += octaveOffset;

			//for arpeggio we don't want to avoid strummed notes
			if ( IsRedundant( chordNote ) && mInstrument.InstrumentData.StrumLength == 0f )
			{
				const int extraStep = 2;
				chordNote = ( chordNote != MusicConstants.SeventhChord[mInstrument.InstrumentData.ChordSize - 2] ) ? chordNote + extraStep : 0;
			}

			mInstrument.CurrentPatternNotes[chordIndex] = chordNote;

			note += GetChordOffset( progressionStep, (int) mMusicGenerator.ConfigurationData.Mode, chordNote );

			if ( mInstrument.CurrentProgressionStep < 0 )
				note += MusicConstants.TritoneStep;

			return MusicConstants.SafeLoop( note, octaveOffset, clamp + MusicConstants.OctaveSize );
		}

		/// <summary>
		/// Returns the repeating notes for this instrument.
		/// </summary>
		/// <returns></returns>
		protected abstract int[] AddRepeatNotes();

		protected bool ClampNotes()
		{
			var needsShifting = false;
			foreach ( var note in mNotes )
			{
				if ( note >= MusicConstants.MaxInstrumentNotes )
				{
					needsShifting = true;
					break;
				}
			}

			if ( needsShifting )
			{
				for ( var index = 0; index < mNotes.Length; index++ )
				{
					if ( mNotes[index] != -1 )
					{
						mNotes[index] -= MusicConstants.OctaveSize;
					}
				}
			}

			return needsShifting;
		}

		/// <summary>
		/// Returns notes for a percussion instrument.
		/// </summary>
		/// <returns></returns>
		protected int[] GetPercussionNotes()
		{
			if ( mInstrument.InstrumentData.StrumLength == 0f )
			{
				AddSingleNote( 0, true );
			}
			else
			{
				for ( var index = 0; index < mInstrument.InstrumentData.NumStrumNotes; index++ )
				{
					mNotes[index] = 0;
				}
			}

			return mNotes;
		}

		/// <summary>
		/// returns our available min/max octaves
		/// </summary>
		/// <param name="minOctave"></param>
		/// <param name="maxOctave"></param>
		protected void GetMinMaxOctaves( out int minOctave, out int maxOctave )
		{
			maxOctave = 0;
			minOctave = 2;
			for ( var index = 0; index < mInstrument.InstrumentData.OctavesToUse.Count; index++ )
			{
				var octave = mInstrument.InstrumentData.OctavesToUse[index];
				if ( octave != -1 )
				{
					if ( octave > maxOctave )
					{
						maxOctave = octave;
					}
					else if ( octave < minOctave )
					{
						minOctave = octave;
					}
				}
			}
		}

		/// <summary>
		/// Generates arpeggio notes
		/// </summary>
		protected void GenerateArpeggio()
		{
			//for arpeggio, because we'll repeat this pattern for future progression steps we need to limit range to preserve note order
			var octave = Mathf.Clamp( GetOctaveIndex(), 0, MusicConstants.NumOctaves - 2 );

			if ( mInstrument.InstrumentData.UsePattern )
			{
				for ( var offsetIndex = 0; offsetIndex < mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep].Length; offsetIndex++ )
				{
					mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep][offsetIndex] = octave;
				}
			}

			for ( var index = 0; index < mInstrument.InstrumentData.NumStrumNotes; index++ )
			{
				mNotes[index] = GetChordNote( mInstrument.ArpeggioPattern[index], index, octave, octave + 3 );
			}

			var needsShifting = false;
			foreach ( var note in mNotes )
			{
				if ( note >= MusicConstants.MaxInstrumentNotes )
				{
					needsShifting = true;
					break;
				}
			}

			if ( needsShifting )
			{
				for ( var index = 0; index < mNotes.Length; index++ )
				{
					if ( mNotes[index] != -1 )
					{
						mNotes[index] -= MusicConstants.OctaveSize;
						mInstrument.PatternOctaveOffset[mInstrument.CurrentPatternStep][index] -= 1;
					}
				}
			}
		}

		/// <summary>
		/// Returns random available octave
		/// </summary>
		/// <returns></returns>
		protected int GetOctaveIndex()
		{
			return mInstrument.InstrumentData.OctavesToUse[Random.Range( 0, mInstrument.InstrumentData.OctavesToUse.Count )];
		}

		#endregion protected

		#region private

		/// <summary>
		/// Returns whether this note is redundant
		/// </summary>
		/// <param name="note"></param>
		/// <returns></returns>
		private bool IsRedundant( int note )
		{
			if ( mInstrument.InstrumentData.SuccessionType == SuccessionType.Rhythm || mMusicGenerator.InstrumentSet.SixteenthStepsTaken == 0 )
				return false;

			if ( mInstrument.CurrentPatternStep > 0 && note == mInstrument.PatternNoteOffset[mInstrument.CurrentPatternStep - 1][0] )
			{
				return ( Random.Range( 0.0f, 100.0f ) < mInstrument.InstrumentData.RedundancyAvoidance );
			}

			return false;
		}

		/// <summary>
		/// Returns this chord offset for this note.
		/// </summary>
		/// <param name="rootOffset"></param>
		/// <param name="mode"></param>
		/// <param name="chordNote"></param>
		/// <returns></returns>
		private int GetChordOffset( int rootOffset, int mode, int chordNote = 0 )
		{
			var chordOffset = 0;
			var scale = MusicConstants.GetScale( mMusicGenerator.ConfigurationData.Scale );

			for ( var index = 0; index < rootOffset + chordNote; index++ )
			{
				chordOffset += scale[( index + mode ) % scale.Count];
			}

			return chordOffset;
		}

		#endregion private
	}
}

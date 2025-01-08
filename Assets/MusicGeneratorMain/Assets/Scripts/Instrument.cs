using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Instrument Class. Handles selecting notes to play and other instrument specific functionality.
	/// </summary>
	[Serializable]
	public class Instrument
	{
		#region public

		/// <summary>
		/// index of this instrument within MusicGenerator.mInstruments()
		/// </summary>
		public int InstrumentIndex
		{
			get => mInstrumentIndex;
			set => mInstrumentIndex = value < MusicConstants.MaxInstruments ? value : 0;
		}

		public bool NoteRoll( bool force )
		{
			var didSucceed = force || InstrumentData.SuccessionType == SuccessionType.Rhythm || Random.Range( 0, 100 ) < CurrentOddsOfPlaying;

			CurrentOddsOfPlaying = didSucceed ? InstrumentData.SuccessivePlayOdds : InstrumentData.OddsOfPlaying;
			return didSucceed;
		}

		public void ResetMultipliers()
		{
			CurrentOddsOfPlaying = InstrumentData.OddsOfPlaying;
		}

		/// <summary>
		/// Whether this instrument has ever generated a theme for itself.
		/// When created, we flag ourselves to generate a new theme/repeat, as this may have happened mid-measure.
		/// </summary>
		public bool NeedsTheme { get; private set; }

		/// <summary>
		/// array of our chord offsets to use in a pattern
		/// </summary>
		public int[][] PatternNoteOffset { get; private set; } = new int[MusicConstants.InstrumentPatternLengthMax][];

		/// <summary>
		/// array of our octave offsets to use in a pattern
		/// </summary>
		public int[][] PatternOctaveOffset { get; private set; } = new int[MusicConstants.InstrumentPatternLengthMax][];

		/// <summary>
		/// Resets the number of pattern steps taken (this is handled internally by the measure  and shouldn't need to be manually changed.)
		/// </summary>
		public void ResetPatternStepsTaken()
		{
			mPatternStepsTaken = 0;
			mArpeggioStepsTaken = 0;
		}

		/// <summary>
		/// array of notes played last measure
		/// </summary>
		public int[][] RepeatingNotes { get; private set; } = new int[128][];

		/// <summary>
		/// notes of saved theme measure
		/// </summary>
		public int[][] ThemeNotes { get; private set; } = new int[128][];

		/// <summary>
		/// whether we're repeating the pattern this iteration
		/// </summary>
		public bool IsRepeatingPattern { get; private set; }

		/// <summary>
		/// whether we're setting the pattern this iteration
		/// </summary>
		public bool IsSettingPattern { get; private set; }

		/// <summary>
		/// which index of the pattern we're playing this iteration
		/// </summary>
		public int CurrentPatternStep { get; private set; }

		/// <summary>
		/// array of current pattern notes for this step:
		/// </summary>
		public int[] CurrentPatternNotes { get; private set; } =
		{
			-1, -1, -1, -1, -1
		};

		/// <summary>
		/// array of current pattern octave offsets for this step:
		/// </summary>
		public int[] CurrentPatternOctave { get; private set; } =
		{
			0, 0, 0, 0, 0
		};

		/// <summary>
		/// what our current step of the chord progression is.
		/// </summary>
		public int CurrentProgressionStep { get; private set; }

		/// <summary>
		/// pattern for our arpeggio.
		/// </summary>
		public ListArrayInt ArpeggioPattern = new ListArrayInt( new[]
		{
			0, 2, 4, 0, 2
		} );

		/// <summary>
		/// Current odds of playing for this instrument
		/// </summary>
		public float CurrentOddsOfPlaying
		{
			get => mCurrentOddsOfPlaying;
			set => mCurrentOddsOfPlaying = Mathf.Clamp( value, 0f, 100f );
		}

		/// <summary>
		/// our note generators.
		/// </summary>
		public NoteGenerator[] NoteGenerators { get; private set; } =
		{
			new NoteGenerator_Melody(), new NoteGenerator_Rhythm(), new NoteGenerator_Lead()
		};

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="instrumentData"></param>
		public Instrument( ConfigurationData.InstrumentData instrumentData = null )
		{
			InstrumentData = instrumentData;
		}

		/// <summary>
		///  Generates an arpeggio pattern for this measure:
		/// </summary>
		public void GenerateArpeggioPattern()
		{
			ArpeggioPattern.Clear();

			for ( var index = 0; index < InstrumentData.NumStrumNotes; index++ )
			{
				ArpeggioPattern.Add( mTriadPattern[index % mTriadPattern.Length] );
			}

			sRandom = new System.Random();
			for ( var index = ArpeggioPattern.Count - 1; index > 0; index-- )
			{
				var random = sRandom.Next( 0, index );
				( ArpeggioPattern[random], ArpeggioPattern[index] ) = ( ArpeggioPattern[index], ArpeggioPattern[random] );
			}
		}

		/// <summary>
		/// Instrument initialization
		/// </summary>
		/// <param name="musicGenerator"></param>
		/// <param name="index"></param>
		/// <param name="instrumentData"></param>
		public void Initialize( MusicGenerator musicGenerator, int index, ConfigurationData.InstrumentData instrumentData = null )
		{
			mMusicGenerator = musicGenerator;
			InstrumentData = instrumentData ?? new ConfigurationData.InstrumentData();
			NeedsTheme = true;
			IsRepeatingPattern = false;
			IsSettingPattern = false;
			CurrentPatternStep = 0;
			InstrumentIndex = index;
			mPatternStepsTaken = 0;
			mArpeggioStepsTaken = 0;

			InitializeClipNotes();
			InitializeRepeatingAndThemeNotes();
			InitPatternNotes();
			SetupNoteGenerators();
			GenerateArpeggioPattern();
		}

		/// <summary>
		/// Returns an array of ints corresponding to the notes to play
		/// </summary>
		/// <param name="progressionStep"></param>
		/// <param name="isMinRhythm"></param>
		/// <returns></returns>
		public int[] GetProgressionNotes( int progressionStep, bool isMinRhythm = false )
		{
			SetupNoteGeneration( progressionStep );
			SelectNotes( isMinRhythm );
			CheckValidity();
			SetRepeatNotes();
			SetPlayOdds();
			mMusicGenerator.NotesGeneratedArgs.InstrumentSet = mMusicGenerator.InstrumentSet;
			mMusicGenerator.NotesGeneratedArgs.InstrumentName = InstrumentData.InstrumentType;
			mMusicGenerator.NotesGeneratedArgs.Notes = mProgressionNotes;
			mMusicGenerator.NotesGeneratedArgs.Volume = InstrumentData.Volume;
			mMusicGenerator.NotesGeneratedArgs.InstrumentIndex = InstrumentIndex;

			if ( mMusicGenerator.OnNotesGenerated( mMusicGenerator.NotesGeneratedArgs ) == false )
			{
				mProgressionNotes = mMusicGenerator.NotesGeneratedArgs.Notes;
			}

			return mProgressionNotes;
		}

		/// <summary>
		///  Resets the generated instrument variables (theme notes, played lead notes, pattern notes).
		/// </summary>
		public void ResetInstrumentGeneratedNotes()
		{
			ClearThemeNotes();
			ClearPlayedLeadNotes();
			ClearPatternNotes();
		}

		/// <summary>
		/// Clears our played lead notes.
		/// </summary>
		public void ClearPlayedLeadNotes()
		{
			NoteGenerators[(int) SuccessionType.Lead].ClearNotes();
		}

		#endregion public

		#region private

		/// <summary>
		/// Our configuration data for this instrument
		/// </summary>
		public ConfigurationData.InstrumentData InstrumentData { get; private set; }

		/// <summary>
		/// Our current odds of playing a note
		/// </summary>
		private float mCurrentOddsOfPlaying;

		/// <summary>
		/// index of this instrument within MusicGenerator.mInstruments()
		/// </summary>
		private int mInstrumentIndex;

		/// <summary>
		/// How many steps of our pattern we've taken
		/// </summary>
		private int mPatternStepsTaken;

		/// <summary>
		/// How many times we've played our arpeggio pattern;
		/// </summary>
		private int mArpeggioStepsTaken;

		/// <summary>
		/// pattern for our Triad.
		/// </summary>
		private int[] mTriadPattern =
		{
			0, 2, 4
		};

		/// <summary>
		/// Reference to our music generator
		/// </summary>
		private MusicGenerator mMusicGenerator;

		/// <summary>
		/// our progression notes to be played
		/// </summary>
		private int[] mProgressionNotes =
		{
			-1, -1, -1, -1
		};

		/// <summary>
		/// Our Random
		/// </summary>
		/// <returns></returns>
		private static System.Random sRandom = new System.Random();

		/// <summary>
		/// Sets the pattern variables. Mostly for readability in other functions :\
		/// </summary>
		/// <param name="progressionStep"></param>
		private void SetupNoteGeneration( int progressionStep )
		{
			// chord progressions are InstrumentSet in their sensible way: I-IV-V for example starting on 1.
			// it's easier to leave like that as it's readable (from a music perspective, anyhow) and adjust here, rather than 0 based:
			CurrentProgressionStep = progressionStep - ( ( progressionStep < 0 ) ? -1 : 1 );

			if ( mPatternStepsTaken == 0 )
			{
				ClearPatternNotes();
			}

			if ( mArpeggioStepsTaken == 0 )
			{
				GenerateArpeggioPattern();
			}

			CurrentPatternStep = mPatternStepsTaken % InstrumentData.PatternLength;

			IsRepeatingPattern = mPatternStepsTaken >= InstrumentData.PatternLength &&
			                     mPatternStepsTaken < ( InstrumentData.PatternLength * InstrumentData.PatternRepeat ) - InstrumentData.PatternRelease;
			IsSettingPattern = mPatternStepsTaken < InstrumentData.PatternLength;

			if ( CurrentPatternStep < PatternNoteOffset.Length - 1 )
			{
				CurrentPatternNotes = PatternNoteOffset[CurrentPatternStep];
				CurrentPatternOctave = PatternOctaveOffset[CurrentPatternStep];
			}

			mPatternStepsTaken += 1;
			mArpeggioStepsTaken += 1;

			if ( mArpeggioStepsTaken >= InstrumentData.ArpeggioRepeat )
			{
				mArpeggioStepsTaken = 0;
			}

			if ( mPatternStepsTaken >= InstrumentData.PatternLength * InstrumentData.PatternRepeat )
			{
				mPatternStepsTaken = 0;
			}
		}

		/// <summary>
		/// Sets up our note generators.
		/// </summary>
		private void SetupNoteGenerators()
		{
			int[] MelodicFallback( NoteGenerator.Fallback x ) => NoteGenerators[(int) SuccessionType.Rhythm].GenerateNotes( true );
			int[] LeadFallback( NoteGenerator.Fallback x ) => NoteGenerators[(int) SuccessionType.Melody].GenerateNotes( true );
			NoteGenerators[(int) SuccessionType.Lead].Init( this, mMusicGenerator, LeadFallback );
			NoteGenerators[(int) SuccessionType.Melody].Init( this, mMusicGenerator, MelodicFallback );
			NoteGenerators[(int) SuccessionType.Rhythm].Init( this, mMusicGenerator, null );
		}

		/// <summary>
		/// Sets our notes to repeat.
		/// </summary>
		private void SetRepeatNotes()
		{
			var set = mMusicGenerator.InstrumentSet;
			var count = set.SixteenthStepsTaken + ( set.RepeatCount * set.TimeSignature.StepsPerMeasure );

			for ( var index = 0; index < mProgressionNotes.Length; index++ )
			{
				RepeatingNotes[count][index] = mProgressionNotes[index];
			}
		}

		/// <summary>
		///  Sets our array of notes to play, based on rhythm/leading and other variables:
		/// </summary>
		private void SelectNotes( bool isMinRhythm )
		{
			mProgressionNotes = NoteGenerators[(int) InstrumentData.SuccessionType].GenerateNotes( isMinRhythm );
		}

		/// <summary>
		/// Checks for out of range notes in our list and forces it back within range.
		/// </summary>
		private void CheckValidity()
		{
			if ( mProgressionNotes.Length != MusicConstants.MaxGeneratedNotesPerBeat )
			{
				Debug.LogError( $"We haven't fully filled our note array for {InstrumentData.InstrumentType}. Something has gone seriously wrong!" );
				mProgressionNotes = new[]
				{
					MusicConstants.UnplayedNote, MusicConstants.UnplayedNote, MusicConstants.UnplayedNote, MusicConstants.UnplayedNote
				};
				return;
			}

			// This is...perhaps the worst possible approach to solving this issue :/
			for ( var index = 0; index < mProgressionNotes.Length; index++ )
			{
				var note = mProgressionNotes[index];
				const int clipArraySize = MusicConstants.MaxInstrumentNotes;

				if ( note == MusicConstants.UnplayedNote || ( note < clipArraySize && note >= MusicConstants.UnplayedNote ) )
				{
					continue;
				}

				if ( note < 0 )
				{
					note = MusicConstants.SafeLoop( note, 0, MusicConstants.OctaveSize );
				}
				else
				{
					note = MusicConstants.SafeLoop( note, 0, MusicConstants.OctaveSize );
					note += ( InstrumentData.UsePattern && IsRepeatingPattern ) ? CurrentPatternOctave[index] * MusicConstants.OctaveSize : 2 * MusicConstants.OctaveSize;
				}

				// if somehow this is still out of range, we've utterly broken things...
				if ( note < 0 || note > clipArraySize )
				{
					Debug.Log( $"something's gone wrong, note is out of range within {InstrumentData.InstrumentType}." );
					note = 0;
				}

				mProgressionNotes[index] = note;
			}
		}

		/// <summary>
		/// Sets the theme notes from the repeating list.
		/// </summary>
		public void SetThemeNotes()
		{
			NeedsTheme = false;
			for ( var x = 0; x < RepeatingNotes.Length; x++ )
			{
				for ( var y = 0; y < RepeatingNotes[x].Length; y++ )
				{
					ThemeNotes[x][y] = RepeatingNotes[x][y];
				}
			}
		}

		/// <summary>
		/// sets our multiplier for the next played note:
		/// </summary>
		private void SetPlayOdds()
		{
			mCurrentOddsOfPlaying = InstrumentData.OddsOfPlaying;
			foreach ( var note in mProgressionNotes )
			{
				if ( note != MusicConstants.UnplayedNote )
				{
					mCurrentOddsOfPlaying = InstrumentData.SuccessivePlayOdds;
					return;
				}
			}
		}

		/// <summary>
		/// Initializes our repeat/theme notes arrays.
		/// </summary>
		private void InitializeRepeatingAndThemeNotes()
		{
			for ( var x = 0; x < RepeatingNotes.Length; x++ )
			{
				RepeatingNotes[x] = new int[MusicConstants.MaxGeneratedNotesPerBeat];
				ThemeNotes[x] = new int[MusicConstants.MaxGeneratedNotesPerBeat];
				for ( var y = 0; y < MusicConstants.MaxGeneratedNotesPerBeat; y++ )
				{
					RepeatingNotes[x][y] = MusicConstants.UnplayedNote;
					ThemeNotes[x][y] = MusicConstants.UnplayedNote;
				}
			}
		}

		/// <summary>
		/// Clears the repeating note array.
		/// </summary>
		public void ClearRepeatingNotes()
		{
			for ( var x = 0; x < RepeatingNotes.Length; x++ )
			{
				for ( var y = 0; y < MusicConstants.MaxGeneratedNotesPerBeat; y++ )
				{
					RepeatingNotes[x][y] = MusicConstants.UnplayedNote;
				}
			}
		}

		/// <summary>
		/// Clears the theme note array.
		/// </summary>
		public void ClearThemeNotes()
		{
			NeedsTheme = true;
			for ( var x = 0; x < ThemeNotes.Length; x++ )
			{
				for ( var y = 0; y < ThemeNotes[x].Length; y++ )
				{
					ThemeNotes[x][y] = MusicConstants.UnplayedNote;
				}
			}
		}

		/// <summary>
		/// Clears the pattern notes.
		/// </summary>
		public void ClearPatternNotes()
		{
			// we want to give our unset pattern a sensible value:
			var octave = InstrumentData.OctavesToUse[Random.Range( 0, InstrumentData.OctavesToUse.Count )];

			for ( var x = 0; x < MusicConstants.InstrumentPatternLengthMax; x++ )
			{
				for ( var y = 0; y < MusicConstants.MaxGeneratedNotesPerBeat; y++ )
				{
					PatternNoteOffset[x][y] = MusicConstants.UnplayedNote;
					PatternOctaveOffset[x][y] = octave;
				}
			}

			mPatternStepsTaken = 0;
		}

		/// <summary>
		/// Initializes our pattern notes.
		/// </summary>
		private void InitPatternNotes()
		{
			// we want to give our unset pattern a sensible value:
			var octave = InstrumentData.OctavesToUse[Random.Range( 0, InstrumentData.OctavesToUse.Count )];

			for ( var x = 0; x < MusicConstants.InstrumentPatternLengthMax; x++ )
			{
				PatternOctaveOffset[x] = new int[MusicConstants.MaxGeneratedNotesPerBeat];
				PatternNoteOffset[x] = new int[MusicConstants.MaxGeneratedNotesPerBeat];
				for ( var y = 0; y < MusicConstants.MaxGeneratedNotesPerBeat; y++ )
				{
					PatternNoteOffset[x][y] = MusicConstants.UnplayedNote;
					PatternOctaveOffset[x][y] = octave;
				}
			}
		}

		/// <summary>
		/// Initializes our empty clip note arrays
		/// TODO: should probably just be in the data initialization, or rethink the class architecture.
		/// </summary>
		private void InitializeClipNotes()
		{
			if ( mMusicGenerator.ConfigurationData.IsSingleClip == false )
			{
				return;
			}

			InstrumentData.ClipNotes = new List<ClipNotesMeasure>();
			for ( var x = 0; x < MusicConstants.NumMeasures; x++ )
			{
				InstrumentData.ClipNotes.Add( new ClipNotesMeasure() );
				for ( var y = 0; y < MusicConstants.MaxStepsPerMeasure; y++ )
				{
					InstrumentData.ClipNotes[x].Beats.Add( new ClipBeat() );
					for ( var z = 0; z < MusicConstants.MaxStepsPerTimestep; z++ )
					{
						InstrumentData.ClipNotes[x].Beats[y].Steps.Add( new ClipBeatStep() );
					}
				}
			}
		}

		#endregion private
	}
}

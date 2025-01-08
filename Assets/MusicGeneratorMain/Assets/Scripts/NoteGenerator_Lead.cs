using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// This class generates lead notes for a measure
	/// I regret almost everything about this class :P
	/// It's a house of cards that _will_ collapse if you so much as breathe on it.
	/// </summary>
	public class NoteGenerator_Lead : NoteGenerator
	{
		#region public

		/// <summary>
		/// Clears the stored melodic notes that have played.
		/// </summary>
		public override void ClearNotes()
		{
			mPlayedMelodicNotes.Clear();
		}

		/// <summary>
		/// Generates the next notes for this lead instrument.
		/// </summary>
		/// <returns></returns>
		public override int[] GenerateNotes( bool isMinRhythm )
		{
			mIsStandardPentatonic = mInstrument.InstrumentData.IsPentatonic;

			var tritone = mInstrument.CurrentProgressionStep < 0;
			var canPlay = mInstrument.NoteRoll( isMinRhythm );

			if ( canPlay == false || mRunEnded )
			{
				mRunEnded = false;
				AddEmptyNotes();
				return mNotes;
			}

			if ( mInstrument.InstrumentData.IsPercussion )
			{
				return GetPercussionNotes();
			}

			if ( ( Random.Range( 0, 100 ) >= mInstrument.InstrumentData.OddsOfUsingChordNotes &&
			       tritone == false ) )
			{
				var isEndOfRun = Random.Range( 0, 100 ) > mInstrument.InstrumentData.SuccessivePlayOdds;

				if ( isEndOfRun )
				{
					var note = NearestChordNote();
					AdjustRawLeadIndex( ref note );
					AddSingleNote( note );
					mInstrument.CurrentOddsOfPlaying = mInstrument.InstrumentData.OddsOfPlaying;
					mRunEnded = true;
					return mNotes;
				}

				var nextNote = GetRawLeadNoteIndex();

				// here we find the shortest rhythm step and make sure we're not playing something discordant if it may be playing as well.
				var shortestRhythmTimestep = (int) mMusicGenerator.InstrumentSet.GetShortestSuccessionType( SuccessionType.Rhythm );
				var shortestMelodicTimestep = (int) mMusicGenerator.InstrumentSet.GetShortestSuccessionType( SuccessionType.Melody );
				var shortestTimestep = Mathf.Min( shortestRhythmTimestep, shortestMelodicTimestep );
				shortestTimestep = mMusicGenerator.InstrumentSet.GetInverseProgressionRate( shortestTimestep );
				if ( shortestTimestep == 1 || mMusicGenerator.InstrumentSet.SixteenthStepsTaken % shortestTimestep == 0 )
				{
					if ( IsAvoidNote( nextNote ) )
					{
						nextNote = FixAvoidNote( nextNote );
					}
				}

				mPlayedMelodicNotes.Add( nextNote );
				AdjustRawLeadIndex( ref nextNote );

				AddSingleNote( nextNote );

				return mNotes;
			}

			return mFallback();
		}

		/// <summary>
		/// Returns true if this note should be avoided in pentatonic scales.
		/// </summary>
		/// <param name="note"></param>
		/// <returns></returns>
		public bool IsPentatonicAvoid( int note )
		{
			var scaleNote = MusicConstants.SafeLoop( note, 0, MusicConstants.ScaleLength );
			mAvoidNotes = mInstrument.InstrumentData.LeadAvoidNotes;

			if ( mIsStandardPentatonic )
			{
				if ( mMusicGenerator.ConfigurationData.Scale == Scale.Major || mMusicGenerator.ConfigurationData.Scale == Scale.HarmonicMajor )
				{
					return scaleNote == MusicConstants.MajorPentatonicAvoid[0] || scaleNote == MusicConstants.MajorPentatonicAvoid[1];
				}

				if ( mMusicGenerator.ConfigurationData.Scale == Scale.NatMinor ||
				     mMusicGenerator.ConfigurationData.Scale == Scale.HarmonicMinor ||
				     mMusicGenerator.ConfigurationData.Scale == Scale.MelodicMinor )
				{
					return scaleNote == MusicConstants.MinorPentatonicAvoid[0] || scaleNote == MusicConstants.MinorPentatonicAvoid[1];
				}
			}
			else
			{
				return ( mAvoidNotes[0] > 0 && scaleNote == mAvoidNotes[0] ) || ( mAvoidNotes[1] > 0 && scaleNote == mAvoidNotes[1] );
			}

			return false;
		}

		#endregion public

		#region protected

		protected override int[] AddRepeatNotes()
		{
			//not implemented
			return new[]
			{
				-1, -1, -1, -1, -1
			};
		}

		#endregion protected

		#region private

		// Unused
		//private const int DiminishedStep = 6;

		/// <summary>
		/// list of melodic notes we've already played (for determining ascend/descend)
		/// </summary>
		private readonly List<int> mPlayedMelodicNotes = new List<int>();

		/// <summary>
		/// for lead influence
		/// </summary>
		private const int DESCENDING_INFLUENCE = -1;

		/// <summary>
		/// for lead influence
		/// </summary>
		private const int ASCENDING_INFLUENCE = 1;

		/// <summary>
		/// Whether we ended a run of notes.
		/// </summary>
		private bool mRunEnded;

		/// <summary>
		/// Whether we're currently using the pentatonic scale
		/// </summary>
		private bool mIsStandardPentatonic;

		/// <summary>
		/// Our current avoid notes
		/// </summary>
		private int[] mAvoidNotes =
		{
			-1, -1
		};

		/// <summary>
		/// Whether the next selected note will have its ascension forced in its current direction
		/// </summary>
		private bool mForceAscension;

		/// <summary>
		/// Gets the next melodic note.
		/// </summary>
		/// <returns></returns>
		private int GetRawLeadNoteIndex()
		{
			GetMinMaxStepOctaves( out var min, out var max );

			var note = Random.Range( min, max );

			if ( mPlayedMelodicNotes.Count == 0 )
			{
				if ( mForceAscension == false )
				{
					mInstrument.InstrumentData.LeadInfluence = Random.Range( 0, 100 ) > 50 ? ASCENDING_INFLUENCE : DESCENDING_INFLUENCE;
				}
			}
			else
			{
				var ultimateNote = mPlayedMelodicNotes[mPlayedMelodicNotes.Count - 1];
				var step = Random.Range( mInstrument.InstrumentData.LeadInfluence,
					( mInstrument.InstrumentData.LeadMaxSteps * mInstrument.InstrumentData.LeadInfluence ) );

				if ( ultimateNote + step >= min && ultimateNote + step < max )
				{
					note = ultimateNote + step;
					mForceAscension = false;
				}
				else
				{
					note = ultimateNote - step;
					mInstrument.InstrumentData.LeadInfluence *= -1;
					mForceAscension = true;
				}
			}

			if ( Random.Range( 0, 100 ) > mInstrument.InstrumentData.AscendDescendInfluence && mForceAscension == false )
			{
				mInstrument.InstrumentData.LeadInfluence *= -1;
			}

			return note;
		}

		/// <summary>
		/// Returns true if this lead note is a half step above a chord note:
		/// </summary>
		/// <param name="noteIn"></param>
		/// <returns></returns>
		private bool IsAvoidNote( int noteIn )
		{
			if ( IsPentatonicAvoid( noteIn ) )
			{
				return true;
			}

			if ( noteIn < 0 )
			{
				noteIn = MusicConstants.ScaleLength + noteIn;
			}

			var note = MusicConstants.SafeLoop( noteIn, 0, MusicConstants.ScaleLength );
			var scaleNote = MusicConstants.SafeLoop( note - 1 + (int) mMusicGenerator.ConfigurationData.Mode,
				0,
				MusicConstants.ScaleLength );
			var scale = MusicConstants.GetScale( mMusicGenerator.ConfigurationData.Scale );
			var isHalfStep = scale[scaleNote] == MusicConstants.HalfStep;
			var progressionStep = ( mInstrument.CurrentProgressionStep < 0 ) ? mInstrument.CurrentProgressionStep * -1 : mInstrument.CurrentProgressionStep;
			var isAboveChordNode = ( note == ( MusicConstants.SeventhChord[0] + progressionStep + 1 ) % MusicConstants.ScaleLength ||
			                         note == ( MusicConstants.SeventhChord[1] + progressionStep + 1 ) % MusicConstants.ScaleLength ||
			                         note == ( MusicConstants.SeventhChord[2] + progressionStep + 1 ) % MusicConstants.ScaleLength );

			//var isAvoidedSeventh = note == MusicConstants.SeventhChord[3];
			return ( isHalfStep && isAboveChordNode ); // || isAvoidedSeventh;
		}

		/// <summary>
		/// Fixes an avoid note to (hopefully) not be discordant:
		/// </summary>
		/// <param name="nextNote"></param>
		/// <returns></returns>
		private int FixAvoidNote( int nextNote )
		{
			var adjustedNote = nextNote;
			var isAvoidNote = true;

			// we use scale length because it will attempt to fix each possible step
			GetMinMaxStepOctaves( out var min, out var max );
			var maxAttempts = MusicConstants.ScaleLength * 2;

			for ( var index = 1; index < maxAttempts && isAvoidNote; index++ )
			{
				var testNote = nextNote + ( index * mInstrument.InstrumentData.LeadInfluence );
				if ( testNote < min || testNote >= max )
				{
					mInstrument.InstrumentData.LeadInfluence *= -1;
					mForceAscension = true;
				}

				adjustedNote = nextNote + ( index * mInstrument.InstrumentData.LeadInfluence );

				if ( IsAvoidNote( adjustedNote ) == false )
				{
					nextNote = adjustedNote;
					isAvoidNote = false;
				}
			}

			return adjustedNote;
		}

		/// <summary>
		/// steps the note through the scale, adjusted for mode, key, progression step to find th
		/// actual note index instead of our raw steps.
		/// </summary>
		/// <param name="noteIn"></param>
		/// <returns></returns>
		private void AdjustRawLeadIndex( ref int noteIn )
		{
			var note = 0;
			var scale = MusicConstants.GetScale( mMusicGenerator.ConfigurationData.Scale );
			var isNegative = noteIn < 0;
			var modifier = isNegative ? -1 : 1;
			noteIn = Mathf.Abs( noteIn );

			for ( var j = 0; j < noteIn; j++ )
			{
				var testNote = note;
				//oof
				// eh...here, we step _down_ the scale, rather than up. This is all very...inelegant. 
				// Fair warning if you try to edit this though, it unravels quickly... O.o
				var index = isNegative ? MusicConstants.ScaleLength - j + (int) mMusicGenerator.ConfigurationData.Mode - 1 : j + (int) mMusicGenerator.ConfigurationData.Mode;

				if ( index < 0 )
				{
					index *= -1;
				}

				index %= MusicConstants.ScaleLength;
				testNote += scale[index] * modifier;

				note = testNote;
			}


			note += (int) mMusicGenerator.ConfigurationData.Key;

			var unClampedNote = note;

			// under normal circumstances this will not be clamped, but if changing available octaves in real time, it can become necessary.
			// Mostly needed for ui editor support. 
			if ( ClampNote( note, out var clampedNote ) )
			{
				mInstrument.InstrumentData.LeadInfluence = unClampedNote > clampedNote ? -1 : 1;
				mForceAscension = true;
			}

			noteIn = clampedNote;
		}

		/// <summary>
		/// Returns the number of additional steps for a give key offset.
		/// Because we shift things over for the key, in order to maintain our min/max of 0-35 we need to shift things down
		/// This varies per key, depending on mode, scale, etc.
		/// </summary>
		/// <param name="negative"></param>
		/// <returns></returns>
		private int GetNumberOfAdditionalStepsInKey( bool negative )
		{
			var key = (int) mMusicGenerator.ConfigurationData.Key;
			var scale = MusicConstants.GetScale( mMusicGenerator.ConfigurationData.Scale );
			var steps = 0;
			for ( var index = 0; index < key; index++ )
			{
				var stepIndex = negative ?
					MusicConstants.ScaleLength - index + (int) mMusicGenerator.ConfigurationData.Mode - 1 :
					index + (int) mMusicGenerator.ConfigurationData.Mode;

				if ( stepIndex < 0 )
				{
					stepIndex = MusicConstants.ScaleLength + stepIndex;
				}

				stepIndex %= MusicConstants.ScaleLength;

				steps += scale[stepIndex];

				if ( steps > key )
				{
					return index;
				}
			}

			return 0;
		}

		/// <summary>
		/// Attempts to find the nearest chord note
		/// </summary>
		/// <returns></returns>
		private int NearestChordNote()
		{
			var next = GetRawLeadNoteIndex();
			var attempts = 0;
			GetMinMaxStepOctaves( out _, out var max );
			var progressionStep = ( mInstrument.CurrentProgressionStep < 0 ) ? mInstrument.CurrentProgressionStep * -1 : mInstrument.CurrentProgressionStep;

			while ( attempts < MusicConstants.ScaleLength )
			{
				// the next bit is weird, but best left alone...
				if ( next >= max )
				{
					next = MusicConstants.SafeLoop( next, max - MusicConstants.ScaleLength, max );
				}

				var note = next;

				if ( note < 0 )
				{
					note = MusicConstants.ScaleLength + note;
				}


				var test = note % MusicConstants.ScaleLength;

				var isChordNote = ( test == ( MusicConstants.SeventhChord[0] + progressionStep ) % MusicConstants.ScaleLength ||
				                test == ( MusicConstants.SeventhChord[1] + progressionStep ) % MusicConstants.ScaleLength ||
				                test == ( MusicConstants.SeventhChord[2] + progressionStep ) % MusicConstants.ScaleLength );

				if ( isChordNote &&
				     ( mPlayedMelodicNotes.Count == 0 || note != mPlayedMelodicNotes[mPlayedMelodicNotes.Count - 1] ) )
				{
					mPlayedMelodicNotes.Add( note );
					return note;
				}

				attempts++;
				next++;
			}

			return 0;
		}

		/// <summary>
		/// Returns the min/max number of steps for our octave range, taking into account the key offset
		/// </summary>
		/// <param name="minOctave"></param>
		/// <param name="maxOctave"></param>
		private void GetMinMaxStepOctaves( out int minOctave, out int maxOctave )
		{
			var highestOctave = 0;
			var lowestOctave = 2;
			for ( var index = 0; index < mInstrument.InstrumentData.OctavesToUse.Count; index++ )
			{
				var octave = mInstrument.InstrumentData.OctavesToUse[index];
				highestOctave = octave > highestOctave ? octave : highestOctave;
				lowestOctave = octave < lowestOctave ? octave : lowestOctave;
			}

			minOctave = lowestOctave * MusicConstants.ScaleLength - GetNumberOfAdditionalStepsInKey( true );
			maxOctave = highestOctave * MusicConstants.ScaleLength + MusicConstants.ScaleLength - GetNumberOfAdditionalStepsInKey( true );
		}

		/// <summary>
		/// Clamps our note within available octaves
		/// </summary>
		/// <param name="note"></param>
		/// <param name="clampedNote"></param>
		/// <returns></returns>
		private bool ClampNote( int note, out int clampedNote )
		{
			GetOctaveRanges( out var min, out var max );
			clampedNote = MusicConstants.SafeLoop( note, min, max );
			return clampedNote != note;
		}

		/// <summary>
		/// returns the available octave range
		/// </summary>
		/// <param name="minOctave"></param>
		/// <param name="maxOctave"></param>
		private void GetOctaveRanges( out int minOctave, out int maxOctave )
		{
			var highestOctave = 0;
			var lowestOctave = 2;
			for ( var index = 0; index < mInstrument.InstrumentData.OctavesToUse.Count; index++ )
			{
				var octave = mInstrument.InstrumentData.OctavesToUse[index];
				highestOctave = octave > highestOctave ? octave : highestOctave;
				lowestOctave = octave < lowestOctave ? octave : lowestOctave;
			}

			minOctave = lowestOctave * MusicConstants.OctaveSize;
			maxOctave = highestOctave * MusicConstants.OctaveSize + MusicConstants.OctaveSize;
		}

		#endregion private
	}
}

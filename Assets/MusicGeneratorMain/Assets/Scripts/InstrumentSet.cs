using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// The InstrumentSet of instruments for a configuration. Handles the timing, playing, repeating and other settings for its instruments.
	/// For normal uses of the generator, you should not need to call any of the public functions in here, as they're handled by the
	/// MusicGenerator or the SingleClip logic.
	/// </summary>
	[Serializable]
	public class InstrumentSet
	{
		#region public

		/// <summary>
		/// repeat count for sixteenth steps
		/// </summary>
		public int SixteenthRepeatCount
		{
			get => mSixteenthRepeatCount;
			set => mSixteenthRepeatCount = Mathf.Clamp( value, 0, MusicConstants.MaxStepsPerMeasure * Data.RepeatMeasuresNum );
		}

		/// <summary>
		/// number of 1/16 steps taken for current measure
		/// </summary>
		public int SixteenthStepsTaken
		{
			get => mSixteenthStepsTaken;
			set => mSixteenthStepsTaken = Mathf.Clamp( value, 0, MusicConstants.MaxStepsPerMeasure );
		}

		/// <summary>
		/// timer for single steps
		/// </summary>
		public float SixteenthStepTimer { get; set; }

		/// <summary>
		/// length of measures. Used for timing.InstrumentSet on start
		/// </summary>
		public float BeatLength { get; private set; }

		/// <summary>
		/// how many times we've repeated the measure.
		/// </summary>
		public int RepeatCount { get; set; }

		/// <summary>
		/// How many times we've repeated the percussion measure
		/// </summary>
		public int PercussionRepeatCount { get; set; }

		/// <summary>
		/// delay to balance out when we start a new measure
		/// </summary>
		public float MeasureStartTimer { get; set; }

		/// <summary>
		/// how many steps in the chord progression have been taken
		/// </summary>
		public int ProgressionStepsTaken
		{
			get => mProgressionStepsTaken;
			set => mProgressionStepsTaken = Mathf.Clamp( value, -1, MusicGenerator == null ? MusicConstants.MaxFullstepsTaken : MusicGenerator.CurrentChordProgression.Count );
		}

		/// <summary>
		/// resets the progression steps taken.
		/// </summary>
		public void ResetProgressionSteps()
		{
			ProgressionStepsTaken = -1;
		}

		/// <summary>
		/// Our Configuration data for this instrument InstrumentSet
		/// </summary>
		public ConfigurationData Data { get; private set; }

		/// <summary>
		/// Sets our Configuration data
		/// </summary>
		/// <param name="data"></param>
		public void SetData( ConfigurationData data )
		{
			Data = data;
		}

		/// <summary>
		/// Reference to the music generator
		/// </summary>
		public MusicGenerator MusicGenerator { get; private set; }

		/// <summary>
		/// Our time signature object.
		/// </summary>
		public TimeSignature TimeSignature { get; private set; } = new TimeSignature();

		/// <summary>
		/// list of our current instruments
		/// </summary>
		public List<Instrument> Instruments { get; private set; } = new List<Instrument>();

		/// <summary>
		/// Returns whether this instrument's group is currently playing
		/// </summary>
		public bool[] GroupIsPlaying => mGroupIsPlaying;

		/// <summary>
		/// Overrides our GroupIsPlaying value for index
		/// Note: this applies only until the generator decides new values. If you need this permanent
		/// Additionally InstrumentSet the ManualGroupOdds value to true.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="isPlaying"></param>
		public void OverrideGroupIsPlaying( int index, bool isPlaying )
		{
			mGroupIsPlaying[index] = isPlaying;
			MusicGenerator.GroupsWereChosen.Invoke();
		}

		/// <summary>
		/// Initializes music InstrumentSet.
		/// </summary>
		public InstrumentSet( MusicGenerator musicGenerator )
		{
			MusicGenerator = musicGenerator;
			BeatLength = 0;
			mCurrentGroupLevel = 0;
			TimeSignature.Initialize( musicGenerator );
		}

		/// <summary>
		/// Returns the shortest rhythm timestep
		/// </summary>
		/// <returns></returns>
		public Timestep GetShortestSuccessionType( SuccessionType successionType )
		{
			var shortestTime = Timestep.Whole;
			foreach ( var instrument in Instruments )
			{
				if ( instrument.InstrumentData.SuccessionType == successionType )
				{
					shortestTime = instrument.InstrumentData.TimeStep < shortestTime ? instrument.InstrumentData.TimeStep : shortestTime;
				}
			}

			return shortestTime;
		}

		/// <summary>
		/// Sets the time signature data:
		/// </summary>
		/// <param name="signature"></param>
		public void SetTimeSignature( TimeSignatures signature )
		{
			if ( Data == null )
			{
				return;
			}

			Data.TimeSignature = signature;
			TimeSignature.SetTimeSignature( Data.TimeSignature );
			var progressionRateIndex = GetProgressionRateIndex( ( Data.ProgressionRate ) );
			SetInverseProgressionRate( progressionRateIndex );
		}

		/// <summary>
		/// Resets the instrument InstrumentSet values:
		/// </summary>
		public void Reset()
		{
			if ( MusicGenerator == false )
			{
				return;
			}

			RepeatCount = 0;
			PercussionRepeatCount = 0;
			mCurrentGroupLevel = 0;

			ProgressionStepsTaken = -1;

			if ( MusicGenerator.GeneratorState == GeneratorState.Repeating )
			{
				MusicGenerator.SetState( GeneratorState.Playing );
			}

			foreach ( var instrument in Instruments )
			{
				instrument.ResetInstrumentGeneratedNotes();
			}

			if ( Data.ManualGroupOdds == false && MusicGenerator.GroupsAreTemporarilyOverriden == false )
			{
				ResetGroups();
			}
		}

		/// <summary>
		/// Gets the inverse progression rate.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int GetInverseProgressionRate( int value )
		{
			value = value >= 0 && value < TimeSignature.NotesPerTimestepInverse.Length ? value : 0;
			return TimeSignature.NotesPerTimestepInverse[value];
		}

		/// <summary>
		/// Returns the progression rate.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int GetProgressionRate( int value )
		{
			return TimeSignature.NotesPerTimestep[Mathf.Clamp( value, 0, TimeSignature.NotesPerTimestep.Length )];
		}

		/// <summary>
		/// Returns the index for the progression rate.
		/// </summary>
		/// <param name="progressionRate"></param>
		/// <returns></returns>
		public int GetProgressionRateIndex( int progressionRate )
		{
			/* This isn't 100% accurate when switching signatures, will favor higher rate for signatures with
			 * duplicate steps (instrumentIndex.e. 3/4 time has both its half and quarter step at 3 steps.)
			 * So, when switching between them the data of which one were were 'really' at is lost*/
			switch ( progressionRate )
			{
				case 20:
				case 16:
				case 12:
					return 4;
				case 10:
				case 8:
				case 6:
					return 3;
				case 5:
				case 4:
				case 3:
					return 2;
				case 2:
					return 1;
				case 1:
					return 0;
			}

			return -1;
		}

		/// <summary>
		/// Sets the progression rate.
		/// </summary>
		/// <param name="value"></param>
		public void SetProgressionRate( int value )
		{
			Data.ProgressionRate = GetProgressionRate( Mathf.Clamp( value, 0, TimeSignature.StepsPerMeasure ) );
		}

		/// <summary>
		/// Sets our progression rate via its inverse index (bad coding on my part, some containers have this inverse :/ )
		/// </summary>
		/// <param name="value"></param>
		public void SetInverseProgressionRate( int value )
		{
			Data.ProgressionRate = GetInverseProgressionRate( Mathf.Clamp( value, 0, TimeSignature.StepsPerMeasure ) );
		}

		/// <summary>
		/// Updates the tempo.
		/// </summary>
		public void UpdateTempo()
		{
			var minute = 60f;
			BeatLength = minute / Data.Tempo; //beats per minute
		}

		/// <summary>
		/// strums a clip.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="instIndex"></param>
		public void Strum( int[] clip, int instIndex )
		{
			MusicGenerator.StartCoroutine( StrumClip( clip, instIndex ) );
		}

		/// <summary>
		/// staggers the playClip() call:
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="instrumentIndex"></param>
		/// <returns></returns>
		public IEnumerator StrumClip( int[] clip, int instrumentIndex )
		{
			// just in case we've killed things in the interim.
			if ( Instruments.Count <= instrumentIndex ||
			     ( MusicGenerator.GeneratorState != GeneratorState.Playing &&
			       MusicGenerator.GeneratorState != GeneratorState.Repeating &&
			       MusicGenerator.GeneratorState != GeneratorState.ManualPlay ) )
			{
				yield break;
			}

			if ( Instruments[instrumentIndex].InstrumentData.SuccessionType != SuccessionType.Lead && Instruments[instrumentIndex].InstrumentData.Arpeggio == false )
			{
				if ( Instruments[instrumentIndex].InstrumentData.ReverseStrum )
				{
					Array.Sort( clip, ( ( i1, i2 ) => i2.CompareTo( i1 ) ) );
				}
				else
				{
					Array.Sort( clip );
				}
			}

			var variation = UnityEngine.Random.Range( 0, Instruments[instrumentIndex].InstrumentData.StrumVariation );

			foreach ( var note in clip )
			{
				if ( Instruments.Count <= instrumentIndex ||
				     ( MusicGenerator.GeneratorState != GeneratorState.Playing &&
				       MusicGenerator.GeneratorState != GeneratorState.Repeating &&
				       MusicGenerator.GeneratorState != GeneratorState.ManualPlay ) )
				{
					yield break;
				}

				if ( note == MusicConstants.UnplayedNote || Instruments.Count <= instrumentIndex )
				{
					continue;
				}

				MusicGenerator.PlayAudioClip( this,
					Instruments[instrumentIndex].InstrumentData.InstrumentType,
					note,
					Instruments[instrumentIndex].InstrumentData.Volume,
					instrumentIndex );
				var notesPerTimestep = TimeSignature.NotesPerTimestepInverse[(int) Instruments[instrumentIndex].InstrumentData.TimeStep];
				var delay = Mathf.Max(
					( ( Instruments[instrumentIndex].InstrumentData.StrumLength * BeatLength * notesPerTimestep ) /
					  Instruments[instrumentIndex].InstrumentData.NumStrumNotes ) + variation,
					float.MinValue );
				yield return new WaitForSeconds( delay );
			}
		}

		/// <summary>
		/// Resets groups to only the first playing. Ignored if manual groups are enabled
		/// </summary>
		public void ResetGroups()
		{
			if ( Data.ManualGroupOdds || MusicGenerator.GroupsAreTemporarilyOverriden )
			{
				return;
			}

			for ( var index = 0; index < mGroupIsPlaying.Length; index++ )
			{
				mGroupIsPlaying[index] = index == 0;
			}
		}

		/// <summary>
		/// Selects which instrument groups will play next measure.
		/// </summary>
		public void SelectGroups()
		{
			var roll = UnityEngine.Random.Range( 0, 100 );

			var groupChangeRoll = roll < MusicGenerator.ConfigurationData.OverallGroupChangeOdds;

			if ( groupChangeRoll == false || Data.ManualGroupOdds || MusicGenerator.GroupsAreTemporarilyOverriden )
			{
				return;
			}

			var rate = MusicGenerator.ConfigurationData.GroupRate;

			if ( ( rate == GroupRate.Measure ) ||
			     ( ( rate == GroupRate.Progression && ProgressionStepsTaken >= MusicGenerator.CurrentChordProgression.Count - 1 ) ) )
			{
				// Either randomly choose which groups play or:
				if ( MusicGenerator.ConfigurationData.DynamicStyle == DynamicStyle.Random )
				{
					for ( var index = 0; index < mGroupIsPlaying.Length; index++ )
						mGroupIsPlaying[index] = ( UnityEngine.Random.Range( 0, 100.0f ) < MusicGenerator.ConfigurationData.GroupOdds[index] );
				}
				else //we ascend / descend through our levels.
				{
					const int ascend = 1;
					const int descend = -1;
					var numGroup = MusicGenerator.ConfigurationData.GroupOdds.Length;

					var change = mCurrentGroupLevel == 0 ? ascend : UnityEngine.Random.Range( 0, 100 ) < 50 ? ascend : descend;

					var potentialLevel = change + mCurrentGroupLevel;

					if ( potentialLevel < 0 )
					{
						potentialLevel = mCurrentGroupLevel;
					}

					if ( potentialLevel >= MusicGenerator.ConfigurationData.GroupOdds.Length )
					{
						potentialLevel = 0;
					}

					//roll to see if we can change.
					if ( UnityEngine.Random.Range( 0, 100.0f ) > MusicGenerator.ConfigurationData.GroupOdds[potentialLevel] )
					{
						potentialLevel = mCurrentGroupLevel;
					}

					mCurrentGroupLevel = potentialLevel;
					for ( var index = 0; index < numGroup; index++ )
					{
						mGroupIsPlaying[index] = index <= mCurrentGroupLevel;
					}
				}

				MusicGenerator.GroupsWereChosen.Invoke();
			}
		}

		/// <summary>
		/// Sets all multipliers back to their base.
		/// </summary>
		public void ResetMultipliers()
		{
			foreach ( var instrument in Instruments )
			{
				instrument.ResetMultipliers();
			}
		}

		#endregion public

		#region private

		/// <summary>
		/// repeat count for sixteenth steps
		/// </summary>
		private int mSixteenthRepeatCount;

		/// <summary>
		/// number of 1/16 steps taken for current measure
		/// </summary>
		private int mSixteenthStepsTaken;

		/// <summary>
		/// how many steps in the chord progression have been taken
		/// </summary>
		private int mProgressionStepsTaken = -1;

		/// <summary>
		/// if using linear dynamic style, this is our current level of groups that are playing.
		/// </summary>
		private int mCurrentGroupLevel;

		/// <summary>
		/// whether this group is currently playing
		/// </summary>
		private bool[] mGroupIsPlaying =
		{
			true, false, false, false
		};

		#endregion private
	}
}

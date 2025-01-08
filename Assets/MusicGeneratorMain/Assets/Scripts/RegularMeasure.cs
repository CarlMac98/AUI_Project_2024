using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// A regular, non-repeating measure.
	/// </summary>
	public class RegularMeasure : Measure
	{
		/// <summary>
		/// Plays through the next step in the measure.
		/// </summary>
		/// <param name="set"></param>
		public override void PlayMeasure( InstrumentSet set )
		{
			set.UpdateTempo();
			set.SixteenthStepTimer -= Time.deltaTime;

			if ( set.MusicGenerator == false || ( set.SixteenthStepTimer > 0 ) )
			{
				return;
			}

			if ( set.SixteenthStepsTaken == set.TimeSignature.StepsPerMeasure )
			{
				set.MusicGenerator.GenerateNewProgression();
				ResetMeasure( set, set.MusicGenerator.SetThemeRepeat );
				return;
			}

			PlayBeat( set );
		}

		/// <summary>
		/// Plays a beat for this measure. If manually overriding, use this to advance
		/// </summary>
		/// <param name="set"></param>
		[UsedImplicitly]
		public void PlayBeat( InstrumentSet set )
		{
			if ( set.MusicGenerator == false )
			{
				return;
			}

			if ( set.SixteenthStepsTaken % set.Data.ProgressionRate == set.TimeSignature.Whole )
			{
				set.ProgressionStepsTaken += 1;
				set.ProgressionStepsTaken = set.ProgressionStepsTaken % set.MusicGenerator.CurrentChordProgression.Count;
				set.MusicGenerator.CheckKeyChange();
			}

			set.MusicGenerator.BeatWillPlay.Invoke();

			if ( set.SixteenthStepsTaken % set.TimeSignature.Half == 0 )
			{
				TakeStep( set, Timestep.Eighth, set.ProgressionStepsTaken );
			}

			if ( set.SixteenthStepsTaken % set.TimeSignature.Quarter == 0 )
			{
				TakeStep( set, Timestep.Quarter, set.ProgressionStepsTaken );
			}

			if ( set.SixteenthStepsTaken % set.TimeSignature.Eighth == 0 )
			{
				TakeStep( set, Timestep.Half, set.ProgressionStepsTaken );
			}

			if ( set.SixteenthStepsTaken % set.TimeSignature.Sixteenth == 0 )
			{
				TakeStep( set, Timestep.Whole, set.ProgressionStepsTaken );
				set.MeasureStartTimer = -set.SixteenthStepTimer;
			}

			TakeStep( set, Timestep.Sixteenth, set.ProgressionStepsTaken );

			set.SixteenthStepTimer = set.BeatLength + set.SixteenthStepTimer;
			set.SixteenthStepsTaken += 1;
			set.MusicGenerator.BeatDidPlay.Invoke();
		}

		/// <summary>
		/// /// Plays the next step in the measure.
		/// </summary>
		/// <param name="set"></param>
		/// <param name="timeStep"></param>
		/// <param name="stepsTaken"></param>
		public override void TakeStep( InstrumentSet set, Timestep timeStep, int stepsTaken = 0 )
		{
			for ( var instIndex = 0; instIndex < set.Instruments.Count; instIndex++ )
			{
				var instrument = set.Instruments[instIndex];
				var groupIsPlaying = set.GroupIsPlaying[instrument.InstrumentData.Group];
				var useLeitmotif = set.MusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif &&
				                   instrument.InstrumentData.Leitmotif.IsEnabled &&
				                   set.MusicGenerator.LeitmotifIsTemporarilySuspended == false &&
				                   IsUsingLeitmotif && set.RepeatCount < set.MusicGenerator.ConfigurationData.NumLeitmotifMeasures &&
				                   ( groupIsPlaying || set.MusicGenerator.ConfigurationData.LeitmotifIgnoresGroups );
				var useForcedPercussion = set.Instruments[instIndex].InstrumentData.UseForcedPercussion;
				var minRhythm = set.MusicGenerator.InstrumentSet.GetInverseProgressionRate( (int) instrument.InstrumentData.MinMelodicRhythmTimeStep );
				var canPlayMinRhythm = instrument.InstrumentData.KeepMelodicRhythm && instrument.InstrumentData.SuccessionType != SuccessionType.Rhythm && minRhythm != 0 &&
				                       set.SixteenthStepsTaken % minRhythm == 0 && instrument.InstrumentData.ForceBeat == false;
				var forceBeat = instrument.InstrumentData.ForceBeat && instrument.InstrumentData.ForcedBeats[set.SixteenthStepsTaken] && timeStep == Timestep.Sixteenth;
				var isMinRhythm = instrument.InstrumentData.SuccessionType != SuccessionType.Rhythm &&
				                  instrument.InstrumentData.KeepMelodicRhythm && forceBeat == false && timeStep == instrument.InstrumentData.MinMelodicRhythmTimeStep;

				var isNormalTimestep = instrument.InstrumentData.ForceBeat == false && canPlayMinRhythm == false &&
				                       instrument.InstrumentData.TimeStep == timeStep;

				if ( instrument.InstrumentData.IsMuted ||
				     ( groupIsPlaying == false && useLeitmotif == false ) )
				{
					continue;
				}

				if ( useLeitmotif &&
				     timeStep == Timestep.Sixteenth )
				{
					PlayLeitmotifNotes( set, instrument, instIndex );
				}
				else if ( useLeitmotif == false && useForcedPercussion )
				{
					PlayForcedPercussion( set, instrument, instIndex, isMinRhythm );
				}
				else if ( useLeitmotif == false &&
				          ( isNormalTimestep ||
				            isMinRhythm ||
				            forceBeat ) )
				{
					PlayNotes( set, instrument, stepsTaken, instIndex, isMinRhythm );
				}
			}
		}

		/// <summary>
		/// Exits a non-repeating measure, resetting values to be able to play the next:
		/// </summary>
		/// <param name="set"></param>
		/// <param name="setThemeRepeat"></param>
		/// <param name="hardReset"></param>
		/// <param name="isRepeating"></param>
		public override void ResetMeasure( InstrumentSet set, Action setThemeRepeat = null, bool hardReset = false, bool isRepeating = true )
		{
			ResetRegularMeasure( set, setThemeRepeat, hardReset );
		}

		/// <summary>
		/// Plays the notes for this timestep
		/// </summary>
		/// <param name="set"></param>
		/// <param name="instrument"></param>
		/// <param name="stepsTaken"></param>
		/// <param name="instIndex"></param>
		/// <param name="isMinRhythm"></param>
		private static void PlayNotes( InstrumentSet set, Instrument instrument, int stepsTaken, int instIndex, bool isMinRhythm )
		{
			var progressionStep = set.MusicGenerator.CurrentChordProgression[stepsTaken];

			if ( instrument.InstrumentData.StrumLength == 0.0f || instrument.InstrumentData.SuccessionType == SuccessionType.Lead )
			{
				foreach ( var note in instrument.GetProgressionNotes( progressionStep, isMinRhythm ) )
				{
					if ( note != MusicConstants.UnplayedNote )
					{
						try
						{
							set.MusicGenerator.PlayAudioClip( set,
								instrument.InstrumentData.InstrumentType,
								note,
								instrument.InstrumentData.Volume,
								instIndex );
						}
						catch ( ArgumentOutOfRangeException e )
						{
							throw new ArgumentOutOfRangeException( e.Message );
						}
					}
				}
			}
			else
				set.Strum( instrument.GetProgressionNotes( progressionStep, isMinRhythm ), instIndex );
		}

		/// <summary>
		/// Plays a forced percussion note
		/// </summary>
		/// <param name="set"></param>
		/// <param name="instrument"></param>
		/// <param name="instIndex"></param>
		/// <param name="isMinRhythm"></param>
		private static void PlayForcedPercussion( InstrumentSet set, Instrument instrument, int instIndex, bool isMinRhythm )
		{
			var step = MusicConstants.GetBeat( set.Data.TimeSignature, set.SixteenthStepsTaken );
			var stepsTaken = MusicConstants.GetBeatStep( set.Data.TimeSignature, set.SixteenthStepsTaken );
			var passedRoll = instrument.NoteRoll( false );

			if ( passedRoll && instrument.InstrumentData.ForcedPercussiveNotes.Measures[set.PercussionRepeatCount % set.Data.NumForcedPercussionMeasures]
				.Timesteps[step]
				.Notes[stepsTaken] )
			{
				if ( instrument.InstrumentData.StrumLength == 0.0f )
				{
					set.MusicGenerator.PlayAudioClip( set, instrument.InstrumentData.InstrumentType, 0, instrument.InstrumentData.Volume, instIndex );
				}
				else
				{
					var progressionStep = set.MusicGenerator.CurrentChordProgression[stepsTaken];
					set.Strum( instrument.GetProgressionNotes( progressionStep, isMinRhythm ), instIndex );
				}
			}
		}

		/// <summary>
		/// Plays a note from the leitmotif
		/// </summary>
		/// <param name="set"></param>
		/// <param name="instrument"></param>
		/// <param name="instIndex"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private static void PlayLeitmotifNotes( InstrumentSet set, Instrument instrument, int instIndex )
		{
			MusicConstants.GetBeatInfo( set.TimeSignature.Signature, set.SixteenthStepsTaken, out var beat, out var beatStep );
			var leitmotifNotes = instrument.InstrumentData.Leitmotif.GetLeitmotifNotes( set.RepeatCount, beat, beatStep );
			var finalNotes = new List<int>();
			foreach ( var note in leitmotifNotes )

			{
				if ( note.ScaledNote == MusicConstants.UnplayedNote )
				{
					continue;
				}

				finalNotes.Add( instrument.InstrumentData.IsPercussion ? 0 : Leitmotif.GetUnscaledNoteIndex( note, set.MusicGenerator ) );
			}

			if ( instrument.InstrumentData.StrumLength == 0 )
			{
				foreach ( var note in finalNotes )
				{
					try
					{
						set.MusicGenerator.PlayAudioClip( set,
							instrument.InstrumentData.InstrumentType,
							note,
							instrument.InstrumentData.Volume,
							instIndex );
					}
					catch ( ArgumentOutOfRangeException e )
					{
						throw new ArgumentOutOfRangeException( e.Message );
					}
				}
			}
			else
			{
				try
				{
					set.Strum( finalNotes.ToArray(), instIndex );
				}
				catch ( ArgumentOutOfRangeException e )
				{
					throw new ArgumentOutOfRangeException( e.Message );
				}
			}
		}
	}
}

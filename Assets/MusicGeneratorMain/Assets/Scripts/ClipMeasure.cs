using System;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Measure logic for standalone clips.
	/// </summary>
	public class ClipMeasure : Measure
	{
		/// <summary>
		/// Plays the next steps in the measure.
		/// </summary>
		/// <param name="set"></param>
		public override void PlayMeasure( InstrumentSet set )
		{
			set.UpdateTempo();

			if ( set.MusicGenerator == false )
			{
				return;
			}

			set.SixteenthStepTimer -= Time.deltaTime;

			// we'll take a step when the timer hits zero, or it's the first step
			if ( set.SixteenthStepTimer <= 0 && set.SixteenthStepsTaken < set.TimeSignature.StepsPerMeasure )
			{
				if ( set.SixteenthStepsTaken % set.Data.ProgressionRate == 0 )
				{
					set.ProgressionStepsTaken += 1;
				}

				if ( set.ProgressionStepsTaken > set.MusicGenerator.CurrentChordProgression.Count - 1 )
				{
					set.ProgressionStepsTaken = -1;
				}

				TakeStep( set, (int) Timestep.Sixteenth, set.SixteenthRepeatCount );
				set.SixteenthRepeatCount += 1;
				set.SixteenthStepTimer = set.BeatLength;

				set.SixteenthStepsTaken += 1;
			}
			else if ( set.SixteenthStepsTaken == set.TimeSignature.StepsPerMeasure ) //< Reset once we've reached the end
			{
				set.MeasureStartTimer += Time.deltaTime;

				if ( set.MeasureStartTimer > set.BeatLength )
				{
					var hardReset = set.RepeatCount >= set.Data.RepeatMeasuresNum - 1;
					ResetMeasure( set, set.MusicGenerator.SetThemeRepeat, hardReset );
				}
			}
		}

		/// <summary>
		/// Exits our measure
		/// </summary>
		/// <param name="set"></param>
		/// <param name="setThemeRepeat"></param>
		/// <param name="hardReset"></param>
		/// <param name="isRepeating"></param>
		public override void ResetMeasure( InstrumentSet set, Action setThemeRepeat = null, bool hardReset = false, bool isRepeating = true )
		{
			ResetRepeatMeasure( set, hardReset, isRepeating );
		}

		/// <summary>
		///  Takes a measure step.
		/// </summary>
		/// <param name="set"></param>
		/// <param name="timeStep"></param>
		/// <param name="step"></param>
		public override void TakeStep( InstrumentSet set, Timestep timeStep, int step = 0 )
		{
			MusicConstants.GetBeatInfo( set.TimeSignature.Signature, step % set.TimeSignature.StepsPerMeasure, out var beat, out var beatStep );
			foreach ( var instrument in set.Instruments )
			{
				if ( instrument.InstrumentData.IsMuted )
				{
					continue;
				}

				if ( instrument.InstrumentData.IsPercussion )
				{
					PlayPercussionNotes( set, instrument, beat, beatStep );
				}
				else
				{
					PlayInstrumentNotes( set, instrument, beat, beatStep );
				}
			}
		}

		private static void PlayPercussionNotes( InstrumentSet set, Instrument instrument, int beat, int beatStep )
		{
			if ( instrument.InstrumentData.ForcedPercussiveNotes.Measures[set.RepeatCount].Timesteps[beat].Notes[beatStep] )
			{
				set.MusicGenerator.PlayAudioClip(
					set,
					instrument.InstrumentData.InstrumentType,
					0,
					instrument.InstrumentData.Volume,
					instrument.InstrumentIndex );
			}
		}

		private static void PlayInstrumentNotes( InstrumentSet set, Instrument instrument, int beat, int beatStep )
		{
			foreach ( var note in instrument.InstrumentData.ClipNotes[set.RepeatCount].Beats[beat].Steps[beatStep].Notes )
			{
				if ( note != MusicConstants.UnplayedNote )
				{
					set.MusicGenerator.PlayAudioClip(
						set,
						instrument.InstrumentData.InstrumentType,
						note,
						instrument.InstrumentData.Volume,
						instrument.InstrumentIndex );
				}
			}
		}
	}
}

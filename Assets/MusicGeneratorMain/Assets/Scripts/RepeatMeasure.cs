using System;
using JetBrains.Annotations;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Repeating measure class. Handles logic for repeating the previous measure.
	/// </summary>
	public class RepeatMeasure : Measure
	{
		/// <summary>
		/// Plays the next sequence in the measure.
		/// </summary>
		/// <param name="set"></param>
		public override void PlayMeasure( InstrumentSet set )
		{
			if ( set.MusicGenerator == false )
				return;

			set.UpdateTempo();
			set.SixteenthStepTimer -= Time.deltaTime;

			// Not the cleanest InstrumentSet of logic, but run with it...
			if ( set.SixteenthStepTimer <= 0 )
			{
				if ( set.SixteenthRepeatCount >= set.TimeSignature.StepsPerMeasure * set.Data.RepeatMeasuresNum )
				{
					ResetMeasure( set );
					return;
				}

				PlayBeat( set );
			}
		}

		/// <summary>
		/// Plays a beat from the instrument set
		/// Note: Leave public for manual playing
		/// </summary>
		/// <param name="set"></param>
		[UsedImplicitly]
		public void PlayBeat( InstrumentSet set )
		{
			if ( set.SixteenthRepeatCount % set.TimeSignature.Half == 0 )
			{
				TakeStep( set, Timestep.Eighth );
			}

			if ( set.SixteenthRepeatCount % set.TimeSignature.Quarter == 0 )
			{
				TakeStep( set, Timestep.Quarter );
			}

			if ( set.SixteenthRepeatCount % set.TimeSignature.Eighth == 0 )
			{
				TakeStep( set, Timestep.Half );
			}

			if ( set.SixteenthRepeatCount % set.TimeSignature.Sixteenth == 0 )
			{
				TakeStep( set, Timestep.Whole );
				set.MeasureStartTimer = -set.SixteenthStepTimer;
			}

			TakeStep( set, (int) Timestep.Sixteenth );

			set.SixteenthRepeatCount += 1;
			set.SixteenthStepTimer = set.BeatLength + set.SixteenthStepTimer;
			set.SixteenthStepsTaken += 1;
			set.MusicGenerator.BeatDidPlay.Invoke();
		}

		/// <summary>
		/// Resets the measure
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
		/// Takes a single step through the measure:
		/// </summary>
		/// <param name="set"></param>
		/// <param name="timeStep"></param>
		/// <param name="stepsTaken"></param>
		public override void TakeStep( InstrumentSet set, Timestep timeStep, int stepsTaken = 0 )
		{
			var usingTheme = set.MusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Theme;
			var repeatingMeasure = set.MusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Repeat;

			for ( var instIndex = 0; instIndex < set.Instruments.Count; instIndex++ )
			{
				var instrument = set.Instruments[instIndex];
				var instType = instrument.InstrumentData.InstrumentType;

				if ( instrument.InstrumentData.IsMuted ||
				     ( instrument.InstrumentData.ForceBeat == false && instrument.InstrumentData.TimeStep != timeStep ) ||
				     ( instrument.InstrumentData.ForceBeat && timeStep != Timestep.Sixteenth ) )
				{
					continue;
				}

				if ( set.MusicGenerator.InstrumentAudio.TryGetValue( instType, out _ ) )
				{
					if ( usingTheme )
					{
						PlayThemeNotes( set, instrument, instType, instIndex );
					}
					else if ( repeatingMeasure )
					{
						PlayRepeatNotes( set, instrument, instIndex );
					}
				}
			}
		}

		/// <summary>
		/// Plays the repeating notes for this timestep.
		/// </summary>
		/// <param name="set"></param>
		/// <param name="instrument"></param>
		/// <param name="instIndex"></param>
		private static void PlayRepeatNotes( InstrumentSet set, Instrument instrument, int instIndex )
		{
			for ( var chordNote = 0; chordNote < MusicConstants.MaxGeneratedNotesPerBeat; chordNote++ )
			{
				if ( instrument.RepeatingNotes.Length <= set.SixteenthRepeatCount ||
				     instrument.RepeatingNotes[set.SixteenthRepeatCount][chordNote] == MusicConstants.UnplayedNote )
				{
					continue;
				}

				if ( instrument.InstrumentData.StrumLength == 0.0f )
				{
					set.MusicGenerator.PlayAudioClip( set,
						instrument.InstrumentData.InstrumentType,
						instrument.RepeatingNotes[set.SixteenthRepeatCount][chordNote],
						instrument.InstrumentData.Volume,
						instIndex );
				}
				else
				{
					var clip = instrument.ThemeNotes[set.SixteenthRepeatCount];
					set.Strum( clip, instIndex );
					break;
				}
			}
		}

		/// <summary>
		/// Plays the theme notes for this repeat step.
		/// </summary>
		/// <param name="set"></param>
		/// <param name="instrument"></param>
		/// <param name="instType"></param>
		/// <param name="instIndex"></param>
		private static void PlayThemeNotes( InstrumentSet set, Instrument instrument, string instType, int instIndex )
		{
			for ( var chordNote = 0; chordNote < instrument.InstrumentData.ChordSize; chordNote++ )
			{
				var notes = instrument.ThemeNotes;
				if ( notes.Length <= set.SixteenthRepeatCount || notes[set.SixteenthRepeatCount].Length <= chordNote ||
				     notes[set.SixteenthRepeatCount][chordNote] == MusicConstants.UnplayedNote )
				{
					continue;
				}

				if ( instrument.InstrumentData.StrumLength == 0.0f && instrument.InstrumentData.SuccessionType == SuccessionType.Rhythm )
				{
					var note = notes[set.SixteenthRepeatCount][chordNote];
					set.MusicGenerator.PlayAudioClip( set, instType, note, instrument.InstrumentData.Volume, instIndex );
				}
				else
				{
					set.Strum( notes[set.SixteenthRepeatCount], instIndex );
					break;
				}
			}
		}
	}
}

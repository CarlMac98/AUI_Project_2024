using System;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	#region public

	/// <summary>
	/// A measure is a InstrumentSet of 16th steps, this class handles the playing of them.
	/// </summary>
	public abstract class Measure
	{
		/// <summary>
		/// Plays a step through measure.
		/// </summary>
		public abstract void PlayMeasure( InstrumentSet set );

		/// <summary>
		/// Resets the measure
		/// </summary>
		public abstract void ResetMeasure( InstrumentSet set, Action setThemeRepeat = null, bool hardReset = false, bool isRepeating = true );

		/// <summary>
		/// Takes a single measure step.
		/// </summary>
		public abstract void TakeStep( InstrumentSet set, Timestep timeStep, int stepsTaken = 0 );

		/// <summary>
		/// Whether this measure is currently using a leitmotif
		/// </summary>
		public bool IsUsingLeitmotif { get; private set; } = true; //< Just default to true so the configuration starts as such

		#endregion public

		#region protected

		/// <summary>
		/// Resets a repeating measure.
		/// </summary>
		/// <param name="set"></param>
		/// <param name="hardReset"></param>
		/// <param name="isRepeating"></param>
		protected void ResetRepeatMeasure( InstrumentSet set, bool hardReset = false, bool isRepeating = true )
		{
			set.MusicGenerator.RepeatedMeasureExited.Invoke( set.MusicGenerator.GeneratorState );

			if ( hardReset )
			{
				set.RepeatCount = 0;
				set.PercussionRepeatCount = 0;

				if ( set.MusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif )
				{
					IsUsingLeitmotif = true;
				}
			}
			else
			{
				set.RepeatCount += 1;
				set.PercussionRepeatCount += 1;
			}

			set.ResetMultipliers();

			if ( isRepeating == false )
				return;
			ResetRepeatValues( set, hardReset );
			ClearRepeatNotes( set );
		}

		/// <summary>
		/// Resets a regular measure.
		/// </summary>
		/// <param name="set"></param>
		/// <param name="setThemeRepeat"></param>
		/// <param name="hardReset"></param>
		protected void ResetRegularMeasure( InstrumentSet set, Action setThemeRepeat = null, bool hardReset = false )
		{
			if ( set.MusicGenerator == false )
			{
				Debug.LogError( "MusicGenerator is missing, unable to reset measure. Please pass a valid Instrument Set" );
				return;
			}

			if ( hardReset )
			{
				set.RepeatCount = 0;
				set.PercussionRepeatCount = 0;
				set.SixteenthRepeatCount = 0;
				foreach ( var instrument in set.Instruments )
				{
					instrument.ResetPatternStepsTaken();
					instrument.ClearPatternNotes();
				}

				if ( set.MusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif )
				{
					IsUsingLeitmotif = true;
				}
			}
			else
			{
				set.RepeatCount += 1;
				set.PercussionRepeatCount += 1;
			}

			set.SixteenthStepsTaken = 0;
			setThemeRepeat?.Invoke();

			foreach ( var instrument in set.Instruments )
			{
				instrument.ClearPlayedLeadNotes();
			}

			set.SelectGroups();

			if ( set.ProgressionStepsTaken >= set.MusicGenerator.CurrentChordProgression.Count - 1 )
				set.ProgressionStepsTaken = -1;

			ResetRepeatValues( set );

			set.ResetMultipliers();
			if ( set.Data.IsSingleClip == false )
			{
				set.MusicGenerator.NormalMeasureExited.Invoke();
			}
		}

		#endregion protected

		#region private

		private void ResetRepeatValues( InstrumentSet set, bool hardReset = false )
		{
			set.SixteenthStepsTaken = 0;
			set.MeasureStartTimer = 0;
			set.SixteenthStepTimer = 0.0f;

			if ( hardReset )
			{
				set.RepeatCount = 0;
				set.PercussionRepeatCount = 0;
				set.SixteenthRepeatCount = 0;
			}

			if ( set.Data.IsSingleClip )
			{
				return;
			}

			if ( set.PercussionRepeatCount >= set.Data.NumForcedPercussionMeasures )
			{
				set.PercussionRepeatCount = 0;
			}

			switch ( set.MusicGenerator.ConfigurationData.ThemeRepeatOptions )
			{
				case ThemeRepeatOptions.None:
					set.RepeatCount = 0;
					ClearRepeatNotes( set );
					break;
				case ThemeRepeatOptions.Leitmotif:
					ClearRepeatNotes( set );
					if ( set.RepeatCount >= set.Data.NumLeitmotifMeasures )
					{
						IsUsingLeitmotif = UnityEngine.Random.Range( 0, 100 ) <= set.MusicGenerator.ConfigurationData.PlayThemeOdds;
						set.RepeatCount = 0;
					}

					break;
				default:
					var isStopped = set.MusicGenerator.GeneratorState == GeneratorState.Stopped && set == set.MusicGenerator.InstrumentSet;
					var repeatNum = isStopped ? set.Data.RepeatMeasuresNum + 1 : set.Data.RepeatMeasuresNum * 2;
					if ( set.RepeatCount >= repeatNum || isStopped || hardReset )
					{
						set.RepeatCount = 0;
						set.SixteenthRepeatCount = 0;
						ClearRepeatNotes( set );

						if ( set.MusicGenerator.GeneratorState == GeneratorState.Repeating && set == set.MusicGenerator.InstrumentSet )
						{
							set.MusicGenerator.SetState( GeneratorState.Playing );
						}
					}

					break;
			}
		}

		/// <summary>
		/// Clears our Repeat notes
		/// </summary>
		/// <param name="set"></param>
		private void ClearRepeatNotes( InstrumentSet set )
		{
			foreach ( var instrument in set.Instruments )
			{
				instrument.ClearRepeatingNotes();
			}
		}

		#endregion private
	}
}

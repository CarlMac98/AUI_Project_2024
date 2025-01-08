using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
	public class InstrumentHandler
	{
		public void Initialize(MusicGenerator musicGenerator, int instrumentIndex)
		{
			mMusicGenerator = musicGenerator;
			mInstrumentIndex = instrumentIndex;
		}

		public void SetInstrumentIndex(int instrumentIndex)
		{
			mInstrumentIndex = instrumentIndex;
		}

		/// <summary>
		/// Plays a randomly generated note that fits into the currently generated configuration
		/// </summary>
		public void PlayNote()
		{
			// just index sanity check
			if (mInstrumentIndex >= mMusicGenerator.InstrumentSet.Instruments.Count)
			{
				return;
			}

			// here we grab an instrument
			var instrument = mMusicGenerator.InstrumentSet.Instruments[mInstrumentIndex];
			
			// find our progression step (-1 is set on progression reset, hence the range checking)
			var step = Mathf.Max(mMusicGenerator.InstrumentSet.ProgressionStepsTaken, 0);
			
			// find our current chord progression step
			var progressionStep = mMusicGenerator.InstrumentSet.MusicGenerator.CurrentChordProgression[step];
			
			// Generate some notes
			var notes = mMusicGenerator.InstrumentSet.Instruments[mInstrumentIndex].GetProgressionNotes(progressionStep, true);

			foreach (var note in notes)
			{
				// the GetProgressionNotes may return 'unplayed notes', we ignore those
				if (note == MusicConstants.UnplayedNote)
				{
					continue;
				}

				// directly play a note through the music generator
				mMusicGenerator.PlayAudioClip(mMusicGenerator.InstrumentSet, instrument.InstrumentData.InstrumentType, note,
					instrument.InstrumentData.Volume,
					mInstrumentIndex);
			}
		}

		/// <summary>
		/// Plays a specific note through the music generator
		/// </summary>
		/// <param name="index"></param>
		public void PlaySpecificNote(int index)
		{
			// instrument range check
			if (mInstrumentIndex >= mMusicGenerator.InstrumentSet.Instruments.Count)
			{
				return;
			}

			// grab our instrument
			var instrument = mMusicGenerator.InstrumentSet.Instruments[mInstrumentIndex];
			
			// grab the specific note by index
			var note = mMusicGenerator.InstrumentSet.Instruments[mInstrumentIndex].NoteGenerators[2].GetSpecificNote(index, 1);

			if (note == MusicConstants.UnplayedNote)
			{
				return;
			}
			
			// play the note directly through the music generator
			mMusicGenerator.PlayNote(mMusicGenerator.InstrumentSet, instrument.InstrumentData.Volume, instrument.InstrumentData.InstrumentType, note,
				mInstrumentIndex);
		}

		private int mInstrumentIndex;
		private MusicGenerator mMusicGenerator;
	}
}
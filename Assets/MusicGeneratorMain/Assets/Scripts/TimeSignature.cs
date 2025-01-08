using System;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Manages the time signature for a measure
	/// </summary>
	[Serializable]
	public class TimeSignature
	{
#region public

		/// <summary>
		/// number of steps per measure
		/// </summary>
		public int StepsPerMeasure { get; private set; }

		/// <summary>
		/// Notes per timestep
		/// </summary>
		public int[] NotesPerTimestep { get; private set; }

		/// <summary>
		/// inverted notes per timestep
		/// </summary>
		public int[] NotesPerTimestepInverse { get; private set; }

		/// <summary>
		/// sixteenth note; Bear in mind, this really is the equivalent of 'fastest' and is not actually a 16th note.
		/// </summary>
		public int Sixteenth { get; private set; } = 16;

		/// <summary>
		/// eighth note
		/// </summary>
		public int Eighth { get; private set; } = 8;

		/// <summary>
		/// quarter note;
		/// </summary>
		public int Quarter { get; private set; } = 4;

		/// <summary>
		/// half note
		/// </summary>
		public int Half { get; private set; } = 2;

		/// <summary>
		/// whole note;
		/// </summary>
		public int Whole { get; private set; }

		/// <summary>
		/// our currently InstrumentSet time signature.
		/// </summary>
		public TimeSignatures Signature
		{
			get => mSignature;
			set => SetTimeSignature( value );
		}

		/// <summary>
		/// Initializes the time signature
		/// </summary>
		public void Initialize( MusicGenerator musicGenerator )
		{
			mMusicGenerator = musicGenerator;
			StepsPerMeasure = 16;
			NotesPerTimestep = new[] {16, 8, 4, 2, 1};
			NotesPerTimestepInverse = new[] {1, 2, 4, 8, 16};
			SetTimeSignature( musicGenerator.ConfigurationData.TimeSignature );
		}

		/// <summary>
		/// Sets our time signature and adjusts values.
		/// </summary>
		/// <param name="signature"></param>
		public void SetTimeSignature( TimeSignatures signature )
		{
			mSignature = signature;
			// Apologies for all the magic numbers. This is a bit of a hacky approach.
			// trying to shoehorn everything to the same system.
			switch ( mSignature )
			{
				case TimeSignatures.FourFour:
				{
					StepsPerMeasure = 16;
					NotesPerTimestep = new[] {16, 8, 4, 2, 1};
					NotesPerTimestepInverse = new[] {1, 2, 4, 8, 16};
					Sixteenth = 16;
					Eighth = 8;
					Quarter = 4;
					Half = 2;
					Whole = 0;
					break;
				}
				case TimeSignatures.ThreeFour:
				{
					StepsPerMeasure = 12;
					NotesPerTimestep = new[] {12, 6, 3, 3, 1};
					NotesPerTimestepInverse = new[] {1, 3, 3, 6, 12};
					Sixteenth = 12;
					Eighth = 6;
					Quarter = 3;
					Half = 3;
					Whole = 0;
					break;
				}
				case TimeSignatures.FiveFour:
				{
					StepsPerMeasure = 20;
					NotesPerTimestep = new[] {20, 10, 5, 5, 1};
					NotesPerTimestepInverse = new[] {1, 5, 5, 10, 20};
					Sixteenth = 20;
					Eighth = 10;
					Quarter = 5;
					Half = 5;
					Whole = 0;
					break;
				}
			}

			mMusicGenerator.ResetPlayer();
		}

#endregion public

#region private

		/// <summary>
		/// our currently InstrumentSet time signature.
		/// </summary>
		private TimeSignatures mSignature = TimeSignatures.FourFour;

		/// <summary>
		/// reference to our music generator
		/// </summary>
		private MusicGenerator mMusicGenerator;

#endregion private
	}
}

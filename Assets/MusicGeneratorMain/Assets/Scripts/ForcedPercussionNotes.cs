using System;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Data class representing all percussion notes for forced percussion for an instrument.
	/// TODO: this is a terrible setup requiring large containers of mostly unused integers.
	/// </summary>
	[Serializable]
	public class ForcedPercussionNotes
	{
		[Serializable]
		public class PercussionTimestep
		{
			public bool[] Notes => mNotes;

			[SerializeField] private bool[] mNotes = new bool[MusicConstants.MaxStepsPerTimestep];
		}

		[Serializable]
		public class PercussionMeasure
		{
			public PercussionTimestep[] Timesteps => mTimesteps;

			[SerializeField] private PercussionTimestep[] mTimesteps =
			{
				new PercussionTimestep(), new PercussionTimestep(), new PercussionTimestep(), new PercussionTimestep()
			};
		}

		public PercussionMeasure[] Measures => mMeasures;

		[SerializeField] private PercussionMeasure[] mMeasures =
		{
			new PercussionMeasure(), new PercussionMeasure(), new PercussionMeasure(), new PercussionMeasure()
		};
	}
}

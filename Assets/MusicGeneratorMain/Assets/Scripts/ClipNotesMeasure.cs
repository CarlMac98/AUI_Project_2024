using System;
using System.Collections.Generic;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Data class representing the notes of a beat step
	/// </summary>
	[Serializable]
	public class ClipBeatStep
	{
		public List<int> Notes = new List<int>();
	}

	/// <summary>
	/// data class representing the beat steps of a ClipBeat
	/// </summary>
	[Serializable]
	public class ClipBeat
	{
		public List<ClipBeatStep> Steps = new List<ClipBeatStep>();
	}

	/// <summary>
	/// Data class representing the ClipBeats of a measure
	/// </summary>
	[Serializable]
	public class ClipNotesMeasure
	{
		public List<ClipBeat> Beats = new List<ClipBeat>();
	}
}

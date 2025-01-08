using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Data struct for a note
	/// </summary>
	public readonly struct NoteData
	{
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="beat"></param>
		/// <param name="noteIndex"></param>
		public NoteData( Vector2Int beat, int noteIndex )
		{
			Beat = beat;
			NoteIndex = noteIndex;
		}

		/// <summary>
		/// Getter for our Beat Info
		/// </summary>
		public readonly Vector2Int Beat;

		/// <summary>
		/// Getter for our NoteIndex
		/// </summary>
		public readonly int NoteIndex;
	}

	/// <summary>
	/// Note Data container for the measure editor (and variants)
	/// </summary>
	public readonly struct MeasureEditorNoteData
	{
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="beat"></param>
		/// <param name="noteIndex"></param>
		/// <param name="measure"></param>
		/// <param name="offsetPosition"></param>
		public MeasureEditorNoteData( Vector2Int beat, int noteIndex, int measure, Vector3 offsetPosition )
		{
			Beat = beat;
			NoteIndex = noteIndex;
			Measure = measure;
			OffsetPosition = offsetPosition;
			NoteInfo = new NoteData( beat, noteIndex );
		}

		/// <summary>
		/// Our beat. Zero-based. (based on 4/4 time, so (0, 3) is the first beat, third subbeat, (3rd sixteenth step of the chord progression,
		/// and (2,1) is the third beat, second subbeat.
		/// </summary>
		public readonly Vector2Int Beat;

		/// <summary>
		/// Note Index (horizontal position, this loosely corresponds to the scaled note, but is used differently, (some data usage strips out the mode, for example)
		/// </summary>
		public readonly int NoteIndex;

		/// <summary>
		/// Measure of this note
		/// </summary>
		public readonly int Measure;

		/// <summary>
		/// Offset position of this note (based on beat, noteIndex)
		/// </summary>
		public readonly Vector3 OffsetPosition;

		/// <summary>
		/// Reference to our Note Data (duplicate data here, but this is used as a key elsewhere).
		/// </summary>
		public readonly NoteData NoteInfo;
	}
}
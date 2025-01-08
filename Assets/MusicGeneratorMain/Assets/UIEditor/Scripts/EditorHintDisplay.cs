using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Display overlay handling hint notes for an Editor Display
	/// </summary>
	public class EditorHintDisplay : EditorDisplayOverlay
	{
		#region public

		///<inheritdoc/>
		public override void ShowNotes( int measureIndex )
		{
			if ( mUIDisplayEditor.IsShowingEditorHints )
			{
				StartCoroutine( GenerateHints( CurrentInstrument ) );
			}
			else
			{
				ResetMeasures();
			}
		}

		///<inheritdoc/>
		public override bool AddOrRemoveNote( int measureIdx,
			int currentMeasure,
			Instrument instrument,
			MeasureEditorNoteData noteData,
			float colorAlpha = 1f,
			Action callback = null )
		{
			var position = noteData.OffsetPosition + new Vector3( 0, 0, -.05f );
			if ( mMeasures[measureIdx].TryGetNotes( noteData.NoteInfo, out var notes ) )
			{
				if ( notes.mInstrumentNotes.ContainsKey( instrument ) )
				{
					return false;
				}
			}

			SpawnNote( noteData.NoteInfo,
				position,
				transform,
				instrument,
				measureIdx,
				measureIdx == currentMeasure,
				colorAlpha,
				isHint: true,
				callback );
			return true;
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override float NoteColorAlpha => mNoteColorAlpha;

		/// <summary>
		/// Reference to the Music Generator's current chord progression
		/// </summary>
		protected virtual IReadOnlyList<int> CurrentProgression => mUIManager.MusicGenerator.CurrentChordProgression;

		#endregion protected

		///<inheritdoc/>
		protected override Vector3 UpdateZPosition( int measureIdx, Vector3 offsetPosition, NoteData dataKey )
		{
			var offset = base.UpdateZPosition( measureIdx, offsetPosition, dataKey );
			offset.z = -.05f;
			return offset;
		}

		#region private

		/// <summary>
		/// Our color alpha for hint notes
		/// </summary>
		[SerializeField] private float mNoteColorAlpha = .45f;

		/// <summary>
		/// Modifier for the alpha of the root note of a chord for lead instruments
		/// </summary>
		[SerializeField] private float mRootAlphaModifier = 1.5f;

		/// <summary>
		/// Reference to the UI Display editor's current instrument InstrumentSet's time signature
		/// </summary>
		private TimeSignature CurrentTimeSignature => mUIDisplayEditor.InstrumentSet.TimeSignature;

		/// <summary>
		/// Reference to the Instrument List Panel's currently selected instrument
		/// </summary>
		private Instrument CurrentInstrument => mUIManager.InstrumentListPanelUI.SelectedInstrument;

		private bool isGeneratingHints;

		/// <summary>
		/// Generates hint notes for an instrument
		/// </summary>
		/// <param name="instrument"></param>
		private IEnumerator GenerateHints( Instrument instrument )
		{
			if ( instrument == null )
			{
				yield break;
			}

			yield return new WaitUntil( () => isGeneratingHints == false );
			isGeneratingHints = true;
			ResetMeasures();

			var quarterStep = mUIDisplayEditor.InstrumentSet.TimeSignature.Quarter;

			var stepsPerMeasure = mUIDisplayEditor.InstrumentSet.TimeSignature.StepsPerMeasure;
			var handledHints = 0;
			for ( var index = 0; index < stepsPerMeasure; index++ )
			{
				if ( index % quarterStep != 0 && instrument.InstrumentData.SuccessionType == SuccessionType.Lead )
				{
					StartCoroutine( GenerateLeadHint( index, () => { handledHints++; } ) );
				}
				else
				{
					StartCoroutine( GenerateRhythmHint( index, () => { handledHints++; } ) );
				}
			}

			yield return new WaitUntil( () => handledHints == stepsPerMeasure );
			isGeneratingHints = false;
		}

		/// <summary>
		/// Generates a hint note for a lead instrument for a given step
		/// </summary>
		/// <param name="stepIndex"></param>
		/// <param name="callback"></param>
		private IEnumerator GenerateLeadHint( int stepIndex, Action callback = null )
		{
			const int totalNotes = MusicConstants.MaxInstrumentNotes;
			const int scaleLength = MusicConstants.ScaleLength;
			var scale = MusicConstants.GetScale( mUIDisplayEditor.Scale );
			var notesPerTimeStep = CurrentTimeSignature.NotesPerTimestepInverse[( int )CurrentInstrument.InstrumentData.TimeStep];

			if ( stepIndex % notesPerTimeStep != 0 )
			{
				callback?.Invoke();
				yield break;
			}

			var handledNotes = 0;
			for ( var index = 0; index < MusicConstants.TotalScaleNotes; index++ )
			{
				var noteIndex = ( int )mUIDisplayEditor.Key;
				for ( var subIndex = 0; subIndex < index; subIndex++ )
				{
					var scaleIndex = ( subIndex + ( int )mUIDisplayEditor.Mode ) % scaleLength;
					noteIndex += scale[scaleIndex];
				}

				noteIndex %= totalNotes;

				// Ignore pentatonic for hints.
				//if ( CurrentInstrument.InstrumentData.IsPentatonic && leadGenerator.IsPentatonicAvoid( noteIndex ) )
				//{
				//	handledNotes++;
				//	continue;
				//}

				var beatIndex = new Vector2Int( stepIndex / CurrentTimeSignature.Quarter, stepIndex % CurrentTimeSignature.Quarter );
				GetOffsetAndNoteIndex( CurrentMeasure, beatIndex, noteIndex, out var offsetPosition );
				if ( offsetPosition.y > 0 )
				{
					AddOrRemoveNote( CurrentMeasure,
						CurrentMeasure,
						CurrentInstrument,
						new MeasureEditorNoteData( beatIndex, noteIndex, mUIManager.UIMeasureEditor.CurrentMeasure, offsetPosition ),
						callback: () => { handledNotes++; } );
				}
				else
				{
					handledNotes++;
				}
			}

			yield return new WaitUntil( () => handledNotes == MusicConstants.TotalScaleNotes );
			callback?.Invoke();
		}

		/// <summary>
		/// Generates hint for rhythm instruments for a given timeStep
		/// </summary>
		/// <param name="stepIndex"></param>
		/// <param name="callback"></param>
		private IEnumerator GenerateRhythmHint( int stepIndex, Action callback = null )
		{
			if ( stepIndex % ( CurrentTimeSignature.NotesPerTimestepInverse[( int )CurrentInstrument.InstrumentData.TimeStep] ) != 0 )
			{
				callback?.Invoke();
				yield break;
			}

			var handledNotes = 0;
			for ( var noteIndex = 0; noteIndex < MusicConstants.TotalScaleNotes; noteIndex++ )
			{
				if ( noteIndex % 7 != 0 && noteIndex % 7 != 2 && noteIndex % 7 != 4 )
				{
					handledNotes++;
					continue;
				}

				var measureOffset = mUIDisplayEditor.InstrumentSet.TimeSignature.StepsPerMeasure * CurrentMeasure;
				var note = GetNoteOffset( stepIndex + measureOffset, noteIndex );
				var beatInfo = MusicConstants.GetBeatInfo( CurrentTimeSignature.Signature, stepIndex );
				GetOffsetAndNoteIndex( CurrentMeasure, beatInfo, note, out Vector3 offsetPosition );

				if ( offsetPosition.y > 0 )
				{
					AddOrRemoveNote( CurrentMeasure,
						CurrentMeasure,
						CurrentInstrument,
						new MeasureEditorNoteData( beatInfo, note, mUIManager.UIMeasureEditor.CurrentMeasure, offsetPosition ),
						mRootAlphaModifier,
						() => { handledNotes++; } );
				}
				else
				{
					handledNotes++;
				}
			}

			yield return new WaitUntil( () => handledNotes == MusicConstants.TotalScaleNotes );
			callback?.Invoke();
		}

		/// <summary>
		/// Returns the vertical offset for a note given its timeStep.
		/// </summary>
		/// <param name="timeStep"></param>
		/// <param name="note"></param>
		/// <returns></returns>
		private int GetNoteOffset( int timeStep, int note )
		{
			var scale = mUIDisplayEditor.Scale;
			var noteIndex = ( int )mUIDisplayEditor.Key;
			var progressionRate = mUIDisplayEditor.InverseProgressionRate;
			var chordStep = ( timeStep / progressionRate ) % CurrentProgression.Count;
			var stepToTake = CurrentProgression[chordStep] - 1; //< progression is 1-based.

			for ( var index = 0; index < note + stepToTake; index++ )
			{
				var offsetIndex = ( index + ( int )mUIDisplayEditor.Mode ) % MusicConstants.ScaleLength;
				offsetIndex %= MusicConstants.ScaleLength;
				noteIndex += MusicConstants.GetScale( scale )[offsetIndex];
			}

			noteIndex += ( MusicConstants.OctaveSize * ( note / MusicConstants.ScaleLength ) );
			noteIndex %= MusicConstants.MaxInstrumentNotes;

			return noteIndex;
		}

		#endregion private
	}
}
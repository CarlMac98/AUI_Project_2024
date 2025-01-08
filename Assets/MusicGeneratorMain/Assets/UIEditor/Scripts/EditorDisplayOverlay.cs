using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles the visible interface for an Editor Display. I.e. visible measures, notes, hints, etc.
	/// </summary>
	public abstract class EditorDisplayOverlay : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Alpha value for the notes in this overlay
		/// </summary>
		protected abstract float NoteColorAlpha { get; }

		/// <summary>
		/// Show the notes for a particular measure
		/// </summary>
		/// <param name="measureIdx"></param>
		public abstract void ShowNotes( int measureIdx );

		/// <summary>
		/// Add or remove notes to the overlay
		/// </summary>
		/// <param name="measureIdx"></param>
		/// <param name="currentMeasure"></param>
		/// <param name="instrument"></param>
		/// <param name="noteData"></param>
		/// <param name="alphaModifier"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public abstract bool AddOrRemoveNote( int measureIdx,
			int currentMeasure,
			Instrument instrument,
			MeasureEditorNoteData noteData,
			float alphaModifier = 1f,
			Action callback = null );

		/// <summary>
		/// List of our measure displays
		/// </summary>
		public IReadOnlyList<MeasureDisplay> Measures => mMeasures;

		/// <summary>
		/// will return note with the matching noteData for a particular measure
		/// </summary>
		/// <param name="measureIdx"></param>
		/// <param name="instrument"></param>
		/// <param name="noteData"></param>
		/// <param name="measureEditorNote"></param>
		/// <returns></returns>
		public bool TryGetNote( int measureIdx, Instrument instrument, MeasureEditorNoteData noteData, out MeasureEditorNote measureEditorNote )
		{
			measureEditorNote = null;
			if ( mMeasures[measureIdx].TryGetNotes( noteData.NoteInfo, out var notes ) )
			{
				measureEditorNote = notes.mInstrumentNotes[instrument];
				return measureEditorNote != false;
			}

			return false;
		}

		/// <summary>
		/// Initializes the Display overlay
		/// </summary>
		/// <param name="uiManager"></param>
		public void Initialize( UIManager uiManager )
		{
			mUIManager = uiManager;
		}

		/// <summary>
		/// Sets the reference to our UIDisplayEditor
		/// </summary>
		/// <param name="displayEditor"></param>
		public void SetUIDisplayEditor( UIDisplayEditor displayEditor )
		{
			// This is mostly needed as our percussion overlay is instantiated.
			mUIDisplayEditor = displayEditor;
		}

		/// <summary>
		/// Reset all measures (please note, this destroys all notes contained within the measure and they will need to be regenerated)
		/// </summary>
		public void ResetMeasures()
		{
			foreach ( var measure in mMeasures )
			{
				foreach ( var measureNote in measure.MeasureNotes )
				{
					foreach ( var instrumentNote in measureNote.Value.mInstrumentNotes )
					{
						Destroy( instrumentNote.Value.gameObject );
					}
				}

				measure.ClearMeasureNotes();
			}
		}

		/// <summary>
		/// Initializes the measures
		/// </summary>
		public void InitializeMeasures()
		{
			ResetMeasures();
			mMeasures.Clear();
			for ( var index = 0; index < MusicConstants.NumMeasures; index++ )
			{
				mMeasures.Add( new MeasureDisplay() );
			}
		}

		/// <summary>
		/// Returns the offset and note index for a given note.
		/// Offset refers to the horizontal offset of the note along the keyboard and the vertical offset of its step
		/// </summary>
		/// <param name="measureIdx"></param>
		/// <param name="position"></param>
		/// <param name="noteData"></param>
		public virtual void GetOffsetAndNoteIndex( int measureIdx, Vector2 position, out MeasureEditorNoteData noteData )
		{
			var offsetPosition = new Vector3(
				mUIManager.UIKeyboard.GetKeyHorizontalOffset( position, out var noteIndex ),
				mDisplayEditor.CurrentTimeSignature.GetStepVerticalOffset( position, out var beat ),
				-1
			);
			offsetPosition = UpdateZPosition( measureIdx, offsetPosition, new NoteData( beat, noteIndex ) );
			noteData = new MeasureEditorNoteData( beat, noteIndex, mUIDisplayEditor.CurrentMeasure, offsetPosition );
		}

		/// <summary>
		/// Returns the offset and note index for a given note
		/// Offset refers to the horizontal offset of the note along the keyboard and the vertical offset of its step
		/// </summary>
		/// <param name="measureIdx"></param>
		/// <param name="beat"></param>
		/// <param name="note"></param>
		/// <param name="offsetPosition"></param>
		public virtual void GetOffsetAndNoteIndex( int measureIdx, Vector2Int beat, int note, out Vector3 offsetPosition )
		{
			offsetPosition = new Vector3(
				mUIManager.UIKeyboard.GetKeyHorizontalOffset( note ),
				mDisplayEditor.CurrentTimeSignature.GetStepVerticalOffset( beat ),
				-1 );

			offsetPosition = UpdateZPosition( measureIdx, offsetPosition, new NoteData( beat, note ) );
		}

		/// <summary>
		/// Hides the display overlay
		/// </summary>
		public void Hide()
		{
			foreach ( var measure in mMeasures )
			{
				foreach ( var noteIndex in measure.MeasureNotes )
				{
					foreach ( var note in noteIndex.Value.mInstrumentNotes )
					{
						note.Value.Hide();
					}
				}
			}
		}

		/// <summary>
		/// Shows the display overlay
		/// </summary>
		public void Show()
		{
			foreach ( var measure in mMeasures )
			{
				foreach ( var noteIndex in measure.MeasureNotes )
				{
					foreach ( var note in noteIndex.Value.mInstrumentNotes )
					{
						note.Value.Show();
					}
				}
			}
		}

		/// <summary>
		/// Fades in the display overlay over time
		/// </summary>
		public void FadeIn()
		{
			foreach ( var measure in mMeasures )
			{
				foreach ( var noteIndex in measure.MeasureNotes )
				{
					foreach ( var note in noteIndex.Value.mInstrumentNotes )
					{
						note.Value.FadeIn();
					}
				}
			}
		}

		/// <summary>
		/// Fades out the display overlay over time
		/// </summary>
		public void FadeOut()
		{
			foreach ( var measure in mMeasures )
			{
				foreach ( var noteIndex in measure.MeasureNotes )
				{
					foreach ( var note in noteIndex.Value.mInstrumentNotes )
					{
						note.Value.FadeOut();
					}
				}
			}
		}

		#endregion public

		#region protected

		/// <summary>
		/// Reference to our UIManager
		/// </summary>
		protected UIManager mUIManager;

		/// <summary>
		/// Returns the measure currently InstrumentSet by our UIDisplayEditor 
		/// </summary>
		protected int CurrentMeasure => mUIDisplayEditor.CurrentMeasure;

		/// <summary>
		/// List of our MeasureDisplays
		/// </summary>
		protected readonly List<MeasureDisplay> mMeasures = new List<MeasureDisplay>();

		/// <summary>
		/// Reference to our Display Editor
		/// </summary>
		[SerializeField] protected DisplayEditor mDisplayEditor;

		/// <summary>
		/// Reference to our UIDisplayEditor
		/// </summary>
		[SerializeField] protected UIDisplayEditor mUIDisplayEditor;

		/// <summary>
		/// Spawns a note, parented to parameter parent transform and sets its position.
		/// Be advised the note is note available until the addressable is instantiated.
		/// If needed immediately, please add a callback to this method or InstrumentSet to async task/coroutine
		/// </summary>
		/// <param name="dataKey"></param>
		/// <param name="position"></param>
		/// <param name="parent"></param>
		/// <param name="instrument"></param>
		/// <param name="measureIndex"></param>
		/// <param name="isActive"></param>
		/// <param name="alphaModifier"></param>
		/// <param name="isHint"></param>
		/// <param name="callback"></param>
		protected void SpawnNote( NoteData dataKey,
			Vector3 position,
			Transform parent,
			Instrument instrument,
			int measureIndex,
			bool isActive,
			float alphaModifier = 1f,
			bool isHint = false,
			Action callback = null )
		{
			var basePrefab = isHint ? mBaseHint : mBaseNote;
			var instantiatedObject = Instantiate( basePrefab, position, quaternion.identity, parent );
			var color = mUIManager.Colors[( int )instrument.InstrumentData.StaffPlayerColor];
			color.a = NoteColorAlpha * alphaModifier;
			var measureEditorNote = instantiatedObject.GetComponent<MeasureEditorNote>();
			measureEditorNote.SetColor( color );
			mMeasures[measureIndex].Add( dataKey, instrument, measureEditorNote );
			measureEditorNote.gameObject.SetActive( isActive );
			ResizeNotes( mMeasures[measureIndex].MeasureNotes[dataKey].mInstrumentNotes );
			callback?.Invoke();
		}

		/// <summary>
		/// Adjusts the z position to accomodate overlapping notes
		/// </summary>
		/// <param name="measureIdx"></param>
		/// <param name="offsetPosition"></param>
		/// <param name="dataKey"></param>
		/// <returns></returns>
		protected virtual Vector3 UpdateZPosition( int measureIdx, Vector3 offsetPosition, NoteData dataKey )
		{
			if ( mMeasures[measureIdx].TryGetNotes( dataKey, out var notes ) )
			{
				offsetPosition.z -= notes.mInstrumentNotes.Count * .1f;
			}

			return offsetPosition;
		}

		/// <summary>
		/// Resizes our notes to accomodate overlapping notes
		/// </summary>
		/// <param name="instrumentNotes"></param>
		protected void ResizeNotes( Dictionary<Instrument, MeasureEditorNote> instrumentNotes )
		{
			var sizeDelta = mNoteMaxSize / instrumentNotes.Count;
			var index = 0;
			var showAll = mUIDisplayEditor.ShouldShowAllInstruments;
			foreach ( var instrumentNote in instrumentNotes )
			{
				if ( showAll )
				{
					instrumentNote.Value.Resize( mNoteMaxSize - ( sizeDelta * index ) );
					var position = instrumentNote.Value.transform.position;
					position.z = -1 - ( index * .1f ); //< need to reset our z-values, as things may have been added/removed.
					instrumentNote.Value.Transform.position = position;
					index++;
				}
				else
				{
					if ( instrumentNote.Key == CurrentInstrument )
					{
						instrumentNote.Value.Resize( mNoteMaxSize );
					}
				}
			}
		}

		/// <summary>
		/// Removes a note from our display
		/// </summary>
		/// <param name="measureIdx"></param>
		/// <param name="index"></param>
		/// <param name="instrument"></param>
		/// <param name="notes"></param>
		protected void RemoveNote( int measureIdx, NoteData index, Instrument instrument, MeasureDisplay.InstrumentNotes notes )
		{
			Destroy( notes.mInstrumentNotes[instrument].gameObject );
			notes.mInstrumentNotes.Remove( instrument );
			if ( notes.mInstrumentNotes.Count == 0 )
			{
				mMeasures[measureIdx].Remove( index );
			}

			ResizeNotes( notes.mInstrumentNotes );
		}

		#endregion protected

		#region private

		/// <summary>
		/// Asset reference to our base note
		/// </summary>
		[Tooltip( "Reference to our base note" )]
		[SerializeField] private MeasureEditorNote mBaseNote;

		/// <summary>
		/// Asset reference to our base hint note
		/// </summary>
		[Tooltip( "Reference to our base hint note" )]
		[SerializeField] private MeasureEditorNote mBaseHint;

		/// <summary>
		/// Maximum size of our notes
		/// </summary>
		[Tooltip( "Maximum dimensions of notes" )]
		[SerializeField] private Vector2 mNoteMaxSize = new Vector2( 20f, 20f );

		/// <summary>
		/// Reference to the currently selected instrument in the instrument list panel
		/// </summary>
		private Instrument CurrentInstrument => mUIManager.InstrumentListPanelUI.SelectedInstrument;

		#endregion private
	}
}
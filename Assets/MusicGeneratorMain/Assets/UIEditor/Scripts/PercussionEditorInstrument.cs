using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Slightly different from the other display editors, percussion instruments have each instrument function as the
	/// entirety of the display editor (rather than all the instruments in one editor, each is their own here)
	/// </summary>
	public class PercussionEditorInstrument : DisplayEditor
	{
		#region public

		/// <summary>
		/// Event for Note Update
		/// </summary>
		public class OnNoteUpdatedEvent : UnityEvent<MeasureEditorNoteData, bool, Instrument> { }

		/// <summary>
		/// Exposed reference to our Editor Display Overlay
		/// </summary>
		public EditorDisplayOverlay InstrumentOverlay => mInstrumentDisplay;

		/// <summary>
		/// Reference to our NoteUpdated event
		/// </summary>
		public OnNoteUpdatedEvent OnNoteUpdated { get; } = new OnNoteUpdatedEvent();

		/// <summary>
		/// Reference to our Instrument Moved event
		/// </summary>
		public UnityEvent OnInstrumentMoved => mOnInstrumentMoved;

		///<inheritdoc/>
		public override void Play()
		{
			// Bypassing Base.
		}

		///<inheritdoc/>
		public override void DoUpdate()
		{
			if ( !mIsEnabled )
			{
				return;
			}

			if ( mUIDisplayEditor.DisplayIsBroken )
			{
				RebuildDisplay();
				InitializeInstrument( mInstrument );
			}
			else if ( mUIDisplayEditor.DisplayIsDirty )
			{
				var currentMeasure = mUIManager.MusicGenerator.GeneratorState == GeneratorState.Stopped ? CurrentMeasure : RepeatCount % NumMeasures;
				RefreshDisplay( currentMeasure );
			}

			if ( Input.GetMouseButtonDown( 0 ) &&
			     mUIManager.PercussionEditor.Collider.bounds.Contains( mUIManager.MouseWorldPoint ) &&
			     mCollider.bounds.Contains( mUIManager.MouseWorldPoint ) )
			{
				ClickNote( CurrentMeasure );
			}
		}

		/// <summary>
		/// Initializes the percussion editor instrument
		/// </summary>
		/// <param name="uiManager"></param>
		/// <param name="displayEditor"></param>
		/// <param name="instrument"></param>
		/// <param name="callback"></param>
		public void Initialize( UIManager uiManager, UIDisplayEditor displayEditor, Instrument instrument, Action<bool> callback = null )
		{
			base.Initialize( uiManager );
			mUIDisplayEditor = displayEditor;
			InstrumentOverlay.SetUIDisplayEditor( displayEditor );
			SetPanelActive( true );
			InitializeInstrument( instrument, callback );
		}

		/// <summary>
		/// Plays a note on the Percussion Editor Instrument
		/// </summary>
		/// <param name="args"></param>
		public void PlayNote( NotePlayedArgs args )
		{
			var beatInfo = MusicConstants.GetBeatInfo( args.InstrumentSet.Data.TimeSignature, args.InstrumentSet.SixteenthStepsTaken );
			var noteData = new MeasureEditorNoteData( beatInfo, noteIndex: 0, 0, Vector3.zero );
			if ( mInstrumentDisplay.TryGetNote(
				RepeatCount % NumMeasures,
				args.InstrumentSet.Instruments[args.InstrumentIndex],
				noteData,
				out var note ) )
			{
				note.SetColor( Color.red );
			}
		}

		/// <summary>
		/// Resets all played notes (red highlights)
		/// </summary>
		public void ResetPlayedNotes()
		{
			foreach ( var measure in mInstrumentDisplay.Measures )
			{
				foreach ( var measureNotes in measure.MeasureNotes )
				{
					foreach ( var note in measureNotes.Value.mInstrumentNotes )
					{
						note.Value.SetColor( mUIManager.Colors[( int )note.Key.InstrumentData.StaffPlayerColor] );
					}
				}
			}
		}

		#endregion public

		#region protected

		protected virtual int RepeatCount => mUIManager == null ? 0 : mUIManager.CurrentInstrumentSet.PercussionRepeatCount;
		protected virtual int NumMeasures => mUIManager == null ? 0 : mUIManager.MusicGenerator.ConfigurationData.NumForcedPercussionMeasures;

		/// <summary>
		/// Reference to the current UI Display Editor's Current Measure
		/// </summary>
		protected override int CurrentMeasure => mUIDisplayEditor.CurrentMeasure;

		[SerializeField, Tooltip( "Reference to our drag element" )]
		protected UIWorldDragElement mDragElement;

		/// <summary>
		/// Reference to our associated instrument
		/// </summary>
		protected Instrument mInstrument;

		/// <summary>
		/// Invoked upon the instrument being moved
		/// </summary>
		protected readonly UnityEvent mOnInstrumentMoved = new UnityEvent();

		/// <summary>
		/// Reference to our UIDisplayEditor
		/// </summary>
		protected UIDisplayEditor mUIDisplayEditor;

		///<inheritdoc/>
		protected override void InitializeInstrument( Instrument instrument, Action<bool> callback = null )
		{
			if ( instrument.InstrumentData.IsPercussion == false )
			{
				callback?.Invoke( true );
				return;
			}

			mInstrument = instrument;
			var forcedPercussionNotes = mInstrument.InstrumentData.ForcedPercussiveNotes;
			var timestepLength = mUIManager.MusicGenerator.InstrumentSet.TimeSignature.StepsPerMeasure / 4;

			for ( var measureIdx = 0; measureIdx < forcedPercussionNotes.Measures.Length; measureIdx++ )
			{
				for ( var timestepIdx = 0; timestepIdx < forcedPercussionNotes.Measures[measureIdx].Timesteps.Length; timestepIdx++ )
				{
					for ( var noteIdx = 0; noteIdx < timestepLength; noteIdx++ )
						if ( forcedPercussionNotes.Measures[measureIdx].Timesteps[timestepIdx].Notes[noteIdx] )
						{
							InitializeInstrumentNotes( timestepIdx, noteIdx, measureIdx );
						}
				}
			}

			mDragElement.Initialize( () => { mOnInstrumentMoved.Invoke(); },
				mUIManager );
			mInstrument = instrument;
			RefreshInstrument();
			callback?.Invoke( true );
		}

		///<inheritdoc/>
		protected override void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument )
		{
			//mInstrument.mData.ForcedPercussiveNotes.Measures[noteData.Measure].Notes[noteData.Timestep] = wasAdded;
			OnNoteUpdated.Invoke( noteData, wasAdded, instrument );
			mUIManager.MusicGenerator.PlayNote( mUIManager.CurrentInstrumentSet,
				instrument.InstrumentData.Volume,
				instrument.InstrumentData.InstrumentType,
				0,
				instrument.InstrumentIndex );
		}

		/// <summary>
		/// Refreshes the instrument (updates text, color, etc)
		/// </summary>
		protected void RefreshInstrument()
		{
			mTitleText.text = mInstrument.InstrumentData.InstrumentType;
			var instrumentColor = mUIManager.Colors[( int )mInstrument.InstrumentData.StaffPlayerColor];
			mTitleSpriteRenderer.color = instrumentColor;
			//mTitleText.color = MusicConstants.InvertTextColor( instrumentColor );
		}

		///<inheritdoc/>
		protected override void ClickNote( int measureIndex )
		{
			// Bypassing base.
			if ( mUIManager.InstrumentPanelUI.Instrument == null )
			{
				return;
			}

			mInstrumentDisplay.GetOffsetAndNoteIndex( measureIndex, mUIManager.MouseWorldPoint, out var noteData );

			if ( noteData.OffsetPosition.x != 0 && noteData.OffsetPosition.y != 0 )
			{
				var wasAdded = mInstrumentDisplay.AddOrRemoveNote( measureIndex, CurrentMeasure, mInstrument, noteData );
				UpdateClipNote( noteData, wasAdded, mInstrument );
			}
		}

		///<inheritdoc/>
		protected override void RefreshDisplay( int measureIndex )
		{
			//bypassing base so we don't show hints
			if ( mIsEnabled && mInstrumentDisplay )
			{
				mInstrumentDisplay.ShowNotes( measureIndex );
				RefreshInstrument();
			}
		}

		#endregion protected

		#region private

		[SerializeField, Tooltip( "Reference to our Title Text" )]
		private TMP_Text mTitleText;

		[SerializeField, Tooltip( "Reference to our Title's Sprite Renderer" )]
		private SpriteRenderer mTitleSpriteRenderer;

		/// <summary>
		/// Adds all instrument notes for this instrument
		/// </summary>
		/// <param name="timestepIdx"></param>
		/// <param name="noteIdx"></param>
		/// <param name="measureIdx"></param>
		private void InitializeInstrumentNotes( int timestepIdx, int noteIdx, int measureIdx )
		{
			var timestep = new Vector2Int( timestepIdx, noteIdx );
			mInstrumentDisplay.GetOffsetAndNoteIndex(
				measureIdx,
				timestep,
				note: 0,
				out var offsetPosition );
			if ( offsetPosition.y > 0 )
			{
				mInstrumentDisplay.AddOrRemoveNote(
					measureIdx,
					CurrentMeasure,
					mInstrument,
					new MeasureEditorNoteData( timestep, noteIndex: 0, CurrentMeasure, offsetPosition )
				);
			}
		}

		#endregion private
	}
}
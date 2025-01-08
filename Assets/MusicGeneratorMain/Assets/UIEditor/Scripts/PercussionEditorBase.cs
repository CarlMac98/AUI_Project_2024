using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/* This class is a bit obtuse. For percussion, each instrument will function as its own
	 * DisplayEditor, so, this acts more as a parent, forwarding each command to the child editors
	 */
	public abstract class PercussionEditorBase : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Getter for our collider
		/// </summary>
		public BoxCollider2D Collider => mCollider;

		/// <summary>
		/// Initialization 
		/// </summary>
		/// <param name="uiManager"></param>
		public virtual void Initialize( UIManager uiManager )
		{
			mIsInitializing = true;
			mUIManager = uiManager;
			mPercussionScrollElement.Initialize( () => { }, uiManager);
			mIsInitializing = false;
			AddListeners();
		}

		/// <summary>
		/// Initializes the Percussion Editor instruments
		/// </summary>
		/// <param name="instruments"></param>
		/// <returns></returns>
		public IEnumerator InitializeInstruments( List<Instrument> instruments )
		{
			mIsInitializing = true;
			var completedSpawns = 0;
			var requiredSpawns = 0;

			foreach ( var instrument in instruments )
			{
				if ( instrument.InstrumentData.IsPercussion == false )
				{
					continue;
				}

				requiredSpawns++;
				SpawnInstrument( instrument,
					( result ) =>
					{
						if ( result != null )
						{
							completedSpawns++;
						}
						else
						{
							requiredSpawns--; // error is handled in spawn request, but in order to fail gracefully(ish) here, just decrement this and move on.
						}
					} );
			}

			yield return new WaitUntil( () => completedSpawns == requiredSpawns );
			mUIManager.DirtyEditorDisplays();
			Stop();
			mIsInitializing = false;
		}

		/// <summary>
		/// Destroys all Percussion Editor Instruments
		/// </summary>
		public void DestroyInstruments()
		{
			foreach ( var instrument in mPercussionInstruments )
			{
				instrument.Value.OnNoteUpdated.RemoveListener( UpdateClipNote );
				instrument.Value.OnInstrumentMoved.RemoveListener( OnInstrumentMoved );
				Destroy( instrument.Value.gameObject );
			}

			mPercussionInstruments.Clear();
		}

		/// <summary>
		/// Sets panel active or inactive
		/// </summary>
		/// <param name="isActive"></param>
		public virtual void SetPanelActive( bool isActive )
		{
			// ensure these are clean.
			mUIManager.UIPercussionEditor.CleanDisplay();
			mUIManager.UIPercussionEditor.RepairDisplay();
			if ( mIsEnabled == isActive )
			{
				return;
			}

			mIsEnabled = isActive;
			mPercussionScrollElement.SetActive( isActive );
			mEditorInstrumentSpawnParent.gameObject.SetActive( isActive );
		}

		/// <summary>
		/// Stop all Percussion Editor Instruments and dirty display
		/// </summary>
		public void Stop()
		{
			foreach ( var instrument in mPercussionInstruments )
			{
				instrument.Value.Stop();
			}

			mUIManager.DirtyEditorDisplays();
		}

		/// <summary>
		/// Pauses all Percussion Editor instruments
		/// </summary>
		public void Pause()
		{
			foreach ( var instrument in mPercussionInstruments )
			{
				instrument.Value.Pause();
			}
		}

		/// <summary>
		/// Plays all Percussion Editor Instruments
		/// </summary>
		public virtual void Play()
		{
			foreach ( var instrument in mPercussionInstruments )
			{
				instrument.Value.Play();
			}
		}

		/// <summary>
		/// Manual Update loop
		/// </summary>
		public void DoUpdate()
		{
			if ( mIsInitializing || mIsEnabled == false )
			{
				return;
			}

			foreach ( var instrument in mPercussionInstruments )
			{
				instrument.Value.DoUpdate();
			}

			if ( mDisplayEditor.DisplayIsDirty )
			{
				mDisplayEditor.CleanDisplay();
			}

			if ( mDisplayEditor.DisplayIsBroken )
			{
				mDisplayEditor.RepairDisplay();
			}
		}

		/// <summary>
		/// Plays individual note of the percussion editor
		/// </summary>
		/// <param name="args"></param>
		public virtual void PlayNote( NotePlayedArgs args )
		{
			var instrument = mUIManager.MusicGenerator.InstrumentSet.Instruments[args.InstrumentIndex];
			mPercussionInstruments[instrument].PlayNote( args );
		}

		#endregion public

		#region protected

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		protected UIManager mUIManager;

		/// <summary>
		/// Whether the percussion editor is enabled
		/// </summary>
		protected bool mIsEnabled;

		/// <summary>
		/// Reference to our Display Editor
		/// </summary>
		protected UIDisplayEditor mDisplayEditor;

		/// <summary>
		/// List of our Percussion Instruments
		/// </summary>
		protected Dictionary<Instrument, PercussionEditorInstrument> mPercussionInstruments { get; } =
			new Dictionary<Instrument, PercussionEditorInstrument>();

		/// <summary>
		/// Handles adding/removing clip notes data containers
		/// </summary>
		/// <param name="noteData"></param>
		/// <param name="wasAdded"></param>
		/// <param name="instrument"></param>
		protected abstract void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument );

		/// <summary>
		/// On Enable
		/// </summary>
		protected virtual void AddListeners()
		{
			mUIManager.MusicGenerator.NormalMeasureExited.AddListener( ResetPlayedNotes );
			mUIManager.MusicGenerator.RepeatedMeasureExited.AddListener( ResetPlayedNotes );
			mUIManager.MusicGenerator.InstrumentAdded.AddListener( OnInstrumentAdded );
			mUIManager.MusicGenerator.InstrumentWillBeRemoved.AddListener( OnInstrumentRemoved );
		}

		/// <summary>
		/// OnDisable
		/// </summary>
		protected virtual void OnDisable()
		{
			if ( mUIManager != false )
			{
				mUIManager.MusicGenerator.NormalMeasureExited.RemoveListener( ResetPlayedNotes );
				mUIManager.MusicGenerator.RepeatedMeasureExited.RemoveListener( ResetPlayedNotes );
				mUIManager.MusicGenerator.InstrumentAdded.RemoveListener( OnInstrumentAdded );
				mUIManager.MusicGenerator.InstrumentWillBeRemoved.RemoveListener( OnInstrumentRemoved );
			}
		}

		#endregion protected

		#region private

		[SerializeField, Tooltip( " Reference to our collider " )]
		private BoxCollider2D mCollider;

		[SerializeField, Tooltip( "Reference to our Percussion Editor Base Asset" )]
		private GameObject mPercussionInstrumentBase;

		[SerializeField, Tooltip( "Reference to Transform to which our instruments are parented" )]
		private Transform mEditorInstrumentSpawnParent;

		[Header( "Positioning" ), Space( 10 )]
		[SerializeField]
		private Transform[] mInstrumentParentTransforms;

		[SerializeField, Tooltip( "Reference to our Transform to which we parent cached instruments" )]
		private Transform mCacheTransform;

		[SerializeField, Tooltip( "Reference to our Percussion scroll element" )]
		private UIHorizontalScrollBar mPercussionScrollElement;

		/// <summary>
		/// Resets our played notes (the unused parameter is to handle the MusicGenerator event.
		/// 
		/// </summary>
		/// <param name="state"></param>
		private void ResetPlayedNotes( GeneratorState state )
		{
			ResetPlayedNotes();
		}

		/// <summary>
		/// Resets our played notes
		/// </summary>
		private void ResetPlayedNotes()
		{
			if ( mIsEnabled == false )
			{
				return;
			}

			mDisplayEditor.UpdateUIElementValues();
			foreach ( var instrument in mPercussionInstruments )
			{
				instrument.Value.ResetPlayedNotes();
			}
		}

		/// <summary>
		/// Handles an instrument being moved
		/// </summary>
		private void OnInstrumentMoved()
		{
			IEnumerable<PercussionEditorInstrument> sortedInstruments = from instrument in mPercussionInstruments.Values
				orderby instrument.transform.position.x
				select instrument;

			var index = 0;
			foreach ( var instrument in sortedInstruments )
			{
				SetInstrumentParent( instrument, index );
				index++;
			}
		}

		/// <summary>
		/// Listener to InstrumentAdded events
		/// </summary>
		/// <param name="instrument"></param>
		private void OnInstrumentAdded( Instrument instrument )
		{
			if ( mIsInitializing || mDisplayEditor.State == DisplayEditorState.Initializing )
			{
				return;
			}

			if ( mPercussionInstruments.ContainsKey( instrument ) ||
			     mDisplayEditor.IsEnabled == false ||
			     instrument.InstrumentData.IsPercussion == false )
			{
				return;
			}

			SpawnInstrument( instrument, PositionNewInstrument );
		}

		/// <summary>
		/// Listener to InstrumentRemoved events
		/// </summary>
		/// <param name="instrument"></param>
		private void OnInstrumentRemoved( Instrument instrument )
		{
			if ( mDisplayEditor.IsEnabled && instrument.InstrumentData.IsPercussion )
			{
				mPercussionInstruments[instrument].OnNoteUpdated.RemoveListener( UpdateClipNote );
				mPercussionInstruments[instrument].OnInstrumentMoved.RemoveListener( OnInstrumentMoved );
				Destroy( mPercussionInstruments[instrument].gameObject );
				mPercussionInstruments.Remove( instrument );
			}

			UpdateInstrumentParents();
		}

		/// <summary>
		/// Spawns a new instrument from asset reference for a specific instrument
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="onComplete"></param>
		private void SpawnInstrument( Instrument instrument, Action<PercussionEditorInstrument> onComplete )
		{
			var percussionInstrument = Instantiate( mPercussionInstrumentBase, Vector3.zero, Quaternion.identity, mEditorInstrumentSpawnParent );
			if ( percussionInstrument == null )
			{
				Debug.LogError(
					$"Unable to spawn instrument: {instrument.InstrumentData.InstrumentType}. Please ensure there is a valid asset reference" );
				onComplete.Invoke( null );
				return;
			}

			InitializeInstrument( instrument, onComplete, percussionInstrument );
			UpdateInstrumentParents();
		}

		/// <summary>
		/// Initializes a new percussion instrument
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="onComplete"></param>
		/// <param name="result"></param>
		private void InitializeInstrument( Instrument instrument,
			Action<PercussionEditorInstrument> onComplete,
			GameObject result )
		{
			result.gameObject.transform.localPosition = Vector3.zero;
			var percussionInstrument = result.GetComponent<PercussionEditorInstrument>();
			mPercussionInstruments[instrument] = percussionInstrument;
			percussionInstrument.OnNoteUpdated.AddListener( UpdateClipNote );
			percussionInstrument.OnInstrumentMoved.AddListener( OnInstrumentMoved );
			PositionNewInstrument( percussionInstrument );

			percussionInstrument.Initialize( mUIManager,
				mDisplayEditor,
				instrument,
				_ => { onComplete.Invoke( percussionInstrument ); } );
		}

		/// <summary>
		/// Positions new Percussion Instruments
		/// </summary>
		/// <param name="instrument"></param>
		private void PositionNewInstrument( PercussionEditorInstrument instrument )
		{
			SetInstrumentParent( instrument, mPercussionInstruments.Count - 1 );
		}

		/// <summary>
		/// Parents the new percussion instrument and updates visibility based on whether it is within our view
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="index"></param>
		private void SetInstrumentParent( PercussionEditorInstrument instrument, int index )
		{
			if ( index < mInstrumentParentTransforms.Length )
			{
				instrument.transform.SetParent( mInstrumentParentTransforms[index] );
				instrument.InstrumentOverlay.FadeIn();
			}
			else
			{
				instrument.transform.SetParent( mCacheTransform );
				instrument.InstrumentOverlay.FadeOut();
			}

			instrument.transform.localPosition = Vector3.zero;
		}

		/// <summary>
		/// re-parents instruments 
		/// </summary>
		private void UpdateInstrumentParents()
		{
			var index = 0;
			foreach ( var instrument in mPercussionInstruments )
			{
				SetInstrumentParent( instrument.Value, index );
				index++;
			}

			if ( mPercussionInstruments.Count > 0 )
			{
				mPercussionScrollElement.UpdateScrollEndTransform(
					mInstrumentParentTransforms[mPercussionInstruments.Count - 1] );
			}
		}

		/// <summary>
		/// Whether the percussion editor is currently in an initializing state
		/// </summary>
		private bool mIsInitializing;

		#endregion private
	}
}
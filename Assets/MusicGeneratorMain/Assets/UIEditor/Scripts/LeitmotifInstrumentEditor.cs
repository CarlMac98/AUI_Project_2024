using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
    /// <summary>
    /// Display Editor for the Leitmotif Editor Instruments
    /// </summary>
    public class LeitmotifInstrumentEditor : DisplayEditor
    {
        #region public

        ///<inheritdoc/>
        protected override int CurrentMeasure => mUIManager.UILeitmotifEditor.CurrentMeasure;

        ///<inheritdoc/>
        public override void DoUpdate()
        {
            if ( !mIsEnabled || mUIManager.MusicGenerator.GeneratorState != GeneratorState.Stopped )
            {
                return;
            }

            if ( mUIManager.UILeitmotifEditor.DisplayIsBroken )
            {
                mUIManager.LeitmotifEditor.SetIsInitializing( true );
                RebuildDisplay();
                StartCoroutine( InitializeInstruments( mUIManager.MusicGenerator.InstrumentSet.Instruments,
                    () =>
                    {
                        //RefreshDisplay( CurrentMeasure );

                        mUIManager.LeitmotifEditor.SetIsInitializing( false );
                    } ) );
                return;
            }

            // Highlight
            mUIManager.UIKeyboard.GetKeyHorizontalOffset( mUIManager.MouseWorldPoint, out var noteIndex );
            mUIManager.UIKeyboard.PlayKeyLight( noteIndex,
                mUIManager.Colors[( int )mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.StaffPlayerColor] );

            if ( mUIManager.UILeitmotifEditor.DisplayIsDirty )
            {
                RefreshDisplay( CurrentMeasure );
            }

            if ( Input.GetMouseButtonDown( 0 ) && mCollider.bounds.Contains( mUIManager.MouseWorldPoint ) )
            {
                ClickNote( CurrentMeasure );
            }

            mUIManager.UILeitmotifEditor.RepairDisplay();
            mUIManager.UILeitmotifEditor.CleanDisplay();
        }

        ///<inheritdoc/>
        protected override void InitializeInstrument( Instrument instrument, Action<bool> callback = null )
        {
            if ( instrument.InstrumentData.IsPercussion || mUIManager.InstrumentListPanelUI.PercussionIsSelected )
            {
                callback?.Invoke( true );
                return;
            }

            IReadOnlyList<Leitmotif.LeitmotifMeasure> leitmotif = instrument.InstrumentData.Leitmotif.Notes;
            for ( var measureIdx = 0; measureIdx < leitmotif.Count; measureIdx++ )
            {
                for ( var stepIdx = 0; stepIdx < leitmotif[measureIdx].Beat.Count; stepIdx++ )
                {
                    for ( var noteIdx = 0; noteIdx < leitmotif[measureIdx].Beat[stepIdx].SubBeat.Count; noteIdx++ )
                    {
                        foreach ( var leitmotifNote in leitmotif[measureIdx].Beat[stepIdx].SubBeat[noteIdx].notes )
                        {
                            var note = Leitmotif.GetUnscaledNoteIndex( leitmotifNote, mUIManager.MusicGenerator );
                            if ( note == -1 )
                            {
                                continue;
                            }

                            var timestep = new Vector2Int( stepIdx, noteIdx );
                            mInstrumentDisplay.GetOffsetAndNoteIndex(
                                measureIdx,
                                timestep,
                                note,
                                out var offsetPosition );
                            if ( offsetPosition.y > 0 )
                            {
                                mInstrumentDisplay.AddOrRemoveNote( measureIdx,
                                    CurrentMeasure,
                                    instrument,
                                    new MeasureEditorNoteData( timestep, note, CurrentMeasure, offsetPosition ));
                            }
                        }
                    }
                }
            }

            callback?.Invoke( true );
        }

        ///<inheritdoc/>
        public override void
            Play()
        {
            base.Play();
            mUIManager.UIKeyboard.Play( playMode: UIKeyboard.PlayMode.LeitmotifInstrument );
        }

        #endregion public

        #region protected

        ///<inheritdoc/>
        protected override void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument )
        {
            var note = GetLeitmotifNote( noteData.NoteIndex );
            if ( wasAdded )
            {
                AddLeitmotifNote( noteData.Beat.x, noteData.Beat.y, note, noteData.Measure );
            }
            else
            {
                RemoveLeitmotifNote( noteData.Beat.x, noteData.Beat.y, note, noteData.Measure );
            }

            var indices = Leitmotif.GetUnscaledNoteArray( mNotes[noteData.Measure].Beat[noteData.Beat.x].SubBeat[noteData.Beat.y].notes,
                mUIManager.MusicGenerator );
            foreach ( var timestepNotes in indices )
            {
                mUIManager.MusicGenerator.PlayNote( mUIManager.CurrentInstrumentSet,
                    instrument.InstrumentData.Volume,
                    instrument.InstrumentData.InstrumentType,
                    timestepNotes,
                    instrument.InstrumentIndex );
            }
        }

        #endregion protected

        #region private

        /// <summary>
        /// Adds a leitmotif note to our container
        /// </summary>
        /// <param name="timestep"></param>
        /// <param name="step"></param>
        /// <param name="note"></param>
        /// <param name="measure"></param>
        private void AddLeitmotifNote( int timestep, int step, LeitmotifNote note, int measure = 0 )
        {
            mNotes[measure].Beat[timestep].SubBeat[step].notes.Add( note );
        }

        /// <summary>
        /// removes a leitmotif note from our container
        /// </summary>
        /// <param name="beat"></param>
        /// <param name="subBeat"></param>
        /// <param name="note"></param>
        /// <param name="measure"></param>
        private void RemoveLeitmotifNote( int beat, int subBeat, LeitmotifNote note, int measure = 0 )
        {
            for ( var index = 0; index < mNotes[measure].Beat[beat].SubBeat[subBeat].notes.Count; index++ )
            {
                if ( mNotes[measure].Beat[beat].SubBeat[subBeat].notes[index].Equals( note ) )
                {
                    mNotes[measure].Beat[beat].SubBeat[subBeat].notes.Remove( note );
                }
            }
        }

        /// <summary>
        /// Reference  to our list of leitmotif measures (bit poorly named, but make other things clearer).
        /// </summary>
        private IReadOnlyList<Leitmotif.LeitmotifMeasure> mNotes =>
            mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.Leitmotif.Notes;

        /// <summary>
        /// Returns the leitmotif note based on a raw note index, taking into account, key, scale, mode, etc.
        /// </summary>
        /// <param name="rawNote"></param>
        /// <returns></returns>
        private LeitmotifNote GetLeitmotifNote( int rawNote )
        {
            if ( rawNote < 0 )
            {
                return new LeitmotifNote();
            }

            var musicGenerator = mUIManager.MusicGenerator;
            const int scaleLength = MusicConstants.ScaleLength;
            var scale = MusicConstants.GetScale( musicGenerator.ConfigurationData.Scale );
            var foundSharp = false;
            var key = ( int )musicGenerator.ConfigurationData.Key;
            var finalScaledNote = 0;

            for ( var index = 0; index < MusicConstants.TotalScaleNotes; index++ )
            {
                var noteIndex = key;

                for ( var subIndex = 0; subIndex < index; subIndex++ )
                {
                    var scaleIndex = ( subIndex + ( int )musicGenerator.ConfigurationData.Mode ) % scaleLength;
                    noteIndex += scale[scaleIndex];
                }

                noteIndex %= MusicConstants.MaxInstrumentNotes;

                //ugh, this is really brute force and ugly :/
                if ( Mathf.Abs( noteIndex - rawNote ) <= 1 )
                {
                    finalScaledNote = MusicConstants.SafeLoop( index, 0, MusicConstants.TotalScaleNotes );
                    var accidental = 0;
                    if ( noteIndex - rawNote > 0 )
                    {
                        accidental = -1;
                    }
                    else if ( noteIndex - rawNote < 0 )
                    {
                        accidental = 1;
                    }

                    if ( accidental > 0 )
                    {
                        foundSharp = true;
                    }
                    else if ( foundSharp && accidental < 0 ) // previously found potential sharp was actually this valid note
                    {
                        return new LeitmotifNote( finalScaledNote - 1, 1 );
                    }
                    else // non-accidentals and flats
                    {
                        return new LeitmotifNote( finalScaledNote, accidental );
                    }
                }
                else if ( foundSharp )
                {
                    return new LeitmotifNote( finalScaledNote, 1 );
                }
            }

            // handles 36th note for certain scales :/
            if ( foundSharp )
            {
                return new LeitmotifNote( finalScaledNote, 1 );
            }

            Debug.LogError( "Selected note is not part of a valid scale or outside our range of notes" );
            return new LeitmotifNote();
        }

        #endregion private
    }
}
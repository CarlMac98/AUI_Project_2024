#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Percussion handler for measure display editor, handles differences between percussion/instrument ui.
	/// </summary>
	public class MeasurePercussionEditor : PercussionEditorBase
	{
		///<inheritdoc/>
		public override void Initialize( UIManager uiManager )
		{
			mDisplayEditor = uiManager.UIMeasureEditor;
			base.Initialize( uiManager );
		}

		///<inheritdoc/>
		public override void PlayNote( NotePlayedArgs args )
		{
			var instrument = mUIManager.CurrentInstrumentSet.Instruments[args.InstrumentIndex];
			mPercussionInstruments[instrument].PlayNote( args );
		}

		///<inheritdoc/>
		protected override void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument )
		{
			instrument.InstrumentData.ForcedPercussiveNotes.Measures[noteData.Measure].Timesteps[noteData.Beat.x].Notes[noteData.Beat.y] = wasAdded;

			mUIManager.MusicGenerator.PlayNote( mUIManager.CurrentInstrumentSet,
				instrument.InstrumentData.Volume,
				instrument.InstrumentData.InstrumentType,
				noteData.NoteIndex,
				instrument.InstrumentIndex );
		}
	}
}
#pragma warning disable 0649

namespace ProcGenMusic
{
	/* This class is a bit ugly. For percussion, each instrument will function as its own
	 * DisplayEditor, so, this acts more as a parent forwarding each command to the children
	 */
	public class PercussionEditor : PercussionEditorBase
	{
		///<inheritdoc/>
		public override void Initialize( UIManager uiManager )
		{
			mDisplayEditor = uiManager.UIPercussionEditor;
			base.Initialize( uiManager );
		}

		///<inheritdoc/>
		public override void SetPanelActive( bool isActive )
		{
			mUIManager.UIKeyboard.Stop( true );
			base.SetPanelActive( isActive );
			mUIManager.InstrumentListPanelUI.SetPercussionOnly( isActive );
		}

		///<inheritdoc/>
		public override void Play()
		{
			base.Play();

			mUIManager.UIKeyboard.Play( playMode: UIKeyboard.PlayMode.Percussion );
		}

		///<inheritdoc/>
		protected override void UpdateClipNote( MeasureEditorNoteData noteData, bool wasAdded, Instrument instrument )
		{
			instrument.InstrumentData.ForcedPercussiveNotes.Measures[noteData.Measure].Timesteps[noteData.Beat.x].Notes[noteData.Beat.y] = wasAdded;
		}

		///<inheritdoc/>
		protected override void AddListeners()
		{
			mUIManager.MusicGenerator.NormalMeasureExited.AddListener( OnMeasureExited );
			base.AddListeners();
		}

		///<inheritdoc/>
		protected override void OnDisable()
		{
			if ( mUIManager != false )
			{
				mUIManager.MusicGenerator.NormalMeasureExited.RemoveListener( OnMeasureExited );
			}

			base.OnDisable();
		}

		/// <summary>
		/// Invoked when measures are exited. (we dirty the editor display so it will generate the new measure)
		/// </summary>
		private void OnMeasureExited()
		{
			if ( mIsEnabled )
			{
				mUIManager.DirtyEditorDisplays();
			}
		}
	}
}
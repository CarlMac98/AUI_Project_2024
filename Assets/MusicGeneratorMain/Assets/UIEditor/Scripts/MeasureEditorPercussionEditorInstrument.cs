using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Percussion Editor Instrument for the Leitmotif Editor
	/// </summary>
	public class MeasureEditorPercussionEditorInstrument : PercussionEditorInstrument
	{
		protected override int RepeatCount => mUIManager == null ? 0 : mUIManager.UIMeasureEditor.RepeatCount;
		protected override int NumMeasures => mUIManager == null ? 0 : mUIManager.UIMeasureEditor.NumberOfMeasures;

		///<inheritdoc/>
		public override void DoUpdate()
		{
			//bypassing base,
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
				var currentMeasure = mUIManager.UIMeasureEditor.State == DisplayEditorState.Stopped ? CurrentMeasure : RepeatCount % NumMeasures;
				RefreshDisplay( currentMeasure );
			}

			if ( Input.GetMouseButtonDown( 0 ) && mCollider.bounds.Contains( mUIManager.MouseWorldPoint ) )
			{
				ClickNote( CurrentMeasure );
			}
		}
	}
}
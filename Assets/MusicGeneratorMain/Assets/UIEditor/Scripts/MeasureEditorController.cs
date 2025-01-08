using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class MeasureEditorController : UIController
	{
		#region public

		///<inheritdoc/>
		public override void DoUpdate() { }

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			base.UpdateUIElementValues();
			SetChordProgression();
		}

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			mMusicGenerator = uiManager.MusicGenerator;
			base.Initialize( uiManager, isEnabled );
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			//bypassing base. We initialize our time signature scroller differently.

			mTimeSignatureScroller.Initialize( ( ( value ) =>
				{
					UpdateTimeSignature( value );
					switch ( value )
					{
						case 0:
							mTimeSignatureScroller.Text.text = "4/4";
							break;
						case 1:
							mTimeSignatureScroller.Text.text = "3/4";
							break;
						case 2:
							mTimeSignatureScroller.Text.text = "5/4";
							break;
					}

					if ( mUIMeasureEditor.IsEnabled )
					{
						mUIMeasureEditor.InstrumentSet?.SetTimeSignature( ( TimeSignatures )value );
					}
				} ),
				initialIndex: ( int )mUIManager.CurrentInstrumentSet.TimeSignature.Signature,
				elements: new[] {0, 1, 2} ); // We'll initialize before measure editor is loaded

			mTonicChord.Initialize( ( value ) =>
				{
					mCurrentChordProgression[0] = value;
					if ( mUIManager.MeasureEditorIsActive )
					{
						if ( mUIManager.MusicGenerator.CurrentChordProgression != null )
						{
							SetChordProgression();
						}

						mUIManager.DirtyEditorDisplays();
					}

					mTonicChord.Text.text = $"{value}";
				},
				initialIndex: 0,
				elements: new[] {1, 2, 3, 4, 5, 6, 7} );

			mSubdominantChord.Initialize( ( value ) =>
				{
					mCurrentChordProgression[1] = value;
					if ( mUIManager.MeasureEditorIsActive )
					{
						if ( mUIManager.MusicGenerator.CurrentChordProgression != null )
						{
							SetChordProgression();
						}

						mUIManager.DirtyEditorDisplays();
					}

					mSubdominantChord.Text.text = $"{value}";
				},
				initialIndex: 0,
				elements: new[] {1, 2, 3, 4, 5, 6, 7} );

			mFloaterChord.Initialize( ( value ) =>
				{
					mCurrentChordProgression[2] = value;
					if ( mUIManager.MeasureEditorIsActive )
					{
						if ( mUIManager.MusicGenerator.CurrentChordProgression != null )
						{
							SetChordProgression();
						}

						mUIManager.DirtyEditorDisplays();
					}

					mFloaterChord.Text.text = $"{value}";
				},
				initialIndex: 0,
				elements: new[] {1, 2, 3, 4, 5, 6, 7} );

			mDominantChord.Initialize( ( value ) =>
				{
					mCurrentChordProgression[3] = value;
					if ( mUIManager.MeasureEditorIsActive )
					{
						if ( mUIManager.MusicGenerator.CurrentChordProgression != null )
						{
							SetChordProgression();
						}

						mUIManager.DirtyEditorDisplays();
					}

					mDominantChord.Text.text = $"{value}";
				},
				initialIndex: 0,
				elements: new[] {1, 2, 3, 4, 5, 6, 7} );

			mPlay.onClick.AddListener( mUIManager.UIMeasureEditor.Play );
			mPause.onClick.AddListener( mUIManager.UIMeasureEditor.Pause );
			mStop.onClick.AddListener( mUIManager.UIMeasureEditor.Stop );
			mExport.onClick.AddListener( ExportMeasureConfigFile );
		}

		///<inheritdoc/>
		protected override void OnDestroy()
		{
			base.OnDestroy();
			mPlay.onClick.RemoveAllListeners();
			mPause.onClick.RemoveAllListeners();
			mStop.onClick.RemoveAllListeners();
			mExport.onClick.RemoveAllListeners();
		}

		#endregion protected

		#region private

		/// <summary>
		/// Our current chord progression, as InstrumentSet by the time signature scroller elements
		/// </summary>
		private readonly int[] mCurrentChordProgression = {1, 4, 4, 5};

		[SerializeField, Tooltip( "Reference to our UIMeasureEditor" )]
		private UIMeasureEditor mUIMeasureEditor;

		[SerializeField, Tooltip( "Reference to our Tonic Chord Scroller" )]
		private UIIntScroller mTonicChord;

		[SerializeField, Tooltip( "Reference to our Subdominant Chord Scroller" )]
		private UIIntScroller mSubdominantChord;

		[SerializeField, Tooltip( "Reference to our Floater Chord Scroller" )]
		private UIIntScroller mFloaterChord;

		[SerializeField, Tooltip( "Reference to our Dominant Chord Scroller" )]
		private UIIntScroller mDominantChord;

		/// <summary>
		/// Exports the current configuration
		/// </summary>
		private void ExportMeasureConfigFile()
		{
			mUIManager.UIMeasureEditor.ExportFile( mExportFileName.text );
		}

		/// <summary>
		/// Sets the Music Generator's Chord Progression to our scroller values
		/// </summary>
		private void SetChordProgression()
		{
			mUIManager.MusicGenerator.CurrentChordProgression = mCurrentChordProgression;
		}

		#endregion private
	}
}
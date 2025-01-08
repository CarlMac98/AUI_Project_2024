using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Play controller (play, pause, stop, export, etc) for the Leitmotif Editor
	/// </summary>
	public class LeitmotifController : UIController
	{
		#region public

		///<inheritdoc/>
		public override void DoUpdate() { }

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			mUILeitmotifEditor = uiManager.UILeitmotifEditor;
			base.Initialize( uiManager, isEnabled );
		}

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			base.UpdateUIElementValues();
			for ( var index = 0; index < mProgressionChords.Length; index++ )
			{
				var value = index >= mUIManager.MusicGenerator.InstrumentSet.Data.LeitmotifProgression.Length ?
					1 :
					mUIManager.MusicGenerator.InstrumentSet.Data.LeitmotifProgression[index];
				mProgressionChords[index].Option.Value = value;
			}
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			base.InitializeListeners();

			mPlay.onClick.AddListener( mUILeitmotifEditor.Play );
			mPause.onClick.AddListener( mUILeitmotifEditor.Pause );
			mStop.onClick.AddListener( mUILeitmotifEditor.Stop );
			mExport.onClick.AddListener( ExportFile );

			var leitmotifLength = mUIManager.MusicGenerator.InstrumentSet.Data.LeitmotifProgression.Length;
			for ( var index = 0; index < mProgressionChords.Length; index++ )
			{
				var progressionIndex = index;
				var initialValue = progressionIndex >= leitmotifLength ? 0 : mUIManager.MusicGenerator.InstrumentSet.Data.LeitmotifProgression[progressionIndex] - 1;
				mProgressionChords[index].Initialize( ( value ) =>
					{
						if ( mUIManager.MusicGenerator.InstrumentSet.Data.LeitmotifProgression.Length > progressionIndex )
						{
							mUIManager.MusicGenerator.InstrumentSet.Data.LeitmotifProgression[progressionIndex] = value;
							mUIManager.DirtyEditorDisplays();
							mProgressionChords[progressionIndex].Text.text = $"{value}";
						}
					},
					initialIndex: initialValue,
					elements: new[] {1, 2, 3, 4, 5, 6, 7} );
			}
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

		/// <summary>
		/// Exports our configuration file with current settings
		/// </summary>
		protected override void ExportFile()
		{
			if ( string.IsNullOrEmpty( mExportFileName.text ) ||
			     mExportFileName.text.Equals( "New" ) )
			{
				return;
			}

			Debug.Log( "exporting configuration " + mExportFileName.text );

			//Restore leitmotifOdds in order to save
			var leitmotifOddsCache = mUILeitmotifEditor.CachedLeitmotifOdds;
			mUILeitmotifEditor.RestoreLeitmotifOdds();
			mMusicGenerator.SaveCurrentConfiguration( mExportFileName.text );
			mUILeitmotifEditor.SetLeitmotifOddsCache( leitmotifOddsCache );
			mUIManager.GeneralMenuPanel.AddPresetOption( mExportFileName.text );
		}

		#endregion protected

		#region private

		[SerializeField, Tooltip( "Reference to our progression chord 1" )]
		private UIIntScroller[] mProgressionChords;

		private UILeitmotifEditor mUILeitmotifEditor;

		#endregion private
	}
}
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Player controller handles top-bar controls (play, pause, time signature, export, etc).
	/// </summary>
	public class PlayerController : UIController
	{
		#region public

		///<inheritdoc/>
		public override void DoUpdate()
		{
			UpdateChordProgressionText( mUIManager.MusicGenerator.InstrumentSet );
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			base.InitializeListeners();

			mExport.onClick.AddListener( ExportFile );
			mPlay.onClick.AddListener( mUIManager.UIKeyboard.Play );
			mPause.onClick.AddListener( mUIManager.UIKeyboard.Pause );
			mStop.onClick.AddListener( mUIManager.UIKeyboard.Stop );
			mStop.onClick.AddListener( mUIManager.UIKeyboard.Reset );
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
		/// Reference to our current chord progression values
		/// </summary>
		private readonly int[] mCurrentChordProgression = {-1, -1, -1, -1, -1, -1, -1, -1};

		/// <summary>
		/// Current Progression Step
		/// </summary>
		private int mCurrentProgressionStep;

		[SerializeField, Tooltip( "Reference to our current progression step text" )]
		private TMP_Text mCurrentProgStep;

		/// <summary>
		/// Updates our current chord progression text
		/// </summary>
		/// <param name="set"></param>
		private void UpdateChordProgressionText( InstrumentSet set )
		{
			var dirty = false;
			for ( var index = 0; index < mMusicGenerator.CurrentChordProgression.Count; index++ )
			{
				if ( mCurrentChordProgression[index] != mMusicGenerator.CurrentChordProgression[index] )
				{
					mCurrentChordProgression[index] = mMusicGenerator.CurrentChordProgression[index];
					dirty = true;
				}
			}

			var stepsTaken = set.ProgressionStepsTaken >= 0 ? set.ProgressionStepsTaken : 0;
			if ( mCurrentChordProgression[stepsTaken] != mCurrentProgressionStep )
			{
				mCurrentProgressionStep = mCurrentChordProgression[stepsTaken];
				dirty = true;
			}

			if ( dirty )
			{
				mCurrentProgStep.text =
					$"{{{mCurrentChordProgression[0]}-{mCurrentChordProgression[1]}-{mCurrentChordProgression[2]}-{mCurrentChordProgression[3]}}}:{mCurrentProgressionStep}";
			}
		}

		#endregion private
	}
}
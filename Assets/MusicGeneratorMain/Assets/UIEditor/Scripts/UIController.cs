using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Base class for player controllers (play, pause, export etc)
	/// </summary>
	public abstract class UIController : UIPanel
	{
		#region public

		public bool ExportIsHovered => mExportFileName != null && mExportFileName.isFocused;

		/// <summary>
		/// Manual Update Loop (must be manually invoked).
		/// </summary>
		public abstract void DoUpdate();

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			mTimeSignatureScroller.Option.Value = ( int )mUIManager.CurrentInstrumentSet.TimeSignature.Signature;
			if ( mMusicGenerator.ConfigurationData.ConfigurationName == "New" ) { }

			mExportFileName.text = mMusicGenerator.ConfigurationData.ConfigurationName == "New" ? "NewConfiguration" : mMusicGenerator.ConfigurationData.ConfigurationName;
		}

		#endregion public

		#region protected

		[SerializeField, Tooltip( "Reference to our export button's input field" )]
		protected TMP_InputField mExportFileName;

		[SerializeField, Tooltip( "Reference to our Time signature scroller" )]
		protected UIIntScroller mTimeSignatureScroller;

		[SerializeField, Tooltip( "Reference to our play button" )]
		protected Button mPlay;

		[SerializeField, Tooltip( "Reference to our stop button" )]
		protected Button mStop;

		[SerializeField, Tooltip( "Reference to our pause button" )]
		protected Button mPause;

		[SerializeField, Tooltip( "Reference to our export button" )]
		protected Button mExport;

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			mTimeSignatureScroller.Initialize( ( value ) =>
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
				},
				initialIndex: ( int )mUIManager.CurrentInstrumentSet.Data.TimeSignature,
				elements: new[] {0, 1, 2} );
		}

		/// <summary>
		/// Exports our configuration file with current settings
		/// </summary>
		protected virtual void ExportFile()
		{
			if ( string.IsNullOrEmpty( mExportFileName.text ) ||
			     mExportFileName.text.Equals( "New" ) )
			{
				return;
			}

			Debug.Log( "exporting configuration " + mExportFileName.text );

			mMusicGenerator.SaveCurrentConfiguration( mExportFileName.text );
			mUIManager.GeneralMenuPanel.AddPresetOption( mExportFileName.text );
		}

		/// <summary>
		/// Updates the time signature with the current instrument InstrumentSet.
		/// </summary>
		/// <param name="timeSignature"></param>
		protected void UpdateTimeSignature( int timeSignature )
		{
			var signature = ( TimeSignatures )timeSignature;
			if ( signature != mUIManager.CurrentInstrumentSet.Data.TimeSignature )
			{
				mUIManager.CurrentInstrumentSet.SetTimeSignature( signature );
				mUIManager.BreakEditorDisplays();
			}
		}

		#endregion protected
	}
}
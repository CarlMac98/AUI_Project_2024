#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles play controller bar (play, pause, stop, export, etc) for the Percussion Editor
	/// </summary>
	public class PercussionEditorController : UIController
	{
		#region public

		///<inheritdoc/>
		public override void DoUpdate() { }

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			mUIPercussionEditor = uiManager.UIPercussionEditor;
			base.Initialize( uiManager, isEnabled );
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			base.InitializeListeners();

			mPlay.onClick.AddListener( mUIPercussionEditor.Play );
			mPause.onClick.AddListener( mUIPercussionEditor.Pause );
			mStop.onClick.AddListener( mUIPercussionEditor.Stop );
			mExport.onClick.AddListener( ExportFile );
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
		/// Reference to the UIPercussion Editor
		/// </summary>
		private UIPercussionEditor mUIPercussionEditor;

		#endregion private
	}
}
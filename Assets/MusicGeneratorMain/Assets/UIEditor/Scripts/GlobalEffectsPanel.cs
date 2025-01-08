using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Settings panel controlling the Global Effects
	/// </summary>
	public class GlobalEffectsPanel : UIPanel
	{
		#region public

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
#if FMOD_ENABLED
            mFmodMasterFXPanelUI.UpdateUIElementValues();
#else
			mMasterFXPanelUI.UpdateUIElementValues();
#endif //FMOD_ENABLED
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
#if FMOD_ENABLED
            mFmodMasterFXPanelUI.gameObject.SetActive( true );
            mMasterFXPanelUI.gameObject.SetActive( false );
            mFmodMasterFXPanelUI.InitializeFXPanel( mMusicGenerator );
#else
			mFmodMasterFXPanelUI.gameObject.SetActive( false );
			mMasterFXPanelUI.gameObject.SetActive( true );
			mMasterFXPanelUI.InitializeFXPanel( mMusicGenerator );
#endif //FMOD_ENABLED
		}

		#endregion protected

		#region private

		[SerializeField]
		private FmodMasterFXPanelUI mFmodMasterFXPanelUI;

		[SerializeField]
		private MasterFXPanelUI mMasterFXPanelUI;

		#endregion private
	}
}
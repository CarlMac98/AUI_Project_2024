using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class ModUIObject : MonoBehaviour
	{
		public string ModName => mMod != null ? mMod.ModName : null;

		/// <summary>
		/// Toggles this mod on/off
		/// </summary>
		/// <param name="isEnabled"></param>
		public void Toggle( bool isEnabled )
		{
			mEnabledToggle.Option.isOn = isEnabled;
		}

		/// <summary>
		/// Initializes this ui object
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="uiManager"></param>
		public void Initialize( GeneratorMod mod, UIManager uiManager )
		{
			mMod = mod;
			mTooltip.Initialize( uiManager );
			mTooltip.SetDescription( mMod.Description );

			mEnabledToggle.Initialize( ( isEnabled ) =>
				{
					mModName.text = mod.ModName;
					if ( isEnabled )
					{
						mMod.EnableMod( uiManager.MusicGenerator );
						mMod.LoadData();
					}
					else
					{
						mMod.DisableMod();
					}
				},
				mMod.enabled );
			mEnabledToggle.Tooltip.Initialize( uiManager );
		}

		[SerializeField, Tooltip( "Reference to our toggle" )]
		private UIToggle mEnabledToggle;

		[SerializeField, Tooltip( "Reference to our name text" )]
		private TMP_Text mModName;

		[SerializeField, Tooltip( "Reference to the mod tooltip" )]
		private Tooltip mTooltip;

		/// <summary>
		/// Mod associated with this object
		/// </summary>
		private GeneratorMod mMod;
	}
}
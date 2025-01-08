using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class UIModsEditor : UIPanel
	{
		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			UpdateEnabledMods();
		}

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			foreach ( var mod in mMusicGenerator.Mods )
			{
				var modObject = Instantiate( m_modUIObject, Vector3.zero, Quaternion.identity, mModParentTransform );
				if ( modObject != false )
				{
					modObject.Initialize( mod, mUIManager );
					modObject.transform.localScale = Vector3.one;
					mInstantiatedModObjects.Add( modObject );
				}
			}
		}

		private void UpdateEnabledMods()
		{
			foreach ( var mod in mInstantiatedModObjects )
			{
				mod.Toggle( mMusicGenerator.ConfigurationData.Mods.Contains( mod.ModName ) );
			}
		}

		[SerializeField, Tooltip( "Reference to the mod parent transform" )]
		private Transform mModParentTransform;

		[SerializeField, Tooltip( "Asset Reference to our mod ui object" )]
		private ModUIObject m_modUIObject;

		/// <summary>
		/// Our instantiated objects
		/// </summary>
		private readonly List<ModUIObject> mInstantiatedModObjects = new List<ModUIObject>();
	}
}
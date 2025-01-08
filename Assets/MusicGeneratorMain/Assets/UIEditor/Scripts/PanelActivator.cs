using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles ensuring that panels not actively hovered are not interactable/raycast
	/// </summary>
	public class PanelActivator : MonoBehaviour
	{
		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name="uIManager"></param>
		public void Initialize( UIManager uIManager )
		{
			mUIManager = uIManager;
		}

		/// <summary>
		/// Manual Update loop
		/// </summary>
		public void DoUpdate()
		{
			if ( mUIManager == null )
			{
				return;
			}

			foreach ( var panel in mPanels )
			{
				panel.ToggleRaycaster( panel.Bounds.Contains( mUIManager.MouseWorldPoint ) );
			}
		}

		[SerializeField, Tooltip( "Reference to our panel toggles" )]
		private PanelToggle[] mPanels;

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;
	}
}

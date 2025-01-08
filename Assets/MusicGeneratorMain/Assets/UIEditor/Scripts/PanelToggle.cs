using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles toggling the intractability and raycast-ableness(?) of canvas groups
	/// </summary>
	public class PanelToggle : MonoBehaviour
	{
		/// <summary>
		/// Collider bounds for this panel toggle
		/// </summary>
		public Bounds Bounds => mCollider.bounds;

		/// <summary>
		/// Toggles the interactable and raycasted state of the canvas group of this panel toggle
		/// </summary>
		/// <param name="isEnabled"></param>
		public void ToggleRaycaster( bool isEnabled )
		{
			mCanvasGroup.interactable = isEnabled;
			mCanvasGroup.blocksRaycasts = isEnabled;
		}

		[SerializeField, Tooltip( "reference to our collider for this panel toggle" )]
		private Collider2D mCollider;

		[SerializeField, Tooltip( "Reference to the canvas group that this toggle belongs to" )]
		private CanvasGroup mCanvasGroup;

		/// <summary>
		/// On awake. by default interactable state is false
		/// </summary>
		private void Awake()
		{
			mCanvasGroup.interactable = false;
			mCanvasGroup.blocksRaycasts = false;
		}
	}
}
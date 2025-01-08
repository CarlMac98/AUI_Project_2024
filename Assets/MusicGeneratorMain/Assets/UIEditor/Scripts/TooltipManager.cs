using System.Collections;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Manages showing the tooltip object.
	/// Each tooltip simply reports to this handler which shows the single instantiated object
	/// </summary>
	public class TooltipManager : MonoBehaviour
	{
		#region public

		public void Initialize( UIManager uiManager )
		{
			mUIManager = uiManager;

			var result = Instantiate( mTooltipBase, Vector3.zero, Quaternion.identity, mTooltipCanvas.transform );
			if ( result == null )
			{
				return;
			}

			mInstantiatedTooltipObject = result.GetComponent<UITooltipObject>();
			mInstantiatedTooltipObject.Initialize( mUIManager );
			mInstantiatedTooltipObject.gameObject.SetActive( false );
		}

		/// <summary>
		/// Hides the tooltip
		/// </summary>
		public void HideTooltip()
		{
			if ( mInstantiatedTooltipObject && mInstantiatedTooltipObject.gameObject.activeSelf )
			{
				mInstantiatedTooltipObject.gameObject.SetActive( false );
			}
		}

		/// <summary>
		/// Shows and positions the tooltip
		/// </summary>
		/// <param name="description"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public IEnumerator ShowAndPositionTooltip( string description, Vector3 position )
		{
			mInstantiatedTooltipObject.gameObject.SetActive( true );
			mInstantiatedTooltipObject.SetText( description );

			// We need to allow the TMP asset to resize
			yield return new WaitForEndOfFrame();

			mInstantiatedTooltipObject.transform.position = position;

			// Here we offset the child's local position by its height (so it appears above/below the
			// cursor and doesn't go offscreen)
			var offset = mInstantiatedTooltipObject.ChildTransform.rect.height / 2;
			offset *= Input.mousePosition.y > Screen.height / 2f ? -1f : 1f;
			mInstantiatedTooltipObject.ChildTransform.localPosition = Vector3.up * offset;
		}

		#endregion public

		#region private

		[SerializeField, Tooltip( "Reference to our base asset reference for the tooltip object" )]
		private UITooltipObject mTooltipBase;

		[SerializeField, Tooltip( "Reference to canvas to which we'll parent our tooltip" )]
		private Canvas mTooltipCanvas;

		/// <summary>
		/// Reference to our instantiated tooltip object
		/// </summary>
		private UITooltipObject mInstantiatedTooltipObject;

		/// <summary>
		/// Reference to the UIManager
		/// </summary>
		private UIManager mUIManager;

		#endregion private
	}
}
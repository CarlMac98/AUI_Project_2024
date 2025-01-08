using System;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		/// <summary>
		/// Initializes the tooltip
		/// </summary>
		/// <param name="uiManager"></param>
		public void Initialize( UIManager uiManager )
		{
			mUIManager = uiManager;
			mUIManager.OnLeftShiftDown.AddListener( CheckTooltip );
			mUIManager.OnLeftShiftUp.AddListener( HideTooltip );
			mUIManager.OnLeftClickUp.AddListener( HideTooltip );
			mTooltipManager = mUIManager.TooltipManager;
		}

		/// <summary>
		/// Sets the description of this tooltip
		/// </summary>
		/// <param name="description"></param>
		public void SetDescription( string description )
		{
			mDescription = description;
		}

		/// <summary>
		/// OnPointerExit. We always hide the tooltip
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerExit( PointerEventData eventData )
		{
			mIsHovered = false;
			mIsShown = false;
		}

		/// <summary>
		/// OnPointerEnter. We check if we can display the tooltip
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerEnter( PointerEventData eventData )
		{
			mIsHovered = true;
			CheckTooltip();
		}

		/// <summary>
		/// OnDestroy
		/// </summary>
		public void OnDestroy()
		{
			if ( mUIManager )
			{
				mUIManager.OnLeftShiftDown.RemoveListener( CheckTooltip );
				mUIManager.OnLeftShiftUp.RemoveListener( HideTooltip );
				mUIManager.OnLeftClickUp.RemoveListener( HideTooltip );
			}
		}

		[SerializeField, Range( -500f, 500f ), Tooltip( "Offset position from mouse pointer" )]
		private float mXOffset = 125.0f;

		[SerializeField, Range( -500f, 500f ), Tooltip( "Offset position from mouse pointer" )]
		private float mYOffset = -25f;

		/// <summary>
		/// Whether our tooltip is hovered
		/// </summary>
		private bool mIsHovered;

		/// <summary>
		/// Whether our tooltip is shown
		/// </summary>
		private bool mIsShown;

		/// <summary>
		/// Description of the tooltip
		/// </summary>
		[SerializeField, TextArea( minLines: 10, maxLines: 50 )]
		private string mDescription;

		/// <summary>
		/// Tooltip Manager
		/// </summary>
		private TooltipManager mTooltipManager;

		/// <summary>
		/// Offset of this tooltip (from mouse pointer)
		/// </summary>
		private Vector3 mOffsetPosition = Vector3.zero;

		/// <summary>
		/// Shown position of this tooltip
		/// </summary>
		private Vector3 mPosition = Vector3.zero;

		/// <summary>
		/// Reference to our UI Manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Checks to see if this tooltip is able to be shown
		/// </summary>
		private void CheckTooltip()
		{
			if ( mIsHovered && isActiveAndEnabled && mIsShown == false && mUIManager.LeftShiftIsDown )
			{
				ShowAndPositionTooltip(
					( description, position ) => { StartCoroutine( mTooltipManager.ShowAndPositionTooltip( description, position ) ); } );
			}
		}

		/// <summary>
		/// Shows and positions the tooltip
		/// </summary>
		/// <param name="onShow"></param>
		private void ShowAndPositionTooltip( Action<string, Vector3> onShow )
		{
			mIsShown = true;
			mOffsetPosition.x = ( Input.mousePosition.x > Screen.width / 2f ) ? -mXOffset : mXOffset;
			mOffsetPosition.y = ( Input.mousePosition.y > Screen.height / 2f ) ? -mYOffset : mYOffset;
			mPosition = Input.mousePosition + mOffsetPosition;
			onShow.Invoke( mDescription, mPosition );
		}

		/// <summary>
		/// Hides the tooltip
		/// </summary>
		private void HideTooltip()
		{
			mIsShown = false;
			mTooltipManager.HideTooltip();
		}
	}
}
using ProcGenMusic;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

/// <summary>
/// Tooltip object. Handles url clicking,
/// </summary>
public class UITooltipObject : MonoBehaviour
{
	/// <summary>
	/// Initialization
	/// </summary>
	/// <param name="uiManager"></param>
	public void Initialize( UIManager uiManager )
	{
		mUIManager = uiManager;
		mUIManager.OnLeftClickDown.AddListener( OnLeftClickDown );
	}

	/// <summary>
	/// Reference to our child rect transform
	/// </summary>
	public RectTransform ChildTransform => mChildTransform;

	/// <summary>
	/// Sets the tooltip text
	/// </summary>
	/// <param name="description"></param>
	public void SetText( string description )
	{
		mTooltipText.text = description;
	}

	[SerializeField, Tooltip( "Reference to our Tooltip Text" )]
	private TMP_Text mTooltipText;

	[SerializeField, Tooltip( "Reference to our Reference to our child rect transform" )]
	private RectTransform mChildTransform;

	/// <summary>
	/// Reference to our ui manager
	/// </summary>
	private UIManager mUIManager;

	/// <summary>
	/// On pointer click. For tooltips, this will check for hyperlink and open if possible
	/// </summary>
	private void OnLeftClickDown()
	{
		if ( gameObject.activeInHierarchy == false )
		{
			return;
		}

		// To keep it simple, just a single link per tooltip.
		if ( mTooltipText.textInfo.linkCount <= 0 )
		{
			return;
		}

		// Screen space overlay doesn't use a camera, so null is passed
		var linkIndex = TMP_TextUtilities.FindIntersectingLink( mTooltipText, Input.mousePosition, camera: null );
		if ( linkIndex != -1 )
		{
			var linkInfo = mTooltipText.textInfo.linkInfo[linkIndex];
			Input.ResetInputAxes();
			Application.OpenURL( linkInfo.GetLinkID() );
		}
	}

	/// <summary>
	/// OnDestroy
	/// </summary>
	private void OnDestroy()
	{
		if ( mUIManager )
		{
			mUIManager.OnLeftClickDown.RemoveListener( OnLeftClickDown );
		}
	}
}
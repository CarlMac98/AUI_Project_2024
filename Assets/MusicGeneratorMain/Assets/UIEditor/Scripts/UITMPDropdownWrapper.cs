using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;

namespace ProcGenMusic
{
	public class UITMPDropdownWrapper : TMP_Dropdown
	{
		public void ToggleShouldUpdatePosition( bool shouldUpdatePosition )
		{
			mShouldUpdatePosition = shouldUpdatePosition;
		}

		public override void OnPointerClick( PointerEventData eventData )
		{
			if ( mIsShowing )
			{
				return;
			}

			base.OnPointerClick( eventData );

			if ( options.Count <= 0 || mShouldUpdatePosition == false )
			{
				return;
			}

			// ugh, it drives me up the wall tmp doesn't cache any of this and creates/destroys the content each time.
			// Also, that this is necessary to have to do at all :/ 
			var itemTemplate = GetComponentInChildren<DropdownItem>();
			if ( itemTemplate != null )
			{
				var contentRectTransform = itemTemplate.rectTransform.parent.gameObject.transform as RectTransform;
				var itemSize = itemTemplate.rectTransform.rect.size;

				if ( contentRectTransform != null )
				{
					// We're re-anchoring the scroll content to our selected value.
					contentRectTransform.anchoredPosition = new Vector2( 0, value * itemSize.y );
				}
			}
		}

		protected override GameObject CreateDropdownList( GameObject templateDropdown )
		{
			mIsShowing = true;
			return base.CreateDropdownList( templateDropdown );
		}

		protected override void DestroyDropdownList( GameObject dropdownList )
		{
			mIsShowing = false;
			base.DestroyDropdownList( dropdownList );
		}

		private bool mIsShowing;
		private bool mShouldUpdatePosition;
	}
}
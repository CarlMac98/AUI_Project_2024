using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles the scroll view of instruments in the generator's display
	/// </summary>
	public class UIInstrumentListScrollView : MonoBehaviour
	{
		/// <summary>
		/// Returns instrument at index (or null)
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Instrument GetInstrument( int index )
		{
			var instrumentIndex = 0;
			foreach ( var groupInstrumentRect in mGroupInstrumentRects )
			{
				var childCount = groupInstrumentRect.childCount;
				if ( childCount == 0 )
				{
					continue;
				}

				if ( instrumentIndex + childCount <= index )
				{
					instrumentIndex += childCount;
				}
				else
				{
					return groupInstrumentRect.GetChild( index - instrumentIndex ).GetComponent<InstrumentListUIObject>().Instrument;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns whether we're currently over any instrument's group drag button. Returns group index or -1
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public int IsOverGroup( Vector3 position )
		{
			for ( var index = 0; index < mGroupDropRects.Length; index++ )
			{
				Vector2 localMousePosition = mGroupDropRects[index].InverseTransformPoint( position );
				if ( mGroupDropRects[index].rect.Contains( localMousePosition ) )
				{
					return index;
				}
			}

			return -1;
		}

		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name="uiManager"></param>
		public void Initialize( UIManager uiManager )
		{
			mInstrumentListPanelUI = uiManager.InstrumentListPanelUI;
			mUIManager = uiManager;
			for ( var index = 0; index < mManualGroupOverrides.Length; index++ )
			{
				var groupIndex = index;
				mManualGroupOverrides[index]
					.Initialize( ( value ) =>
						{
							if ( value && mUIManager.MusicGenerator.GroupsAreTemporarilyOverriden == false )
							{
								ResetManualGroups();
							}

							mUIManager.CurrentInstrumentSet.OverrideGroupIsPlaying( groupIndex, value );

							// if we just disabled the last one, we re-enable the first index so something is still playing
							if ( value == false && HasManualGroupOverride() == false )
							{
								mUIManager.CurrentInstrumentSet.OverrideGroupIsPlaying( 0, true );
							}
						},
						initialValue: false );
			}
		}

		/// <summary>
		/// Resets all of the rect groups to their proper size
		/// </summary>
		public void ResetRectGroups()
		{
			var childCount = 1;
			foreach ( var group in mGroupInstrumentRects )
			{
				childCount += group.childCount;
			}

			float width = 0;
			if ( childCount > 1 )
			{
				width = mInstrumentListPanelUI.GetIconWidth();
			}

			mScrollRect.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, childCount * width );

			for ( var index = 0; index < mGroupInstrumentRects.Length; index++ )
			{
				mGroupInstrumentRects[index].SetSizeWithCurrentAnchors(
					RectTransform.Axis.Horizontal,
					mGroupInstrumentRects[index].childCount * width +
					mGroupLayoutGroups[index].padding.left +
					mGroupLayoutGroups[index].padding.right +
					( mGroupInstrumentRects[index].childCount - 1 ) * mGroupLayoutGroups[index].spacing
				);
			}
		}

		/// <summary>
		/// sets all group overrides to false. We do this when toggling _on_ an override for the first time to ensure we update the existing groups
		/// </summary>
		public void DisableAllManualGroups()
		{
			for ( var index = 0; index < mManualGroupOverrides.Length; index++ )
			{
				mManualGroupOverrides[index].Option.isOn = false;
				mUIManager.CurrentInstrumentSet.OverrideGroupIsPlaying( index, false );
			}
		}

		/// <summary>
		/// Returns the rect transform for a given group index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public RectTransform GetGroupRect( int index )
		{
			if ( index >= 0 && index < mGroupInstrumentRects.Length )
			{
				return mGroupInstrumentRects[index];
			}

			return null;
		}

		/// <summary>
		/// Sets the scroll view visible state
		/// </summary>
		/// <param name="isVisible"></param>
		/// <param name="currentlyPlayingGroups"></param>
		public void SetVisibility( bool isVisible, bool[] currentlyPlayingGroups )
		{
			mAnimator.SetBool( mAnimIsVisibleParameter, isVisible );
			var isOverridden = mUIManager && mUIManager.MusicGenerator.GroupsAreTemporarilyOverriden;
			if ( isVisible )
			{
				for ( var index = 0; index < currentlyPlayingGroups.Length; index++ )
				{
					mManualGroupOverrides[index].Option.isOn = currentlyPlayingGroups[index] && isOverridden;
				}
			}
		}

		/// <summary>
		/// Returns whether any group override is manually toggled on
		/// </summary>
		/// <returns></returns>
		public bool HasManualGroupOverride()
		{
			foreach ( var groupOverride in mManualGroupOverrides )
			{
				if ( groupOverride.Option.isOn )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Reference to our instrument list panel ui
		/// </summary>
		private InstrumentListPanelUI mInstrumentListPanelUI;

		[SerializeField, Tooltip( "Reference to our scroll rect for the list scroll view" )]
		private RectTransform mScrollRect;

		[SerializeField, Tooltip( "Reference to our rect transforms for the instruments " )]
		private RectTransform[] mGroupInstrumentRects;

		[SerializeField, Tooltip( "Reference to our Rect transforms for the group drop areas" )]
		private RectTransform[] mGroupDropRects;

		[SerializeField, Tooltip( "Reference to our Horizontal layout groups for the groups" )]
		private HorizontalLayoutGroup[] mGroupLayoutGroups;

		[SerializeField, Tooltip( "Reference to our scroll view animator" )]
		private Animator mAnimator;

		[SerializeField, Tooltip( "Reference to our IsVisible animation parameter" )]
		private string mAnimIsVisibleParameter = "IsVisible";

		[Tooltip( "Reference to our manual group override toggles" )]
		[SerializeField] private UIToggle[] mManualGroupOverrides;

		/// <summary>
		/// sets all group overrides to false. We do this when toggling _on_ an override for the first time to ensure we update the existing groups
		/// </summary>
		private void ResetManualGroups()
		{
			for ( var index = 0; index < mManualGroupOverrides.Length; index++ )
			{
				mUIManager.CurrentInstrumentSet.OverrideGroupIsPlaying( index, false );
			}
		}
	}
}
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class UIHorizontalScrollBar : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Initializes the horizontal scroll bar
		/// </summary>
		/// <param name="action"></param>
		/// <param name="uiManager"></param>
		public void Initialize( UnityAction action, UIManager uiManager )
		{
			mUIManager = uiManager;
			mOnValueChanged.RemoveAllListeners();
			mOnValueChanged.AddListener( action );
		}

		/// <summary>
		/// Sets the scrollbar active state
		/// </summary>
		/// <param name="isActive"></param>
		public void SetActive( bool isActive )
		{
			mIsActive = isActive;
		}

		/// <summary>
		/// Event handler for mouse down
		/// </summary>
		public void OnMouseDown()
		{
			mIsDragging = true;
			mPreviousPosition = mUIManager.MouseWorldPoint;
		}

		/// <summary>
		/// Event handler for mouse up
		/// </summary>
		public void OnMouseUp()
		{
			mIsDragging = false;
			mOnValueChanged.Invoke();
		}

		/// <summary>
		/// Event handler for mouse over
		/// </summary>
		public void OnMouseOver()
		{
			if ( mIsDragging )
			{
				return;
			}

			var scrollDelta = Input.mouseScrollDelta.y;
			mScrollVelocity += scrollDelta * mScrollWheelMultiplier;
		}

		/// <summary>
		/// Updates the end point of where we're allowed to scroll
		/// </summary>
		/// <param name="endTransform"></param>
		public void UpdateScrollEndTransform( Transform endTransform )
		{
			mParentEnd = endTransform;
		}

		#endregion public

		#region private

		[SerializeField, Tooltip( "Reference to the scrollbar transform" )]
		private Transform mTransform;

		[SerializeField, Tooltip( "Multiplier for scroll wheel movement" ), Range( 0f, 1000f )]
		private float mScrollWheelMultiplier;

		[SerializeField, Tooltip( "Drag for scroll velocity" ), Range( 0f, 1f )]
		private float mScrollDrag = .99f;

		[SerializeField, Tooltip( "Reference to the scrollbar parent start point" )]
		private Transform mParentStart;

		[SerializeField, Tooltip( "Reference to the scrollbar parent end point" )]
		private Transform mParentEnd;

		[SerializeField, Tooltip( "Reference to the scrollbar scroll start point" )]
		private Transform mScrollStart;

		[SerializeField, Tooltip( "Reference to the scrollbar scroll end point" )]
		private Transform mScrollEnd;

		/// <summary>
		/// Event for handling value changed
		/// </summary>
		private readonly UnityEvent mOnValueChanged = new UnityEvent();

		/// <summary>
		/// Current drag state of the bar
		/// </summary>
		private bool mIsDragging;

		/// <summary>
		/// Reference to the ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Previous position of the bar
		/// </summary>
		private Vector2 mPreviousPosition;

		/// <summary>
		/// Current scroll velocity
		/// </summary>
		private float mScrollVelocity;

		/// <summary>
		/// Active state
		/// </summary>
		private bool mIsActive;

		/// <summary>
		/// Update
		/// </summary>
		private void Update()
		{
			if ( mIsActive == false )
			{
				return;
			}

			if ( mIsDragging )
			{
				Drag();
			}
			else if ( mScrollVelocity != 0 )
			{
				Scroll();
			}
		}

		/// <summary>
		/// Drags the scrollbar
		/// </summary>
		private void Drag()
		{
			var xDelta = mUIManager.MouseWorldPoint.x - mPreviousPosition.x;

			if ( mParentStart.position.x + xDelta < mScrollEnd.position.x &&
			     mParentEnd.position.x + xDelta > mScrollStart.position.x )
			{
				var position = mTransform.position;
				position.x += xDelta;
				mTransform.position = position;
			}

			mPreviousPosition = mUIManager.MouseWorldPoint;
		}

		/// <summary>
		/// Handles scrolling the horizontal bar. 
		/// </summary>
		private void Scroll()
		{
			var drag = 1f - mScrollDrag;
			mScrollVelocity -= mScrollVelocity / drag * Time.deltaTime;

			if ( mScrollVelocity < .1f && mScrollVelocity > -1f )
			{
				mScrollVelocity = 0f;
				mOnValueChanged.Invoke();
			}
			else if ( mParentStart.position.x + mScrollVelocity < mScrollEnd.position.x &&
			          mParentEnd.position.x + mScrollVelocity > mScrollStart.position.x )
			{
				var position = mTransform.position;
				position.x += mScrollVelocity;
				mTransform.position = position;
			}
		}

		#endregion private
	}
}
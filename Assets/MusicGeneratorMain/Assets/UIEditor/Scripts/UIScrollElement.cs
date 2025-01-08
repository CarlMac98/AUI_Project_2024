using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI Element to handle scroll panel
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class UIScrollElement<T> : MonoBehaviour, IPointerClickHandler, IScrollHandler where T : IEquatable<T>
	{
		#region public

		/// <summary>
		/// Generic event for our scroller
		/// </summary>
		public class TEvent : UnityEvent<T> { }

		/// <summary>
		/// Current index of our scroll list
		/// </summary>
		public int SelectedIndex { get; private set; }

		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name="elements"></param>
		/// <param name="initialIndex"></param>
		public void Initialize( T[] elements, int initialIndex )
		{
			mScrollElement = elements;
			SelectedIndex = initialIndex;
			mValue = mScrollElement[SelectedIndex];
		}

		/// <summary>
		/// On Value Changed event
		/// </summary>
		public TEvent OnValueChanged { get; } = new TEvent();

		/// <summary>
		/// Our Generic value
		/// </summary>
		public T Value
		{
			get => mValue;
			set
			{
				mValue = value;
				UpdateIndex( value );
				OnValueChanged.Invoke( mValue );
			}
		}


		/// <summary>
		/// On Scroll
		/// </summary>
		/// <param name="eventData"></param>
		public void OnScroll( PointerEventData eventData )
		{
			mScrollDelta += Input.mouseScrollDelta.y;
			if ( mScrollDelta > mMinScroll )
			{
				Scroll( 1 );
			}
			else if ( mScrollDelta < -mMinScroll )
			{
				Scroll( -1 );
			}
		}

		public void OnPointerClick( PointerEventData eventData )
		{
			if ( eventData.pointerPress )
			{
				Scroll( 1 );
			}
		}

		#endregion public

		#region protected

		[SerializeField, Tooltip( "Reference to our array of elements" )]
		protected T[] mScrollElement;

		[SerializeField, Tooltip( "Minimum scroll delta value to count as a scroll event" )]
		protected float mMinScroll = .1f;

		#endregion protected

		#region private

		/// <summary>
		/// Scroll delta value
		/// </summary>
		private float mScrollDelta;

		/// <summary>
		/// Current value of the scroller
		/// </summary>
		private T mValue;

		/// <summary>
		/// Handles scroll logic
		/// </summary>
		/// <param name="delta"></param>
		private void Scroll( int delta )
		{
			SelectedIndex = MusicConstants.SafeLoop( SelectedIndex + delta, 0, mScrollElement.Length );
			mValue = mScrollElement[SelectedIndex];
			OnValueChanged.Invoke( mValue );
			mScrollDelta = 0;
		}

		/// <summary>
		/// Updates our elected index
		/// </summary>
		/// <param name="value"></param>
		private void UpdateIndex( T value )
		{
			for ( var index = 0; index < mScrollElement.Length; index++ )
			{
				if ( mScrollElement[index].Equals( value ) )
				{
					SelectedIndex = index;
				}
			}
		}

		#endregion private
	}
}
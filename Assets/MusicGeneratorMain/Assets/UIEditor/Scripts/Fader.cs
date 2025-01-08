using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles fading logic.
	/// </summary>
	public abstract class Fader : MonoBehaviour
	{
		/// <summary>
		/// Sets the fade state to idle.
		/// </summary>
		public void Hide()
		{
			mFadeValue = mMinValue;
			mFadeState = FadeState.Idle;
		}

		/// <summary>
		/// Sets the fade state to max value and idle
		/// </summary>
		public void Show()
		{
			mFadeValue = mMaxValue;
			mFadeState = FadeState.Idle;
		}

		/// <summary>
		/// Sets the fade state to fade in
		/// </summary>
		public void FadeIn()
		{
			mFadeState = FadeState.In;
		}

		/// <summary>
		/// sets the fade state to fade out
		/// </summary>
		public void FadeOut()
		{
			mFadeState = FadeState.Out;
		}

		/// <summary>
		/// State of this faded object
		/// </summary>
		protected enum FadeState
		{
			Idle,
			In,
			Out
		}

		/// <summary>
		/// Updates our fade state
		/// </summary>
		protected virtual void UpdateFadeState()
		{
			switch ( mFadeState )
			{
				case FadeState.In:
					FadeValueIn();
					break;
				case FadeState.Out:
					FadeValueOut();
					break;
			}
		}

		/// <summary>
		/// State of this faded object
		/// </summary>
		[SerializeField]
		protected FadeState mFadeState;

		/// <summary>
		/// Current fade value
		/// </summary>
		[SerializeField] protected float mFadeValue;

		/// <summary>
		/// Max fade value
		/// </summary>
		[SerializeField] protected float mMaxValue;

		/// <summary>
		/// Multiplier against our fade rate
		/// </summary>
		[SerializeField, Range( .1f, 100f )] private float mTotalFadeTime = 1f;

		/// <summary>
		/// Minimum fade value
		/// </summary>
		[SerializeField] protected float mMinValue;

		/// <summary>
		/// Fades our value in
		/// </summary>
		private void FadeValueIn()
		{
			if ( ( mFadeValue < mMaxValue ) == false )
			{
				mFadeState = FadeState.Idle;
				return;
			}

			mMaxValue = Mathf.Max( mMaxValue, mMinValue );
			var fadeDistance = mMaxValue - mMinValue;
			var fadeTime = fadeDistance / mTotalFadeTime;
			mFadeValue = Mathf.Clamp( mFadeValue + ( fadeTime * Time.deltaTime ), mMinValue, mMaxValue );

			if ( mFadeValue >= mMaxValue )
			{
				mFadeValue = mMaxValue;
				mFadeState = FadeState.Idle;
			}
		}

		/// <summary>
		/// Fades our value out
		/// </summary>
		private void FadeValueOut()
		{
			if ( ( mFadeValue > mMinValue ) == false )
			{
				return;
			}

			mMaxValue = Mathf.Max( mMaxValue, mMinValue );
			var fadeDistance = mMaxValue - mMinValue;
			var fadeTime = fadeDistance / mTotalFadeTime;
			mFadeValue = Mathf.Clamp( mFadeValue - ( fadeTime * Time.deltaTime ), mMinValue, mMaxValue );

			if ( mFadeValue <= mMinValue )
			{
				mFadeValue = mMinValue;
				mFadeState = FadeState.Idle;
			}
		}

		/// <summary>
		/// Just update :P
		/// </summary>
		private void Update()
		{
			UpdateFadeState();
		}
	}
}
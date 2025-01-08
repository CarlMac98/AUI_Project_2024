	using UnityEngine;

#pragma warning disable 0649
#pragma warning disable 0414

namespace ProcGenMusic
{
	public class DBVisualizer : MonoBehaviour
	{
		/// <summary>
		/// Sets our color
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( Color color )
		{
			mFader.SetColor( color );
			mVisualObject.material.color = color;
		}

		/// <summary>
		/// Toggles our visibility
		/// </summary>
		/// <param name="isOn"></param>
		public void ToggleFadeState( bool isOn )
		{
			if ( isOn )
			{
				mFader.FadeIn();
			}
			else
			{
				mFader.FadeOut();
			}
		}

		[SerializeField, Tooltip( "Reference to our visual object for dB" )]
		private SpriteRenderer mVisualObject;

		[SerializeField, Range( .1f, 1f ), Tooltip( "Fall rate of the visual pulse" )]
		private float mFallRate = .3f;

		[SerializeField, Range( 1f, 3f ), Tooltip( "Visual pulse multiplier" )]
		private float mMultiplier = 3f;

		[SerializeField, Tooltip( "Reference to our sprite fader" )]
		private SpriteFade mFader;

		/// <summary>
		/// Current scale
		/// </summary>
		private float mScale = 1f;

		/// <summary>
		/// db min
		/// </summary>
		private const float MIN = -80f;

		/// <summary>
		/// db max
		/// </summary>
		private const float MAX = 20f;

		/// <summary>
		/// Update.
		/// </summary>
		private void Update()
		{
#if FMOD_ENABLED == false
			var normal = Mathf.Clamp( ( AudioVisualizer.DBValue - MIN ) / ( MAX - MIN ), 0f, 1f );

			var scale = normal;
			if ( scale < mScale )
			{
				mScale -= Time.deltaTime * mFallRate;
			}
			else
			{
				mScale = scale;
			}

			mVisualObject.transform.localScale = Vector3.one + Vector3.one * ( mScale * mMultiplier );
#endif //FMOD_ENABLED == false
		}
	}
}

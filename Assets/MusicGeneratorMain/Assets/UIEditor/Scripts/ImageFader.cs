using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles fading images in/out
	/// </summary>
	public class ImageFader : Fader
	{
		///<inheritdoc/>
		protected override void UpdateFadeState()
		{
			mColor = mImage.material.color;
			base.UpdateFadeState();
			mColor.a = mFadeValue;
			mImage.material.color = mColor;
		}

		/// <summary>
		/// Awake!
		/// </summary>
		private void Awake()
		{
			mColor = mImage.material.color;
		}

		/// <summary>
		/// Reference to our image
		/// </summary>
		[Tooltip( "Reference to our image to fade" )]
		[SerializeField] private Image mImage;

		/// <summary>
		/// Current Color for the Image
		/// </summary>
		private Color mColor;
	}
}
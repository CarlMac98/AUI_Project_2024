using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles fading images in/out
	/// </summary>
	public class LineFader : Fader
	{
		///<inheritdoc/>
		protected override void UpdateFadeState()
		{
			mColor = mLineRenderer.startColor;
			base.UpdateFadeState();
			mColor.a = mFadeValue;
			mLineRenderer.startColor = mColor;
			mLineRenderer.endColor = mColor;
		}

		/// <summary>
		/// Awake!
		/// </summary>
		private void Awake()
		{
			mColor = mLineRenderer.startColor;
		}

		/// <summary>
		/// Reference to our image
		/// </summary>
		[Tooltip( "Reference to our line renderer to fade" )]
		[SerializeField] private LineRenderer mLineRenderer;

		/// <summary>
		/// Current Color for the Image
		/// </summary>
		private Color mColor;
	}
}
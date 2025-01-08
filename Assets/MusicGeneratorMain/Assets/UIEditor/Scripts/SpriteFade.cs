using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Sprite Fade handles fading in/out sprites
	/// </summary>
	public class SpriteFade : Fader
	{
		/// <summary>
		/// Sets the color
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( Color color )
		{
			mColor = color;
		}

		///<inheritdoc/>
		protected override void UpdateFadeState()
		{
			base.UpdateFadeState();
			mColor.a = mFadeValue;
			mMaterial.color = mColor;
		}

		/// <summary>
		/// Awake
		/// </summary>
		private void Awake()
		{
			mMaterial = mSpriteRenderer.material;
			mColor = mMaterial.color;
		}

		[SerializeField, Tooltip( "Reference to our sprite renderer" )]
		private SpriteRenderer mSpriteRenderer;

		/// <summary>
		/// Our color to fade.
		/// </summary>
		private Color mColor;

		/// <summary>
		/// Reference to our sprite material
		/// </summary>
		private Material mMaterial;
	}
}
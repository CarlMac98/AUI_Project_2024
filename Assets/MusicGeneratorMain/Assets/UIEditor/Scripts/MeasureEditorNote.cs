using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Note for the Measure Editor. Handles size,color, visibility, etc
	/// </summary>
	public class MeasureEditorNote : MonoBehaviour
	{
		/// <summary>
		/// Resizes our editor note
		/// </summary>
		/// <param name="size"></param>
		public void Resize( Vector2 size )
		{
			mSpriteRenderer.size = size;
		}

		/// <summary>
		/// Sets the color for the editor note
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( Color color )
		{
			mSpriteRenderer.color = color;
		}

		/// <summary>
		/// Hides the editor note
		/// </summary>
		public void Hide()
		{
			mColorFade.Hide();
		}

		/// <summary>
		/// Shows the editor note
		/// </summary>
		public void Show()
		{
			mColorFade.Show();
		}

		/// <summary>
		/// Fades the editor note in
		/// </summary>
		public void FadeIn()
		{
			mColorFade.FadeIn();
		}

		/// <summary>
		/// Fades the editor note out
		/// </summary>
		public void FadeOut()
		{
			mColorFade.FadeOut();
		}

		/// <summary>
		/// Getter for our transform
		/// </summary>
		public Transform Transform => mTransform;

		[SerializeField, Tooltip( "Reference to our Sprite Renderer for the Editor note" )]
		private SpriteRenderer mSpriteRenderer;

		[SerializeField, Tooltip( "Reference to our Transform for the editor note" )]
		private Transform mTransform;

		[SerializeField, Tooltip( "Reference to our Color fader for the editor note" )]
		private Fader mColorFade;
	}
}
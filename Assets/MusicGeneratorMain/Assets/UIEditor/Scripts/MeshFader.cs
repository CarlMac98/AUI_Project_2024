using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles fading images in/out
	/// </summary>
	public class MeshFader : Fader
	{
		public void SetEmissionIntensity( float intensity )
		{
			mEmissionIntensity = intensity;
		}

		///<inheritdoc/>
		protected override void UpdateFadeState()
		{
			if ( mFadeState == FadeState.Idle )
			{
				return;
			}

			base.UpdateFadeState();
			mColor = mMeshRenderer.sharedMaterial.color;
			mColor.a = mFadeValue;
			mMaterial.color = mColor;

			//ugh, negative value here is wrong.
			var multiplier = Mathf.LinearToGammaSpace( mFadeValue * mEmissionIntensity );
			mMeshRenderer.sharedMaterial.SetColor( EmissionColorID, mColor * multiplier );
		}

		/// <summary>
		/// Awake!
		/// </summary>
		private void Awake()
		{
			mMaterial = mMeshRenderer.sharedMaterial;
			mColor = mMaterial.color;
		}

		/// <summary>
		/// Reference to our image
		/// </summary>
		[Tooltip( "Reference to our image to fade" )]
		[SerializeField] private MeshRenderer mMeshRenderer;

		private Material mMaterial;

		/// <summary>
		/// Current Color for the Image
		/// </summary>
		private Color mColor;

		private float mEmissionIntensity = 1f;

		private static readonly int EmissionColorID = Shader.PropertyToID( "_EmissionColor" );
	}
}
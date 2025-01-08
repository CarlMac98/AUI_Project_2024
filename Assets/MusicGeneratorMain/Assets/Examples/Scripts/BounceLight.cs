using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
	public class BounceLight : MonoBehaviour
	{
		public void Play()
		{
			mLight.intensity = mStartIntensity;
			mEmissionMultiplier = mStartIntensity;
			mMeshRenderer.material.SetColor(EmissionColor, mColor * Mathf.LinearToGammaSpace(mEmissionMultiplier));
		}

		[SerializeField]
		private Light mLight;

		[SerializeField]
		private float mStartIntensity;

		[SerializeField]
		private MeshRenderer mMeshRenderer;

		[SerializeField]
		private Color mColor;

		[SerializeField]
		private float mEmissionMultiplier = 1;

		[SerializeField]
		private float mColorDelta = 1f;

		[SerializeField]
		private float mEmissionDelta = 1f;

		private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
		private MusicGenerator mMusicGenerator;
		private int minstrumentIndex;

		private void Update()
		{
			if (mLight.intensity > 0)
			{
				mLight.intensity -= mColorDelta * Time.deltaTime;
			}

			if (mEmissionMultiplier > 0)
			{
				mMeshRenderer.material.SetColor(EmissionColor, mColor * Mathf.LinearToGammaSpace(mEmissionMultiplier));
				mEmissionMultiplier -= Time.deltaTime * mEmissionDelta;
			}
		}
	}
}
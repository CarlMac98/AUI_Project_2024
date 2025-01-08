using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
	public class WaterObject : MonoBehaviour
	{
		public void CreateRipple(Vector3 point, Color color)
		{
			var thisPosition = mWaterTransform.position;

			var distanceX = point.x - thisPosition.x;
			var distanceZ = thisPosition.z - point.z;

			var percentDistanceX = 1f / (mLocalScale.x / distanceX);
			var percentDistanceZ = 1f / (mLocalScale.y / distanceZ);
			var position = new Vector3(percentDistanceX, percentDistanceZ, 0);

			mMaterial.SetVector(mRippleOriginID[mCurrentRipple], position);
			mMaterial.SetColor(mColorID[mCurrentRipple], color);
			mAmplitude[mCurrentRipple] = mBaseAmplitude;
			mDistance[mCurrentRipple] = 0;
			mCurrentRipple++;

			if (mCurrentRipple >= mRipplePoolSize)
			{
				mCurrentRipple = 0;
			}
		}

		[SerializeField]
		private MeshRenderer mMeshRenderer;

		[SerializeField]
		private float mBaseAmplitude;

		[SerializeField]
		private float mSpeed = 1;

		[SerializeField]
		private float mAmplitudeMultiplier = 1;

		[SerializeField]
		private int mRipplePoolSize = 10;

		[SerializeField]
		private Transform mWaterTransform;

		private Material mMaterial;
		private Vector3 mLocalScale;
		private float[] mAmplitude;
		private float[] mDistance;
		private int[] mAmplitudeID;
		private int[] mDistanceID;
		private int[] mRippleOriginID;
		private int[] mColorID;
		private int mCurrentRipple;

		private void Awake()
		{
			mMaterial = mMeshRenderer.material;
			mAmplitude = new float[mRipplePoolSize];
			mDistance = new float[mRipplePoolSize];

			mAmplitudeID = new int[mRipplePoolSize];
			mDistanceID = new int[mRipplePoolSize];
			mRippleOriginID = new int[mRipplePoolSize];
			mColorID = new int[mRipplePoolSize];

			for (var index = 0; index < mRipplePoolSize; index++)
			{
				mAmplitudeID[index] = Shader.PropertyToID($"_Amplitude_{index}");
				mDistanceID[index] = Shader.PropertyToID($"_Distance_{index}");
				mRippleOriginID[index] = Shader.PropertyToID($"_RippleOrigin_{index}");
				mColorID[index] = Shader.PropertyToID($"_GradientColor_{index}");
			}

			mLocalScale = mWaterTransform.localScale;
		}

		private void Update()
		{
			for (var index = 0; index < mRipplePoolSize; index++)
			{
				if (mAmplitude[index] <= 0)
				{
					continue;
				}

				mDistance[index] += Time.deltaTime * mSpeed;
				mAmplitude[index] -= Time.deltaTime * mAmplitudeMultiplier;
				mAmplitude[index] = Mathf.Max(0, mAmplitude[index] - Time.deltaTime * mAmplitudeMultiplier);

				mMaterial.SetFloat(mAmplitudeID[index], mAmplitude[index]);
				mMaterial.SetFloat(mDistanceID[index], mDistance[index]);
			}
		}
	}
}
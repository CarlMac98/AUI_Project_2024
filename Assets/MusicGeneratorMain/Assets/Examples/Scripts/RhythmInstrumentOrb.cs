using UnityEngine;
using UnityEngine.Serialization;

namespace ProcGenMusic.ExampleScene
{
	/// <summary>
	/// Manages the 'bouncing orbs' in the scene 
	/// </summary>
	public class RhythmInstrumentOrb : MonoBehaviour
	{
		public void Initialize(MusicGenerator musicGenerator, int instrumentIndex, WaterObject waterObject)
		{
			mInstrumentIndex = instrumentIndex;
			mMusicGenerator = musicGenerator;
			mInstrumentHandler.Initialize(mMusicGenerator, mInstrumentIndex);
			gameObject.SetActive(false);
			mWaterObject = waterObject;
		}

		public void Stop()
		{
			gameObject.SetActive(false);
		}

		public void Play(Vector3 position, int instrumentIndex, Color color)
		{
			if (mMusicGenerator == false)
			{
				return;
			}

			mInstrumentHandler.SetInstrumentIndex(instrumentIndex);
			mFloor = position;
			gameObject.SetActive(true);
			mColor = color;

			var timeStep = mMusicGenerator.InstrumentSet.GetInverseProgressionRate((int)mMusicGenerator.InstrumentSet.Instruments[instrumentIndex].InstrumentData.TimeStep);
			mBounceLength = mMusicGenerator.InstrumentSet.BeatLength * timeStep;
			var apex = FallDistanceOverTime(mBounceLength / 2f);
			mVelocity = VelocityToHitPointWithSetApex(apex);
			mElapsedTime = 0;
			PlayBounce();
		}

		[SerializeField]
		private Rigidbody mRigidbody;

		[SerializeField]
		private BounceLight mBounceLight;

		[SerializeField]
		private MusicGenerator mMusicGenerator;

		[SerializeField]
		private int mInstrumentIndex = 1;

		[FormerlySerializedAs("mWaterInputHandler")]
		[SerializeField]
		private WaterObject mWaterObject;

		private Vector3 mFloor;
		private Vector3 mVelocity;
		private readonly InstrumentHandler mInstrumentHandler = new InstrumentHandler();
		private float mBounceLength = 5f;
		private float mElapsedTime;
		private Color mColor;

		private void Update()
		{
			var dt = Time.deltaTime;
			mElapsedTime += dt;
			if (mElapsedTime >= mBounceLength)
			{
				mElapsedTime = mElapsedTime - mBounceLength;
				PlayBounce();
			}
		}

		private void PlayBounce()
		{
			mRigidbody.MovePosition(mFloor);
			mRigidbody.linearVelocity = mVelocity;
			mBounceLight.Play();
			mInstrumentHandler.PlayNote();
			mWaterObject.CreateRipple(mFloor, mColor);
		}

		private float FallDistanceOverTime(float time)
		{
			return 0.5f * -Physics.gravity.y * (time * time);
		}

		private Vector3 VelocityToHitPointWithSetApex(float apex)
		{
			var velocity = Vector3.up;
			velocity.y = Mathf.Sqrt(2.0f * -Physics.gravity.y * apex);
			return velocity;
		}
	}
}
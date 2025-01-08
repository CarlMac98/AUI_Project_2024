using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
	public class ClickPad : MonoBehaviour
	{
		[SerializeField]
		private Collider mCollider;

		[SerializeField]
		private ParticleSystem[] mParticlePool;

		[SerializeField]
		private InputHandler mInputHandler;

		[SerializeField]
		private MusicGenerator mMusicGenerator;

		[SerializeField]
		private Material mPadMaterial;

		[SerializeField, ColorUsage(true, true)]
		private Color mLeftClickPadColor;

		[SerializeField, ColorUsage(true, true)]
		private Color mRightClickPadColor;

		private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

		[SerializeField]
		private int[] mInstrumentIndices;

		private InstrumentHandler[] mInstrumentHandlers;

		private int mCurrentIndex;

		private void Awake()
		{
			mInputHandler.LeftClickDown.AddListener(OnLeftClick);
			mInputHandler.RightClickDown.AddListener(OnRightClick);
			mInputHandler.MiddleClickDown.AddListener(OnMiddleClick);
			mInstrumentHandlers = new InstrumentHandler[mInstrumentIndices.Length];

			for (var index = 0; index < mInstrumentHandlers.Length; index++)
			{
				mInstrumentHandlers[index] = new InstrumentHandler();
				mInstrumentHandlers[index].Initialize(mMusicGenerator, mInstrumentIndices[index]);
			}
		}

		private void OnLeftClick()
		{
			if (mInstrumentHandlers.Length > 0)
			{
				OnClick(mLeftClickPadColor, mInstrumentHandlers[0]);
			}
		}

		private void OnRightClick()
		{
			if (mInstrumentHandlers.Length > 1)
			{
				OnClick(mRightClickPadColor, mInstrumentHandlers[1]);
			}
		}

		private void OnMiddleClick()
		{
			if (mInstrumentHandlers.Length > 2)
			{
				OnClick(mRightClickPadColor, mInstrumentHandlers[2]);
			}
		}

		private void OnClick(Color color, InstrumentHandler instrumentHandler)
		{
			if (mInputHandler.HitInfo.collider &&
			    mInputHandler.HitInfo.transform == null ||
			    mInputHandler.HitInfo.collider != mCollider)
			{
				return;
			}

			mPadMaterial.SetColor(EmissionColorID, color);
			mPadMaterial.color = color;
			mParticlePool[mCurrentIndex].transform.position = mInputHandler.HitInfo.point;
			mParticlePool[mCurrentIndex].time = 0;
			mParticlePool[mCurrentIndex].Play();
			mCurrentIndex = mCurrentIndex < mParticlePool.Length - 1 ? mCurrentIndex + 1 : 0;
			instrumentHandler.PlayNote();
		}
	}
}
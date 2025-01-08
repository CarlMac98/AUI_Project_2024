using TMPro;
using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
	public abstract class MusicHandle : MonoBehaviour
	{
#region protected

		protected abstract int HandleTypeLength { get; }
		protected abstract string GetHandleText(int handleType);
		protected abstract void UpdateHandleType(int handleType);
		protected abstract float GetInitialHandleAngle();
		protected abstract string GetInitialHandleText();

		[SerializeField]
		protected MusicGenerator mMusicGenerator;

		[SerializeField]
		protected MusicGeneratorHandler mMusicGeneratorHandler;

		[SerializeField]
		protected float mRotationMax = 45f;

		protected int mHandleTypeLength;

#endregion // protected

#region private

		[SerializeField]
		private Transform mHandleTransform;

		[SerializeField]
		private Collider mHandleCollider;

		[SerializeField]
		private TMP_Text mHandleText;

		[SerializeField]
		private InputHandler mInputHandler;

		[SerializeField]
		private Camera mCamera;

		private bool mIsClicking;

		private void Awake()
		{
			mHandleTypeLength = HandleTypeLength;
			mMusicGenerator.Ready.AddListener(OnMusicGeneratorReady);
			mMusicGenerator.StateSet.AddListener(OnMusicGeneratorStateSet);
			mInputHandler.LeftClickDown.AddListener(OnLeftClickDown);
			mInputHandler.LeftClickUp.AddListener(OnLeftClickUp);
			mInputHandler.LeftClickIsDown.AddListener(OnLeftClickIsDown);
		}

		private void OnLeftClickIsDown()
		{
			if (mIsClicking == false)
			{
				return;
			}

			var ray = mCamera.ScreenPointToRay(Input.mousePosition);
			var position = mHandleTransform.position;
			var hitPoint = ray.GetPoint(Vector3.Distance(mCamera.gameObject.transform.position, position));
			mHandleTransform.rotation = Quaternion.LookRotation(Vector3.forward, hitPoint - position);

			UpdateText();
		}

		private void OnLeftClickUp()
		{
			if (mIsClicking == false)
			{
				return;
			}

			mIsClicking = false;
			UpdateText();
		}

		private void UpdateText()
		{
			mHandleTransform.rotation.ToAngleAxis(out var angle, out var axis);
			angle = Mathf.Clamp(angle, -mRotationMax, mRotationMax) * axis.z;
			var handleType = mHandleTypeLength - ((mHandleTypeLength) * (mRotationMax + angle) / (mRotationMax * 2));
			handleType = Mathf.Clamp(handleType, 0, mHandleTypeLength - 1);

			UpdateHandleType((int)Mathf.Round(handleType));

			mHandleTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			mHandleText.SetText(GetHandleText((int)handleType));
		}

		private void OnLeftClickDown()
		{
			if (mInputHandler.HitInfo.collider &&
			    mInputHandler.HitInfo.collider == mHandleCollider)
			{
				mIsClicking = true;
			}
		}

		private void OnMusicGeneratorStateSet(GeneratorState state)
		{
			if (state != GeneratorState.Playing)
			{
				return;
			}

			InitializeHandleAngle();
		}

		private void OnMusicGeneratorReady()
		{
			InitializeHandleAngle();
		}

		private void InitializeHandleAngle()
		{
			var angle = GetInitialHandleAngle();
			mHandleTransform.rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
			mHandleText.SetText(GetInitialHandleText());
		}

#endregion //private
	}
}
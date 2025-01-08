using UnityEngine;
using UnityEngine.Events;

namespace ProcGenMusic
{
	public class InputHandler : MonoBehaviour
	{
		[SerializeField]
		private Camera mCamera;

		public UnityEvent LeftClickDown => mLeftClickDown;
		public UnityEvent LeftClickUp => mLeftClickUp;
		public UnityEvent LeftClickIsDown => mLeftClickIsDown;

		public UnityEvent RightClickDown => mRightClickDown;
		public UnityEvent MiddleClickDown => mMiddleClickDown;
		public UnityEvent DoubleLeftClick => mDoubleLeftClick;

		public RaycastHit HitInfo => mHitInfo;

		public Vector3 MouseWorldPos => mMouseWorldPos;

		private readonly UnityEvent mLeftClickDown = new UnityEvent();
		private readonly UnityEvent mLeftClickIsDown = new UnityEvent();
		private readonly UnityEvent mLeftClickUp = new UnityEvent();
		private readonly UnityEvent mRightClickDown = new UnityEvent();
		private readonly UnityEvent mMiddleClickDown = new UnityEvent();
		private readonly UnityEvent mDoubleLeftClick = new UnityEvent();

		private RaycastHit mHitInfo;

		private Vector3 mMouseWorldPos;
		private float mDoubleClickTimer;

		[SerializeField]
		private float mDoubleClickThreshold = .25f;

		private void Update()
		{
			mMouseWorldPos = mCamera.ScreenToWorldPoint(Input.mousePosition);
			mDoubleClickTimer += Time.deltaTime;

			if (Input.GetMouseButtonDown(0))
			{
				if (mCamera == false)
				{
					return;
				}

				Physics.Raycast(mCamera.ScreenPointToRay(Input.mousePosition), out mHitInfo);

				if (mDoubleClickTimer < mDoubleClickThreshold)
				{
					mDoubleLeftClick.Invoke();
				}

				mLeftClickDown.Invoke();

				mDoubleClickTimer = 0;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				if (mCamera == false)
				{
					return;
				}

				Physics.Raycast(mCamera.ScreenPointToRay(Input.mousePosition), out mHitInfo);
				mLeftClickUp.Invoke();
			}
			else if (Input.GetMouseButton(0))
			{
				if (mCamera == false)
				{
					return;
				}

				Physics.Raycast(mCamera.ScreenPointToRay(Input.mousePosition), out mHitInfo);
				mLeftClickIsDown.Invoke();
			}

			if (Input.GetMouseButtonDown(1))
			{
				if (mCamera == false)
				{
					return;
				}

				Physics.Raycast(mCamera.ScreenPointToRay(Input.mousePosition), out mHitInfo);
				mRightClickDown.Invoke();
			}

			if (Input.GetMouseButtonDown(2))
			{
				if (mCamera == false)
				{
					return;
				}

				Physics.Raycast(mCamera.ScreenPointToRay(Input.mousePosition), out mHitInfo);
				mMiddleClickDown.Invoke();
			}
		}
	}
}
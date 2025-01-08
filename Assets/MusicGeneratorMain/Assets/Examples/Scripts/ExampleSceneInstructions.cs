using UnityEngine;
using UnityEngine.UI;

namespace ProcGenMusic.ExampleScene
{
	public class ExampleSceneInstructions : MonoBehaviour
	{
		[SerializeField]
		private CanvasGroup mCanvasGroup;

		[SerializeField]
		private float mFadeTime = 1f;

		[SerializeField]
		private Button mButton;

		[SerializeField]
		private float mDisplayTime = 3f;

		private float mElapsedTime;
		private float mVisibleTime;

		private bool mIsHidden;

		private void Awake()
		{
			mButton.onClick.AddListener(OnClick);
		}

		private void Update()
		{
			var dt = Time.deltaTime;
			mVisibleTime += dt;
			if (mIsHidden == false && mVisibleTime > mDisplayTime == false)
			{
				return;
			}

			mElapsedTime += dt;
			var alpha = 1 - (mElapsedTime / mFadeTime);
			if (alpha <= 0)
			{
				gameObject.SetActive(false);
			}

			mCanvasGroup.alpha = alpha;
		}

		private void OnClick()
		{
			mIsHidden = true;
		}
	}
}
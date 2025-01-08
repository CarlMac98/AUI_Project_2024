using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
	public class UndoHandle : MonoBehaviour
	{
		[SerializeField]
		private InputHandler mInputHandler;

		[SerializeField]
		private RhythmInstrumentOrbManager mRhythmInstrumentOrbManager;

		private void Awake()
		{
			mInputHandler.LeftClickDown.AddListener(OnLeftClickDown);
		}

		private void OnLeftClickDown()
		{
			if (mInputHandler.HitInfo.collider && mInputHandler.HitInfo.collider.gameObject != gameObject)
			{
				return;
			}

			mRhythmInstrumentOrbManager.Undo();
		}
	}
}
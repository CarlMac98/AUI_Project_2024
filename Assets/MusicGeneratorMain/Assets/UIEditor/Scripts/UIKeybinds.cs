using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	[CreateAssetMenu( fileName = "Keybinds", menuName = "ProcGenMusic/Editor/Keybinds", order = 1 )]
	public class UIKeybinds : ScriptableObject
	{
		[SerializeField]
		private KeyCode mPlayPause = KeyCode.RightArrow;

		public KeyCode PlayPause => mPlayPause;

		[SerializeField]
		private KeyCode mStop = KeyCode.LeftArrow;

		public KeyCode Stop => mStop;

		[SerializeField]
		private KeyCode mSoloSelected = KeyCode.S;

		public KeyCode SoloSelected => mSoloSelected;

		[SerializeField]
		private KeyCode mToggleFX = KeyCode.F;

		public KeyCode ToggleFX => mToggleFX;

		[SerializeField]
		private KeyCode mToggleInstrumentPercussion = KeyCode.I;

		public KeyCode ToggleInstrumentPercussion => mToggleInstrumentPercussion;

		[SerializeField]
		private KeyCode mMuteSelected = KeyCode.M;

		public KeyCode MuteSelected => mMuteSelected;

		[SerializeField]
		private KeyCode[] mInstruments;

		public KeyCode[] Instruments => mInstruments;
	}
}
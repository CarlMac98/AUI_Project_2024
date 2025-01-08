using UnityEngine;
using UnityEngine.UI;

namespace ProcGenMusic
{
	public class UIToggleGroup : MonoBehaviour
	{
		public UIToggle[] Toggles => mToggles;

		[SerializeField]
		private UIToggle[] mToggles;

		[SerializeField]
		private ToggleGroup mToggleGroup;

		private void Awake()
		{
			foreach ( var toggle in mToggles )
			{
				mToggleGroup.RegisterToggle( toggle.Option );
			}
		}
	}
}
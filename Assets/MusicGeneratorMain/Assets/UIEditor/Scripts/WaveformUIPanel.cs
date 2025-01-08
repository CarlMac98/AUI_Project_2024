using JetBrains.Annotations;
using UnityEngine;

namespace ProcGenMusic
{
	public class WaveformUIPanel : MonoBehaviour
	{
		public void Initialize( [NotNull] UIManager uiManager )
		{
			mUIManager = uiManager;
			mColorSlider.Initialize( value => { mOutputVisualizer.SetColor( mUIManager.Colors[( int )value] ); },
				mUIManager.FXSettings.mWaveformColor,
				resetValue: 0,
				createDividers: true );

			mWidthSlider.Initialize( value =>
				{
					mOutputVisualizer.SetWidth( value );
					mWidthSlider.Text.text = $"{value:0.00}";
				},
				mUIManager.FXSettings.mWaveformWidth );
		}

		public void SetVisibility( bool isVisible )
		{
			mVisibleObject.SetActive( isVisible );
		}

		[SerializeField]
		private GameObject mVisibleObject;

		[SerializeField]
		private UISlider mColorSlider;

		[SerializeField]
		private UISlider mWidthSlider;

		[SerializeField]
		private OutputVisualizer mOutputVisualizer;

		private UIManager mUIManager;
	}
}
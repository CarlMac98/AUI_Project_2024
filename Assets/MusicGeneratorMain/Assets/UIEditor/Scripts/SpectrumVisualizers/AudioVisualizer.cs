using UnityEngine;

#pragma warning disable 0649
#pragma warning disable 0414

namespace ProcGenMusic
{
	public class AudioVisualizer : MonoBehaviour
	{
		/// <summary>
		///  Our sample size for the spectrum and output visualizers
		/// </summary>
		public static int SAMPLE_SIZE = 2048;

		public void ToggleWaveformVisibility( bool isVisible )
		{
			mWaveformVisibility = isVisible;
		}

		public void ToggleGeneralVisibility( bool isVisible )
		{
			mGeneralVisibility = isVisible;
			mVisibleObject.SetActive( isVisible );
		}

		public void DirtyVisuals()
		{
			var uiStyle = mUIManager.FXSettings.UIStyle;
			var useVisualizers = uiStyle == UIEditorFXSettings.UIEditorStyle.VisualizerOnly ||
			                     uiStyle == UIEditorFXSettings.UIEditorStyle.PianoRollAndVisualizer;
			mSpectrumVisualizer.SetColor( mUIManager.CurrentColors[(int) ColorFieldType.UI_1].Color );

			mDBVisualizer.SetColor( mUIManager.CurrentColors[(int) ColorFieldType.UI_3].Color );

			ToggleVisualizers( useVisualizers );
		}

		public void FadeOut()
		{
			mDBVisualizer.ToggleFadeState( false );
			mSpectrumVisualizer.ToggleFadeState( false );
		}

		public void FadeIn()
		{
			mDBVisualizer.ToggleFadeState( true );
			mSpectrumVisualizer.ToggleFadeState( true );
		}

		/// <summary>
		/// Array of our current spectrum data
		/// </summary>
		public static float[] SpectrumData { get; } = new float[SAMPLE_SIZE];

		/// <summary>
		/// Array of our current output data
		/// </summary>
		public static float[] OutputData { get; } = new float[SAMPLE_SIZE];

		/// <summary>
		/// Current dB Value
		/// </summary>
		public static float DBValue { get; private set; }

		private bool mGeneralVisibility;
		private bool mWaveformVisibility;

		[SerializeField]
		private GameObject mVisibleObject;

		[SerializeField, Tooltip( "dB Scale" )]
		private float mDbScale = 0.1f;

		[SerializeField, Tooltip( "Reference to our ui manager" )]
		private UIManager mUIManager;

		[SerializeField, Tooltip( "Reference to our spectrum visualizer" )]
		private SpectrumVisualizer mSpectrumVisualizer;

		[SerializeField, Tooltip( "Reference to our dB Visualizer" )]
		private DBVisualizer mDBVisualizer;

		[SerializeField, Tooltip( "Reference to our output visualizer" )]
		private OutputVisualizer mOutputVisualizer;

		private static float mRmsValue;

		/// <summary>
		/// Awake.
		/// </summary>
		private void Awake()
		{
			DirtyVisuals();
		}

		/// <summary>
		/// Update.
		/// </summary>
		private void Update()
		{
			if ( mWaveformVisibility == false &&
			     mGeneralVisibility == false )
			{
				return;
			}

#if FMOD_ENABLED == false
			AudioListener.GetSpectrumData( SpectrumData, 0, FFTWindow.Hamming );
			AudioListener.GetOutputData( OutputData, 0 );

			var sum = 0f;
			foreach ( var t in OutputData )
			{
				sum += t * t;
			}

			if ( OutputData.Length > 0 )
			{
				mRmsValue = Mathf.Sqrt( sum / OutputData.Length );
				DBValue = 20f * Mathf.Log10( mRmsValue / mDbScale );
			}
#endif //FMOD_ENABLED == false
		}

		private void ToggleVisualizers( bool isActive )
		{
			mSpectrumVisualizer.gameObject.SetActive( isActive );
			mDBVisualizer.gameObject.SetActive( isActive );
		}
	}
}

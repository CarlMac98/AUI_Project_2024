using UnityEngine;
using UnityEngine.Serialization;

namespace ProcGenMusic
{
	public class FmodInstrumentFXPanelUI : MonoBehaviour
	{
#if FMOD_ENABLED
        public void InitializeFXPanel( Instrument instrument )
        {
            mInstrument = instrument;
            mDelayFXToggle.Initialize( ( value ) => { mDelayFXPanel.SetActive( value ); }, initialValue: false );
            mReverbFXToggle.Initialize( ( value ) => { mReverbFXPanel.SetActive( value ); }, initialValue: false );
            mFlangeFXToggle.Initialize( ( value ) => { mFlangeFXPanel.SetActive( value ); }, initialValue: false );
            mDistortionFXToggle.Initialize( ( value ) => { mDistortionFXPanel.SetActive( value ); }, initialValue: false );
            mChorusFXToggle.Initialize( ( value ) => { mChorusFXPanel.SetActive( value ); }, initialValue: false );
            mEQFXToggle.Initialize( ( value ) => { mEQFXPanel.SetActive( value ); }, initialValue: false );

            mFlangerRate.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodFlangerRate = value;
                    mFlangerRate.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodFlangerRateName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodFlangerRate,
                resetValue: MusicConstants.BaseFmodFlangerRate );

            mFlangerDepth.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodFlangerDepth = value;
                    mFlangerDepth.Text.text = $"{value + cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodFlangerDepthName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodFlangerDepth,
                resetValue: MusicConstants.BaseFmodFlangerDepth );

            mFlangerMix.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodFlangerMix = value;
                    mFlangerMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodFlangerMixName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodFlangerMix,
                resetValue: MusicConstants.BaseFmodFlangerMix );

            mDelay.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodDelay = value;
                    mDelay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodDelayName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodDelay,
                resetValue: MusicConstants.BaseFmodDelay );

            mDelayWet.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodDelayWet = value;
                    mDelayWet.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodDelayWetName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodDelayWet,
                resetValue: MusicConstants.BaseFmodDelayWet );

            mDelayFeedback.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodDelayFeedback = value;
                    mDelayFeedback.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodDelayFeedbackName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodDelayFeedback,
                resetValue: MusicConstants.BaseFmodDelayFeedback );

            mDelayDry.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodDelayDry = value;
                    mDelayDry.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodDelayDryName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodDelayDry,
                resetValue: MusicConstants.BaseFmodDelayDry );

            mReverbTime.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbTime = value;
                    mReverbTime.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbTimeName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbTime,
                resetValue: MusicConstants.BaseFmodReverbTime );

            mReverbHFDecay.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbHFDecay = value;
                    mReverbHFDecay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbHFDecayName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbHFDecay,
                resetValue: MusicConstants.BaseFmodReverbHFDecay );

            mReverbDiffusion.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbDiffusion = value;
                    mReverbDiffusion.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbDiffusionName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbDiffusion,
                resetValue: MusicConstants.BaseFmodReverbDiffusion );

            mReverbLowGain.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbLowGain = value;
                    mReverbLowGain.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbLowGainName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbLowGain,
                resetValue: MusicConstants.BaseFmodReverbLowGain );

            mReverbHighCut.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbHighCut = value;
                    mReverbHighCut.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbHighCutName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbHighCut,
                resetValue: MusicConstants.BaseFmodReverbHighCut );

            mReverbWetMix.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbWetMix = value;
                    mReverbWetMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbWetMixName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbWetMix,
                resetValue: MusicConstants.BaseFmodReverbWetMix );

            mReverbDryMix.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbDryMix = value;
                    mReverbDryMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbDryMixName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbDryMix,
                resetValue: MusicConstants.BaseFmodReverbDryMix );

            mReverbEarlyDelay.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbEarlyDelay = value;
                    mReverbEarlyDelay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbEarlyDelayName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbEarlyDelay,
                resetValue: MusicConstants.BaseFmodReverbEarlyDelay );

            mReverbLateDelay.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbLateDelay = value;
                    mReverbLateDelay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbLateDelayName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbLateDelay,
                resetValue: MusicConstants.BaseFmodReverbLateDelay );

            mReverbHFReference.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbHFReference = value;
                    mReverbHFReference.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbHFReferenceName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbHFReference,
                resetValue: MusicConstants.BaseFmodReverbHFReference );

            mReverbDensity.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbDensity = value;
                    mReverbDensity.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbDensityName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbDensity,
                resetValue: MusicConstants.BaseFmodReverbDensity );

            mReverbLowFreq.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbLowFreq = value;
                    mReverbLowFreq.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbLowFreqName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbLowFreq,
                resetValue: MusicConstants.BaseFmodReverbLowFreq );

            mReverbEarlyLateMix.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodReverbEarlyLateMix = value;
                    mReverbEarlyLateMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodReverbEarlyLateMixName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodReverbEarlyLateMix,
                resetValue: MusicConstants.BaseFmodReverbEarlyLateMix );

            mDistortion.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodDistortion = value;
                    mDistortion.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodDistortionName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodDistortion,
                resetValue: MusicConstants.BaseFmodDistortion );

            mChorusRate.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodChorusRate = value;
                    mChorusRate.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodChorusRateName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodChorusRate,
                resetValue: MusicConstants.BaseFmodChorusRate );

            mChorusDepth.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodChorusDepth = value;
                    mChorusDepth.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodChorusDepthName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodChorusDepth,
                resetValue: MusicConstants.BaseFmodChorusDepth );

            mChorusMix.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodChorusMix = value;
                    mChorusMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodChorusMixName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodChorusMix,
                resetValue: MusicConstants.BaseFmodChorusMix );

            mEQLow.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodEQLow = value;
                    mEQLow.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodEQLowName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodEQLow,
                resetValue: MusicConstants.BaseFmodEQLow );

            mEQMid.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodEQMid = value;
                    mEQMid.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodEQMidName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodEQMid,
                resetValue: MusicConstants.BaseFmodEQMid );

            mEQHigh.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodEQHigh = value;
                    mEQHigh.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodEQHighName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodEQHigh,
                resetValue: MusicConstants.BaseFmodEQHigh );

            mEQXLow.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodEQXLow = value;
                    mEQXLow.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodEQXLowName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodEQXLow,
                resetValue: MusicConstants.BaseFmodEQXLow );

            mEQXHigh.Initialize( ( value ) =>
                {
                    mInstrument.InstrumentData.FmodEQXHigh = value;
                    mEQXHigh.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodEQXHighName, mInstrument.InstrumentIndex, value );
                },
                mInstrument.InstrumentData.FmodEQXHigh,
                resetValue: MusicConstants.BaseFmodEQXHigh );
        }

        public void UpdateUIElementValues( Instrument instrument )
        {
            mInstrument = instrument;
            mFlangerRate.Option.value = mInstrument.InstrumentData.FmodFlangerRate;
            mFlangerDepth.Option.value = mInstrument.InstrumentData.FmodFlangerDepth;
            mFlangerMix.Option.value = mInstrument.InstrumentData.FmodFlangerMix;
            mDelay.Option.value = mInstrument.InstrumentData.FmodDelay;
            mDelayWet.Option.value = mInstrument.InstrumentData.FmodDelayWet;
            mDelayFeedback.Option.value = mInstrument.InstrumentData.FmodDelayFeedback;
            mDelayDry.Option.value = mInstrument.InstrumentData.FmodDelayDry;
            mReverbTime.Option.value = mInstrument.InstrumentData.FmodReverbTime;
            mReverbHFDecay.Option.value = mInstrument.InstrumentData.FmodReverbHFDecay;
            mReverbDiffusion.Option.value = mInstrument.InstrumentData.FmodReverbDiffusion;
            mReverbLowGain.Option.value = mInstrument.InstrumentData.FmodReverbLowGain;
            mReverbHighCut.Option.value = mInstrument.InstrumentData.FmodReverbHighCut;
            mReverbWetMix.Option.value = mInstrument.InstrumentData.FmodReverbWetMix;
            mReverbDryMix.Option.value = mInstrument.InstrumentData.FmodReverbDryMix;
            mReverbEarlyDelay.Option.value = mInstrument.InstrumentData.FmodReverbEarlyDelay;
            mReverbLateDelay.Option.value = mInstrument.InstrumentData.FmodReverbLateDelay;
            mReverbHFReference.Option.value = mInstrument.InstrumentData.FmodReverbHFReference;
            mReverbDensity.Option.value = mInstrument.InstrumentData.FmodReverbDensity;
            mReverbLowFreq.Option.value = mInstrument.InstrumentData.FmodReverbLowFreq;
            mReverbEarlyLateMix.Option.value = mInstrument.InstrumentData.FmodReverbEarlyLateMix;
            mDistortion.Option.value = mInstrument.InstrumentData.FmodDistortion;
            mChorusRate.Option.value = mInstrument.InstrumentData.FmodChorusRate;
            mChorusDepth.Option.value = mInstrument.InstrumentData.FmodChorusDepth;
            mChorusMix.Option.value = mInstrument.InstrumentData.FmodChorusMix;
            mEQLow.Option.value = mInstrument.InstrumentData.FmodEQLow;
            mEQMid.Option.value = mInstrument.InstrumentData.FmodEQMid;
            mEQHigh.Option.value = mInstrument.InstrumentData.FmodEQHigh;
            mEQXLow.Option.value = mInstrument.InstrumentData.FmodEQXLow;
            mEQXHigh.Option.value = mInstrument.InstrumentData.FmodEQXHigh;
        }
#endif //FMOD_ENABLED

		private const float cUnNormalizedConversion = 100f;

		private Instrument mInstrument;

		[SerializeField]
		private MusicGenerator mMusicGenerator;

		[FormerlySerializedAs( "mEchoFXToggle" )]
		[SerializeField, Tooltip( "Reference to our Echo FX Toggle" )]
		private UIToggle mDelayFXToggle;

		[FormerlySerializedAs( "mEchoFXPanel" )]
		[SerializeField, Tooltip( "Reference to our echo fx panel" )]
		private GameObject mDelayFXPanel;

		[SerializeField, Tooltip( "Reference to our Reverb FX Toggle" )]
		private UIToggle mReverbFXToggle;

		[SerializeField, Tooltip( "Reference to our reverb fx panel" )]
		private GameObject mReverbFXPanel;

		[SerializeField, Tooltip( "Reference to our Flange FX Toggle" )]
		private UIToggle mFlangeFXToggle;

		[SerializeField, Tooltip( "Reference to our flange fx panel" )]
		private GameObject mFlangeFXPanel;

		[SerializeField, Tooltip( "Reference to our Distortion FX Toggle" )]
		private UIToggle mDistortionFXToggle;

		[SerializeField, Tooltip( "Reference to our distortion fx panel" )]
		private GameObject mDistortionFXPanel;

		[SerializeField, Tooltip( "Reference to our Chorus FX Toggle" )]
		private UIToggle mChorusFXToggle;

		[SerializeField, Tooltip( "Reference to our chorus fx panel" )]
		private GameObject mChorusFXPanel;

		[SerializeField, Tooltip( "Reference to our EQ FX Toggle" )]
		private UIToggle mEQFXToggle;

		[SerializeField, Tooltip( "Reference to our eq fx panel" )]
		private GameObject mEQFXPanel;

		[SerializeField, Tooltip( "FMOD FlangerRate Value" )]
		private UISlider mFlangerRate;

		[SerializeField, Tooltip( "FMOD FlangerDepth Value" )]
		private UISlider mFlangerDepth;

		[SerializeField, Tooltip( "FMOD FlangerMix Value" )]
		private UISlider mFlangerMix;

		[SerializeField, Tooltip( "FMOD Delay Value" )]
		private UISlider mDelay;

		[SerializeField, Tooltip( "FMOD DelayWet Value" )]
		private UISlider mDelayWet;

		[SerializeField, Tooltip( "FMOD DelayFeedback Value" )]
		private UISlider mDelayFeedback;

		[SerializeField, Tooltip( "FMOD DelayDry Value" )]
		private UISlider mDelayDry;

		[SerializeField, Tooltip( "FMOD ReverbTime Value" )]
		private UISlider mReverbTime;

		[SerializeField, Tooltip( "FMOD ReverbHFDecay Value" )]
		private UISlider mReverbHFDecay;

		[SerializeField, Tooltip( "FMOD ReverbDiffusion Value" )]
		private UISlider mReverbDiffusion;

		[SerializeField, Tooltip( "FMOD ReverbLowGain Value" )]
		private UISlider mReverbLowGain;

		[SerializeField, Tooltip( "FMOD ReverbHighCut Value" )]
		private UISlider mReverbHighCut;

		[SerializeField, Tooltip( "FMOD ReverbWetMix Value" )]
		private UISlider mReverbWetMix;

		[SerializeField, Tooltip( "FMOD ReverbDryMix Value" )]
		private UISlider mReverbDryMix;

		[SerializeField, Tooltip( "FMOD ReverbEarlyDelay Value" )]
		private UISlider mReverbEarlyDelay;

		[SerializeField, Tooltip( "FMOD ReverbLateDelay Value" )]
		private UISlider mReverbLateDelay;

		[SerializeField, Tooltip( "FMOD ReverbHFReference Value" )]
		private UISlider mReverbHFReference;

		[SerializeField, Tooltip( "FMOD ReverbDensity Value" )]
		private UISlider mReverbDensity;

		[SerializeField, Tooltip( "FMOD ReverbLowFreq Value" )]
		private UISlider mReverbLowFreq;

		[SerializeField, Tooltip( "FMOD ReverbEarlyLateMix Value" )]
		private UISlider mReverbEarlyLateMix;

		[SerializeField, Tooltip( "FMOD Distortion Value" )]
		private UISlider mDistortion;

		[SerializeField, Tooltip( "FMOD ChorusRate Value" )]
		private UISlider mChorusRate;

		[SerializeField, Tooltip( "FMOD ChorusDepth Value" )]
		private UISlider mChorusDepth;

		[SerializeField, Tooltip( "FMOD ChorusMix Value" )]
		private UISlider mChorusMix;

		[SerializeField, Tooltip( "FMOD EQLow Value" )]
		private UISlider mEQLow;

		[SerializeField, Tooltip( "FMOD EQMid Value" )]
		private UISlider mEQMid;

		[SerializeField, Tooltip( "FMOD EQHigh Value" )]
		private UISlider mEQHigh;

		[SerializeField, Tooltip( "FMOD EQXLow Value" )]
		private UISlider mEQXLow;

		[SerializeField, Tooltip( "FMOD EQXHigh Value" )]
		private UISlider mEQXHigh;
	}
}
using UnityEngine;
using UnityEngine.Serialization;

namespace ProcGenMusic
{
    public class FmodMasterFXPanelUI : MonoBehaviour
    {
#if FMOD_ENABLED
        public void InitializeFXPanel( MusicGenerator musicGenerator )
        {
            mMusicGenerator = musicGenerator;
            mDelayFXToggle.Initialize( ( value ) => { mDelayFXPanel.SetActive( value ); }, initialValue: false );
            mReverbFXToggle.Initialize( ( value ) => { mReverbFXPanel.SetActive( value ); }, initialValue: false );
            mFlangeFXToggle.Initialize( ( value ) => { mFlangeFXPanel.SetActive( value ); }, initialValue: false );
            mDistortionFXToggle.Initialize( ( value ) => { mDistortionFXPanel.SetActive( value ); }, initialValue: false );
            mChorusFXToggle.Initialize( ( value ) => { mChorusFXPanel.SetActive( value ); }, initialValue: false );
            mEQFXToggle.Initialize( ( value ) => { mEQFXPanel.SetActive( value ); }, initialValue: false );

            mFlangerRate.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterFlangerRate = value;
                    mFlangerRate.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterFlangerRateName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterFlangerRate,
                resetValue: MusicConstants.BaseFmodFlangerRate );

            mFlangerDepth.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterFlangerDepth = value;
                    mFlangerDepth.Text.text = $"{value + cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterFlangerDepthName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterFlangerDepth,
                resetValue: MusicConstants.BaseFmodFlangerDepth );

            mFlangerMix.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterFlangerMix = value;
                    mFlangerMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterFlangerMixName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterFlangerMix,
                resetValue: MusicConstants.BaseFmodFlangerMix );

            mDelay.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterDelay = value;
                    mDelay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterDelayName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterDelay,
                resetValue: MusicConstants.BaseFmodDelay );

            mDelayWet.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterDelayWet = value;
                    mDelayWet.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterDelayWetName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterDelayWet,
                resetValue: MusicConstants.BaseFmodDelayWet );

            mDelayFeedback.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterDelayFeedback = value;
                    mDelayFeedback.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterDelayFeedbackName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterDelayFeedback,
                resetValue: MusicConstants.BaseFmodDelayFeedback );

            mDelayDry.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterDelayDry = value;
                    mDelayDry.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterDelayDryName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterDelayDry,
                resetValue: MusicConstants.BaseFmodDelayDry );

            mReverbTime.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbTime = value;
                    mReverbTime.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbTimeName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbTime,
                resetValue: MusicConstants.BaseFmodReverbTime );

            mReverbHFDecay.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbHFDecay = value;
                    mReverbHFDecay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbHFDecayName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbHFDecay,
                resetValue: MusicConstants.BaseFmodReverbHFDecay );

            mReverbDiffusion.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbDiffusion = value;
                    mReverbDiffusion.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbDiffusionName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbDiffusion,
                resetValue: MusicConstants.BaseFmodReverbDiffusion );

            mReverbLowGain.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbLowGain = value;
                    mReverbLowGain.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbLowGainName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbLowGain,
                resetValue: MusicConstants.BaseFmodReverbLowGain );

            mReverbHighCut.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbHighCut = value;
                    mReverbHighCut.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbHighCutName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbHighCut,
                resetValue: MusicConstants.BaseFmodReverbHighCut );

            mReverbWetMix.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbWetMix = value;
                    mReverbWetMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbWetMixName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbWetMix,
                resetValue: MusicConstants.BaseFmodReverbWetMix );

            mReverbDryMix.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbDryMix = value;
                    mReverbDryMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbDryMixName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbDryMix,
                resetValue: MusicConstants.BaseFmodReverbDryMix );

            mReverbEarlyDelay.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyDelay = value;
                    mReverbEarlyDelay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbEarlyDelayName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyDelay,
                resetValue: MusicConstants.BaseFmodReverbEarlyDelay );

            mReverbLateDelay.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbLateDelay = value;
                    mReverbLateDelay.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbLateDelayName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbLateDelay,
                resetValue: MusicConstants.BaseFmodReverbLateDelay );

            mReverbHFReference.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbHFReference = value;
                    mReverbHFReference.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbHFReferenceName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbHFReference,
                resetValue: MusicConstants.BaseFmodReverbHFReference );

            mReverbDensity.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbDensity = value;
                    mReverbDensity.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbDensityName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbDensity,
                resetValue: MusicConstants.BaseFmodReverbDensity );

            mReverbLowFreq.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbLowFreq = value;
                    mReverbLowFreq.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbLowFreqName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbLowFreq,
                resetValue: MusicConstants.BaseFmodReverbLowFreq );

            mReverbEarlyLateMix.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyLateMix = value;
                    mReverbEarlyLateMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterReverbEarlyLateMixName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyLateMix,
                resetValue: MusicConstants.BaseFmodReverbEarlyLateMix );

            mDistortion.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterDistortion = value;
                    mDistortion.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterDistortionName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterDistortion,
                resetValue: MusicConstants.BaseFmodDistortion );

            mChorusRate.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterChorusRate = value;
                    mChorusRate.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterChorusRateName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterChorusRate,
                resetValue: MusicConstants.BaseFmodChorusRate );

            mChorusDepth.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterChorusDepth = value;
                    mChorusDepth.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterChorusDepthName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterChorusDepth,
                resetValue: MusicConstants.BaseFmodChorusDepth );

            mChorusMix.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterChorusMix = value;
                    mChorusMix.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterChorusMixName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterChorusMix,
                resetValue: MusicConstants.BaseFmodChorusMix );

            mEQLow.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterEQLow = value;
                    mEQLow.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterEQLowName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterEQLow,
                resetValue: MusicConstants.BaseFmodEQLow );

            mEQMid.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterEQMid = value;
                    mEQMid.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterEQMidName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterEQMid,
                resetValue: MusicConstants.BaseFmodEQMid );

            mEQHigh.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterEQHigh = value;
                    mEQHigh.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterEQHighName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterEQHigh,
                resetValue: MusicConstants.BaseFmodEQHigh );

            mEQXLow.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterEQXLow = value;
                    mEQXLow.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterEQXLowName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterEQXLow,
                resetValue: MusicConstants.BaseFmodEQXLow );

            mEQXHigh.Initialize( ( value ) =>
                {
                    mMusicGenerator.ConfigurationData.FmodMasterEQXHigh = value;
                    mEQXHigh.Text.text = $"{value * cUnNormalizedConversion:0.00}%";
                    mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterEQXHighName, parameterValue: value );
                },
                mMusicGenerator.ConfigurationData.FmodMasterEQXHigh,
                resetValue: MusicConstants.BaseFmodEQXHigh );
        }

        public void UpdateUIElementValues()
        {
            mFlangerRate.Option.value = mMusicGenerator.ConfigurationData.FmodMasterFlangerRate;
            mFlangerDepth.Option.value = mMusicGenerator.ConfigurationData.FmodMasterFlangerDepth;
            mFlangerMix.Option.value = mMusicGenerator.ConfigurationData.FmodMasterFlangerMix;
            mDelay.Option.value = mMusicGenerator.ConfigurationData.FmodMasterDelay;
            mDelayWet.Option.value = mMusicGenerator.ConfigurationData.FmodMasterDelayWet;
            mDelayFeedback.Option.value = mMusicGenerator.ConfigurationData.FmodMasterDelayFeedback;
            mDelayDry.Option.value = mMusicGenerator.ConfigurationData.FmodMasterDelayDry;
            mReverbTime.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbTime;
            mReverbHFDecay.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbHFDecay;
            mReverbDiffusion.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbDiffusion;
            mReverbLowGain.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbLowGain;
            mReverbHighCut.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbHighCut;
            mReverbWetMix.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbWetMix;
            mReverbDryMix.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbDryMix;
            mReverbEarlyDelay.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyDelay;
            mReverbLateDelay.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbLateDelay;
            mReverbHFReference.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbHFReference;
            mReverbDensity.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbDensity;
            mReverbLowFreq.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbLowFreq;
            mReverbEarlyLateMix.Option.value = mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyLateMix;
            mDistortion.Option.value = mMusicGenerator.ConfigurationData.FmodMasterDistortion;
            mChorusRate.Option.value = mMusicGenerator.ConfigurationData.FmodMasterChorusRate;
            mChorusDepth.Option.value = mMusicGenerator.ConfigurationData.FmodMasterChorusDepth;
            mChorusMix.Option.value = mMusicGenerator.ConfigurationData.FmodMasterChorusMix;
            mEQLow.Option.value = mMusicGenerator.ConfigurationData.FmodMasterEQLow;
            mEQMid.Option.value = mMusicGenerator.ConfigurationData.FmodMasterEQMid;
            mEQHigh.Option.value = mMusicGenerator.ConfigurationData.FmodMasterEQHigh;
            mEQXLow.Option.value = mMusicGenerator.ConfigurationData.FmodMasterEQXLow;
            mEQXHigh.Option.value = mMusicGenerator.ConfigurationData.FmodMasterEQXHigh;
        }
#endif //FMOD_ENABLED

        private const float cUnNormalizedConversion = 100f;

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
using UnityEngine;

namespace ProcGenMusic
{
	public class InstrumentFXPanelUI : MonoBehaviour
	{
		public void InitializeFXPanel( Instrument instrument )
		{
			mInstrument = instrument;
			mEchoFXToggle.Initialize( ( value ) => { mEchoFXPanel.SetActive( value ); }, initialValue: false );
			mReverbFXToggle.Initialize( ( value ) => { mReverbFXPanel.SetActive( value ); }, initialValue: false );
			mFlangeFXToggle.Initialize( ( value ) => { mFlangeFXPanel.SetActive( value ); }, initialValue: false );
			mDistortionFXToggle.Initialize( ( value ) => { mDistortionFXPanel.SetActive( value ); }, initialValue: false );
			mChorusFXToggle.Initialize( ( value ) => { mChorusFXPanel.SetActive( value ); }, initialValue: false );
			mEQFXToggle.Initialize( ( value ) => { mEQFXPanel.SetActive( value ); }, initialValue: false );

#region protected.reverb

			mReverb.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.Reverb = value;
					mReverb.Text.text = $"{value:0.00}mB";
					mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.MixerReverbName + mInstrument.InstrumentIndex, parameterValue: value );
				},
				mInstrument.InstrumentData.Reverb,
				resetValue: MusicConstants.BaseReverb );

			mRoomSize.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.RoomSize = value;
					mRoomSize.Text.text = $"{value:0.00}mB";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerRoomSizeName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.RoomSize,
				resetValue: MusicConstants.BaseReverbRoom );

			mReverbDry.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbDry = value;
					mReverbDry.Text.text = $"{value:0.00}mB";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbDryName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbDry,
				resetValue: MusicConstants.BaseReverbDry );

			mReverbDelay.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbDelay = value;
					mReverbDelay.Text.text = $"{value:0.00}s";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbDelayName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbDelay,
				resetValue: MusicConstants.BaseReverbDelay );


			mReverbRoomHF.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbRoomHF = value;
					mReverbRoomHF.Text.text = $"{value:0.00}mB";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbRoomHFName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbRoomHF,
				resetValue: MusicConstants.BaseReverbRoomHF );

			mReverbDecayTime.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbDecayTime = value;
					mReverbDecayTime.Text.text = $"{value:0.00}s";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbDecayTimeName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbDecayTime,
				resetValue: MusicConstants.BaseReverbDecayTime );

			mReverbDecayHFRatio.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbDecayHFRatio = value;
					mReverbDecayHFRatio.Text.text = $"{value:0.00}";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbDecayHFRatioName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbDecayTime,
				resetValue: MusicConstants.BaseReverbDecayHFRatio );

			mReverbReflections.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbReflections = value;
					mReverbReflections.Text.text = $"{value:0.00}mB";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbReflectionsName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbReflections,
				resetValue: MusicConstants.BaseReverbReflections );

			mReverbReflectDelay.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbReflectDelay = value;
					mReverbReflectDelay.Text.text = $"{value:0.00}s";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbReflectDelayName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbReflectDelay,
				resetValue: MusicConstants.BaseReverbReflectDelay );

			mReverbDiffusion.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbDiffusion = value;
					mReverbDiffusion.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbDiffusionName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbDiffusion,
				resetValue: MusicConstants.BaseReverbDiffusion );

			mReverbDensity.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbDensity = value;
					mReverbDensity.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbDensityName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbDensity,
				resetValue: MusicConstants.BaseReverbDensity );

			mReverbHFReference.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbHFReference = value;
					mReverbHFReference.Text.text = $"{value:0.00}Hz";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbHFReferenceName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbHFReference,
				resetValue: MusicConstants.BaseReverbHFReference );

			mReverbRoomLF.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbRoomLF = value;
					mReverbRoomLF.Text.text = $"{value:0.00}mB";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbRoomLFName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbRoomLF,
				resetValue: MusicConstants.BaseReverbRoomLF );


			mReverbLFReference.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ReverbLFReference = value;
					mReverbLFReference.Text.text = $"{value:0.00}Hz";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerReverbLFReferenceName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ReverbLFReference,
				resetValue: MusicConstants.BaseReverbLFReference );

#endregion protected.reverb

#region protected.echo

			mEcho.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.Echo = value / 100f;
					mEcho.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerEchoName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.Echo * 100f,
				resetValue: MusicConstants.BaseEchoWet * 100f );

			mEchoDry.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.EchoDry = value / 100f;
					mEchoDry.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerEchoDryName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.EchoDry * 100f,
				resetValue: MusicConstants.BaseEchoDry * 100f );

			mEchoDelay.Initialize( ( value ) =>
				{
					mEchoDelay.Text.text = $"{value:0.00}ms";
					mInstrument.InstrumentData.EchoDelay = value;
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerEchoDelayName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.EchoDelay,
				resetValue: MusicConstants.BaseEchoDelay * 100f );

			mEchoDecay.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.EchoDecay = value / 100f;
					mEchoDecay.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerEchoDecayName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.EchoDecay * 100f,
				resetValue: MusicConstants.BaseEchoDecay * 100f );

#endregion protected.echo

#region protected.flange

			mFlange.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.Flanger = value / 100f;
					mFlange.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerFlangeName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.Flanger,
				resetValue: MusicConstants.BaseFlangeWet * 100f );

			mFlangeDry.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.FlangeDry = value / 100f;
					mFlangeDry.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerFlangeDryName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.FlangeDry * 100f,
				resetValue: MusicConstants.BaseFlangeDry * 100f );

			mFlangeDepth.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.FlangeDepth = value;
					mFlangeDepth.Text.text = $"{value:0.00}";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerFlangeDepthName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.FlangeDepth,
				resetValue: MusicConstants.BaseFlangeDepth );

			mFlangeRate.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.FlangeRate = value;
					mFlangeRate.Text.text = $"{value:0.00}Hz";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerFlangeRateName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.FlangeRate,
				resetValue: MusicConstants.BaseFlangeRate );

#endregion protected.flange

#region protected.distortion

			mDistortion.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.Distortion = value / 100f;
					mDistortion.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerDistortionName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.Distortion * 100f,
				resetValue: MusicConstants.BaseDistortion * 100f );

#endregion protected.distortion

#region protected.chorus

			mChorusWetMixTap1.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.Chorus = value / 100f;
					mChorusWetMixTap1.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorusName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.Chorus * 100f,
				resetValue: MusicConstants.BaseChorusWetMixTap1 * 100f );

			mChorusWetMixTap2.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.Chorus2 = value / 100f;
					mChorusWetMixTap2.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorus2Name}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.Chorus2 * 100f,
				resetValue: MusicConstants.BaseChorusWetMixTap2 * 100f );

			mChorusWetMixTap3.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.Chorus3 = value / 100f;
					mChorusWetMixTap3.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorus3Name}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.Chorus3 * 100f,
				resetValue: MusicConstants.BaseChorusWetMixTap3 * 100f );

			mChorusDry.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ChorusDry = value / 100f;
					mChorusDry.Text.text = $"{value:0.00}%";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorusDryName}{mInstrument.InstrumentIndex}", parameterValue: value / 100f );
				},
				mInstrument.InstrumentData.ChorusDry * 100f,
				resetValue: MusicConstants.BaseChorusDry * 100f );

			mChorusDelay.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ChorusDelay = value;
					mChorusDelay.Text.text = $"{value:0.00}ms";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorusDelayName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ChorusDelay,
				resetValue: MusicConstants.BaseChorusDelay );

			mChorusRate.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ChorusRate = value;
					mChorusRate.Text.text = $"{value:0.00}Hz";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorusRateName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ChorusRate,
				resetValue: MusicConstants.BaseChorusRate );

			mChorusDepth.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ChorusDepth = value;
					mChorusDepth.Text.text = $"{value:0.00}";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorusDepthName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ChorusDepth,
				resetValue: MusicConstants.BaseChorusDepth );

			mChorusFeedback.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ChorusFeedback = value;
					mChorusFeedback.Text.text = $"{value:0.00}";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerChorusFeedbackName}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ChorusFeedback,
				resetValue: MusicConstants.BaseChorusFeedback );

#endregion protected.chorus

#region protected.paramEQ

			mParamEQCenterFreq.Initialize( ( value ) =>
				{
					// This godawful...thing...below, is my clumsy attempt to smooth out unity's logarithmic scale for their audio mixer params.
					// If you know a better way, please let me know. Or, don't. Either/or, I'd almost prefer to never look at this method again :/
					value = Mathf.Pow( 10f, value ) * ( MusicConstants.MaxParamEQCenterFreq * .1f );
					mInstrument.InstrumentData.ParamEQCenterFreq = value;
					mParamEQCenterFreq.Text.text = $"{value:0.00}Hz";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerCenterFreq}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				Mathf.Log10( mInstrument.InstrumentData.ParamEQCenterFreq / ( MusicConstants.MaxParamEQCenterFreq * .1f ) ),
				resetValue: Mathf.Log10( MusicConstants.BaseParamEQCenterFreq / ( MusicConstants.MaxParamEQCenterFreq * .1f ) ) );

			mParamEQFreqGain.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ParamEQFreqGain = value;
					mParamEQFreqGain.Text.text = $"{value:0.00}";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerFreqGain}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ParamEQFreqGain,
				resetValue: MusicConstants.BaseParamEQFreqGain );

			mParamEQOctaveRange.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.ParamEQOctaveRange = value;
					mParamEQOctaveRange.Text.text = $"{value:0.00}";
					mMusicGenerator.AudioHandler.UpdateParameter( $"{MusicConstants.MixerOctaveRange}{mInstrument.InstrumentIndex}", parameterValue: value );
				},
				mInstrument.InstrumentData.ParamEQOctaveRange,
				resetValue: MusicConstants.BaseParamEQOctaveRange );

#endregion protected.paramEQ
		}

		public void UpdateUIElementValues( Instrument instrument )
		{
			mInstrument = instrument;
			mReverb.Option.value = instrument.InstrumentData.Reverb;
			mRoomSize.Option.value = instrument.InstrumentData.RoomSize;
			mReverbDry.Option.value = instrument.InstrumentData.ReverbDry;
			mReverbDelay.Option.value = instrument.InstrumentData.ReverbDelay;
			mReverbRoomHF.Option.value = instrument.InstrumentData.ReverbRoomHF;
			mReverbDecayTime.Option.value = instrument.InstrumentData.ReverbDecayTime;
			mReverbDecayHFRatio.Option.value = instrument.InstrumentData.ReverbDecayHFRatio;
			mReverbReflections.Option.value = instrument.InstrumentData.ReverbReflections;
			mReverbReflectDelay.Option.value = instrument.InstrumentData.ReverbReflectDelay;
			mReverbDiffusion.Option.value = instrument.InstrumentData.ReverbDiffusion;
			mReverbDensity.Option.value = instrument.InstrumentData.ReverbDensity;
			mReverbHFReference.Option.value = instrument.InstrumentData.ReverbHFReference;
			mReverbRoomLF.Option.value = instrument.InstrumentData.ReverbRoomLF;
			mReverbLFReference.Option.value = instrument.InstrumentData.ReverbLFReference;

			//echo:
			mEcho.Option.value = instrument.InstrumentData.Echo * 100f;
			mEchoDry.Option.value = instrument.InstrumentData.EchoDry * 100f;
			mEchoDelay.Option.value = instrument.InstrumentData.EchoDelay;
			mEchoDecay.Option.value = instrument.InstrumentData.EchoDecay * 100f;

			//flange:
			mFlange.Option.value = instrument.InstrumentData.Flanger * 100f;
			mFlangeDry.Option.value = instrument.InstrumentData.FlangeDry * 100f;
			mFlangeDepth.Option.value = instrument.InstrumentData.FlangeDepth;
			mFlangeRate.Option.value = instrument.InstrumentData.FlangeRate;

			//distortion:
			mDistortion.Option.value = instrument.InstrumentData.Distortion * 100f;

			//chorus:
			mChorusWetMixTap1.Option.value = instrument.InstrumentData.Chorus * 100f;
			mChorusWetMixTap2.Option.value = instrument.InstrumentData.Chorus2 * 100f;
			mChorusWetMixTap3.Option.value = instrument.InstrumentData.Chorus3 * 100f;
			mChorusDry.Option.value = instrument.InstrumentData.ChorusDry * 100f;
			mChorusDelay.Option.value = instrument.InstrumentData.ChorusDelay;
			mChorusRate.Option.value = instrument.InstrumentData.ChorusRate;
			mChorusDepth.Option.value = instrument.InstrumentData.ChorusDepth;
			mChorusFeedback.Option.value = instrument.InstrumentData.ChorusFeedback;

			//paramEQ:
			mParamEQCenterFreq.Option.value = Mathf.Log10( instrument.InstrumentData.ParamEQCenterFreq / ( MusicConstants.MaxParamEQCenterFreq * .1f ) );
			mParamEQFreqGain.Option.value = instrument.InstrumentData.ParamEQFreqGain;
			mParamEQOctaveRange.Option.value = instrument.InstrumentData.ParamEQOctaveRange;
		}

		[SerializeField, Tooltip( "Reference to our music generator" )]
		private MusicGenerator mMusicGenerator;

		[SerializeField, Tooltip( "Reference to our Echo FX Toggle" )]
		private UIToggle mEchoFXToggle;

		[SerializeField, Tooltip( "Reference to our echo fx panel" )]
		private GameObject mEchoFXPanel;

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

		[SerializeField, Tooltip( "Reference to our reverb slider" )]
		private UISlider mReverb;

		[SerializeField, Tooltip( "Reference to our ReverbDry slider" )]
		private UISlider mReverbDry;

		[SerializeField, Tooltip( "Reference to our ReverbRoomHF slider" )]
		private UISlider mReverbRoomHF;

		[SerializeField, Tooltip( "Reference to our ReverbDecayTime slider" )]
		private UISlider mReverbDecayTime;

		[SerializeField, Tooltip( "Reference to our ReverbDecayHFRatio slider" )]
		private UISlider mReverbDecayHFRatio;

		[SerializeField, Tooltip( "Reference to our ReverbReflections slider" )]
		private UISlider mReverbReflections;

		[SerializeField, Tooltip( "Reference to our ReverbReflectDelay slider" )]
		private UISlider mReverbReflectDelay;

		[SerializeField, Tooltip( "Reference to our ReverbDelay slider" )]
		private UISlider mReverbDelay;

		[SerializeField, Tooltip( "Reference to our ReverbDiffusion slider" )]
		private UISlider mReverbDiffusion;

		[SerializeField, Tooltip( "Reference to our ReverbDensity slider" )]
		private UISlider mReverbDensity;

		[SerializeField, Tooltip( "Reference to our ReverbHFReference slider" )]
		private UISlider mReverbHFReference;

		[SerializeField, Tooltip( "Reference to our ReverbRoomLF slider" )]
		private UISlider mReverbRoomLF;

		[SerializeField, Tooltip( "Reference to our ReverbLFReference slider" )]
		private UISlider mReverbLFReference;

		[SerializeField, Tooltip( "Reference to our room size slider" )]
		private UISlider mRoomSize;

		[SerializeField, Tooltip( "Reference to our ChorusDry slider" )]
		private UISlider mChorusDry;

		[SerializeField, Tooltip( "Reference to our ChorusWetMixTap1 slider" )]
		private UISlider mChorusWetMixTap1;

		[SerializeField, Tooltip( "Reference to our ChorusWetMixTap2 slider" )]
		private UISlider mChorusWetMixTap2;

		[SerializeField, Tooltip( "Reference to our ChorusWetMixTap3 slider" )]
		private UISlider mChorusWetMixTap3;

		[SerializeField, Tooltip( "Reference to our ChorusDelay slider" )]
		private UISlider mChorusDelay;

		[SerializeField, Tooltip( "Reference to our ChorusRate slider" )]
		private UISlider mChorusRate;

		[SerializeField, Tooltip( "Reference to our ChorusDepth slider" )]
		private UISlider mChorusDepth;

		[SerializeField, Tooltip( "Reference to our ChorusFeedback slider" )]
		private UISlider mChorusFeedback;

		[SerializeField, Tooltip( "Reference to our ParamEQCenterFreq slider" )]
		private UISlider mParamEQCenterFreq;

		[SerializeField, Tooltip( "Reference to our ParamEQOctaveRange slider" )]
		private UISlider mParamEQOctaveRange;

		[SerializeField, Tooltip( "Reference to our ParamEQFreqGain slider" )]
		private UISlider mParamEQFreqGain;

		[SerializeField, Tooltip( "Reference to our Flange slider" )]
		private UISlider mFlange;

		[SerializeField, Tooltip( "Reference to our FlangeDry slider" )]
		private UISlider mFlangeDry;

		[SerializeField, Tooltip( "Reference to our FlangeDepth slider" )]
		private UISlider mFlangeDepth;

		[SerializeField, Tooltip( "Reference to our FlangeRate slider" )]
		private UISlider mFlangeRate;

		[SerializeField, Tooltip( "Reference to our distortion slider" )]
		private UISlider mDistortion;

		[SerializeField, Tooltip( "Reference to our echo slider" )]
		private UISlider mEcho;

		[SerializeField, Tooltip( "Reference to our echo delay slider" )]
		private UISlider mEchoDelay;

		[SerializeField, Tooltip( "Reference to our echo decay slider" )]
		private UISlider mEchoDecay;

		[SerializeField, Tooltip( "Reference to our echo dry slider" )]
		private UISlider mEchoDry;

		private Instrument mInstrument;
	}
}

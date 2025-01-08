using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Our configuration data for the music generator.
	/// </summary>
	[Serializable]
	public class ConfigurationData
	{
		/// <summary>
		/// Version of the generator this configuration data was created with
		/// </summary>
		public float Version { get; private set; }

		/// <summary>
		/// Memberwise clone of the configuration data
		/// </summary>
		/// <returns></returns>
		public ConfigurationData Clone()
		{
			//Please ensure we're not including reference types here.
			return (ConfigurationData) MemberwiseClone();
		}

		#region GeneratorData

		[Header( "Generator Data (overwritten at runtime)" ), Space( MusicConstants.HeaderSpace )]
		[SerializeField, Tooltip( "Name of the configuration" )]
		public string ConfigurationName;

		/// <summary>
		/// List of mods enabled for the generator.
		/// </summary>
		public List<string> Mods = new List<string>();

#if FMOD_ENABLED
		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterFader Value" )]
		private float mFmodMasterFader = MusicConstants.BaseFmodMasterFader;

		public float FmodMasterFader
		{
			get => mFmodMasterFader;
			set => mFmodMasterFader = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}
#endif //FMOD_ENABLED

		[SerializeField, Range( MusicConstants.MinVolume, MusicConstants.MaxVolume ), Tooltip( "Master Volume Level" )]
		private float mMasterVolume = MusicConstants.BaseMasterVolume;

		/// <summary>
		/// Master Volume Level
		/// </summary>
		public float MasterVolume
		{
			get => mMasterVolume;
			set => mMasterVolume = Mathf.Clamp( value, MusicConstants.MinVolume, MusicConstants.MaxVolume );
		}

		[SerializeField, Range( MusicConstants.VolFadeRateMin, MusicConstants.VolFadeRateMax ),
		 Tooltip( "The rate at which we'll fade Volume in/out" )]
		private float mVolFadeRate = MusicConstants.BaseVolFadeRate;

		/// <summary>
		/// The rate at which we'll fade Volume in/out
		/// </summary>
		public float VolFadeRate
		{
			get => mVolFadeRate;
			set => mVolFadeRate = Mathf.Clamp( value, MusicConstants.VolFadeRateMin, MusicConstants.VolFadeRateMax );
		}

		[Tooltip( "Our mode see: https://en.wikipedia.org/wiki/Mode_(music)" )]
		public Mode Mode = Mode.Ionian;

		[Tooltip( "Whether our player will repeat refrains, use a theme, or neither." )]
		public ThemeRepeatOptions ThemeRepeatOptions = ThemeRepeatOptions.None;

		[SerializeField, Range( MusicConstants.KeyStepsMin, MusicConstants.KeyStepsMax ),
		 Tooltip( "Storage for whether our key change will ascend/descend through the circle of fifths." )]
		private int mKeySteps;

		/// <summary>
		/// Storage for whether our key change will ascend/descend through the circle of fifths.
		/// </summary>
		public int KeySteps
		{
			get => mKeySteps;
			set => mKeySteps = Mathf.Clamp( value, MusicConstants.KeyStepsMin, MusicConstants.KeyStepsMax );
		}

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
		 Tooltip( "Odds our key change will ascend/descend through the circle of fifths." )]
		private float mKeyChangeAscendDescend = MusicConstants.OddsMid;

		/// <summary>
		/// Odds our key change will ascend/descend through the circle of fifths.
		/// </summary>
		public float KeyChangeAscendDescend
		{
			get => mKeyChangeAscendDescend;
			set => mKeyChangeAscendDescend = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ), Tooltip( "Odds a new theme will be selected" )]
		private float mSetThemeOdds = MusicConstants.BaseSetThemeOdds;

		/// <summary>
		/// Odds a new theme will be selected
		/// </summary>
		public float SetThemeOdds
		{
			get => mSetThemeOdds;
			set => mSetThemeOdds = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ), Tooltip( "Odds the theme will play" )]
		private float mPlayThemeOdds = MusicConstants.BasePlayThemeOdds;

		/// <summary>
		/// Odds the theme will play
		/// </summary>
		public float PlayThemeOdds
		{
			get => mPlayThemeOdds;
			set => mPlayThemeOdds = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		[Tooltip( "Our scale. see: https://en.wikipedia.org/wiki/Scale_(music)" )]
		public Scale Scale = Scale.Major;

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ), Tooltip( "Odds we choose a new progression" )]
		private float mProgressionChangeOdds = MusicConstants.BaseProgressionChangeOdds;

		/// <summary>
		/// Odds we choose a new progression
		/// </summary>
		public float ProgressionChangeOdds
		{
			get => mProgressionChangeOdds;
			set => mProgressionChangeOdds = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		[Tooltip( "Our key. see: https://en.wikipedia.org/wiki/Key_(music)" )]
		public Key Key = 0;

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ), Tooltip( "Odds we'll change key." )]
		private float mKeyChangeOdds = MusicConstants.OddsMin;

		/// <summary>
		/// Odds we'll change key.
		/// </summary>
		public float KeyChangeOdds
		{
			get => mKeyChangeOdds;
			set => mKeyChangeOdds = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		/// <summary>
		/// The odds we roll for group changes at all
		/// </summary>
		public float OverallGroupChangeOdds = 100f;

		[SerializeField, Tooltip( "The odds each group will play for any given measure/progression (see: DynamicStyle)" )]
		private float[] mGroupOdds =
		{
			MusicConstants.OddsMax, MusicConstants.OddsMax, MusicConstants.OddsMax, MusicConstants.OddsMax
		};

		/// <summary>
		/// The odds each group will play for any given measure/progression (see: DynamicStyle)
		/// </summary>
		public float[] GroupOdds => mGroupOdds;

		[Tooltip( "Whether the group odds will be managed manually by the user" )]
		public bool ManualGroupOdds;

		[Tooltip( "Whether we choose groups at the end of a measure or the end of a chord progression" )]
		public GroupRate GroupRate = GroupRate.Progression;

		[Tooltip( " selecting groups, whether they are chosen randomly, or crescendo linearly" )]
		public DynamicStyle DynamicStyle = DynamicStyle.Linear;

		[Tooltip( "our time signature. see: https://en.wikipedia.org/wiki/Time_signature" )]
		public TimeSignatures TimeSignature = TimeSignatures.FourFour;

		[SerializeField, Range( MusicConstants.MinDistortion, MusicConstants.MaxDistortion ), Tooltip( "Global Distortion Effect" )]
		private float mDistortion = MusicConstants.BaseDistortion;

		#region GlobalDistortion

		/// <summary>
		/// Global Distortion Effect
		/// </summary>
		public float Distortion
		{
			get => mDistortion;
			set => mDistortion = Mathf.Clamp( value, MusicConstants.MinDistortion, MusicConstants.MaxDistortion );
		}

		#endregion GlobalDistortion

		#region GlobalParamEQ

		[SerializeField, Range( MusicConstants.MinParamEQCenterFreq, MusicConstants.MaxParamEQCenterFreq ),
		 Tooltip( "Global ParamEQ Center Freq. SFX" )]
		private float mParamEQCenterFreq = MusicConstants.BaseParamEQCenterFreq;

		/// <summary>
		/// Global ParamEQ Center Freq
		/// </summary>
		public float ParamEQCenterFreq
		{
			get => mParamEQCenterFreq;
			set => mParamEQCenterFreq = Mathf.Clamp( value, MusicConstants.MinParamEQCenterFreq, MusicConstants.MaxParamEQCenterFreq );
		}

		[SerializeField, Range( MusicConstants.MinParamEQOctaveRange, MusicConstants.MaxParamEQOctaveRange ),
		 Tooltip( "Global ParamEQ Octave Range SFX" )]
		private float mParamEQOctaveRange = MusicConstants.BaseParamEQOctaveRange;

		/// <summary>
		/// Global ParamEQ Octave Range
		/// </summary>
		public float ParamEQOctaveRange
		{
			get => mParamEQOctaveRange;
			set => mParamEQOctaveRange = Mathf.Clamp( value, MusicConstants.MinParamEQOctaveRange, MusicConstants.MaxParamEQOctaveRange );
		}

		[SerializeField, Range( MusicConstants.MinParamEQFreqGain, MusicConstants.MaxParamEQFreqGain ),
		 Tooltip( "Global ParamEQ Freq Gain SFX" )]
		private float mParamEQFreqGain = MusicConstants.BaseParamEQFreqGain;

		/// <summary>
		/// Global ParamEQ Freq Gain
		/// </summary>
		public float ParamEQFreqGain
		{
			get => mParamEQFreqGain;
			set => mParamEQFreqGain = Mathf.Clamp( value, MusicConstants.MinParamEQFreqGain, MusicConstants.MaxParamEQFreqGain );
		}

		#endregion GlobalParamEQ

		#region GlobalEcho

		[SerializeField, Range( MusicConstants.MinEchoDelay, MusicConstants.MaxEchoDelay ), Tooltip( "Global value for mixer Echo Delay effect" )]
		private float mEchoDelay = MusicConstants.BaseEchoDelay;

		/// <summary>
		/// Global value for mixer Echo Delay effect
		/// </summary>
		public float EchoDelay
		{
			get => mEchoDelay;
			set => mEchoDelay = Mathf.Clamp( value, MusicConstants.MinEchoDelay, MusicConstants.MaxEchoDelay );
		}

		[SerializeField, Range( MusicConstants.MinEchoDecay, MusicConstants.MaxEchoDecay ), Tooltip( "Global value for mixer Echo Decay effect" )]
		private float mEchoDecay = MusicConstants.BaseEchoDecay;

		/// <summary>
		/// Global value for mixer Echo Decay effect
		/// </summary>
		public float EchoDecay
		{
			get => mEchoDecay;
			set => mEchoDecay = Mathf.Clamp( value, MusicConstants.MinEchoDecay, MusicConstants.MaxEchoDecay );
		}

		[SerializeField, Range( MusicConstants.MinEchoDry, MusicConstants.MaxEchoDry ), Tooltip( "Global value for mixer Echo Dry value" )]
		private float mEchoDry = MusicConstants.BaseEchoDry;

		/// <summary>
		/// Global value for mixer Echo Dry value
		/// </summary>
		public float EchoDry
		{
			get => mEchoDry;
			set => mEchoDry = Mathf.Clamp( value, MusicConstants.MinEchoDry, MusicConstants.MaxEchoDry );
		}

		[SerializeField, Range( MusicConstants.MinEchoWet, MusicConstants.MaxEchoWet ), Tooltip( "Global value for mixer Echo Wet value" )]
		private float mEchoWet = MusicConstants.BaseEchoWet;

		/// <summary>
		/// Global value for mixer Echo Wet value
		/// </summary>
		public float EchoWet
		{
			get => mEchoWet;
			set => mEchoWet = Mathf.Clamp( value, MusicConstants.MinEchoWet, MusicConstants.MaxEchoWet );
		}

		#endregion GlobalEcho

		#region GlobalFlange

		[SerializeField, Range( MusicConstants.MinFlangeWet, MusicConstants.MaxFlangeWet ), Tooltip( "Global Flanger SFX" )]
		private float mFlanger = MusicConstants.BaseFlangeWet;

		/// <summary>
		/// Global Flanger SFX
		/// </summary>
		public float Flanger
		{
			get => mFlanger;
			set => mFlanger = Mathf.Clamp( value, MusicConstants.MinFlangeWet, MusicConstants.MaxFlangeWet );
		}

		[SerializeField, Range( MusicConstants.MinFlangeDry, MusicConstants.MaxFlangeDry ), Tooltip( "Global Flange Dry SFX" )]
		private float mFlangeDry = MusicConstants.BaseFlangeDry;

		/// <summary>
		/// Global Flange Dry SFX
		/// </summary>
		public float FlangeDry
		{
			get => mFlangeDry;
			set => mFlangeDry = Mathf.Clamp( value, MusicConstants.MinFlangeDry, MusicConstants.MaxFlangeDry );
		}

		[SerializeField, Range( MusicConstants.MinFlangeDepth, MusicConstants.MaxFlangeDepth ), Tooltip( "Global Flange Depth SFX" )]
		private float mFlangeDepth = MusicConstants.BaseFlangeDepth;

		/// <summary>
		/// Global Flange Depth SFX
		/// </summary>
		public float FlangeDepth
		{
			get => mFlangeDepth;
			set => mFlangeDepth = Mathf.Clamp( value, MusicConstants.MinFlangeDepth, MusicConstants.MaxFlangeDepth );
		}

		[SerializeField, Range( MusicConstants.MinFlangeRate, MusicConstants.MaxFlangeRate ), Tooltip( "Global Flange Rate SFX" )]
		private float mFlangeRate = MusicConstants.BaseFlangeRate;

		/// <summary>
		/// Global Flange Rate SFX
		/// </summary>
		public float FlangeRate
		{
			get => mFlangeRate;
			set => mFlangeRate = Mathf.Clamp( value, MusicConstants.MinFlangeRate, MusicConstants.MaxFlangeRate );
		}

		#endregion GlobalFlange

		#region GlobalChorusSFX

		[SerializeField, Range( MusicConstants.MinChorusWetMixTap1, MusicConstants.MaxChorusWetMixTap1 ), Tooltip( "Global Chorus SFX" )]
		private float mChorus = MusicConstants.BaseChorusWetMixTap1;

		/// <summary>
		/// Global Chorus SFX
		/// </summary>
		public float Chorus
		{
			get => mChorus;
			set => mChorus = Mathf.Clamp( value, MusicConstants.MinChorusWetMixTap1, MusicConstants.MaxChorusWetMixTap1 );
		}

		[SerializeField, Range( MusicConstants.MinChorusWetMixTap2, MusicConstants.MaxChorusWetMixTap2 ), Tooltip( "Global Chorus SFX" )]
		private float mChorus2 = MusicConstants.BaseChorusWetMixTap2;

		/// <summary>
		/// Global Chorus 2 SFX
		/// </summary>
		public float Chorus2
		{
			get => mChorus2;
			set => mChorus2 = Mathf.Clamp( value, MusicConstants.MinChorusWetMixTap2, MusicConstants.MaxChorusWetMixTap2 );
		}

		[SerializeField, Range( MusicConstants.MinChorusWetMixTap3, MusicConstants.MaxChorusWetMixTap3 ), Tooltip( "Global Chorus SFX" )]
		private float mChorus3 = MusicConstants.BaseChorusWetMixTap3;

		/// <summary>
		/// Global Chorus SFX
		/// </summary>
		public float Chorus3
		{
			get => mChorus3;
			set => mChorus3 = Mathf.Clamp( value, MusicConstants.MinChorusWetMixTap3, MusicConstants.MaxChorusWetMixTap3 );
		}

		[SerializeField, Range( MusicConstants.MinChorusDry, MusicConstants.MaxChorusDry ), Tooltip( "Global Chorus Dry SFX" )]
		private float mChorusDry = MusicConstants.BaseChorusDry;

		/// <summary>
		/// Global Chorus Dry SFX
		/// </summary>
		public float ChorusDry
		{
			get => mChorusDry;
			set => mChorusDry = Mathf.Clamp( value, MusicConstants.MinChorusDry, MusicConstants.MaxChorusDry );
		}

		[SerializeField, Range( MusicConstants.MinChorusDelay, MusicConstants.MaxChorusDelay ), Tooltip( "Global Chorus Delay SFX" )]
		private float mChorusDelay = MusicConstants.BaseChorusDelay;

		/// <summary>
		/// Global Chorus Delay SFX
		/// </summary>
		public float ChorusDelay
		{
			get => mChorusDelay;
			set => mChorusDelay = Mathf.Clamp( value, MusicConstants.MinChorusDelay, MusicConstants.MaxChorusDelay );
		}

		[SerializeField, Range( MusicConstants.MinChorusRate, MusicConstants.MaxChorusRate ), Tooltip( "Global Chorus rate SFX" )]
		private float mChorusRate = MusicConstants.BaseChorusRate;

		/// <summary>
		/// Global Chorus Rate SFX
		/// </summary>
		public float ChorusRate
		{
			get => mChorusRate;
			set => mChorusRate = Mathf.Clamp( value, MusicConstants.MinChorusRate, MusicConstants.MaxChorusRate );
		}

		[SerializeField, Range( MusicConstants.MinChorusDepth, MusicConstants.MaxChorusDepth ), Tooltip( "Global Chorus depth SFX" )]
		private float mChorusDepth = MusicConstants.BaseChorusDepth;

		/// <summary>
		/// Global Chorus Depth SFX
		/// </summary>
		public float ChorusDepth
		{
			get => mChorusDepth;
			set => mChorusDepth = Mathf.Clamp( value, MusicConstants.MinChorusDepth, MusicConstants.MaxChorusDepth );
		}

		[SerializeField, Range( MusicConstants.MinChorusFeedback, MusicConstants.MaxChorusFeedback ), Tooltip( "Global Chorus feedback SFX" )]
		private float mChorusFeedback = MusicConstants.BaseChorusFeedback;

		/// <summary>
		/// Global Chorus Feedback SFX
		/// </summary>
		public float ChorusFeedback
		{
			get => mChorusFeedback;
			set => mChorusFeedback = Mathf.Clamp( value, MusicConstants.MinChorusFeedback, MusicConstants.MaxChorusFeedback );
		}

		#endregion GlobalChorusSFX

		#region GlobalReverb

		[SerializeField, Range( MusicConstants.MinReverb, MusicConstants.MaxReverb ), Tooltip( "Global value for mixer reverb" )]
		private float mReverb = MusicConstants.BaseReverb;

		/// <summary>
		/// Global value for mixer reverb
		/// </summary>
		public float Reverb
		{
			get => mReverb;
			set => mReverb = Mathf.Clamp( value, MusicConstants.MinReverb, MusicConstants.MaxReverb );
		}

		[SerializeField, Range( MusicConstants.MinReverbRoom, MusicConstants.MaxReverbRoom ), Tooltip( "Global value for mixer Room Size value" )]
		private float mRoomSize = MusicConstants.BaseReverbRoom;

		/// <summary>
		/// Global value for mixer Room Size value
		/// </summary>
		public float RoomSize
		{
			get => mRoomSize;
			set => mRoomSize = Mathf.Clamp( value, MusicConstants.MinReverbRoom, MusicConstants.MaxReverbRoom );
		}

		[SerializeField, Range( MusicConstants.MinReverbDecayTime, MusicConstants.MaxReverbDecayTime ), Tooltip( "Global value for mixer Reverb decay" )]
		private float mReverbDecay = MusicConstants.BaseReverbDecayTime;

		/// <summary>
		/// Global value for mixer Reverb decay
		/// </summary>
		public float ReverbDecay
		{
			get => mReverbDecay;
			set => mReverbDecay = Mathf.Clamp( value, MusicConstants.MinReverbDecayTime, MusicConstants.MaxReverbDecayTime );
		}

		[SerializeField, Range( MusicConstants.MinReverbDry, MusicConstants.MaxReverbDry ), Tooltip( "Global Reverb Dry" )]
		private float mReverbDry = MusicConstants.BaseReverbDry;

		/// <summary>
		/// Global Reverb Dry 
		/// </summary>
		public float ReverbDry
		{
			get => mReverbDry;
			set => mReverbDry = Mathf.Clamp( value, MusicConstants.MinReverbDry, MusicConstants.MaxReverbDry );
		}

		[SerializeField, Range( MusicConstants.MinReverbDensity, MusicConstants.MaxReverbDensity ), Tooltip( "Global Reverb Density" )]
		private float mReverbDensity = MusicConstants.BaseReverbDensity;

		/// <summary>
		/// Global Reverb Density
		/// </summary>
		public float ReverbDensity
		{
			get => mReverbDensity;
			set => mReverbDensity = Mathf.Clamp( value, MusicConstants.MinReverbDensity, MusicConstants.MaxReverbDensity );
		}

		[SerializeField, Range( MusicConstants.MinReverbDelay, MusicConstants.MaxReverbDelay ), Tooltip( "Global Reverb Delay" )]
		private float mReverbDelay = MusicConstants.BaseReverbDelay;

		/// <summary>
		/// Global Reverb Delay 
		/// </summary>
		public float ReverbDelay
		{
			get => mReverbDelay;
			set => mReverbDelay = Mathf.Clamp( value, MusicConstants.MinReverbDelay, MusicConstants.MaxReverbDelay );
		}

		[SerializeField, Range( MusicConstants.MinReverbRoomHF, MusicConstants.MaxReverbRoomHF ), Tooltip( "Global Reverb Room HF" )]
		private float mReverbRoomHF = MusicConstants.BaseReverbRoomHF;

		/// <summary>
		/// Global Reverb Room HF 
		/// </summary>
		public float ReverbRoomHF
		{
			get => mReverbRoomHF;
			set => mReverbRoomHF = Mathf.Clamp( value, MusicConstants.MinReverbRoomHF, MusicConstants.MaxReverbRoomHF );
		}

		[SerializeField, Range( MusicConstants.MinReverbDecayHFRatio, MusicConstants.MaxReverbDecayHFRatio ),
		 Tooltip( "Global Reverb Decay HF Ratio" )]
		private float mReverbDecayHFRatio = MusicConstants.BaseReverbDecayHFRatio;

		/// <summary>
		/// Global Reverb Decay HF Ratio
		/// </summary>
		public float ReverbDecayHFRatio
		{
			get => mReverbDecayHFRatio;
			set => mReverbDecayHFRatio = Mathf.Clamp( value, MusicConstants.MinReverbDecayHFRatio, MusicConstants.MaxReverbDecayHFRatio );
		}

		[SerializeField, Range( MusicConstants.MinReverbReflections, MusicConstants.MaxInstrumentReverbReflections ),
		 Tooltip( "Global Reverb Reflections" )]
		private float mReverbReflections = MusicConstants.BaseReverbReflections;

		/// <summary>
		/// Global Reverb Reflections
		/// </summary>
		public float ReverbReflections
		{
			get => mReverbReflections;
			set => mReverbReflections = Mathf.Clamp( value, MusicConstants.MinReverbReflections, MusicConstants.MaxInstrumentReverbReflections );
		}

		[SerializeField, Range( MusicConstants.MinReverbReflectDelay, MusicConstants.MaxReverbReflectDelay ),
		 Tooltip( "Global Reverb Reflect Delay" )]
		private float mReverbReflectDelay = MusicConstants.BaseReverbReflectDelay;

		/// <summary>
		/// Global Reverb Reflect Delay
		/// </summary>
		public float ReverbReflectDelay
		{
			get => mReverbReflectDelay;
			set => mReverbReflectDelay = Mathf.Clamp( value, MusicConstants.MinReverbReflectDelay, MusicConstants.MaxReverbReflectDelay );
		}

		[SerializeField, Range( MusicConstants.MinReverbDiffusion, MusicConstants.MaxReverbDiffusion ), Tooltip( "Global Reverb Diffusion" )]
		private float mReverbDiffusion = MusicConstants.BaseReverbDiffusion;

		/// <summary>
		/// Global Reverb Diffusion
		/// </summary>
		public float ReverbDiffusion
		{
			get => mReverbDiffusion;
			set => mReverbDiffusion = Mathf.Clamp( value, MusicConstants.MinReverbDiffusion, MusicConstants.MaxReverbDiffusion );
		}

		[SerializeField, Range( MusicConstants.MinReverbHFReference, MusicConstants.MaxReverbHFReference ), Tooltip( "Global Reverb HFReference" )]
		private float mReverbHFReference = MusicConstants.BaseReverbHFReference;

		/// <summary>
		/// Global Reverb HF Reference
		/// </summary>
		public float ReverbHFReference
		{
			get => mReverbHFReference;
			set => mReverbHFReference = Mathf.Clamp( value, MusicConstants.MinReverbHFReference, MusicConstants.MaxReverbHFReference );
		}

		[SerializeField, Range( MusicConstants.MinReverbRoomLF, MusicConstants.MaxReverbRoomLF ), Tooltip( "Global Reverb Room LF" )]
		private float mReverbRoomLF = MusicConstants.BaseReverbRoomLF;

		/// <summary>
		/// Global Reverb Room LF
		/// </summary>
		public float ReverbRoomLF
		{
			get => mReverbRoomLF;
			set => mReverbRoomLF = Mathf.Clamp( value, MusicConstants.MinReverbRoomLF, MusicConstants.MaxReverbRoomLF );
		}

		[SerializeField, Range( MusicConstants.MinReverbLFReference, MusicConstants.MaxReverbLFReference ),
		 Tooltip( "Global Reverb LF Reference" )]
		private float mReverbLFReference = MusicConstants.BaseReverbLFReference;

		/// <summary>
		/// Global Reverb LF Reference
		/// </summary>
		public float ReverbLFReference
		{
			get => mReverbLFReference;
			set => mReverbLFReference = Mathf.Clamp( value, MusicConstants.MinReverbLFReference, MusicConstants.MaxReverbLFReference );
		}

		#endregion GlobalReverb

		#region FMODGlobalEffects

#if FMOD_ENABLED
		//Master Values:
		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterFlangerRate Value" )]
		private float mFmodMasterFlangerRate = MusicConstants.BaseFmodFlangerRate;

		public float FmodMasterFlangerRate
		{
			get => mFmodMasterFlangerRate;
			set => mFmodMasterFlangerRate = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterFlangerDepth Value" )]
		private float mFmodMasterFlangerDepth = MusicConstants.BaseFmodFlangerDepth;

		public float FmodMasterFlangerDepth
		{
			get => mFmodMasterFlangerDepth;
			set => mFmodMasterFlangerDepth = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterFlangerMix Value" )]
		private float mFmodMasterFlangerMix = MusicConstants.BaseFmodFlangerMix;

		public float FmodMasterFlangerMix
		{
			get => mFmodMasterFlangerMix;
			set => mFmodMasterFlangerMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod Delay Value" )]
		private float mFmodMasterDelay = MusicConstants.BaseFmodDelay;

		public float FmodMasterDelay
		{
			get => mFmodMasterDelay;
			set => mFmodMasterDelay =
				Mathf.Clamp( value, MusicConstants.NormalizedMid, MusicConstants.NormalizedMax ); //<Note: anything less than .5f for fmod delay sounds pretty bad/useless.
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterDelayWet Value" )]
		private float mFmodMasterDelayWet = MusicConstants.BaseFmodDelayWet;

		public float FmodMasterDelayWet
		{
			get => mFmodMasterDelayWet;
			set => mFmodMasterDelayWet = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterDelayFeedback Value" )]
		private float mFmodMasterDelayFeedback = MusicConstants.BaseFmodDelayFeedback;

		public float FmodMasterDelayFeedback
		{
			get => mFmodMasterDelayFeedback;
			set => mFmodMasterDelayFeedback = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterDelayDry Value" )]
		private float mFmodMasterDelayDry = MusicConstants.BaseFmodDelayDry;

		public float FmodMasterDelayDry
		{
			get => mFmodMasterDelayDry;
			set => mFmodMasterDelayDry = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbTime Value" )]
		private float mFmodMasterReverbTime = MusicConstants.BaseFmodReverbTime;

		public float FmodMasterReverbTime
		{
			get => mFmodMasterReverbTime;
			set => mFmodMasterReverbTime = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbHFDecay Value" )]
		private float mFmodMasterReverbHFDecay = MusicConstants.BaseFmodReverbHFDecay;

		public float FmodMasterReverbHFDecay
		{
			get => mFmodMasterReverbHFDecay;
			set => mFmodMasterReverbHFDecay = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbDiffusion Value" )]
		private float mFmodMasterReverbDiffusion = MusicConstants.BaseFmodReverbDiffusion;

		public float FmodMasterReverbDiffusion
		{
			get => mFmodMasterReverbDiffusion;
			set => mFmodMasterReverbDiffusion = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod ReverbLowGain Value" )]
		private float mFmodMasterReverbLowGain = MusicConstants.BaseFmodReverbLowGain;

		public float FmodMasterReverbLowGain
		{
			get => mFmodMasterReverbLowGain;
			set => mFmodMasterReverbLowGain = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbHighCut Value" )]
		private float mFmodMasterReverbHighCut = MusicConstants.BaseFmodReverbHighCut;

		public float FmodMasterReverbHighCut
		{
			get => mFmodMasterReverbHighCut;
			set => mFmodMasterReverbHighCut = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbWetMix Value" )]
		private float mFmodMasterReverbWetMix = MusicConstants.BaseFmodReverbWetMix;

		public float FmodMasterReverbWetMix
		{
			get => mFmodMasterReverbWetMix;
			set => mFmodMasterReverbWetMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbDryMix Value" )]
		private float mFmodMasterReverbDryMix = MusicConstants.BaseFmodReverbDryMix;

		public float FmodMasterReverbDryMix
		{
			get => mFmodMasterReverbDryMix;
			set => mFmodMasterReverbDryMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbEarlyDelay Value" )]
		private float mFmodMasterReverbEarlyDelay = MusicConstants.BaseFmodReverbEarlyDelay;

		public float FmodMasterReverbEarlyDelay
		{
			get => mFmodMasterReverbEarlyDelay;
			set => mFmodMasterReverbEarlyDelay = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbLateDelay Value" )]
		private float mFmodMasterReverbLateDelay = MusicConstants.BaseFmodReverbLateDelay;

		public float FmodMasterReverbLateDelay
		{
			get => mFmodMasterReverbLateDelay;
			set => mFmodMasterReverbLateDelay = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod ReverbHFReference Value" )]
		private float mFmodMasterReverbHFReference = MusicConstants.BaseFmodReverbHFReference;

		public float FmodMasterReverbHFReference
		{
			get => mFmodMasterReverbHFReference;
			set => mFmodMasterReverbHFReference = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbDensity Value" )]
		private float mFmodMasterReverbDensity = MusicConstants.BaseFmodReverbDensity;

		public float FmodMasterReverbDensity
		{
			get => mFmodMasterReverbDensity;
			set => mFmodMasterReverbDensity = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbLowFreq Value" )]
		private float mFmodMasterReverbLowFreq = MusicConstants.BaseFmodReverbLowFreq;

		public float FmodMasterReverbLowFreq
		{
			get => mFmodMasterReverbLowFreq;
			set => mFmodMasterReverbLowFreq = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterReverbEarlyLateMix Value" )]
		private float mFmodMasterReverbEarlyLateMix = MusicConstants.BaseFmodReverbEarlyLateMix;

		public float FmodMasterReverbEarlyLateMix
		{
			get => mFmodMasterReverbEarlyLateMix;
			set => mFmodMasterReverbEarlyLateMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterDistortion Value" )]
		private float mFmodMasterDistortion = MusicConstants.BaseFmodDistortion;

		public float FmodMasterDistortion
		{
			get => mFmodMasterDistortion;
			set => mFmodMasterDistortion = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterChorusRate Value" )]
		private float mFmodMasterChorusRate = MusicConstants.BaseFmodChorusRate;

		public float FmodMasterChorusRate
		{
			get => mFmodMasterChorusRate;
			set => mFmodMasterChorusRate = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterChorusDepth Value" )]
		private float mFmodMasterChorusDepth = MusicConstants.BaseFmodChorusDepth;

		public float FmodMasterChorusDepth
		{
			get => mFmodMasterChorusDepth;
			set => mFmodMasterChorusDepth = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterChorusMix Value" )]
		private float mFmodMasterChorusMix = MusicConstants.BaseFmodChorusMix;

		public float FmodMasterChorusMix
		{
			get => mFmodMasterChorusMix;
			set => mFmodMasterChorusMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterEQLow Value" )]
		private float mFmodMasterEQLow = MusicConstants.BaseFmodEQLow;

		public float FmodMasterEQLow
		{
			get => mFmodMasterEQLow;
			set => mFmodMasterEQLow = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterEQMid Value" )]
		private float mFmodMasterEQMid = MusicConstants.BaseFmodEQMid;

		public float FmodMasterEQMid
		{
			get => mFmodMasterEQMid;
			set => mFmodMasterEQMid = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterEQHigh Value" )]
		private float mFmodMasterEQHigh = MusicConstants.BaseFmodEQHigh;

		public float FmodMasterEQHigh
		{
			get => mFmodMasterEQHigh;
			set => mFmodMasterEQHigh = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterEQXHigh Value" )]
		private float mFmodMasterEQXHigh = MusicConstants.BaseFmodEQXHigh;

		public float FmodMasterEQXHigh
		{
			get => mFmodMasterEQXHigh;
			set => mFmodMasterEQXHigh = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterEQXLow Value" )]
		private float mFmodMasterEQXLow = MusicConstants.BaseFmodEQXLow;

		public float FmodMasterEQXLow
		{
			get => mFmodMasterEQXLow;
			set => mFmodMasterEQXLow = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterPanner Value" )]
		private float mFmodMasterPanner = MusicConstants.NormalizedMid;

		public float FmodMasterPanner
		{
			get => mFmodMasterPanner;
			set => mFmodMasterPanner = Mathf.Clamp( value, -MusicConstants.NormalizedMax, MusicConstants.NormalizedMax );
		}

		[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
		 Tooltip( "Fmod MasterGain Value" )]
		private float mFmodMasterGain = MusicConstants.BaseFmodMasterGain;

		public float FmodMasterGain
		{
			get => mFmodMasterGain;
			set => mFmodMasterGain = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
		}
#endif //FMOD_ENABLED

		#endregion FMODGlobalEffects

		[Tooltip( "Whether our single clip will repeat" )]
		public bool SingleClipIsRepeating;

		/// <summary>
		/// Whether this configuration data represents a SingleClip
		/// </summary>
		public bool IsSingleClip;

		#endregion GeneratorData

		#region InstrumentSetData

		// Instrument InstrumentSet data:
		[Header( "Instrument Set Data " ), Space( MusicConstants.HeaderSpace )]
		[Tooltip( "how quickly we step through our chord progression " )]
		public int ProgressionRate = 4;

		[SerializeField, Range( MusicConstants.MinTempo, MusicConstants.MaxTempo ),
		 Tooltip( "Tempo the music generator will play at, in beats per minute" )]
		private float mTempo = MusicConstants.BaseTempo;

		/// <summary>
		/// Tempo the music generator will play at, in beats per minute
		/// </summary>
		public float Tempo
		{
			get => mTempo;
			set => mTempo = Mathf.Clamp( value, MusicConstants.MinTempo, MusicConstants.MaxTempo );
		}

		[SerializeField, Range( MusicConstants.RepeatMeasuresNumMin, MusicConstants.RepeatMeasuresNumMax ),
		 Tooltip( "number measure we 'll repeat, if we're repeating measures" )]
		private int mRepeatMeasuresNum = MusicConstants.RepeatMeasuresNumMin;

		/// <summary>
		/// number measure we 'll repeat, if we're repeating measures
		/// </summary>
		public int RepeatMeasuresNum
		{
			get => mRepeatMeasuresNum;
			set => mRepeatMeasuresNum = Mathf.Clamp( value, MusicConstants.RepeatMeasuresNumMin, MusicConstants.RepeatMeasuresNumMax );
		}

		/// <summary>
		/// Leitmotif progression is the chord progression that the leitmotif will use
		/// </summary>
		[SerializeField, HideInInspector] private int[] mLeitmotifProgression =
		{
			1, 4, 4, 5, 1, 4, 4, 5
		};

		/// <summary>
		/// Leitmotif progression is the chord progression that the leitmotif will use
		/// </summary>
		public int[] LeitmotifProgression => mLeitmotifProgression;

		public bool LeitmotifIgnoresGroups = true;

		#endregion InstrumentSetData

		#region ChordProgressionData

		[Header( "Chord Progression Data " ), Space( MusicConstants.HeaderSpace )]
		[Tooltip(
			"which steps of our current scale are excluded from our chord progression.see : https: //en.wikipedia.org/wiki/Chord_progression." )]
		[SerializeField, HideInInspector] private bool[] mExcludedProgSteps =
		{
			false, false, false, false, false, false, false
		};

		/// <summary>
		/// which steps of our current scale are excluded from our chord progression.see : https: //en.wikipedia.org/wiki/Chord_progression.
		/// </summary>
		public bool[] ExcludedProgSteps
		{
			get => mExcludedProgSteps;
			set
			{
				if ( value.Length != MusicConstants.ScaleLength )
				{
					return;
				}

				mExcludedProgSteps = value;
			}
		}

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
		 Tooltip(
			 "influence of the likelihood of playing a tonic chord in our progression. This isn't straight-odds, and is finessed a little. see:  https://en.wikipedia.org/wiki/Tonic_(music)" )]
		private float mTonicInfluence = MusicConstants.OddsMid;

		/// <summary>
		/// influence of the likelihood of playing a tonic chord in our progression. This isn't straight-odds, and is finessed a little. see:  https://en.wikipedia.org/wiki/Tonic_(music)
		/// </summary>
		public float TonicInfluence
		{
			get => mTonicInfluence;
			set => mTonicInfluence = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
		 Tooltip(
			 "influence of the likelihood of playing a subdominant chord in our progression. This isn't straight-odds, and is finessed a little. see:  https://en.wikipedia.org/wiki/Subdominant" )]
		private float mSubdominantInfluence = MusicConstants.OddsMid;

		/// <summary>
		/// influence of the likelihood of playing a subdominant chord in our progression. This isn't straight-odds, and is finessed a little. see:  https://en.wikipedia.org/wiki/Subdominant
		/// </summary>
		public float SubdominantInfluence
		{
			get => mSubdominantInfluence;
			set => mSubdominantInfluence = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
		 Tooltip(
			 "influence of the likelihood of playing a dominant chord in our progression. This isn't straight-odds, and is finessed a little. see:  https://en.wikipedia.org/wiki/Dominant_(music)" )]
		private float mDominantInfluence = MusicConstants.OddsMid;

		/// <summary>
		/// influence of the likelihood of playing a dominant chord in our progression. This isn't straight-odds, and is finessed a little. see:  https://en.wikipedia.org/wiki/Dominant_(music)
		/// </summary>
		public float DominantInfluence
		{
			get => mDominantInfluence;
			set => mDominantInfluence = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
		 Tooltip( "odds of our dominant chord being replaced by a flat-5 substitution. see: https://en.wikipedia.org/wiki/Tritone_substitution" )]
		private float mTritoneSubInfluence = MusicConstants.OddsMid;

		/// <summary>
		/// odds of our dominant chord being replaced by a flat-5 substitution. see: https://en.wikipedia.org/wiki/Tritone_substitution
		/// </summary>
		public float TritoneSubInfluence
		{
			get => mTritoneSubInfluence;
			set => mTritoneSubInfluence = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
		}

		/// <summary>
		/// Number of leitmotif measures we'll use
		/// </summary>
		public int NumLeitmotifMeasures = 1;

		/// <summary>
		/// Number of forced percussion measures we'll use
		/// </summary>
		public int NumForcedPercussionMeasures
		{
			get => mNumForcedPercussionMeasures;
			set => mNumForcedPercussionMeasures = Mathf.Max( 1, value );
		}

		[SerializeField, Tooltip( "Number of forced percussion measures" ), Range( 1, 4 )]
		private int mNumForcedPercussionMeasures = 1;

		#endregion ChordProgressionData

		#region InstrumentData

		[Header( "Instruments" ), Space( MusicConstants.HeaderSpace )]
		[SerializeField, Tooltip( "Our container of instruments" )]
		public List<InstrumentData> Instruments = new List<InstrumentData>();

		/// <summary>
		/// Instrument data is the data associated with a particular instrumnet
		/// </summary>
		[Serializable] public class InstrumentData
		{
			/// <summary>
			/// Memberwise clone of the InstrumentData
			/// </summary>
			/// <returns></returns>
			public InstrumentData Clone()
			{
				//Please ensure we're not including reference types here.
				var clone = (InstrumentData) MemberwiseClone();
				clone.ForcedBeats = ForcedBeats.Clone() as bool[];
				clone.mLeadAvoidNotes = mLeadAvoidNotes.Clone() as int[];
				clone.OctavesToUse = OctavesToUse.Clone();

				clone.WaveformData = new List<BezierControlData>();
				clone.mLeitmotif = Leitmotif.Clone();

				foreach ( var data in WaveformData )
				{
					clone.WaveformData.Add( data );
				}

				clone.EnvelopeData = new List<BezierControlData>();
				foreach ( var data in EnvelopeData )
				{
					clone.EnvelopeData.Add( data );
				}

				clone.PanvelopeData = new List<BezierControlData>();
				foreach ( var data in PanvelopeData )
				{
					clone.PanvelopeData.Add( data );
				}

				if ( CustomSynthWaveform != null )
				{
					clone.CustomSynthWaveform = CustomSynthWaveform.Clone() as float[];
				}

				if ( CustomEnvelope != null )
				{
					clone.CustomEnvelope = CustomEnvelope.Clone() as float[];
				}

				if ( PanvelopeData != null )
				{
					clone.CustomPanvelope = CustomPanvelope.Clone() as float[];
				}

				return clone;
			}

			[Tooltip( "Whether this chord will play a triad or a 7th (tetrad). See: https://en.wikipedia.org/wiki/Chord_(music)#Number_of_notes" )]
			[SerializeField, Range( MusicConstants.TriadCount, MusicConstants.TriadCount + 1 )]
			private int mChordSize = MusicConstants.TriadCount;

			/// <summary>
			/// Whether this chord will play a triad or a 7th (tetrad). See: https://en.wikipedia.org/wiki/Chord_(music)#Number_of_notes
			/// </summary>
			public int ChordSize
			{
				get => mChordSize;
				set => mChordSize = Mathf.Clamp( value, MusicConstants.TriadCount, MusicConstants.TriadCount + 1 );
			}

			[Tooltip( "The color of the m staff player notes used by this instrument:" )]
			public StaffPlayerColors StaffPlayerColor = StaffPlayerColors.Red;

			[Tooltip( "Melody, rhythm or lead. See: https://en.wikipedia.org/wiki/Lead_instrument#Melody_and_harmony" )]
			public SuccessionType SuccessionType = SuccessionType.Rhythm;

			[SerializeField, HideInInspector]
			private Leitmotif mLeitmotif = new Leitmotif();

			/// <summary>
			/// Instrument Leitmotif
			/// </summary>
			public Leitmotif Leitmotif => mLeitmotif;

			[SerializeField, Tooltip( "Our forced percussion notes" )]
			private ForcedPercussionNotes mForcedPercussiveNotes = new ForcedPercussionNotes();

			/// <summary>
			/// Our Forced percussion notes
			/// </summary>
			public ForcedPercussionNotes ForcedPercussiveNotes => mForcedPercussiveNotes;

			[SerializeField, Tooltip( "Name of this instrument type" )]
			private string mInstrumentType = "CelloPlucked";

			/// <summary>
			/// Name of this instrument type
			/// </summary>
			public string InstrumentType
			{
				get => mInstrumentType;
				set
				{
					if ( value.Any( x => char.IsLetterOrDigit( x ) || x == '_' ) == false )
					{
						return;
					}

					mIsSynth = value.Contains( MusicConstants.SynthID );
					mInstrumentType = value;
				}
			}

			/// <summary>
			/// User facing name of this instrument
			/// </summary>
			public string InstrumentName
			{
				get => string.IsNullOrEmpty( mInstrumentName ) ? mInstrumentType : mInstrumentName;
				set
				{
					if ( value.Any( x => char.IsLetterOrDigit( x ) || x == '_' ) == false )
					{
						return;
					}

					mInstrumentName = value;
				}
			}

			[SerializeField, Tooltip( "Our instrument name" )]
			private string mInstrumentName;

			/// <summary>
			/// Whether this is a synth instrument
			/// </summary>
			public bool IsSynth => mIsSynth;

			[SerializeField, HideInInspector]
			private bool mIsSynth;

			/// <summary>
			/// Whether this instrument is a percussion instrument
			/// </summary>
			public bool IsPercussion
			{
				get
				{
					if ( mIsPercussion.HasValue == false )
					{
						mIsPercussion = mInstrumentType.Contains( "P_" );
					}

					return mIsPercussion.Value;
				}
			}

			/// <summary>
			/// Whether this instrument is a percussion instrument
			/// </summary>
			private bool? mIsPercussion;

			[SerializeField, Tooltip( "Whether this instrument uses forced percussion" )]
			private bool mUseForcedPercussion;

			/// <summary>
			/// Whether this instrument uses forced percussion
			/// </summary>
			public bool UseForcedPercussion
			{
				get => mUseForcedPercussion;
				set => mUseForcedPercussion = IsPercussion && value;
			}

			[Range( 0, 2 ), Tooltip( "Which octaves will be used, keep within 0 through 2. See: https://en.wikipedia.org/wiki/Octave" )]
			public ListArrayInt OctavesToUse = new ListArrayInt( new[]
			{
				0, 1, 2
			} );

			[SerializeField, Range( -MusicConstants.NormalizedMax, MusicConstants.NormalizedMax ),
			 Tooltip( "How clips will pan in the audioSource. See: https://docs.unity3d.com/ScriptReference/AudioSource-panStereo.html" )]
			private float mStereoPan;

			/// <summary>
			/// How clips will pan in the audioSource. See: https://docs.unity3d.com/ScriptReference/AudioSource-panStereo.html
			/// </summary>
			public float StereoPan
			{
				get => mStereoPan;
				set => mStereoPan = Mathf.Clamp( value, -MusicConstants.NormalizedMax, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
			 Tooltip( "Odds this note will play, if not already playing. 0-100" )]
			private int mOddsOfPlaying = (int) MusicConstants.OddsMid;

			/// <summary>
			/// Odds this note will play, if not already playing. 0-100
			/// </summary>
			public int OddsOfPlaying
			{
				get => mOddsOfPlaying;
				set => mOddsOfPlaying = Mathf.Clamp( value, (int) MusicConstants.OddsMin, (int) MusicConstants.OddsMax );
			}

			[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
			 Tooltip( "The odds an instrument will play _again_ after successfully playing a first time during a measure" )]
			private float mSuccessivePlayOdds = MusicConstants.BaseSuccessivePlayOdds;

			/// <summary>
			/// The odds an instrument will play _again_ after successfully playing a first time during a measure
			/// </summary>
			public float SuccessivePlayOdds
			{
				get => mSuccessivePlayOdds;
				set => mSuccessivePlayOdds = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ), Tooltip( "Normalized Instrument Volume 0 -1;" )]
			private float mVolume = MusicConstants.NormalizedMid;

			/// <summary>
			/// Normalized Instrument Volume 0 -1;
			/// </summary>
			public float Volume
			{
				get => mVolume;
				set => mVolume = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.MixerGroupVolumeMin, MusicConstants.MixerGroupVolumeMax ),
			 Tooltip( "Volume of this instrument's audio source." )]
			private float mMixerGroupVolume;

			/// <summary>
			/// Volume of this instrument's audio source.
			/// </summary>
			public float MixerGroupVolume
			{
				get => mMixerGroupVolume;
				set => mMixerGroupVolume = Mathf.Clamp( value, MusicConstants.MixerGroupVolumeMin, MusicConstants.MixerGroupVolumeMax );
			}

			[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ), Tooltip( "Odds an octave note will be used: 0 through 100." )]
			private float mOctaveOdds = 20;

			/// <summary>
			/// Odds an octave note will be used: 0 through 100.
			/// </summary>
			public float OctaveOdds
			{
				get => mOctaveOdds;
				set => mOctaveOdds = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
			}

			[SerializeField, Range( 0, MusicConstants.NumGroups - 1 ),
			 Tooltip( "Which instrument group this instrument belongs to. See: https://en.wikipedia.org/wiki/Dynamics_(music)" )]
			private int mGroup;

			/// <summary>
			/// Which instrument group this instrument belongs to. See: https://en.wikipedia.org/wiki/Dynamics_(music)
			/// </summary>
			public int Group
			{
				get => mGroup;
				set => mGroup = Mathf.Clamp( value, 0, MusicConstants.NumGroups - 1 );
			}

			[Tooltip( "Whether the instrument is solo:" )]
			public bool IsSolo;

			[Tooltip( "Whether this instrument uses the pentatonic scale (Lead Only)" )]
			public bool IsPentatonic;

			[SerializeField, Range( 0, MusicConstants.ScaleLength - 1 ), Tooltip( "Our notes to avoid for this lead instrument" )]
			private int[] mLeadAvoidNotes =
			{
				-1, -1
			};

			/// <summary>
			/// Our notes to avoid for this lead instrument
			/// </summary>
			public int[] LeadAvoidNotes
			{
				get => mLeadAvoidNotes;
				set
				{
					for ( var index = 0; index < value.Length; index++ )
					{
						value[index] = Mathf.Clamp( value[index], -1, MusicConstants.ScaleLength );
					}
				}
			}

			[Tooltip( "Whether the uses an arpeggio style (if a melody):" )]
			public bool Arpeggio;

			[SerializeField, Range( MusicConstants.MinArpeggioRepeat, MusicConstants.MaxArpeggioRepeat ), Tooltip( "Number of times we may repeat an arpeggio" )]
			private int mArpeggioRepeat = MusicConstants.BaseArpeggioRepeat;

			/// <summary>
			/// Number of times we may repeat an arpeggio
			/// </summary>
			public int ArpeggioRepeat
			{
				get => mArpeggioRepeat;
				set => mArpeggioRepeat = Mathf.Clamp( value, MusicConstants.MinArpeggioRepeat, MusicConstants.MaxArpeggioRepeat );
			}

			[Tooltip( "Whether instrument is muted" )]
			public bool IsMuted;

			[Tooltip( "Currently used timestep" )]
			public Timestep TimeStep = Timestep.Half;

			/// <summary>
			/// Whether we force the beat for this instrument, ignoring timestep/odds
			/// </summary>
			public bool ForceBeat;

			public bool[] ForcedBeats = new bool[20];

			[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ), Tooltip( "Odds each note of the chord will play" )]
			private float mOddsOfUsingChordNotes = MusicConstants.OddsMid;

			/// <summary>
			/// Odds each note of the chord will play
			/// </summary>
			public float OddsOfUsingChordNotes
			{
				get => mOddsOfUsingChordNotes;
				set => mOddsOfUsingChordNotes = IsPercussion ? 0 : Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
			}

			[SerializeField, Range( 0f, 1f ), Tooltip( "Variation between different strums." )]
			private float mStrumVariation;

			/// <summary>
			/// Variation between different strums.
			/// </summary>
			public float StrumVariation
			{
				get => mStrumVariation;
				set => mStrumVariation = Mathf.Clamp( value, 0f, 10f );
			}

			[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
			 Tooltip( "If we have a redundant melodic note, odds we pick another" )]
			private float mRedundancyAvoidance = MusicConstants.OddsMax;

			/// <summary>
			/// If we have a redundant melodic note, odds we pick another
			/// </summary>
			public float RedundancyAvoidance
			{
				get => mRedundancyAvoidance;
				set => mRedundancyAvoidance = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ), Tooltip( "Delay between notes for strum effect, in seconds" )]
			private float mStrumLength = MusicConstants.NormalizedMin;

			/// <summary>
			/// Delay between notes for strum effect, in seconds
			/// </summary>
			public float StrumLength
			{
				get => mStrumLength;
				set => mStrumLength = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			/// <summary>
			/// Whether we reverse the strum
			/// </summary>
			public bool ReverseStrum;

			/// <summary>
			/// Number of strum notes we have
			/// </summary>
			public int NumStrumNotes
			{
				get => mNumStrumNotes;
				set => mNumStrumNotes = Mathf.Clamp( value, MusicConstants.MinNumStrumNotes, MusicConstants.MaxNumStrumNotes );
			}

			[SerializeField, Range( MusicConstants.MinNumStrumNotes, MusicConstants.MaxNumStrumNotes ), Tooltip( "Number of strum notes" )]
			private int mNumStrumNotes = MusicConstants.BaseNumStrumNotes;

			public string SynthPreset;

			public bool SynthSubOctave1;
			public bool SynthSubOctave2;

			/// <summary>
			/// Synth octave pitch shift
			/// </summary>
			public int SynthOctavePitchShift
			{
				get => mSynthOctavePitchShift;
				set => mSynthOctavePitchShift = Mathf.Clamp( value, MusicConstants.MinSynthOctavePitchShift, MusicConstants.MaxSynthOctavePitchShift );
			}

			[SerializeField, Range( MusicConstants.MinSynthOctavePitchShift, MusicConstants.MaxSynthOctavePitchShift ), Tooltip( "Synth octave pitch shift" )]
			private int mSynthOctavePitchShift = 3;

			public float SynthNoteLength
			{
				get => mSynthLength;
				set => mSynthLength = Mathf.Clamp( value, MusicConstants.MinSynthLength, MusicConstants.MaxSynthLength );
			}

			[SerializeField, Range( MusicConstants.MinSynthLength, MusicConstants.MaxSynthLength ), Tooltip( "Synth note length" )]
			private float mSynthLength = MusicConstants.NormalizedMid;

			public float SynthAttack
			{
				get => mSynthAttack;
				set => mSynthAttack = Mathf.Clamp( value, MusicConstants.MinSynthRamp, MusicConstants.MaxSynthRamp );
			}

			[SerializeField, Range( MusicConstants.MinSynthRamp, MusicConstants.MaxSynthRamp ), Tooltip( "Synth Ramp up time" )]
			private float mSynthAttack = MusicConstants.MinSynthRamp;

			public float SynthDecay
			{
				get => mSynthDecay;
				set => mSynthDecay = Mathf.Clamp( value, MusicConstants.MinSynthRamp, MusicConstants.MaxSynthRamp );
			}

			[SerializeField, Range( MusicConstants.MinSynthRamp, MusicConstants.MaxSynthRamp ), Tooltip( "Synth Ramp down time" )]
			private float mSynthDecay = MusicConstants.MinSynthRamp;

			public SynthWaveType[] SynthWaveTypes =
			{
				SynthWaveType.Square, SynthWaveType.None, SynthWaveType.None, SynthWaveType.None
			};

			public WaveOperator[] SynthWaveOperators =
			{
				WaveOperator.Add, WaveOperator.Add, WaveOperator.Add
			};

			public float[] CustomSynthWaveform =
			{
				0f, 1f, -1f, 0f
			};

			public float[] CustomEnvelope =
			{
				1f, 1f, 0f
			};

			public float[] CustomPanvelope =
			{
				0, 0, 0
			};

			public List<BezierControlData> WaveformData = new List<BezierControlData>();
			public List<BezierControlData> EnvelopeData = new List<BezierControlData>();
			public List<BezierControlData> PanvelopeData = new List<BezierControlData>();

			public bool IsCustomWaveform;

			#region reverbSFX

			[Header( "Instrument Effects Values" ), Space( MusicConstants.HeaderSpace )]
			[SerializeField, Range( MusicConstants.MinReverbRoom, MusicConstants.MaxReverbRoom ), Tooltip( "RoomSize SFX" )]
			private float mRoomSize = MusicConstants.BaseReverbRoom;

			/// <summary>
			/// RoomSize SFX
			/// </summary>
			public float RoomSize
			{
				get => mRoomSize;
				set => mRoomSize = Mathf.Clamp( value, MusicConstants.MinReverbRoom, MusicConstants.MaxReverbRoom );
			}

			[SerializeField, Range( MusicConstants.MinReverb, MusicConstants.MaxReverb ), Tooltip( "Instrument Reverb" )]
			private float mReverb = MusicConstants.BaseReverb;

			/// <summary>
			/// Instrument Reverb
			/// </summary>
			public float Reverb
			{
				get => mReverb;
				set => mReverb = Mathf.Clamp( value, MusicConstants.MinReverb, MusicConstants.MaxReverb );
			}

			[SerializeField, Range( MusicConstants.MinReverbDry, MusicConstants.MaxReverbDry ), Tooltip( "Instrument Reverb Dry" )]
			private float mReverbDry = MusicConstants.BaseReverbDry;

			/// <summary>
			/// Instrument Reverb Dry 
			/// </summary>
			public float ReverbDry
			{
				get => mReverbDry;
				set => mReverbDry = Mathf.Clamp( value, MusicConstants.MinReverbDry, MusicConstants.MaxReverbDry );
			}

			[SerializeField, Range( MusicConstants.MinReverbDelay, MusicConstants.MaxReverbDelay ), Tooltip( "Instrument Reverb Delay" )]
			private float mReverbDelay = MusicConstants.BaseReverbDelay;

			/// <summary>
			/// Instrument Reverb Delay 
			/// </summary>
			public float ReverbDelay
			{
				get => mReverbDelay;
				set => mReverbDelay = Mathf.Clamp( value, MusicConstants.MinReverbDelay, MusicConstants.MaxReverbDelay );
			}

			[SerializeField, Range( MusicConstants.MinReverbRoomHF, MusicConstants.MaxReverbRoomHF ), Tooltip( "Instrument Reverb Room HF" )]
			private float mReverbRoomHF = MusicConstants.BaseReverbRoomHF;

			/// <summary>
			/// Instrument Reverb Room HF 
			/// </summary>
			public float ReverbRoomHF
			{
				get => mReverbRoomHF;
				set => mReverbRoomHF = Mathf.Clamp( value, MusicConstants.MinReverbRoomHF, MusicConstants.MaxReverbRoomHF );
			}

			[SerializeField, Range( MusicConstants.MinReverbDecayTime, MusicConstants.MaxReverbDecayTime ), Tooltip( "Instrument Reverb Decay Time" )]
			private float mReverbDecayTime = MusicConstants.BaseReverbDecayTime;

			/// <summary>
			/// Instrument Reverb Decay Time 
			/// </summary>
			public float ReverbDecayTime
			{
				get => mReverbDecayTime;
				set => mReverbDecayTime = Mathf.Clamp( value, MusicConstants.MinReverbDecayTime, MusicConstants.MaxReverbDecayTime );
			}

			[SerializeField, Range( MusicConstants.MinReverbDecayHFRatio, MusicConstants.MaxReverbDecayHFRatio ),
			 Tooltip( "Instrument Reverb Decay HF Ratio" )]
			private float mReverbDecayHFRatio = MusicConstants.BaseReverbDecayHFRatio;

			/// <summary>
			/// Instrument Reverb Decay HF Ratio
			/// </summary>
			public float ReverbDecayHFRatio
			{
				get => mReverbDecayHFRatio;
				set => mReverbDecayHFRatio = Mathf.Clamp( value, MusicConstants.MinReverbDecayHFRatio, MusicConstants.MaxReverbDecayHFRatio );
			}

			[SerializeField, Range( MusicConstants.MinReverbReflections, MusicConstants.MaxInstrumentReverbReflections ),
			 Tooltip( "Instrument Reverb Reflections" )]
			private float mReverbReflections = MusicConstants.BaseReverbReflections;

			/// <summary>
			/// Instrument Reverb Reflections
			/// </summary>
			public float ReverbReflections
			{
				get => mReverbReflections;
				set => mReverbReflections = Mathf.Clamp( value, MusicConstants.MinReverbReflections, MusicConstants.MaxInstrumentReverbReflections );
			}

			[SerializeField, Range( MusicConstants.MinReverbReflectDelay, MusicConstants.MaxReverbReflectDelay ),
			 Tooltip( "Instrument Reverb Reflect Delay" )]
			private float mReverbReflectDelay = MusicConstants.BaseReverbReflectDelay;

			/// <summary>
			/// Instrument Reverb Reflect Delay
			/// </summary>
			public float ReverbReflectDelay
			{
				get => mReverbReflectDelay;
				set => mReverbReflectDelay = Mathf.Clamp( value, MusicConstants.MinReverbReflectDelay, MusicConstants.MaxReverbReflectDelay );
			}

			[SerializeField, Range( MusicConstants.MinReverbDiffusion, MusicConstants.MaxReverbDiffusion ), Tooltip( "Instrument Reverb Diffusion" )]
			private float mReverbDiffusion = MusicConstants.BaseReverbDiffusion;

			/// <summary>
			/// Instrument Reverb Diffusion
			/// </summary>
			public float ReverbDiffusion
			{
				get => mReverbDiffusion;
				set => mReverbDiffusion = Mathf.Clamp( value, MusicConstants.MinReverbDiffusion, MusicConstants.MaxReverbDiffusion );
			}

			[SerializeField, Range( MusicConstants.MinReverbDensity, MusicConstants.MaxReverbDensity ), Tooltip( "Instrument Reverb Density" )]
			private float mReverbDensity = MusicConstants.BaseReverbDensity;

			/// <summary>
			/// Instrument Reverb Density
			/// </summary>
			public float ReverbDensity
			{
				get => mReverbDensity;
				set => mReverbDensity = Mathf.Clamp( value, MusicConstants.MinReverbDensity, MusicConstants.MaxReverbDensity );
			}

			[SerializeField, Range( MusicConstants.MinReverbHFReference, MusicConstants.MaxReverbHFReference ), Tooltip( "Instrument Reverb HFReference" )]
			private float mReverbHFReference = MusicConstants.BaseReverbHFReference;

			/// <summary>
			/// Instrument Reverb HF Reference
			/// </summary>
			public float ReverbHFReference
			{
				get => mReverbHFReference;
				set => mReverbHFReference = Mathf.Clamp( value, MusicConstants.MinReverbHFReference, MusicConstants.MaxReverbHFReference );
			}

			[SerializeField, Range( MusicConstants.MinReverbRoomLF, MusicConstants.MaxReverbRoomLF ), Tooltip( "Instrument Reverb Room LF" )]
			private float mReverbRoomLF = MusicConstants.BaseReverbRoomLF;

			/// <summary>
			/// Instrument Reverb Room LF
			/// </summary>
			public float ReverbRoomLF
			{
				get => mReverbRoomLF;
				set => mReverbRoomLF = Mathf.Clamp( value, MusicConstants.MinReverbRoomLF, MusicConstants.MaxReverbRoomLF );
			}

			[SerializeField, Range( MusicConstants.MinReverbLFReference, MusicConstants.MaxReverbLFReference ),
			 Tooltip( "Instrument Reverb LF Reference" )]
			private float mReverbLFReference = MusicConstants.BaseReverbLFReference;

			/// <summary>
			/// Instrument Reverb LF Reference
			/// </summary>
			public float ReverbLFReference
			{
				get => mReverbLFReference;
				set => mReverbLFReference = Mathf.Clamp( value, MusicConstants.MinReverbLFReference, MusicConstants.MaxReverbLFReference );
			}

			#endregion reverbSFX

			#region echoSFX

			[SerializeField, Range( MusicConstants.MinEchoWet, MusicConstants.MaxEchoWet ), Tooltip( "Instrument Echo SFX" )]
			private float mEcho = MusicConstants.BaseEchoWet;

			/// <summary>
			/// Instrument Echo SFX
			/// </summary>
			public float Echo
			{
				get => mEcho;
				set => mEcho = Mathf.Clamp( value, MusicConstants.MinEchoWet, MusicConstants.MaxEchoWet );
			}

			[SerializeField, Range( MusicConstants.MinEchoDry, MusicConstants.MaxEchoDry ), Tooltip( "Instrument Echo Dry FX" )]
			private float mEchoDry = MusicConstants.BaseEchoDry;

			public float EchoDry
			{
				get => mEchoDry;
				set => mEchoDry = Mathf.Clamp( value, MusicConstants.MinEchoDry, MusicConstants.MaxEchoDry );
			}

			[SerializeField, Range( MusicConstants.MinEchoDelay, MusicConstants.MaxEchoDelay ),
			 Tooltip( "Instrument EchoDelay SFX" )]
			private float mEchoDelay = MusicConstants.BaseEchoDelay;

			/// <summary>
			/// Instrument EchoDelay SFX
			/// </summary>
			public float EchoDelay
			{
				get => mEchoDelay;
				set => mEchoDelay = Mathf.Clamp( value, MusicConstants.MinEchoDelay, MusicConstants.MaxEchoDelay );
			}

			[SerializeField, Range( MusicConstants.MinEchoDecay, MusicConstants.MaxEchoDecay ),
			 Tooltip( "Instrument EchoDecay SFX" )]
			private float mEchoDecay = MusicConstants.BaseEchoDecay;

			/// <summary>
			/// Instrument EchoDecay SFX
			/// </summary>
			public float EchoDecay
			{
				get => mEchoDecay;
				set => mEchoDecay = Mathf.Clamp( value, MusicConstants.MinEchoDecay, MusicConstants.MaxEchoDecay );
			}

			#endregion echoSFX

			#region flangeSFX

			[SerializeField, Range( MusicConstants.MinFlangeWet, MusicConstants.MaxFlangeWet ), Tooltip( "Instrument Flanger SFX" )]
			private float mFlanger = MusicConstants.BaseFlangeWet;

			/// <summary>
			/// Instrument Flanger SFX
			/// </summary>
			public float Flanger
			{
				get => mFlanger;
				set => mFlanger = Mathf.Clamp( value, MusicConstants.MinFlangeWet, MusicConstants.MaxFlangeWet );
			}

			[SerializeField, Range( MusicConstants.MinFlangeDry, MusicConstants.MaxFlangeDry ), Tooltip( "Instrument Flange Dry SFX" )]
			private float mFlangeDry = MusicConstants.BaseFlangeDry;

			/// <summary>
			/// Instrument Flange Dry SFX
			/// </summary>
			public float FlangeDry
			{
				get => mFlangeDry;
				set => mFlangeDry = Mathf.Clamp( value, MusicConstants.MinFlangeDry, MusicConstants.MaxFlangeDry );
			}

			[SerializeField, Range( MusicConstants.MinFlangeDepth, MusicConstants.MaxFlangeDepth ), Tooltip( "Instrument Flange Depth SFX" )]
			private float mFlangeDepth = MusicConstants.BaseFlangeDepth;

			/// <summary>
			/// Instrument Flange Depth SFX
			/// </summary>
			public float FlangeDepth
			{
				get => mFlangeDepth;
				set => mFlangeDepth = Mathf.Clamp( value, MusicConstants.MinFlangeDepth, MusicConstants.MaxFlangeDepth );
			}

			[SerializeField, Range( MusicConstants.MinFlangeRate, MusicConstants.MaxFlangeRate ), Tooltip( "Instrument Flange Rate SFX" )]
			private float mFlangeRate = MusicConstants.BaseFlangeRate;

			/// <summary>
			/// Instrument Flange Rate SFX
			/// </summary>
			public float FlangeRate
			{
				get => mFlangeRate;
				set => mFlangeRate = Mathf.Clamp( value, MusicConstants.MinFlangeRate, MusicConstants.MaxFlangeRate );
			}

			#endregion flangeSFX

			#region distortionSFX

			[SerializeField, Range( MusicConstants.MinDistortion, MusicConstants.MaxDistortion ),
			 Tooltip( "Instrument Distortion SFX" )]
			private float mDistortion = MusicConstants.BaseDistortion;

			/// <summary>
			/// Instrument Distortion SFX
			/// </summary>
			public float Distortion
			{
				get => mDistortion;
				set => mDistortion = Mathf.Clamp( value, MusicConstants.MinDistortion, MusicConstants.MaxDistortion );
			}

			#endregion distortionSFX

			#region chorusSFX

			[SerializeField, Range( MusicConstants.MinChorusWetMixTap1, MusicConstants.MaxChorusWetMixTap1 ), Tooltip( "Instrument Chorus SFX" )]
			private float mChorus = MusicConstants.BaseChorusWetMixTap1;

			/// <summary>
			/// Instrument Chorus SFX
			/// </summary>
			public float Chorus
			{
				get => mChorus;
				set => mChorus = Mathf.Clamp( value, MusicConstants.MinChorusWetMixTap1, MusicConstants.MaxChorusWetMixTap1 );
			}

			[SerializeField, Range( MusicConstants.MinChorusWetMixTap2, MusicConstants.MaxChorusWetMixTap2 ), Tooltip( "Instrument Chorus SFX" )]
			private float mChorus2 = MusicConstants.BaseChorusWetMixTap2;

			/// <summary>
			/// Instrument Chorus 2 SFX
			/// </summary>
			public float Chorus2
			{
				get => mChorus2;
				set => mChorus2 = Mathf.Clamp( value, MusicConstants.MinChorusWetMixTap2, MusicConstants.MaxChorusWetMixTap2 );
			}

			[SerializeField, Range( MusicConstants.MinChorusWetMixTap3, MusicConstants.MaxChorusWetMixTap3 ), Tooltip( "Instrument Chorus SFX" )]
			private float mChorus3 = MusicConstants.BaseChorusWetMixTap3;

			/// <summary>
			/// Instrument Chorus SFX
			/// </summary>
			public float Chorus3
			{
				get => mChorus3;
				set => mChorus3 = Mathf.Clamp( value, MusicConstants.MinChorusWetMixTap3, MusicConstants.MaxChorusWetMixTap3 );
			}

			[SerializeField, Range( MusicConstants.MinChorusDry, MusicConstants.MaxChorusDry ), Tooltip( "Instrument Chorus Dry SFX" )]
			private float mChorusDry = MusicConstants.BaseChorusDry;

			/// <summary>
			/// Instrument Chorus Dry SFX
			/// </summary>
			public float ChorusDry
			{
				get => mChorusDry;
				set => mChorusDry = Mathf.Clamp( value, MusicConstants.MinChorusDry, MusicConstants.MaxChorusDry );
			}

			[SerializeField, Range( MusicConstants.MinChorusDelay, MusicConstants.MaxChorusDelay ), Tooltip( "Instrument Chorus Delay SFX" )]
			private float mChorusDelay = MusicConstants.BaseChorusDelay;

			/// <summary>
			/// Instrument Chorus Delay SFX
			/// </summary>
			public float ChorusDelay
			{
				get => mChorusDelay;
				set => mChorusDelay = Mathf.Clamp( value, MusicConstants.MinChorusDelay, MusicConstants.MaxChorusDelay );
			}

			[SerializeField, Range( MusicConstants.MinChorusRate, MusicConstants.MaxChorusRate ), Tooltip( "Instrument Chorus rate SFX" )]
			private float mChorusRate = MusicConstants.BaseChorusRate;

			/// <summary>
			/// Instrument Chorus Rate SFX
			/// </summary>
			public float ChorusRate
			{
				get => mChorusRate;
				set => mChorusRate = Mathf.Clamp( value, MusicConstants.MinChorusRate, MusicConstants.MaxChorusRate );
			}

			[SerializeField, Range( MusicConstants.MinChorusDepth, MusicConstants.MaxChorusDepth ), Tooltip( "Instrument Chorus depth SFX" )]
			private float mChorusDepth = MusicConstants.BaseChorusDepth;

			/// <summary>
			/// Instrument Chorus Depth SFX
			/// </summary>
			public float ChorusDepth
			{
				get => mChorusDepth;
				set => mChorusDepth = Mathf.Clamp( value, MusicConstants.MinChorusDepth, MusicConstants.MaxChorusDepth );
			}

			[SerializeField, Range( MusicConstants.MinChorusFeedback, MusicConstants.MaxChorusFeedback ), Tooltip( "Instrument Chorus feedback SFX" )]
			private float mChorusFeedback = MusicConstants.BaseChorusFeedback;

			/// <summary>
			/// Instrument Chorus Feedback SFX
			/// </summary>
			public float ChorusFeedback
			{
				get => mChorusFeedback;
				set => mChorusFeedback = Mathf.Clamp( value, MusicConstants.MinChorusFeedback, MusicConstants.MaxChorusFeedback );
			}

			#endregion chorusSFX

			#region paramEQ

			[SerializeField, Range( MusicConstants.MinParamEQCenterFreq, MusicConstants.MaxParamEQCenterFreq ),
			 Tooltip( "Instrument ParamEQ Center Freq. SFX" )]
			private float mParamEQCenterFreq = MusicConstants.BaseParamEQCenterFreq;

			/// <summary>
			/// Instrument ParamEQ Center Freq
			/// </summary>
			public float ParamEQCenterFreq
			{
				get => mParamEQCenterFreq;
				set => mParamEQCenterFreq = Mathf.Clamp( value, MusicConstants.MinParamEQCenterFreq, MusicConstants.MaxParamEQCenterFreq );
			}

			[SerializeField, Range( MusicConstants.MinParamEQOctaveRange, MusicConstants.MaxParamEQOctaveRange ),
			 Tooltip( "Instrument ParamEQ Octave Range SFX" )]
			private float mParamEQOctaveRange = MusicConstants.BaseParamEQOctaveRange;

			/// <summary>
			/// Instrument ParamEQ Octave Range
			/// </summary>
			public float ParamEQOctaveRange
			{
				get => mParamEQOctaveRange;
				set => mParamEQOctaveRange = Mathf.Clamp( value, MusicConstants.MinParamEQOctaveRange, MusicConstants.MaxParamEQOctaveRange );
			}

			[SerializeField, Range( MusicConstants.MinParamEQFreqGain, MusicConstants.MaxParamEQFreqGain ),
			 Tooltip( "Instrument ParamEQ Freq Gain SFX" )]
			private float mParamEQFreqGain = MusicConstants.BaseParamEQFreqGain;

			/// <summary>
			/// Instrument ParamEQ Freq Gain
			/// </summary>
			public float ParamEQFreqGain
			{
				get => mParamEQFreqGain;
				set => mParamEQFreqGain = Mathf.Clamp( value, MusicConstants.MinParamEQFreqGain, MusicConstants.MaxParamEQFreqGain );
			}

			#endregion paramEQ

			#region FMOD

#if FMOD_ENABLED
			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod FlangerRate Value" )]
			private float mFmodFlangerRate = MusicConstants.BaseFmodFlangerRate;

			public float FmodFlangerRate
			{
				get => mFmodFlangerRate;
				set => mFmodFlangerRate = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod FlangerDepth Value" )]
			private float mFmodFlangerDepth = MusicConstants.BaseFmodFlangerDepth;

			public float FmodFlangerDepth
			{
				get => mFmodFlangerDepth;
				set => mFmodFlangerDepth = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod FlangerMix Value" )]
			private float mFmodFlangerMix = MusicConstants.BaseFmodFlangerMix;

			public float FmodFlangerMix
			{
				get => mFmodFlangerMix;
				set => mFmodFlangerMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod Delay Value" )]
			private float mFmodDelay = MusicConstants.BaseFmodDelay;

			public float FmodDelay
			{
				get => mFmodDelay;
				set => mFmodDelay = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod DelayWet Value" )]
			private float mFmodDelayWet = MusicConstants.BaseFmodDelayWet;

			public float FmodDelayWet
			{
				get => mFmodDelayWet;
				set => mFmodDelayWet = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod DelayFeedback Value" )]
			private float mFmodDelayFeedback = MusicConstants.BaseFmodDelayFeedback;

			public float FmodDelayFeedback
			{
				get => mFmodDelayFeedback;
				set => mFmodDelayFeedback = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod DelayDry Value" )]
			private float mFmodDelayDry = MusicConstants.BaseFmodDelayDry;

			public float FmodDelayDry
			{
				get => mFmodDelayDry;
				set => mFmodDelayDry = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbTime Value" )]
			private float mFmodReverbTime = MusicConstants.BaseFmodReverbTime;

			public float FmodReverbTime
			{
				get => mFmodReverbTime;
				set => mFmodReverbTime = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbHFDecay Value" )]
			private float mFmodReverbHFDecay = MusicConstants.BaseFmodReverbHFDecay;

			public float FmodReverbHFDecay
			{
				get => mFmodReverbHFDecay;
				set => mFmodReverbHFDecay = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbDiffusion Value" )]
			private float mFmodReverbDiffusion = MusicConstants.BaseFmodReverbDiffusion;

			public float FmodReverbDiffusion
			{
				get => mFmodReverbDiffusion;
				set => mFmodReverbDiffusion = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbLowGain Value" )]
			private float mFmodReverbLowGain = MusicConstants.BaseFmodReverbLowGain;

			public float FmodReverbLowGain
			{
				get => mFmodReverbLowGain;
				set => mFmodReverbLowGain = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbHighCut Value" )]
			private float mFmodReverbHighCut = MusicConstants.BaseFmodReverbHighCut;

			public float FmodReverbHighCut
			{
				get => mFmodReverbHighCut;
				set => mFmodReverbHighCut = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbWetMix Value" )]
			private float mFmodReverbWetMix = MusicConstants.BaseFmodReverbWetMix;

			public float FmodReverbWetMix
			{
				get => mFmodReverbWetMix;
				set => mFmodReverbWetMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbDryMix Value" )]
			private float mFmodReverbDryMix = MusicConstants.BaseFmodReverbDryMix;

			public float FmodReverbDryMix
			{
				get => mFmodReverbDryMix;
				set => mFmodReverbDryMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbEarlyDelay Value" )]
			private float mFmodReverbEarlyDelay = MusicConstants.BaseFmodReverbEarlyDelay;

			public float FmodReverbEarlyDelay
			{
				get => mFmodReverbEarlyDelay;
				set => mFmodReverbEarlyDelay = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbLateDelay Value" )]
			private float mFmodReverbLateDelay = MusicConstants.BaseFmodReverbLateDelay;

			public float FmodReverbLateDelay
			{
				get => mFmodReverbLateDelay;
				set => mFmodReverbLateDelay = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbHFReference Value" )]
			private float mFmodReverbHFReference = MusicConstants.BaseFmodReverbHFReference;

			public float FmodReverbHFReference
			{
				get => mFmodReverbHFReference;
				set => mFmodReverbHFReference = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbDensity Value" )]
			private float mFmodReverbDensity = MusicConstants.BaseFmodReverbDensity;

			public float FmodReverbDensity
			{
				get => mFmodReverbDensity;
				set => mFmodReverbDensity = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbLowFreq Value" )]
			private float mFmodReverbLowFreq = MusicConstants.BaseFmodReverbLowFreq;

			public float FmodReverbLowFreq
			{
				get => mFmodReverbLowFreq;
				set => mFmodReverbLowFreq = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ReverbEarlyLateMix Value" )]
			private float mFmodReverbEarlyLateMix = MusicConstants.BaseFmodReverbEarlyLateMix;

			public float FmodReverbEarlyLateMix
			{
				get => mFmodReverbEarlyLateMix;
				set => mFmodReverbEarlyLateMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod Distortion Value" )]
			private float mFmodDistortion = MusicConstants.BaseFmodDistortion;

			public float FmodDistortion
			{
				get => mFmodDistortion;
				set => mFmodDistortion = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ChorusRate Value" )]
			private float mFmodChorusRate = MusicConstants.BaseFmodChorusRate;

			public float FmodChorusRate
			{
				get => mFmodChorusRate;
				set => mFmodChorusRate = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ChorusDepth Value" )]
			private float mFmodChorusDepth = MusicConstants.BaseFmodChorusDepth;

			public float FmodChorusDepth
			{
				get => mFmodChorusDepth;
				set => mFmodChorusDepth = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod ChorusMix Value" )]
			private float mFmodChorusMix = MusicConstants.BaseFmodChorusMix;

			public float FmodChorusMix
			{
				get => mFmodChorusMix;
				set => mFmodChorusMix = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod EQLow Value" )]
			private float mFmodEQLow = MusicConstants.BaseFmodEQLow;

			public float FmodEQLow
			{
				get => mFmodEQLow;
				set => mFmodEQLow = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod EQMid Value" )]
			private float mFmodEQMid = MusicConstants.BaseFmodEQMid;

			public float FmodEQMid
			{
				get => mFmodEQMid;
				set => mFmodEQMid = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod EQHigh Value" )]
			private float mFmodEQHigh = MusicConstants.BaseFmodEQHigh;

			public float FmodEQHigh
			{
				get => mFmodEQHigh;
				set => mFmodEQHigh = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod EQXHigh Value" )]
			private float mFmodEQXHigh = MusicConstants.BaseFmodEQXHigh;

			public float FmodEQXHigh
			{
				get => mFmodEQXHigh;
				set => mFmodEQXHigh = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod EQXLow Value" )]
			private float mFmodEQXLow = MusicConstants.BaseFmodEQXLow;

			public float FmodEQXLow
			{
				get => mFmodEQXLow;
				set => mFmodEQXLow = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod Panner Value" )]
			private float mFmodPanner = 0f;

			public float FmodPanner
			{
				get => mFmodPanner;
				set => mFmodPanner = Mathf.Clamp( value, -MusicConstants.NormalizedMax, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod Volume Value" )]
			private float m_fmodFader = MusicConstants.BaseFmodFader;

			public float FmodFader
			{
				get => m_fmodFader;
				set => m_fmodFader = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

			[SerializeField, Range( MusicConstants.NormalizedMin, MusicConstants.NormalizedMax ),
			 Tooltip( "Fmod Gain Value" )]
			private float mFmodGain = MusicConstants.BaseFmodGain;

			public float FmodGain
			{
				get => mFmodGain;
				set => mFmodGain = Mathf.Clamp( value, MusicConstants.NormalizedMin, MusicConstants.NormalizedMax );
			}

#endif //FMOD_ENABLED

			#endregion FMOD

			[Header( "Instrument Pattern Settings" ), Space( MusicConstants.HeaderSpace )]
			[Tooltip( "Whether we'll use a pattern for this instrument" )]
			public bool UsePattern;

			[SerializeField, Range( MusicConstants.InstrumentPatternLengthMin, MusicConstants.InstrumentPatternLengthMax ),
			 Tooltip( "Length of our pattern:" )]
			private int mPatternlength = MusicConstants.BaseInstrumentPatternLength;

			/// <summary>
			/// Length of our pattern
			/// </summary>
			public int PatternLength
			{
				get => mPatternlength;
				set => mPatternlength = Mathf.Clamp( value, MusicConstants.InstrumentPatternLengthMin, MusicConstants.InstrumentPatternLengthMax );
			}

			[SerializeField, Range( MusicConstants.InstrumentPatternLengthMin, MusicConstants.InstrumentPatternLengthMax ),
			 Tooltip( "At which point we stop using the pattern" )]
			private int mPatternRelease = MusicConstants.BaseInstrumentPatternRelease;

			/// <summary>
			/// At which point we stop using the pattern
			/// </summary>
			public int PatternRelease
			{
				get => mPatternRelease;
				set => mPatternRelease = Mathf.Clamp( value, MusicConstants.InstrumentPatternLengthMin, MusicConstants.InstrumentPatternLengthMax );
			}

			[SerializeField, Range( MusicConstants.InstrumentPatternRepeatMin, MusicConstants.InstrumentPatternRepeatMax ),
			 Tooltip( "How many times we will repeat the pattern" )]
			private int mPatternRepeat = MusicConstants.BaseInstrumentPatternRepeat;

			/// <summary>
			/// How many times we will repeat the pattern
			/// </summary>
			public int PatternRepeat
			{
				get => mPatternRepeat;
				set => mPatternRepeat = Mathf.Clamp( value, MusicConstants.InstrumentPatternRepeatMin, MusicConstants.InstrumentPatternRepeatMax );
			}

			[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ),
			 Tooltip( "Odds we ever play the same note twice. No UI controller for this. Set manually." )]
			private float mRedundancyOdds = MusicConstants.OddsMid;

			/// <summary>
			/// Odds we ever play the same note twice. No UI controller for this. Set manually.
			/// </summary>
			public float RedundancyOdds
			{
				get => mRedundancyOdds;
				set => mRedundancyOdds = Mathf.Clamp( value, 1, MusicConstants.OddsMax );
			}

			[SerializeField, Range( 1, MusicConstants.ScaleLength ), Tooltip( "How many scale steps a melody can take at once" )]
			private int mLeadMaxSteps = MusicConstants.BaseInstrumentLeadMaxSteps;

			/// <summary>
			/// How many scale steps a melody can take at once
			/// </summary>
			public int LeadMaxSteps
			{
				get => mLeadMaxSteps;
				set => mLeadMaxSteps = Mathf.Clamp( value, 1, MusicConstants.ScaleLength );
			}

			[SerializeField, Range( MusicConstants.OddsMin, MusicConstants.OddsMax ), Tooltip( "Likelihood of melody continuing to ascend/descend" )]
			private float mAscendDescendInfluence = MusicConstants.OddsMid;

			/// <summary>
			/// Likelihood of melody continuing to ascend/descend
			/// </summary>
			public float AscendDescendInfluence
			{
				get => mAscendDescendInfluence;
				set => mAscendDescendInfluence = Mathf.Clamp( value, MusicConstants.OddsMin, MusicConstants.OddsMax );
			}

			[SerializeField, Range( -1, 1 ), Tooltip( "How much we tend to ascend/descend" )]
			private int mLeadInfluence = MusicConstants.AscendingInfluence;

			/// <summary>
			/// How much we tend to ascend/descend
			/// </summary>
			public int LeadInfluence
			{
				get => mLeadInfluence;
				set
				{
					value = value == 0 ? 1 : value;
					mLeadInfluence = Mathf.Clamp( value, -1, 1 );
				}
			}

			/// <summary>
			/// Our ClipNotes for this instrument
			/// </summary>
			public List<ClipNotesMeasure> ClipNotes = new List<ClipNotesMeasure>();

			/// <summary>
			/// If a melodic instrument has this selected, it will _always_ play the MinMelodicRhythmTimestep
			/// </summary>
			public bool KeepMelodicRhythm;

			[Tooltip( "Minimum rhythm timestep used for melodic instruments" )]
			public Timestep MinMelodicRhythmTimeStep = Timestep.Quarter;
		}

		#endregion InstrumentData

		public static void LoadConfigurationData( string configurationName, Action<ConfigurationData> callback )
		{
			if ( LoadConfigurationDataFromDirectory( Path.Combine( MusicConstants.ConfigurationPersistentDataPath, $"{configurationName}.txt" ),
				callback ) == false )
			{
				LoadConfigurationDataFromAddressable( configurationName, MusicConstants.ConfigurationDataAddressableKey, callback );
			}
		}

		public static void LoadConfigurationDataFromAddressable<T>( string configurationName, string addressableKey, Action<T> callback )
		{
			var handleKey = $"{addressableKey}/{configurationName}.txt";
			var handle = Addressables.LoadAssetAsync<TextAsset>( handleKey );

			if ( handle.IsValid() == false )
			{
				Debug.LogError( "Please run the setup scripts as outlined in MusicGenerator/Documentation/Procedural Music Generator Documentation.pdf" );
				throw new ArgumentNullException( $"Generator configuration does not exist for {configurationName} " );
			}

			handle.Completed += ( result ) =>
			{
				switch ( handle.Status )
				{
					case AsyncOperationStatus.Succeeded:
						callback.Invoke( JsonUtility.FromJson<T>( result.Result.text ) );
						Addressables.Release( handle );
						break;
					case AsyncOperationStatus.Failed:
						Debug.LogError( "Please run the setup scripts as outlined in MusicGenerator/Documentation/Procedural Music Generator Documentation.pdf" );
						throw new ArgumentNullException( $"Generator configuration does not exist for {configurationName} " );
				}
			};
		}

		private static bool LoadConfigurationDataFromDirectory<T>( string persistentPath, Action<T> callback )
		{
			if ( File.Exists( persistentPath ) == false )
			{
				return false;
			}

			callback.Invoke( JsonUtility.FromJson<T>( File.ReadAllText( persistentPath ) ) );
			return true;
		}

		public static void SaveConfigurationData( ConfigurationData configurationData )
		{
			Debug.Log( $"saving configuration {configurationData.ConfigurationName} to {MusicConstants.ConfigurationPersistentDataPath}" );

			if ( Directory.Exists( MusicConstants.ConfigurationPersistentDataPath ) == false )
			{
				Directory.CreateDirectory( MusicConstants.ConfigurationPersistentDataPath );
			}

			try
			{
				var path = Path.Combine( MusicConstants.ConfigurationPersistentDataPath, $"{configurationData.ConfigurationName}.txt" );
				File.WriteAllText( path, JsonUtility.ToJson( configurationData, prettyPrint: true ) );
				Debug.Log( $"{configurationData.ConfigurationName} was successfully written to file" );
			}
			catch ( IOException e )
			{
				Debug.Log( $"{configurationData.ConfigurationName} failed to write to file with exception {e}" );
			}
		}

		public static void LoadClipConfigurationData( string configurationName, Action<ConfigurationData> callback )
		{
			if ( LoadConfigurationDataFromDirectory( Path.Combine( MusicConstants.PersistentClipsPath, $"{configurationName}.txt" ),
				callback ) == false )
			{
				LoadConfigurationDataFromAddressable( configurationName, MusicConstants.InstrumentClipsAddressableKey, callback );
			}
		}

		public static void SaveClipConfigurationData( ConfigurationData configurationData )
		{
#if UNITY_EDITOR == false
			return;
#else
			configurationData.Version = MusicConstants.Version;

			Debug.Log( $"saving configuration {configurationData.ConfigurationName} to {MusicConstants.PersistentClipsPath}" );

			if ( Directory.Exists( MusicConstants.PersistentClipsPath ) == false )
			{
				Directory.CreateDirectory( MusicConstants.PersistentClipsPath );
			}

			try
			{
				var path = Path.Combine( MusicConstants.PersistentClipsPath, $"{configurationData.ConfigurationName}.txt" );
				File.WriteAllText( path, JsonUtility.ToJson( configurationData, prettyPrint: true ) );
				Debug.Log( $"{configurationData.ConfigurationName} was successfully written to file" );
			}
			catch ( IOException e )
			{
				Debug.Log( $"{configurationData.ConfigurationName} failed to write to file with exception {e}" );
			}
#endif
		}

		public static void LoadModData<T>( string modName, Action<T> callback )
		{
#if UNITY_EDITOR
			var persistentPath = Path.Combine( MusicConstants.ConfigurationPersistentModDataPath, $"{modName}.txt" );
			if ( LoadConfigurationDataFromDirectory( persistentPath, callback ) == false )
			{
				LoadConfigurationDataFromAddressable( modName, MusicConstants.ModDataAddressableKey, callback );
			}
#else
				LoadConfigurationDataFromAddressable( modName, MusicConstants.ModDataAddressableKey, callback );
#endif
		}
	}
}

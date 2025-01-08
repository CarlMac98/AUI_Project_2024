using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ProcGenMusic
{
	public class UnityAudioHandler : MonoBehaviour, PMGAudioHandler
	{
#if FMOD_ENABLED == false
		public void Initialize( MusicGenerator musicGenerator )
		{
			mMusicGenerator = musicGenerator;
			mRawAudioHandler.Initialize( mMusicGenerator );
			mInstrumentAudio = mMusicGenerator.InstrumentAudio;
			mMusicGenerator.StopGenerator.AddListener( StopAudio );
			mMusicGenerator.PauseGenerator.AddListener( PauseAudio );
			mMusicGenerator.PlayGenerator.AddListener( UnpauseAudio );
		}

		public void PlayNote( InstrumentSet set, int note, int instrumentIndex, int instrumentVariantIndex )
		{
			var instrument = set.Instruments[instrumentIndex];

			if ( instrument.InstrumentData.IsSynth )
			{
				mRawAudioHandler.PlayNote( set, note, instrumentIndex );
			}
			else
			{
				PlayAudioClip( set, note, instrumentIndex, instrumentVariantIndex, instrument );
			}
		}

		private void PlayAudioClip( InstrumentSet set, int note, int instrumentIndex, int instrumentVariantIndex, Instrument instrument )
		{
			if ( mInstrumentAudio.TryGetValue( instrument.InstrumentData.InstrumentType, out var instrumentAudio ) == false ||
			     instrumentVariantIndex >= instrumentAudio.Instruments.Count ||
			     instrumentIndex >= set.Instruments.Count )
			{
				return;
			}

			var audioClip = instrumentAudio.Instruments[instrumentVariantIndex].Notes[note];
			var volume = instrument.InstrumentData.Volume;

			foreach ( var audioSource in mAudioSources )
			{
				if ( audioSource.isPlaying )
				{
					continue;
				}

				audioSource.panStereo = set.Instruments[instrumentIndex].InstrumentData.StereoPan;
				audioSource.loop = false;
				audioSource.outputAudioMixerGroup = mMixer.FindMatchingGroups( instrumentIndex.ToString() )[0];
				audioSource.volume = volume;
				audioSource.clip = audioClip;
				audioSource.Play();
				return;
			}

			// we didn't find an idle source, so make a new one
			mAudioSources.Add( mAudioListenerObject.AddComponent<AudioSource>() );
			var newSource = mAudioSources[mAudioSources.Count - 1];
			newSource.panStereo = set.Instruments[instrumentIndex].InstrumentData.StereoPan;
			newSource.outputAudioMixerGroup = mMixer.FindMatchingGroups( instrumentIndex.ToString() )[0];
			newSource.volume = volume;
			newSource.loop = false;
			newSource.clip = audioClip;
			//audioSource.spatialBlend = 0;
			newSource.Play();
		}

		public void LoadInstrument( string instrumentType, InstrumentAudio instrumentAudio )
		{
			mInstrumentAudio[instrumentType].LoadAudioClips();
		}

		public void UnloadInstrument( string instrumentKey, InstrumentAudio instrumentAudio )
		{
			//nothing to do.
		}

		public void InitializeGlobalEffects()
		{
			//Reverb
			UpdateParameter( $"Master{MusicConstants.MixerReverbDryName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbDry );
			UpdateParameter( $"Master{MusicConstants.MixerRoomSizeName}", parameterValue: mMusicGenerator.ConfigurationData.RoomSize );
			UpdateParameter( $"Master{MusicConstants.MixerReverbRoomHFName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbRoomHF );
			UpdateParameter( $"Master{MusicConstants.MixerReverbDecayTimeName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbDecay );
			UpdateParameter( $"Master{MusicConstants.MixerReverbDecayHFRatioName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbDecayHFRatio );
			UpdateParameter( $"Master{MusicConstants.MixerReverbReflectionsName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbReflections );
			UpdateParameter( $"Master{MusicConstants.MixerReverbReflectDelayName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbReflectDelay );
			UpdateParameter( $"Master{MusicConstants.MixerReverbName}", parameterValue: mMusicGenerator.ConfigurationData.Reverb );
			UpdateParameter( $"Master{MusicConstants.MixerReverbDelayName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbDelay );
			UpdateParameter( $"Master{MusicConstants.MixerReverbDiffusionName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbDiffusion );
			UpdateParameter( $"Master{MusicConstants.MixerReverbDensityName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbDensity );
			UpdateParameter( $"Master{MusicConstants.MixerReverbHFReferenceName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbHFReference );
			UpdateParameter( $"Master{MusicConstants.MixerReverbRoomLFName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbRoomLF );
			UpdateParameter( $"Master{MusicConstants.MixerReverbLFReferenceName}", parameterValue: mMusicGenerator.ConfigurationData.ReverbLFReference );

			//echo
			UpdateParameter( $"Master{MusicConstants.MixerEchoDelayName}", parameterValue: mMusicGenerator.ConfigurationData.EchoDelay );
			UpdateParameter( $"Master{MusicConstants.MixerEchoDecayName}", parameterValue: mMusicGenerator.ConfigurationData.EchoDecay );
			UpdateParameter( $"Master{MusicConstants.MixerEchoDryName}", parameterValue: mMusicGenerator.ConfigurationData.EchoDry );
			UpdateParameter( $"Master{MusicConstants.MixerEchoName}", parameterValue: mMusicGenerator.ConfigurationData.EchoWet );

			//flange
			UpdateParameter( $"Master{MusicConstants.MixerFlangeName}", parameterValue: mMusicGenerator.ConfigurationData.Flanger );
			UpdateParameter( $"Master{MusicConstants.MixerFlangeDryName}", parameterValue: mMusicGenerator.ConfigurationData.FlangeDry );
			UpdateParameter( $"Master{MusicConstants.MixerFlangeDepthName}", parameterValue: mMusicGenerator.ConfigurationData.FlangeDepth );
			UpdateParameter( $"Master{MusicConstants.MixerFlangeRateName}", parameterValue: mMusicGenerator.ConfigurationData.FlangeRate );

			//distortion
			UpdateParameter( $"Master{MusicConstants.MixerDistortionName}", parameterValue: mMusicGenerator.ConfigurationData.Distortion );

			//chorus
			UpdateParameter( $"Master{MusicConstants.MixerChorusDryName}", parameterValue: mMusicGenerator.ConfigurationData.ChorusDry );
			UpdateParameter( $"Master{MusicConstants.MixerChorusName}", parameterValue: mMusicGenerator.ConfigurationData.Chorus );
			UpdateParameter( $"Master{MusicConstants.MixerChorus2Name}", parameterValue: mMusicGenerator.ConfigurationData.Chorus2 );
			UpdateParameter( $"Master{MusicConstants.MixerChorus3Name}", parameterValue: mMusicGenerator.ConfigurationData.Chorus3 );
			UpdateParameter( $"Master{MusicConstants.MixerChorusDelayName}", parameterValue: mMusicGenerator.ConfigurationData.ChorusDelay );
			UpdateParameter( $"Master{MusicConstants.MixerChorusRateName}", parameterValue: mMusicGenerator.ConfigurationData.ChorusRate );
			UpdateParameter( $"Master{MusicConstants.MixerChorusDepthName}", parameterValue: mMusicGenerator.ConfigurationData.ChorusDepth );
			UpdateParameter( $"Master{MusicConstants.MixerChorusFeedbackName}", parameterValue: mMusicGenerator.ConfigurationData.ChorusFeedback );

			//paramEQ
			UpdateParameter( $"Master{MusicConstants.MixerCenterFreq}", parameterValue: mMusicGenerator.ConfigurationData.ParamEQCenterFreq );
			UpdateParameter( $"Master{MusicConstants.MixerOctaveRange}", parameterValue: mMusicGenerator.ConfigurationData.ParamEQOctaveRange );
			UpdateParameter( $"Master{MusicConstants.MixerFreqGain}", parameterValue: mMusicGenerator.ConfigurationData.ParamEQFreqGain );
		}

		public void SetAllInstrumentParameters( int instrumentIndex, ConfigurationData.InstrumentData instrumentData )
		{
			//group volume
			UpdateParameter( $"{MusicConstants.MixerVolumeName}{instrumentIndex}", parameterValue: instrumentData.MixerGroupVolume );

			//Reverb
			UpdateParameter( $"{MusicConstants.MixerReverbDryName}{instrumentIndex}", parameterValue: instrumentData.ReverbDry );
			UpdateParameter( $"{MusicConstants.MixerRoomSizeName}{instrumentIndex}", parameterValue: instrumentData.RoomSize );
			UpdateParameter( $"{MusicConstants.MixerReverbRoomHFName}{instrumentIndex}", parameterValue: instrumentData.ReverbRoomHF );
			UpdateParameter( $"{MusicConstants.MixerReverbDecayTimeName}{instrumentIndex}", parameterValue: instrumentData.ReverbDecayTime );
			UpdateParameter( $"{MusicConstants.MixerReverbDecayHFRatioName}{instrumentIndex}", parameterValue: instrumentData.ReverbDecayHFRatio );
			UpdateParameter( $"{MusicConstants.MixerReverbReflectionsName}{instrumentIndex}", parameterValue: instrumentData.ReverbReflections );
			UpdateParameter( $"{MusicConstants.MixerReverbReflectDelayName}{instrumentIndex}", parameterValue: instrumentData.ReverbReflectDelay );
			UpdateParameter( $"{MusicConstants.MixerReverbName}{instrumentIndex}", parameterValue: instrumentData.Reverb );
			UpdateParameter( $"{MusicConstants.MixerReverbDelayName}{instrumentIndex}", parameterValue: instrumentData.ReverbDelay );
			UpdateParameter( $"{MusicConstants.MixerReverbDiffusionName}{instrumentIndex}", parameterValue: instrumentData.ReverbDiffusion );
			UpdateParameter( $"{MusicConstants.MixerReverbDensityName}{instrumentIndex}", parameterValue: instrumentData.ReverbDensity );
			UpdateParameter( $"{MusicConstants.MixerReverbHFReferenceName}{instrumentIndex}", parameterValue: instrumentData.ReverbHFReference );
			UpdateParameter( $"{MusicConstants.MixerReverbRoomLFName}{instrumentIndex}", parameterValue: instrumentData.ReverbRoomLF );
			UpdateParameter( $"{MusicConstants.MixerReverbLFReferenceName}{instrumentIndex}", parameterValue: instrumentData.ReverbLFReference );

			//echo
			UpdateParameter( $"{MusicConstants.MixerEchoDelayName}{instrumentIndex}", parameterValue: instrumentData.EchoDelay );
			UpdateParameter( $"{MusicConstants.MixerEchoDecayName}{instrumentIndex}", parameterValue: instrumentData.EchoDecay );
			UpdateParameter( $"{MusicConstants.MixerEchoDryName}{instrumentIndex}", parameterValue: instrumentData.EchoDry );
			UpdateParameter( $"{MusicConstants.MixerEchoName}{instrumentIndex}", parameterValue: instrumentData.Echo );

			//flange
			UpdateParameter( $"{MusicConstants.MixerFlangeName}{instrumentIndex}", parameterValue: instrumentData.Flanger );
			UpdateParameter( $"{MusicConstants.MixerFlangeDryName}{instrumentIndex}", parameterValue: instrumentData.FlangeDry );
			UpdateParameter( $"{MusicConstants.MixerFlangeDepthName}{instrumentIndex}", parameterValue: instrumentData.FlangeDepth );
			UpdateParameter( $"{MusicConstants.MixerFlangeRateName}{instrumentIndex}", parameterValue: instrumentData.FlangeRate );

			//distortion
			UpdateParameter( $"{MusicConstants.MixerDistortionName}{instrumentIndex}", parameterValue: instrumentData.Distortion );

			//chorus
			UpdateParameter( $"{MusicConstants.MixerChorusDryName}{instrumentIndex}", parameterValue: instrumentData.ChorusDry );
			UpdateParameter( $"{MusicConstants.MixerChorusName}{instrumentIndex}", parameterValue: instrumentData.Chorus );
			UpdateParameter( $"{MusicConstants.MixerChorus2Name}{instrumentIndex}", parameterValue: instrumentData.Chorus2 );
			UpdateParameter( $"{MusicConstants.MixerChorus3Name}{instrumentIndex}", parameterValue: instrumentData.Chorus3 );
			UpdateParameter( $"{MusicConstants.MixerChorusDelayName}{instrumentIndex}", parameterValue: instrumentData.ChorusDelay );
			UpdateParameter( $"{MusicConstants.MixerChorusRateName}{instrumentIndex}", parameterValue: instrumentData.ChorusRate );
			UpdateParameter( $"{MusicConstants.MixerChorusDepthName}{instrumentIndex}", parameterValue: instrumentData.ChorusDepth );
			UpdateParameter( $"{MusicConstants.MixerChorusFeedbackName}{instrumentIndex}", parameterValue: instrumentData.ChorusFeedback );

			//paramEQ
			UpdateParameter( $"{MusicConstants.MixerCenterFreq}{instrumentIndex}", parameterValue: instrumentData.ParamEQCenterFreq );
			UpdateParameter( $"{MusicConstants.MixerOctaveRange}{instrumentIndex}", parameterValue: instrumentData.ParamEQOctaveRange );
			UpdateParameter( $"{MusicConstants.MixerFreqGain}{instrumentIndex}", parameterValue: instrumentData.ParamEQFreqGain );
		}

		public void FadeVolume( float deltaT, out float currentVol )
		{
			currentVol = 0;

			mMixer.GetFloat( "MasterVol", out currentVol );
			var desiredVol = mMusicGenerator.ConfigurationData.MasterVolume;

			switch ( VolumeState )
			{
				case VolumeState.FadingIn:
				{
					if ( currentVol <= desiredVol - ( mMusicGenerator.ConfigurationData.VolFadeRate * deltaT ) )
					{
						currentVol += mMusicGenerator.ConfigurationData.VolFadeRate * deltaT;
					}
					else
					{
						currentVol = desiredVol;
						VolumeState = VolumeState.Idle;
					}

					break;
				}
				case VolumeState.FadingOut:
				{
					if ( currentVol > MusicConstants.MinVolume + ( mMusicGenerator.ConfigurationData.VolFadeRate * deltaT ) )
					{
						currentVol -= mMusicGenerator.ConfigurationData.VolFadeRate * deltaT;
					}
					else
					{
						currentVol = MusicConstants.MinVolume;
						VolumeState = VolumeState.Idle;
					}

					break;
				}
			}

			mMixer.SetFloat( "MasterVol", currentVol );
		}

		public void SetVolume( float volume )
		{
			if ( VolumeState != VolumeState.Idle )
			{
				return;
			}

			mMusicGenerator.ConfigurationData.MasterVolume = volume;
			UpdateParameter( MusicConstants.MasterVolName, parameterValue: volume );
		}

		/// <summary>
		/// fades Volume out
		/// </summary>
		public void VolumeFadeOut()
		{
			VolumeState = VolumeState.FadingOut;
		}

		/// <summary>
		/// fades Volume in
		/// </summary>
		public void VolumeFadeIn()
		{
			VolumeState = VolumeState.FadingIn;
		}

		public void UpdateVolumeState( float deltaT )
		{
			switch ( VolumeState )
			{
				case VolumeState.FadingIn:
				case VolumeState.FadingOut:
					FadeVolume( deltaT, out _ );
					break;
			}
		}

		public void UpdateParameter( string parameterName, int index = -1, float parameterValue = 0 )
		{
			if ( mMixer == false )
			{
				return;
			}

			mMixer.SetFloat( parameterName, parameterValue );
		}

		/// <summary>
		/// list of audio sources :P
		/// </summary>
		private readonly List<AudioSource> mAudioSources = new List<AudioSource>();

		private IReadOnlyDictionary<string, InstrumentAudio> mInstrumentAudio;

		private MusicGenerator mMusicGenerator;

		[SerializeField]
		private AudioMixer mMixer;

		[SerializeField]
		private GameObject mAudioListenerObject;

		[SerializeField, Tooltip( "Reference to our RawAudioHandler" )]
		private RawAudioHandler mRawAudioHandler;

		///<summary>
		/// whether we're fading in or out
		/// </summary>
		public VolumeState VolumeState { get; private set; } = VolumeState.Idle;

		private void OnEnable()
		{
			PurgeAudioSources();
		}

		private void OnDisable()
		{
			PurgeAudioSources();
		}

		/// <summary>
		/// Stops the Audio Sources.
		/// </summary>
		private void StopAudio()
		{
			foreach ( var audioSource in mAudioSources )
			{
				audioSource.Stop();
			}

			PurgeAudioSources();
		}

		/// <summary>
		/// Unpauses the audio
		/// </summary>
		private void UnpauseAudio()
		{
			foreach ( var audioSource in mAudioSources )
			{
				audioSource.UnPause();
			}
		}

		/// <summary>
		/// Pauses audio
		/// </summary>
		private void PauseAudio()
		{
			foreach ( var audioSource in mAudioSources )
			{
				audioSource.Pause();
			}
		}

		/// <summary>
		/// Purges our unused audio sources. 
		/// </summary>
		private void PurgeAudioSources()
		{
			var toRemove = new List<AudioSource>();
			var sources = mAudioListenerObject.GetComponents<AudioSource>();
			foreach ( var audioSource in sources )
			{
				if ( audioSource.isPlaying == false )
				{
					toRemove.Add( audioSource );
				}
			}

			foreach ( var audioSource in toRemove )
			{
				Destroy( audioSource );
			}

			mAudioSources.Clear();
		}
#else
		// Simply because I won't want to break references to this class in prefabs, but also don't want any of the code compiling.
		public void Initialize( MusicGenerator musicGenerator )
		{
		}

		public void PlayNote( InstrumentSet set, int note, int instrumentIndex, int instrumentVariantIndex )
		{
		}

		public void LoadInstrument( string instrumentType, InstrumentAudio instrumentAudio )
		{
		}

		public void UnloadInstrument( string instrumentKey, InstrumentAudio instrumentAudio )
		{
		}

		public void InitializeGlobalEffects()
		{
		}

		public void SetAllInstrumentParameters( int instrumentIndex, ConfigurationData.InstrumentData instrumentData )
		{
		}

		public void FadeVolume( float deltaT, out float currentVol )
		{
			currentVol = 0;
		}

		public void SetVolume( float volume )
		{
		}

		public void UpdateVolumeState( float deltaT )
		{
		}

		public void UpdateParameter( string parameterName, int index = -1, float parameterValue = 0 )
		{
		}

		public VolumeState VolumeState => VolumeState.Idle;

		public void VolumeFadeOut()
		{
		}

		public void VolumeFadeIn()
		{
		}
#endif //FMOD_ENABLED == false
	}
}

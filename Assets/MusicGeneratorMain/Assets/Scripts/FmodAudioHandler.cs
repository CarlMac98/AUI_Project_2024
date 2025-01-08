using UnityEngine;
#if FMOD_ENABLED
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

#endif //FMOD_ENABLED

namespace ProcGenMusic
{
	public class FmodAudioHandler : MonoBehaviour, PMGAudioHandler
	{
#if FMOD_ENABLED
		private void LoadBanks( Action onComplete )
		{
			var count = 0;
			var labelOperation = Addressables.LoadResourceLocationsAsync( "PMGFmodBanks" );
			labelOperation.Completed += ( labelResponse ) =>
			{
				foreach ( var item in labelResponse.Result )
				{
					var resourceOperation = Addressables.LoadAssetAsync<TextAsset>( item.PrimaryKey );
					resourceOperation.Completed += ( result ) =>
					{
						if ( result.Status == AsyncOperationStatus.Succeeded )
						{
							RuntimeManager.LoadBank( result.Result, true );
						}

						count++;

						if ( count >= labelOperation.Result.Count )
						{
							onComplete.Invoke();
						}
					};
				}
			};
		}

		public void Initialize( MusicGenerator musicGenerator )
		{
			mMusicGenerator = musicGenerator;
		}

		public void PlayNote( InstrumentSet set, int note, int instrumentIndex, int instrumentVariantIndex )
		{
			var instrument = set.Instruments[instrumentIndex];
			var instrumentName = instrument.InstrumentData.InstrumentType;
			// Just fixing an annoyance of the way the generator handles mixed instrument  
			instrumentName = instrumentName.Replace( "Mix", "" );

			var key = $"{instrumentName}_{instrumentVariantIndex}/{note + 1}";
			var eventInstance = RuntimeManager.CreateInstance( mInstrumentReferences[instrumentIndex] );

			// Pin the key string in memory and pass a pointer through the user data
			var stringHandle = GCHandle.Alloc( key, GCHandleType.Pinned );
			eventInstance.setUserData( GCHandle.ToIntPtr( stringHandle ) );
			eventInstance.setCallback( mEventCallback );
			eventInstance.start();
			eventInstance.release();
		}

		public void LoadInstrument( string instrumentName, InstrumentAudio instrumentAudio )
		{
			// Just fixing an annoyance of the way the generator handles mixed instrument  
			if ( instrumentName.Contains( "Mix" ) )
			{
				instrumentName = instrumentName.Replace( "Mix", "" );
			}

			for ( var variantIndex = 0; variantIndex < instrumentAudio.Instruments.Count; variantIndex++ )
			{
				for ( var index = 1; index <= instrumentAudio.Instruments[variantIndex].Notes.Count; index++ )
				{
					var key = $"{instrumentName}_{variantIndex}/{index}";
					var keyResult = RuntimeManager.StudioSystem.getSoundInfo( key, out var soundInfo );

					if ( keyResult == RESULT.OK )
					{
						var soundResult = RuntimeManager.CoreSystem.createSound( soundInfo.name_or_data,
							mSoundMode | soundInfo.mode,
							ref soundInfo.exinfo,
							out var sound );

						if ( soundResult == RESULT.OK )
						{
							if ( mSoundCache.ContainsKey( key ) == false )
							{
								mSoundCache.Add( key, new CachedSound( sound, soundInfo ) );
							}
						}
					}
				}
			}
		}

		public void UnloadInstrument( string instrumentName, InstrumentAudio instrumentAudio )
		{
			instrumentName = instrumentName.Replace( "mix", "" );

			for ( var variantIndex = 0; variantIndex < instrumentAudio.Instruments.Count; variantIndex++ )
			{
				for ( var index = 0; index < instrumentAudio.Instruments[variantIndex].Notes.Count; index++ )
				{
					var key = $"{instrumentName}_{variantIndex}/{index + 1}";
					if ( mSoundCache.TryGetValue( key, out var cachedSound ) )
					{
						cachedSound.Sound.release();
						mSoundCache.Remove( key );
					}
				}
			}
		}

		public void InitializeGlobalEffects()
		{
			SetAllMasterParameters();
		}

		public float GetGlobalParameter( string parameterName )
		{
			RuntimeManager.StudioSystem.getParameterByName( parameterName, out var value );
			return value;
		}

		public void SetAllInstrumentParameters( int instrumentIndex, ConfigurationData.InstrumentData instrumentData )
		{
			UpdateParameter( MusicConstants.FmodFlangerRateName, instrumentIndex, instrumentData.FmodFlangerRate );
			UpdateParameter( MusicConstants.FmodFlangerDepthName, instrumentIndex, instrumentData.FmodFlangerDepth );
			UpdateParameter( MusicConstants.FmodFlangerMixName, instrumentIndex, instrumentData.FmodFlangerMix );
			UpdateParameter( MusicConstants.FmodDelayName, instrumentIndex, instrumentData.FmodDelay );
			UpdateParameter( MusicConstants.FmodDelayWetName, instrumentIndex, instrumentData.FmodDelayWet );
			UpdateParameter( MusicConstants.FmodDelayFeedbackName, instrumentIndex, instrumentData.FmodDelayFeedback );
			UpdateParameter( MusicConstants.FmodDelayDryName, instrumentIndex, instrumentData.FmodDelayDry );
			UpdateParameter( MusicConstants.FmodReverbTimeName, instrumentIndex, instrumentData.FmodReverbTime );
			UpdateParameter( MusicConstants.FmodReverbHFDecayName, instrumentIndex, instrumentData.FmodReverbHFDecay );
			UpdateParameter( MusicConstants.FmodReverbDiffusionName, instrumentIndex, instrumentData.FmodReverbDiffusion );
			UpdateParameter( MusicConstants.FmodReverbLowGainName, instrumentIndex, instrumentData.FmodReverbLowGain );
			UpdateParameter( MusicConstants.FmodReverbHighCutName, instrumentIndex, instrumentData.FmodReverbHighCut );
			UpdateParameter( MusicConstants.FmodReverbWetMixName, instrumentIndex, instrumentData.FmodReverbWetMix );
			UpdateParameter( MusicConstants.FmodReverbDryMixName, instrumentIndex, instrumentData.FmodReverbDryMix );
			UpdateParameter( MusicConstants.FmodReverbEarlyDelayName, instrumentIndex, instrumentData.FmodReverbEarlyDelay );
			UpdateParameter( MusicConstants.FmodReverbLateDelayName, instrumentIndex, instrumentData.FmodReverbLateDelay );
			UpdateParameter( MusicConstants.FmodReverbHFReferenceName, instrumentIndex, instrumentData.FmodReverbHFReference );
			UpdateParameter( MusicConstants.FmodReverbDensityName, instrumentIndex, instrumentData.FmodReverbDensity );
			UpdateParameter( MusicConstants.FmodReverbLowFreqName, instrumentIndex, instrumentData.FmodReverbLowFreq );
			UpdateParameter( MusicConstants.FmodReverbEarlyLateMixName, instrumentIndex, instrumentData.FmodReverbEarlyLateMix );
			UpdateParameter( MusicConstants.FmodDistortionName, instrumentIndex, instrumentData.FmodDistortion );
			UpdateParameter( MusicConstants.FmodChorusRateName, instrumentIndex, instrumentData.FmodChorusRate );
			UpdateParameter( MusicConstants.FmodChorusDepthName, instrumentIndex, instrumentData.FmodChorusDepth );
			UpdateParameter( MusicConstants.FmodChorusMixName, instrumentIndex, instrumentData.FmodChorusMix );
			UpdateParameter( MusicConstants.FmodEQLowName, instrumentIndex, instrumentData.FmodEQLow );
			UpdateParameter( MusicConstants.FmodEQMidName, instrumentIndex, instrumentData.FmodEQMid );
			UpdateParameter( MusicConstants.FmodEQHighName, instrumentIndex, instrumentData.FmodEQHigh );
			UpdateParameter( MusicConstants.FmodEQXLowName, instrumentIndex, instrumentData.FmodEQXLow );
			UpdateParameter( MusicConstants.FmodEQXHighName, instrumentIndex, instrumentData.FmodEQXHigh );
			UpdateParameter( MusicConstants.FmodGainName, instrumentIndex, instrumentData.FmodGain );
			UpdateParameter( MusicConstants.FmodPannerName, instrumentIndex, instrumentData.FmodPanner );
			UpdateParameter( MusicConstants.FmodFaderName, instrumentIndex, instrumentData.FmodFader );
		}

		public void FadeVolume( float deltaT, out float currentVol )
		{
			RuntimeManager.StudioSystem.getParameterByName( $"{MusicConstants.FmodMasterFaderName}", out currentVol );

			var desiredVol = mMusicGenerator.ConfigurationData.FmodMasterFader;

			switch ( VolumeState )
			{
				case ProcGenMusic.VolumeState.FadingIn:
				{
					if ( currentVol <= desiredVol - ( mMusicGenerator.ConfigurationData.VolFadeRate * deltaT ) )
					{
						currentVol += mMusicGenerator.ConfigurationData.VolFadeRate * deltaT;
					}
					else
					{
						currentVol = desiredVol;
						VolumeState = ProcGenMusic.VolumeState.Idle;
					}

					break;
				}
				case ProcGenMusic.VolumeState.FadingOut:
				{
					if ( currentVol > MusicConstants.NormalizedMin + ( mMusicGenerator.ConfigurationData.VolFadeRate * deltaT ) )
					{
						currentVol -= mMusicGenerator.ConfigurationData.VolFadeRate * deltaT;
					}
					else
					{
						currentVol = MusicConstants.NormalizedMin;
						VolumeState = VolumeState.Idle;
					}

					break;
				}
			}

			RuntimeManager.StudioSystem.setParameterByName( $"{MusicConstants.FmodMasterFaderName}", currentVol );
		}

		public void SetVolume( float volume )
		{
			if ( VolumeState != VolumeState.Idle )
			{
				return;
			}

			mMusicGenerator.ConfigurationData.FmodMasterFader = volume;
			UpdateParameter( MusicConstants.FmodMasterFaderName, parameterValue: volume );
		}

		public void UpdateVolumeState( float deltaT )
		{
			switch ( VolumeState )
			{
				case ProcGenMusic.VolumeState.FadingIn:
				case ProcGenMusic.VolumeState.FadingOut:
					FadeVolume( deltaT, out _ );
					break;
			}
		}

		/// <summary>
		/// fades Volume out
		/// </summary>
		public void VolumeFadeOut()
		{
			VolumeState = ProcGenMusic.VolumeState.FadingOut;
		}

		/// <summary>
		/// fades Volume in
		/// </summary>
		public void VolumeFadeIn()
		{
			VolumeState = ProcGenMusic.VolumeState.FadingIn;
		}

		public void UpdateParameter( string parameterName, int index = -1, float parameterValue = 0 )
		{
			RuntimeManager.StudioSystem.setParameterByName( index >= 0 ? $"{parameterName}_{index}" : $"{parameterName}", parameterValue );
		}


		[SerializeField]
		private MODE mSoundMode = MODE.LOOP_OFF | MODE._2D | MODE.NONBLOCKING;

		[SerializeField]
		private EventReference[] mInstrumentReferences;

		private MusicGenerator mMusicGenerator;

		///<summary>
		/// whether we're fading in or out
		/// </summary>
		public VolumeState VolumeState { get; private set; } = ProcGenMusic.VolumeState.Idle;

		private EVENT_CALLBACK mEventCallback;
		private readonly Dictionary<string, CachedSound> mSoundCache = new Dictionary<string, CachedSound>();
		private const string BUS_PREFIX = "bus:/InstrumentGroup";
		private readonly List<Bus> mBusses = new List<Bus>();

		private class CachedSound
		{
			public CachedSound( Sound sound, SOUND_INFO soundInfo )
			{
				SoundInfo = soundInfo;
				Sound = sound;
			}

			public readonly SOUND_INFO SoundInfo;
			public Sound Sound { get; }
		}

		private void Start()
		{
			// Explicitly create the delegate object and assign it to a member so it doesn't get freed
			// by the garbage collected while it's being used
			mEventCallback = EventCallback;
			LoadBanks( () =>
			{
				for ( var index = 0; index < MusicConstants.MaxInstruments; index++ )
				{
					RuntimeManager.StudioSystem.getBus( $"{BUS_PREFIX}{index}", out var bus );
					if ( bus.hasHandle() )
					{
						mBusses.Add( bus );
					}
				}
			} );
		}

		private void OnDisable()
		{
			foreach ( var cachedSound in mSoundCache )
			{
				cachedSound.Value.Sound.release();
			}

			mSoundCache.Clear();

			foreach ( var bus in mBusses.Where( bus => bus.hasHandle() ) )
			{
				bus.stopAllEvents( FMOD.Studio.STOP_MODE.IMMEDIATE );
				bus.clearHandle();
			}

			mBusses.Clear();
		}

		[AOT.MonoPInvokeCallback( typeof( EVENT_CALLBACK ) )]
		private RESULT EventCallback( EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr )
		{
			var instance = new EventInstance( instancePtr );

			// Retrieve the user data
			instance.getUserData( out var stringPtr );

			var stringHandle = GCHandle.FromIntPtr( stringPtr );
			var key = stringHandle.Target as string;

			if ( string.IsNullOrEmpty( key ) )
			{
				return RESULT.ERR_EVENT_NOTFOUND;
			}

			switch ( type )
			{
				case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
				{
					var parameter = (PROGRAMMER_SOUND_PROPERTIES) Marshal.PtrToStructure( parameterPtr, typeof( PROGRAMMER_SOUND_PROPERTIES ) );

					if ( mSoundCache.TryGetValue( key, out var cachedSound ) )
					{
						cachedSound.Sound.getOpenState( out var openstate, out _, out _, out _ );
						// we only want to attach if it's ready, otherwise it will play delayed.
						if ( openstate == OPENSTATE.READY )
						{
							parameter.sound = cachedSound.Sound.handle;
							parameter.subsoundIndex = cachedSound.SoundInfo.subsoundindex;
							Marshal.StructureToPtr( parameter, parameterPtr, false );
						}
					}

					break;
				}
				case EVENT_CALLBACK_TYPE.DESTROYED:
				{
					// Now the event has been destroyed, unpin the string memory so it can be garbage collected
					stringHandle.Free();

					break;
				}
			}

			return RESULT.OK;
		}

		private void SetAllMasterParameters()
		{
			UpdateParameter( MusicConstants.FmodMasterFlangerRateName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterFlangerRate );
			UpdateParameter( MusicConstants.FmodMasterFlangerDepthName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterFlangerDepth );
			UpdateParameter( MusicConstants.FmodMasterFlangerMixName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterFlangerMix );
			UpdateParameter( MusicConstants.FmodMasterDelayName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterDelay );
			UpdateParameter( MusicConstants.FmodMasterDelayWetName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterDelayWet );
			UpdateParameter( MusicConstants.FmodMasterDelayFeedbackName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterDelayFeedback );
			UpdateParameter( MusicConstants.FmodMasterDelayDryName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterDelayDry );
			UpdateParameter( MusicConstants.FmodMasterReverbTimeName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbTime );
			UpdateParameter( MusicConstants.FmodMasterReverbHFDecayName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbHFDecay );
			UpdateParameter( MusicConstants.FmodMasterReverbDiffusionName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbDiffusion );
			UpdateParameter( MusicConstants.FmodMasterReverbLowGainName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbLowGain );
			UpdateParameter( MusicConstants.FmodMasterReverbHighCutName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbHighCut );
			UpdateParameter( MusicConstants.FmodMasterReverbWetMixName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbWetMix );
			UpdateParameter( MusicConstants.FmodMasterReverbDryMixName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbDryMix );
			UpdateParameter( MusicConstants.FmodMasterReverbEarlyDelayName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyDelay );
			UpdateParameter( MusicConstants.FmodMasterReverbLateDelayName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbLateDelay );
			UpdateParameter( MusicConstants.FmodMasterReverbHFReferenceName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbHFReference );
			UpdateParameter( MusicConstants.FmodMasterReverbDensityName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbDensity );
			UpdateParameter( MusicConstants.FmodMasterReverbLowFreqName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbLowFreq );
			UpdateParameter( MusicConstants.FmodMasterReverbEarlyLateMixName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterReverbEarlyLateMix );
			UpdateParameter( MusicConstants.FmodMasterDistortionName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterDistortion );
			UpdateParameter( MusicConstants.FmodMasterChorusRateName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterChorusRate );
			UpdateParameter( MusicConstants.FmodMasterChorusDepthName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterChorusDepth );
			UpdateParameter( MusicConstants.FmodMasterChorusMixName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterChorusMix );
			UpdateParameter( MusicConstants.FmodMasterEQLowName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterEQLow );
			UpdateParameter( MusicConstants.FmodMasterEQMidName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterEQMid );
			UpdateParameter( MusicConstants.FmodMasterEQHighName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterEQHigh );
			UpdateParameter( MusicConstants.FmodMasterEQXLowName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterEQXLow );
			UpdateParameter( MusicConstants.FmodMasterEQXHighName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterEQXHigh );
			UpdateParameter( MusicConstants.FmodMasterGainName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterGain );
			UpdateParameter( MusicConstants.FmodMasterPannerName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterPanner );
			UpdateParameter( MusicConstants.FmodMasterFaderName, parameterValue: mMusicGenerator.ConfigurationData.FmodMasterFader );
		}
#else
		// Simply because I won't want to break references to this class in prefabs, but also don't want any of the code compiling.
		public void Initialize( MusicGenerator musicGenerator ) { }

		public void PlayNote( InstrumentSet set, int note, int instrumentIndex, int instrumentVariantIndex ) { }

		public void LoadInstrument( string instrumentType, InstrumentAudio instrumentAudio ) { }

		public void UnloadInstrument( string instrumentKey, InstrumentAudio instrumentAudio ) { }

		public void InitializeGlobalEffects() { }

		public void SetAllInstrumentParameters( int instrumentIndex, ConfigurationData.InstrumentData instrumentData ) { }

		public void FadeVolume( float deltaT, out float currentVol )
		{
			currentVol = 0;
		}

		public void SetVolume( float volume ) { }

		public void UpdateVolumeState( float deltaT ) { }

		public void UpdateParameter( string parameterName, int index = -1, float parameterValue = 0 ) { }

		public VolumeState VolumeState => ProcGenMusic.VolumeState.Idle;

		public void VolumeFadeOut() { }

		public void VolumeFadeIn() { }
#endif //FMOD_ENABLED
	}
}

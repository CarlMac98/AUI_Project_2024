using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace ProcGenMusic
{
	public class RawAudioHandler : MonoBehaviour
	{
#if FMOD_ENABLED == false

		public void Initialize( MusicGenerator musicGenerator )
		{
			mMusicGenerator = musicGenerator;
			mMusicGenerator.StopGenerator.AddListener( StopAudio );
			mMusicGenerator.PauseGenerator.AddListener( PauseAudio );
			mMusicGenerator.PlayGenerator.AddListener( UnpauseAudio );
		}

		public void PlayNote( InstrumentSet set, int note, int instrumentIndex )
		{
			var instrument = set.Instruments[instrumentIndex];
			var octave = ( note / MusicConstants.OctaveSize ) + 1;
			var adjustedNote = note % MusicConstants.OctaveSize;

			if ( PlayWithUnusedSource( set, instrumentIndex, adjustedNote, octave, instrument.InstrumentData ) == false )
			{
				PlayWithNewSource( set, instrumentIndex, adjustedNote, octave, instrument.InstrumentData );
			}

			if ( instrument.InstrumentData.SynthSubOctave1 && instrument.InstrumentData.SynthOctavePitchShift > 0 )
			{
				if ( PlayWithUnusedSource( set, instrumentIndex, adjustedNote, octave, instrument.InstrumentData, 1 ) == false )
				{
					PlayWithNewSource( set, instrumentIndex, adjustedNote, octave, instrument.InstrumentData, 1 );
				}
			}

			if ( instrument.InstrumentData.SynthSubOctave2 && instrument.InstrumentData.SynthOctavePitchShift > 1 )
			{
				if ( PlayWithUnusedSource( set, instrumentIndex, adjustedNote, octave, instrument.InstrumentData, 2 ) == false )
				{
					PlayWithNewSource( set, instrumentIndex, adjustedNote, octave, instrument.InstrumentData, 2 );
				}
			}
		}

		private bool PlayWithUnusedSource( InstrumentSet set,
			int instrumentIndex,
			int adjustedNote,
			int octave,
			ConfigurationData.InstrumentData instrumentData,
			int subOctaveShift = 0 )
		{
			foreach ( var audioHandler in mAudioSources )
			{
				if ( audioHandler.IsPlaying )
				{
					continue;
				}

				audioHandler.AudioSource.panStereo = set.Instruments[instrumentIndex].InstrumentData.StereoPan;
				audioHandler.AudioSource.loop = false;
				audioHandler.AudioSource.outputAudioMixerGroup = mMixer.FindMatchingGroups( instrumentIndex.ToString() )[0];
				audioHandler.AudioSource.volume = instrumentData.Volume;
				audioHandler.Play( adjustedNote, octave, instrumentData, subOctaveShift );
				return true;
			}

			return false;
		}

		private void PlayWithNewSource( InstrumentSet set,
			int instrumentIndex,
			int adjustedNote,
			int octave,
			ConfigurationData.InstrumentData instrumentData,
			int subOctaveShift = 0 )
		{
			var handler = Instantiate( mHandlerPrefab, mAudioListenerObject.transform ).GetComponent<SynthHandler>();
			mAudioSources.Add( handler );
			var newSource = mAudioSources[mAudioSources.Count - 1];
			newSource.AudioSource.panStereo = set.Instruments[instrumentIndex].InstrumentData.StereoPan;
			newSource.AudioSource.outputAudioMixerGroup = mMixer.FindMatchingGroups( instrumentIndex.ToString() )[0];
			newSource.AudioSource.volume = instrumentData.Volume;
			newSource.AudioSource.loop = false;
			newSource.Play( adjustedNote, octave, instrumentData, subOctaveShift );
		}

		/// <summary>
		/// list of audio sources :P
		/// </summary>
		private readonly List<SynthHandler> mAudioSources = new List<SynthHandler>();

		private MusicGenerator mMusicGenerator;

		[FormerlySerializedAs( "mHandlerObject" )]
		[SerializeField]
		private GameObject mHandlerPrefab;

		[SerializeField]
		private AudioMixer mMixer;

		[SerializeField]
		private GameObject mAudioListenerObject;

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
			var sources = mAudioListenerObject.GetComponentsInChildren<AudioSource>();
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
#endif //FMOD_ENABLED == false
	}
}

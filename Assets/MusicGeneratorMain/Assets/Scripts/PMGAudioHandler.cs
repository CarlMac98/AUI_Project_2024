using UnityEngine;

namespace ProcGenMusic
{
	public interface PMGAudioHandler
	{
		void Initialize( MusicGenerator musicGenerator );
		void PlayNote( InstrumentSet set, int note, int instrumentIndex, int instrumentVariantIndex );
		void LoadInstrument( string instrumentType, InstrumentAudio instrumentAudio );
		void UnloadInstrument( string instrumentKey, InstrumentAudio instrumentAudio );
		void InitializeGlobalEffects();
		void SetAllInstrumentParameters( int instrumentIndex, ConfigurationData.InstrumentData instrumentData );
		void FadeVolume( float deltaT, out float currentVol );
		void SetVolume( float volume );
		void UpdateVolumeState( float deltaT );
		void UpdateParameter( string parameterName, int index = -1, float parameterValue = 0 );
		VolumeState VolumeState { get; }
		void VolumeFadeOut();
		void VolumeFadeIn();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProcGenMusic
{
	public class SynthPresets
	{
		static SynthPresets()
		{
			LoadData();
		}

		[Serializable]
		public class SynthPreset
		{
			public string Name;
			public int SynthOctavePitchShift = 3;
			public float SynthNoteLength = MusicConstants.NormalizedMid;
			public float SynthRampUp = MusicConstants.MinSynthRamp;
			public float SynthRampDown = MusicConstants.MinSynthRamp;

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
				0, 1, 0
			};

			public float[] CustomEnvelope =
			{
				0, 1, 0
			};

			public float[] CustomPanvelope =
			{
				0, 0, 0
			};

			public List<BezierControlData> WaveformData = new List<BezierControlData>();
			public List<BezierControlData> EnvelopeData = new List<BezierControlData>();
			public List<BezierControlData> PanvelopeData = new List<BezierControlData>();

			public bool SubOctave1;
			public bool SubOctave2;
			public bool IsCustomWaveform;

			public SynthPreset Clone()
			{
				//Please ensure we're not including reference types here.
				var clone = (SynthPreset) MemberwiseClone();

				clone.WaveformData = new List<BezierControlData>();
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
		}

		public static IReadOnlyList<SynthPreset> Presets => mSynthPresets.mPresets;

		public static void AddSynthPreset( ConfigurationData.InstrumentData data, string name )
		{
			var clone = data.Clone();
			var preset = new SynthPreset()
			{
				Name = name,
				SynthOctavePitchShift = clone.SynthOctavePitchShift,
				SynthNoteLength = clone.SynthNoteLength,
				SynthRampUp = clone.SynthAttack,
				SynthRampDown = clone.SynthDecay,
				SynthWaveTypes = clone.SynthWaveTypes,
				SynthWaveOperators = clone.SynthWaveOperators,
				IsCustomWaveform = clone.IsCustomWaveform,
				CustomSynthWaveform = clone.CustomSynthWaveform,
				CustomEnvelope = clone.CustomEnvelope,
				CustomPanvelope = clone.CustomPanvelope,
				WaveformData = clone.WaveformData,
				EnvelopeData = clone.EnvelopeData,
				PanvelopeData = clone.PanvelopeData,
				SubOctave1 = clone.SynthSubOctave1,
				SubOctave2 = clone.SynthSubOctave2
			};

			mSynthPresets.mPresets.Add( preset );
			SaveConfigurationData();
		}

		public static void RemoveSynthPreset( string presetName )
		{
			var index = mSynthPresets.mPresets.FindIndex( x => x.Name == presetName );
			if ( index >= 0 )
			{
				mSynthPresets.mPresets.RemoveAt( index );
			}

			SaveConfigurationData();
		}

		public static void ApplySynthPreset( int index, ConfigurationData.InstrumentData data )
		{
			if ( index >= 0 && index < mSynthPresets.mPresets.Count )
			{
				ApplySynthPreset( mSynthPresets.mPresets[index], data );
			}
			else
			{
				ResetSynthData( data );
			}

			SaveConfigurationData();
		}

		public static void UpdateSynthPreset( int index, ConfigurationData.InstrumentData data )
		{
			var clone = data.Clone();
			mSynthPresets.mPresets[index].SynthOctavePitchShift = clone.SynthOctavePitchShift;
			mSynthPresets.mPresets[index].SynthNoteLength = clone.SynthNoteLength;
			mSynthPresets.mPresets[index].SynthRampUp = clone.SynthAttack;
			mSynthPresets.mPresets[index].SynthRampDown = clone.SynthDecay;
			mSynthPresets.mPresets[index].SynthWaveTypes = clone.SynthWaveTypes;
			mSynthPresets.mPresets[index].SynthWaveOperators = clone.SynthWaveOperators;
			mSynthPresets.mPresets[index].CustomEnvelope = clone.CustomEnvelope;
			mSynthPresets.mPresets[index].CustomPanvelope = clone.CustomPanvelope;
			mSynthPresets.mPresets[index].CustomSynthWaveform = clone.CustomSynthWaveform;
			mSynthPresets.mPresets[index].EnvelopeData = clone.EnvelopeData;
			mSynthPresets.mPresets[index].PanvelopeData = clone.PanvelopeData;
			mSynthPresets.mPresets[index].WaveformData = clone.WaveformData;
			mSynthPresets.mPresets[index].IsCustomWaveform = clone.IsCustomWaveform;
			mSynthPresets.mPresets[index].SubOctave1 = clone.SynthSubOctave1;
			mSynthPresets.mPresets[index].SubOctave2 = clone.SynthSubOctave2;
			SaveConfigurationData();
		}

		public static void ResetSynthData( ConfigurationData.InstrumentData data )
		{
			data.SynthOctavePitchShift = 3;
			data.SynthNoteLength = MusicConstants.NormalizedMid;
			data.SynthAttack = MusicConstants.MinSynthRamp;
			data.SynthDecay = MusicConstants.MinSynthRamp;
			data.SynthWaveTypes = new[]
			{
				SynthWaveType.Square, SynthWaveType.None, SynthWaveType.None, SynthWaveType.None
			};
			data.SynthWaveOperators = new[]
			{
				WaveOperator.Add, WaveOperator.Add, WaveOperator.Add
			};
			data.CustomEnvelope = new[]
			{
				0f, 1f, 0f
			};
			data.CustomPanvelope = new[]
			{
				0f, 0f, 0f
			};
			data.CustomSynthWaveform = new[]
			{
				0f, 1f, 0f
			};
			data.EnvelopeData = new List<BezierControlData>();
			data.PanvelopeData = new List<BezierControlData>();
			data.WaveformData = new List<BezierControlData>();
			data.IsCustomWaveform = false;
			data.SynthSubOctave1 = false;
			data.SynthSubOctave2 = false;
			SaveConfigurationData();
		}

		[Serializable]
		private class CachedSynthPresets
		{
			public List<SynthPreset> mPresets = new List<SynthPreset>();
		}

		private static CachedSynthPresets mSynthPresets = new CachedSynthPresets();

		private static void SaveConfigurationData()
		{
			var path = MusicConstants.SynthPresetsPath;
			//Debug.Log( $"saving Synth Presets to {path}" );

			if ( Directory.Exists( Application.persistentDataPath ) == false )
			{
				Directory.CreateDirectory( Application.persistentDataPath );
			}

			try
			{
				File.WriteAllText( path, JsonUtility.ToJson( mSynthPresets, prettyPrint: true ) );
				//Debug.Log( "Synth Presets were successfully written to file" );
			}
			catch ( IOException e )
			{
				Debug.Log( $"Synth Presets failed to write to file with exception {e}" );
			}
		}

		private static void LoadData()
		{
			var path = MusicConstants.SynthPresetsPath;

			if ( File.Exists( path ) )
			{
				mSynthPresets = JsonUtility.FromJson<CachedSynthPresets>( File.ReadAllText( path ) );
			}
		}

		private static void ApplySynthPreset( SynthPreset preset, ConfigurationData.InstrumentData data )
		{
			if ( preset == null )
			{
				return;
			}

			var clone = preset.Clone();
			data.SynthOctavePitchShift = clone.SynthOctavePitchShift;
			data.SynthNoteLength = clone.SynthNoteLength;
			data.SynthAttack = clone.SynthRampUp;
			data.SynthDecay = clone.SynthRampDown;
			data.SynthWaveTypes = clone.SynthWaveTypes;
			data.SynthWaveOperators = clone.SynthWaveOperators;
			data.IsCustomWaveform = clone.IsCustomWaveform;
			data.CustomSynthWaveform = clone.CustomSynthWaveform;
			data.CustomEnvelope = clone.CustomEnvelope;
			data.CustomPanvelope = clone.CustomPanvelope;
			data.EnvelopeData = clone.EnvelopeData;
			data.PanvelopeData = clone.PanvelopeData;
			data.WaveformData = clone.WaveformData;
			data.SynthSubOctave1 = clone.SubOctave1;
			data.SynthSubOctave2 = clone.SubOctave2;
			SaveConfigurationData();
		}
	}
}

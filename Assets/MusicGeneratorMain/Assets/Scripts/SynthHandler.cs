using ProcGenMusic;
using UnityEngine;

public class SynthHandler : MonoBehaviour
{
	public bool IsPlaying { get; private set; }
	public AudioSource AudioSource => mAudioSource;

	public void Play( int note, int octave, ConfigurationData.InstrumentData data, int subOctaveShift = 0 )
	{
		if ( data.SynthOctavePitchShift - subOctaveShift < 0 )
		{
			return;
		}

		SetData( note, octave, data, subOctaveShift );
		ResetPhases();

		IsPlaying = true;
	}

	public void Stop()
	{
		IsPlaying = false;
	}

	public void Pause()
	{
		IsPlaying = false;
	}

	public void UnPause()
	{
		IsPlaying = true;
	}

	[SerializeField]
	private AudioSource mAudioSource;

	private float mMaxVolume = .1f;
	private float mFrequency = 440.0f;
	private float mIncrement;
	private float mSinPhase;
	private float mCustomPhase;
	private float mEnvelopePhase;
	private float mPanvelopePhase;
	private float mAttackPhase;
	private float mDecayPhase;
	private float mSawtoothPhase;
	private float mTrianglePhase;
	private float mPingPongPhase;
	private float mSquarePhase;
	private float mSamplingFrequency = 48000.0f;
	private int mChannels;
	private float mSustain;
	private float mAttack = 3f;
	private float mDecay = 3f;
	private float mTotalNoteLength;
	private float mPan;
	private int mOctaveShift = 8;
	private SynthWaveType[] mSynthWaveTypes;
	private WaveOperator[] mWaveOperators;
	private float[] mCustomWave;
	private float[] mCustomEnvelope;
	private float[] mCustomPanvelope;
	private bool mIsCustom;

	private const float WAVE_CEILING = Mathf.PI * 2f;
	private const float SQUARE_CEILING = 0.6f;
	private const float MAX_GENERATED_GAIN = .5F;

	private readonly float[] mFrequencies =
	{
		8.17578125f, //C
		8.661953125f, //C#
		9.17701171875f, //D
		9.72271484375f, //D#
		10.300859375f, //E
		10.9133789062f, //F
		11.5623242188f, //F#
		12.2498632812f, //G
		12.97828125f, //G#
		13.75f, //A
		14.5676171875f, //A#
		15.4338476562f //B
	};

	private void Awake()
	{
		mSamplingFrequency = AudioSettings.outputSampleRate;
	}

	private void OnAudioFilterRead( float[] data, int channels )
	{
		if ( IsPlaying == false )
		{
			return;
		}

		mChannels = channels;

		for ( var index = 0; index < data.Length; index += channels )
		{
			float finalWave;
			if ( mIsCustom )
			{
				finalWave = GetWave( SynthWaveType.Custom );
			}
			else
			{
				var wave1 = GetWave( mSynthWaveTypes[0] );
				var wave2 = GetWave( mSynthWaveTypes[1] );
				var wave3 = GetWave( mSynthWaveTypes[2] );
				var wave4 = GetWave( mSynthWaveTypes[3] );

				finalWave = wave1;
				if ( mWaveOperators[0] != WaveOperator.None &&
				     wave2 != 0 )
				{
					finalWave = CombineWave( mWaveOperators[0], finalWave, wave2 );
				}

				if ( mWaveOperators[1] != WaveOperator.None &&
				     wave3 != 0 )
				{
					finalWave = CombineWave( mWaveOperators[1], finalWave, wave3 );
				}

				if ( mWaveOperators[2] != WaveOperator.None &&
				     wave4 != 0 )
				{
					finalWave = CombineWave( mWaveOperators[2], finalWave, wave4 );
				}
			}

			var envelopeGain = mIsCustom ? CustomGain() : GeneratedGain();
			var panGain = GetPannedGain();
			float left;
			float right;

			if ( mIsCustom )
			{
				left = Mathf.Min( 1f - panGain, 1f );
				right = Mathf.Min( 1f + panGain, 1f );
			}
			else
			{
				left = panGain > 0 ? .5f - ( panGain / 2f ) : 1f;
				right = panGain < 0 ? .5f + ( panGain / 2f ) : 1f;
			}

			data[index] = finalWave * left * envelopeGain * mMaxVolume;

			if ( mChannels == 2 )
			{
				data[index + 1] = finalWave * right * envelopeGain * mMaxVolume;
			}
		}
	}

	private static float CombineWave( WaveOperator waveOperator, float wave1, float wave2 )
	{
		return waveOperator switch
		{
			WaveOperator.Add => ( wave1 + wave2 ) / 2f,
			WaveOperator.Subtract => ( wave1 - wave2 ) * 2f,
			WaveOperator.Multiply => wave1 * wave2 / 2f,
			WaveOperator.Divide => wave1 % wave2 * 2f,
			_ => 0
		};
	}

	private float GetWave( SynthWaveType type )
	{
		return type switch
		{
			SynthWaveType.None => 0,
			SynthWaveType.Sawtooth => SawtoothWave(),
			SynthWaveType.Sin => SinWave(),
			SynthWaveType.Square => SquareWave(),
			SynthWaveType.Triangle => TriangleWave(),
			SynthWaveType.PingPong => PingPongWave(),
			SynthWaveType.Custom => CustomWave(),
			_ => 0
		};
	}

	private float SinWave()
	{
		var sinWave = SynthConstants.SinWave;
		if ( mSinPhase >= sinWave.Length )
		{
			mSinPhase -= sinWave.Length;
		}

		var wave = sinWave[(int) mSinPhase];
		mSinPhase += ( mFrequency * sinWave.Length ) / mSamplingFrequency;
		return wave;
	}

	private float SquareWave()
	{
		var sinWave = SynthConstants.SinWave;
		if ( mSquarePhase >= sinWave.Length )
		{
			mSquarePhase -= sinWave.Length;
		}

		var wave = sinWave[(int) mSquarePhase];
		mSquarePhase += ( mFrequency * sinWave.Length ) / mSamplingFrequency;

		return wave >= 0 ? SQUARE_CEILING : -SQUARE_CEILING;
	}

	private float PingPongWave()
	{
		var pingPonWave = SynthConstants.PingPongWave;
		if ( mPingPongPhase >= pingPonWave.Length )
		{
			mPingPongPhase -= pingPonWave.Length;
		}

		var wave = pingPonWave[(int) mPingPongPhase];
		mPingPongPhase += ( mFrequency * pingPonWave.Length ) / mSamplingFrequency;
		return wave;
	}

	private float TriangleWave()
	{
		mTrianglePhase += mIncrement;

		if ( mTrianglePhase > WAVE_CEILING )
		{
			mTrianglePhase -= WAVE_CEILING;
		}


		if ( mTrianglePhase < Mathf.PI )
		{
			return -( 2f / Mathf.PI * mTrianglePhase );
		}

		return 3f - ( 2f / Mathf.PI * mTrianglePhase );
	}

	private float SawtoothWave()
	{
		mSawtoothPhase += mIncrement;

		if ( mSawtoothPhase > WAVE_CEILING )
		{
			mSawtoothPhase -= WAVE_CEILING;
		}

		return 1f / Mathf.PI * mSawtoothPhase;
	}

	private float GetPannedGain()
	{
		if ( mChannels != 2 )
		{
			return 0f;
		}

		if ( mIsCustom )
		{
			return GetPanvelope();
		}

		return mPan;
	}

	private float GetPanvelope()
	{
		if ( mPanvelopePhase >= mCustomPanvelope.Length )
		{
			IsPlaying = false;
			return 0f;
		}


		var pan = mCustomPanvelope[(int) mPanvelopePhase];
		mPanvelopePhase += ( mCustomPanvelope.Length / mSustain / mSamplingFrequency );
		return pan;
	}

	private float GeneratedGain()
	{
		if ( mEnvelopePhase + mAttackPhase + mDecayPhase >= mTotalNoteLength )
		{
			IsPlaying = false;
			return 0;
		}

		if ( mAttack > 0 && mAttackPhase <= mAttack )
		{
			var attack = Mathf.Lerp( 0f, 1f, mAttackPhase / mAttack );
			mAttackPhase += ( 1f / mSamplingFrequency );
			return attack * MAX_GENERATED_GAIN;
		}

		if ( mSustain > 0 && mEnvelopePhase <= mSustain )
		{
			mEnvelopePhase += ( 1f / mSamplingFrequency );
			return MAX_GENERATED_GAIN;
		}

		if ( mDecay > 0 )
		{
			var decay = Mathf.Lerp( 1, 0f, mDecayPhase / mDecay );
			mDecayPhase += ( 1f / mSamplingFrequency );
			return decay * MAX_GENERATED_GAIN;
		}

		return 0;
	}

	private float CustomGain()
	{
		if ( mEnvelopePhase > mCustomEnvelope.Length )
		{
			IsPlaying = false;
			return 0;
		}

		var envelope = mCustomEnvelope[(int) mEnvelopePhase];
		mEnvelopePhase += ( mCustomEnvelope.Length / mSustain / mSamplingFrequency );
		return envelope;
	}

	private float CustomWave()
	{
		if ( mCustomPhase >= mCustomWave.Length )
		{
			mCustomPhase -= mCustomWave.Length;
		}

		var wave = mCustomWave[(int) mCustomPhase];
		mCustomPhase += ( mFrequency * mCustomWave.Length ) / mSamplingFrequency;
		return wave;
	}

	private void SetData( int note, int octave, ConfigurationData.InstrumentData data, int subOctaveShift = 0 )
	{
		mIsCustom = data.IsCustomWaveform;
		mCustomWave = mIsCustom ? data.CustomSynthWaveform : null;
		mCustomEnvelope = mIsCustom ? data.CustomEnvelope : null;
		mCustomPanvelope = mIsCustom ? data.CustomPanvelope : null;
		mOctaveShift = data.SynthOctavePitchShift - subOctaveShift;
		mSustain = data.SynthNoteLength;
		mMaxVolume = data.Volume;
		mAttack = data.SynthAttack;
		mDecay = data.SynthDecay;
		mPan = data.StereoPan;
		mTotalNoteLength = mAttack + mSustain + mDecay;
		mSynthWaveTypes = mIsCustom ? null : data.SynthWaveTypes;
		mWaveOperators = mIsCustom ? null : data.SynthWaveOperators;

		// We don't actually want to play this as it doesn't exist.
		if ( mOctaveShift < 0 )
		{
			return;
		}

		if ( mOctaveShift > 0 )
		{
			mFrequency = mFrequencies[note] * Mathf.Pow( 2, octave ) * Mathf.Pow( 2, mOctaveShift );
		}
		else
		{
			mFrequency = mFrequencies[note] * Mathf.Pow( 2, octave );
		}

		mIncrement = ( mFrequency * 2.0f * Mathf.PI ) / mSamplingFrequency;
	}

	private void ResetPhases()
	{
		mEnvelopePhase = 0f;
		mPanvelopePhase = 0f;
		mAttackPhase = 0f;
		mDecayPhase = 0f;
		mSinPhase = 0;
		mCustomPhase = 0;
		mSquarePhase = 0;
	}
}

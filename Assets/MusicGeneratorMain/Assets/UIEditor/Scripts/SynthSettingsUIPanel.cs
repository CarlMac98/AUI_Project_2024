using UnityEngine;
using UnityEngine.UI;

namespace ProcGenMusic
{
	public class SynthSettingsUIPanel : MonoBehaviour
	{
		public void Initialize( Instrument instrument, UIManager uiManager )
		{
			mInstrument = instrument;
			mUIManager = uiManager;

			if ( mOpenWaveformEditorButton )
			{
				mOpenWaveformEditorButton.onClick.AddListener( OpenWaveformEditor );
			}

			mSynthOctaveShift.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthOctavePitchShift = ( int )value;
					mSynthOctaveShift.Text.text = $"{value}";
				},
				mInstrument.InstrumentData.SynthOctavePitchShift,
				createDividers: true );

			mSynthOctave1.Initialize( ( value ) => { mInstrument.InstrumentData.SynthSubOctave1 = value; }, mInstrument.InstrumentData.SynthSubOctave1 );

			mSynthOctave2.Initialize( ( value ) => { mInstrument.InstrumentData.SynthSubOctave2 = value; }, mInstrument.InstrumentData.SynthSubOctave2 );

			mSynthLength.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthNoteLength = value;
					mSynthLength.Text.text = $"{value:0.00}";
				},
				mInstrument.InstrumentData.SynthNoteLength );

			mSynthRampUp.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthAttack = value;
					mSynthRampUp.Text.text = $"{value:0.00}";
				},
				mInstrument.InstrumentData.SynthAttack );

			mSynthRampDown.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthDecay = value;
					mSynthRampDown.Text.text = $"{value:0.00}";
				},
				mInstrument.InstrumentData.SynthDecay );

			mSynthWaveType1.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthWaveTypes[0] = ( SynthWaveType )( int )value;
					mSynthWaveType1.Text.text = $"{mInstrument.InstrumentData.SynthWaveTypes[0]}";
				},
				( int )mInstrument.InstrumentData.SynthWaveTypes[0] );

			mSynthWaveType2.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthWaveTypes[1] = ( SynthWaveType )( int )value;
					mSynthWaveType2.Text.text = $"{mInstrument.InstrumentData.SynthWaveTypes[1]}";
				},
				( int )mInstrument.InstrumentData.SynthWaveTypes[1] );

			mSynthWaveType3.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthWaveTypes[2] = ( SynthWaveType )( int )value;
					mSynthWaveType3.Text.text = $"{mInstrument.InstrumentData.SynthWaveTypes[2]}";
				},
				( int )mInstrument.InstrumentData.SynthWaveTypes[2] );

			mSynthWaveType4.Initialize( ( value ) =>
				{
					mInstrument.InstrumentData.SynthWaveTypes[3] = ( SynthWaveType )( int )value;
					mSynthWaveType4.Text.text = $"{mInstrument.InstrumentData.SynthWaveTypes[3]}";
				},
				( int )mInstrument.InstrumentData.SynthWaveTypes[3] );

			mCustomWaveformToggle.Initialize( isOn =>
				{
					mInstrument.InstrumentData.IsCustomWaveform = isOn;
					ToggleCustomWaveform( isOn );
					if ( isOn )
					{
						mUIManager.BezierEditorPanel.InitializeNewSynth( mInstrument );
					}

					mUIManager.DirtyEditorDisplays();
				},
				mInstrument.InstrumentData.IsCustomWaveform );

			for ( var index = 0; index < mTypeOperators1.Toggles.Length; index++ )
			{
				var toggleIndex = index;

				mTypeOperators1.Toggles[toggleIndex].Initialize( isOn =>
					{
						if ( isOn )
						{
							mInstrument.InstrumentData.SynthWaveOperators[0] = ( WaveOperator )toggleIndex;
						}
					},
					mInstrument.InstrumentData.SynthWaveOperators[0] == ( WaveOperator )index );
			}

			for ( var index = 0; index < mTypeOperators2.Toggles.Length; index++ )
			{
				var toggleIndex = index;
				mTypeOperators2.Toggles[index].Initialize( isOn =>
					{
						if ( isOn )
						{
							mInstrument.InstrumentData.SynthWaveOperators[1] = ( WaveOperator )toggleIndex;
						}
					},
					mInstrument.InstrumentData.SynthWaveOperators[1] == ( WaveOperator )index );
			}

			for ( var index = 0; index < mTypeOperators3.Toggles.Length; index++ )
			{
				var toggleIndex = index;
				mTypeOperators3.Toggles[index].Initialize( isOn =>
					{
						if ( isOn )
						{
							mInstrument.InstrumentData.SynthWaveOperators[2] = ( WaveOperator )toggleIndex;
						}
					},
					mInstrument.InstrumentData.SynthWaveOperators[2] == ( WaveOperator )index );
			}

			mSynthPresetDropdown.Initialize( mInstrument, mUIManager );
		}

		public void UpdateUIElementValues( Instrument instrument )
		{
			mInstrument = instrument;
			mSynthPresetDropdown.UpdateUIElementValues( mInstrument );
			mSynthOctaveShift.Option.value = mInstrument.InstrumentData.SynthOctavePitchShift;
			mSynthOctave1.Option.isOn = mInstrument.InstrumentData.SynthSubOctave1;
			mSynthOctave2.Option.isOn = mInstrument.InstrumentData.SynthSubOctave2;
			mSynthLength.Option.value = mInstrument.InstrumentData.SynthNoteLength;
			mSynthRampUp.Option.value = mInstrument.InstrumentData.SynthAttack;
			mSynthRampDown.Option.value = mInstrument.InstrumentData.SynthDecay;
			mSynthWaveType1.Option.value = ( int )mInstrument.InstrumentData.SynthWaveTypes[0];
			mSynthWaveType2.Option.value = ( int )mInstrument.InstrumentData.SynthWaveTypes[1];
			mSynthWaveType3.Option.value = ( int )mInstrument.InstrumentData.SynthWaveTypes[2];
			mSynthWaveType4.Option.value = ( int )mInstrument.InstrumentData.SynthWaveTypes[3];
			mCustomWaveformToggle.Option.isOn = mInstrument.InstrumentData.IsCustomWaveform;

			foreach ( var toggle in mTypeOperators1.Toggles )
			{
				toggle.Option.isOn = false;
			}

			foreach ( var toggle in mTypeOperators2.Toggles )
			{
				toggle.Option.isOn = false;
			}

			foreach ( var toggle in mTypeOperators3.Toggles )
			{
				toggle.Option.isOn = false;
			}

			mTypeOperators1.Toggles[( int )mInstrument.InstrumentData.SynthWaveOperators[0]].Option.isOn = true;
			mTypeOperators2.Toggles[( int )mInstrument.InstrumentData.SynthWaveOperators[1]].Option.isOn = true;
			mTypeOperators3.Toggles[( int )mInstrument.InstrumentData.SynthWaveOperators[2]].Option.isOn = true;
		}

		private UIManager mUIManager;
		private Instrument mInstrument;

		[SerializeField, Tooltip( "Reference to our synth octave shift slider" )]
		private UISlider mSynthOctaveShift;

		[SerializeField, Tooltip( "Reference to our synth sub octave 1 toggle" )]
		private UIToggle mSynthOctave1;

		[SerializeField, Tooltip( "Reference to our synth sub octave 1 toggle" )]
		private UIToggle mSynthOctave2;

		[SerializeField, Tooltip( "Reference to our synth length slider" )]
		private UISlider mSynthLength;

		[SerializeField, Tooltip( "Reference to our synth ramp up slider" )]
		private UISlider mSynthRampUp;

		[SerializeField, Tooltip( "Reference to our synth ramp down slider" )]
		private UISlider mSynthRampDown;

		[SerializeField, Tooltip( "Reference to our synth wave type slider1" )]
		private UISlider mSynthWaveType1;

		[SerializeField, Tooltip( "Reference to our synth wave type slider2" )]
		private UISlider mSynthWaveType2;

		[SerializeField, Tooltip( "Reference to our synth wave type slider3" )]
		private UISlider mSynthWaveType3;

		[SerializeField, Tooltip( "Reference to our synth wave type slider4" )]
		private UISlider mSynthWaveType4;

		[SerializeField, Tooltip( "Reference to our synth operator 1" )]
		private UIToggleGroup mTypeOperators1;

		[SerializeField, Tooltip( "Reference to our synth operator 2" )]
		private UIToggleGroup mTypeOperators2;

		[SerializeField, Tooltip( "Reference to our synth operator 3" )]
		private UIToggleGroup mTypeOperators3;

		[SerializeField]
		private SynthPresetDropdown mSynthPresetDropdown;

		[SerializeField]
		private UIToggle mCustomWaveformToggle;

		[SerializeField]
		private Button mOpenWaveformEditorButton;

		[SerializeField]
		private GameObject mSynthTypesParent;

		private void ToggleCustomWaveform( bool isCustom )
		{
			mSynthTypesParent.SetActive( isCustom == false );
			mSynthRampUp.gameObject.SetActive( isCustom == false );
			mSynthRampDown.gameObject.SetActive( isCustom == false );
		}

		private void OnDestroy()
		{
			if ( mOpenWaveformEditorButton )
			{
				mOpenWaveformEditorButton.onClick.RemoveListener( OpenWaveformEditor );
			}
		}

		private void OpenWaveformEditor()
		{
			mUIManager.EnableWaveformEditor( true );
			mUIManager.GeneralMenuPanel.TogglePanel();
			//mUIManager.DirtyEditorDisplays();
		}
	}
}
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProcGenMusic
{
	public class SynthPresetDropdown : MonoBehaviour
	{
		public void Initialize( Instrument instrument, UIManager uiManager )
		{
			mInstrument = instrument;
			mUIManager = uiManager;
			SetSynthPresetData();
		}

		public void UpdateUIElementValues( Instrument instrument )
		{
			mInstrument = instrument;
			mSynthPresets.Option.SetValueWithoutNotify( -1 );
			mSynthPresets.Text.SetText( "Load Preset" );
		}

		[SerializeField, Tooltip( "Reference to our synth presets dropdown" )]
		private UITMPDropdown mSynthPresets;

		[SerializeField]
		private Button mExportSynthPresetButton;

		[SerializeField]
		private Button mDeleteSynthPresetButton;

		[SerializeField]
		private TMP_InputField mExportPresetInputField;

		private const string DEFAULT_SYNTH_NAME = "Default";
		private Instrument mInstrument;
		private UIManager mUIManager;

		private void DeleteSynthPreset()
		{
			if ( string.IsNullOrEmpty( mExportPresetInputField.text ) )
			{
				return;
			}

			SynthPresets.RemoveSynthPreset( mExportPresetInputField.text );

			var presetIndex = mSynthPresets.Option.options.FindIndex( x => x.text == mExportPresetInputField.text );
			if ( presetIndex > 0 )
			{
				mSynthPresets.Option.options.RemoveAt( presetIndex );
			}
		}

		private void SavePreset()
		{
			if ( string.IsNullOrEmpty( mExportPresetInputField.text ) )
			{
				return;
			}

			var presetName = mExportPresetInputField.text;
			var existingIndex = SynthPresets.Presets.ToList().FindIndex( x => x.Name == presetName );
			if ( existingIndex >= 0 )
			{
				SynthPresets.UpdateSynthPreset( existingIndex, mInstrument.InstrumentData );
				mSynthPresets.Option.SetValueWithoutNotify( -1 );
				mSynthPresets.Text.SetText( "Load Preset" );
			}
			else
			{
				SynthPresets.AddSynthPreset( mInstrument.InstrumentData, presetName );
				var presetData = new TMP_Dropdown.OptionData {text = presetName};
				mSynthPresets.Option.options.Add( presetData );
				mSynthPresets.Option.SetValueWithoutNotify( -1 );
				mSynthPresets.Text.SetText( "Load Preset" );
			}
		}

		private void SetSynthPresetData()
		{
			var defaultPresetData = new TMP_Dropdown.OptionData {text = DEFAULT_SYNTH_NAME};
			mSynthPresets.Option.options.Add( defaultPresetData );

			foreach ( var preset in SynthPresets.Presets )
			{
				var presetData = new TMP_Dropdown.OptionData {text = preset.Name};
				mSynthPresets.Option.options.Add( presetData );
			}

			mSynthPresets.Option.SetValueWithoutNotify( -1 );
			mSynthPresets.Text.SetText( "Load Preset" );

			mSynthPresets.Initialize( value =>
				{
					if ( value > 0 && value < mSynthPresets.Option.options.Count )
					{
						SynthPresets.ApplySynthPreset( value - 1, mInstrument.InstrumentData );
					}
					else
					{
						SynthPresets.ResetSynthData( mInstrument.InstrumentData );
					}

					mSynthPresets.Option.SetValueWithoutNotify( -1 );
					mSynthPresets.Text.SetText( "Load Preset" );
					mInstrument.InstrumentData.SynthPreset = mSynthPresets.Option.options[value].text;
					mUIManager.DirtyEditorDisplays();
				},
				0 );

			mExportSynthPresetButton.onClick.AddListener( SavePreset );
			mDeleteSynthPresetButton.onClick.AddListener( DeleteSynthPreset );
		}
	}
}
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI Interface for the general music generator settings
	/// </summary>
	public class MusicGeneratorUIPanel : UIPanel
	{
		#region public

		/// <summary>
		/// Sets the key
		/// </summary>
		/// <param name="key"></param>
		public void SetKey( int key )
		{
			mKey.Option.value = key;
		}

		/// <summary>
		/// Fades the Volume
		/// </summary>
		/// <param name="volume"></param>
		public void FadeVolume( float volume )
		{
#if FMOD_ENABLED
			mFmodMasterFader.Option.value = volume;
#else
			mMasterVolume.Option.value = volume;
#endif //FMOD_ENABLED
		}

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
#if FMOD_ENABLED
			mFmodMasterFader.Option.value = mMusicGenerator.ConfigurationData.FmodMasterFader;
			mFmodMasterGain.Option.value = mMusicGenerator.ConfigurationData.FmodMasterGain;
#else
			mMasterVolume.Option.value = mMusicGenerator.ConfigurationData.MasterVolume;
#endif //FMOD_ENABLED

			mMode.Option.value = ( int )mMusicGenerator.ConfigurationData.Mode;
			mTempo.Option.value = mMusicGenerator.InstrumentSet.Data.Tempo;
			mScale.Option.value = ( int )mMusicGenerator.ConfigurationData.Scale;
			mKey.Option.value = ( int )mMusicGenerator.ConfigurationData.Key;
			var themeRepeatOptions = mMusicGenerator.ConfigurationData.ThemeRepeatOptions;
			mRepeatThemeOptions.Option.value = ( int )themeRepeatOptions;
			mNewThemeOdds.Option.value = mMusicGenerator.ConfigurationData.SetThemeOdds;
			mRepeatThemeOdds.Option.value = mMusicGenerator.ConfigurationData.PlayThemeOdds;
			mRepeatThemeOdds.Title.text = mMusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif ? mLeitmotifOddsTitle : mRepeatThemeOddsTitle;
			mProgressionRate.Option.value =
				mMusicGenerator.InstrumentSet.GetProgressionRateIndex( ( mMusicGenerator.InstrumentSet.Data.ProgressionRate ) );
			mProgressionRate.Text.text = $"{mMusicGenerator.InstrumentSet.Data.ProgressionRate}";
			mProgressionChangeOdds.Option.value = mMusicGenerator.ConfigurationData.ProgressionChangeOdds;
			mKeyChangeOdds.Option.value = mMusicGenerator.ConfigurationData.KeyChangeOdds;
			mOverallGroupOdds.Option.value = mMusicGenerator.ConfigurationData.OverallGroupChangeOdds;
			mGroupOdds1.Option.value = mMusicGenerator.ConfigurationData.GroupOdds[0];
			mGroupOdds2.Option.value = mMusicGenerator.ConfigurationData.GroupOdds[1];
			mGroupOdds3.Option.value = mMusicGenerator.ConfigurationData.GroupOdds[2];
			mGroupOdds4.Option.value = mMusicGenerator.ConfigurationData.GroupOdds[3];
			mDynamicStyle.Option.isOn = mMusicGenerator.ConfigurationData.DynamicStyle == DynamicStyle.Random;
			OnRepeatOptionsChanged( false );
		}

		/// <summary>
		/// Breaks our display
		/// </summary>
		public void BreakDisplay()
		{
			if ( IsEnabled )
			{
				UpdateUIElementValues();
			}
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			mScale.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.Scale = ( Scale )value;
					mScale.Text.text = $"{mMusicGenerator.ConfigurationData.Scale}";
					mUIManager.DirtyEditorDisplays();
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.Scale,
				createDividers: true );

			mMode.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.Mode = ( Mode )value;
					mMode.Text.text = $"{mMusicGenerator.ConfigurationData.Mode}";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.Mode,
				createDividers: true );

			mKey.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.Key = ( Key )value;
					mKey.Text.text = $"{mMusicGenerator.ConfigurationData.Key}";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.Key,
				createDividers: true );

			mTempo.Initialize( ( value ) =>
				{
					mTempo.Option.maxValue = MusicConstants.MaxTempo;
					mTempo.Option.minValue = MusicConstants.MinTempo;
					mMusicGenerator.InstrumentSet.Data.Tempo = value;
					mMusicGenerator.InstrumentSet.UpdateTempo();
					mUIManager.DirtyEditorDisplays();
					mTempo.Text.text = $"{60f / mMusicGenerator.InstrumentSet.BeatLength:0.00} bpm";
				},
				initialValue: mMusicGenerator.InstrumentSet.Data.Tempo );

#if FMOD_ENABLED
			mFmodMasterFader.VisibleObject.SetActive( true );
			mFmodMasterGain.VisibleObject.SetActive(true);
			mMasterVolume.VisibleObject.SetActive( false );
			mFmodMasterFader.Initialize( ( value ) =>
				{
					mFmodMasterFader.Text.text = $"{value:0.00}%";
					mMusicGenerator.ConfigurationData.FmodMasterFader = value;
					mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterFaderName, parameterValue: value );
				},
				initialValue: mMusicGenerator.ConfigurationData.FmodMasterFader );
			
			mFmodMasterGain.Initialize( ( value ) =>
				{
					mFmodMasterGain.Text.text = $"{value:0.00}%";
					mMusicGenerator.ConfigurationData.FmodMasterGain = value;
					mMusicGenerator.AudioHandler.UpdateParameter( MusicConstants.FmodMasterGainName, parameterValue: value );
				},
				initialValue: mMusicGenerator.ConfigurationData.FmodMasterGain );
#else
			mFmodMasterFader.VisibleObject.SetActive( false );
			mFmodMasterGain.VisibleObject.SetActive( false );
			mMasterVolume.VisibleObject.SetActive( true );
			mMasterVolume.Initialize( ( value ) =>
				{
					mMasterVolume.Text.text = $"{value:0.00} dB";
					mMusicGenerator.SetVolume( value );
				},
				initialValue: mMusicGenerator.ConfigurationData.MasterVolume );
#endif //FMOD_ENABLED

			mRepeatThemeOptions.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.ThemeRepeatOptions = ( ThemeRepeatOptions )value;
					mRepeatThemeOptions.Text.text = $"{( ThemeRepeatOptions )value}";
					OnRepeatOptionsChanged();
					ResizeRepeatOptions( ( int )value );
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.ThemeRepeatOptions,
				resetValue: null,
				true );

			var repeatLength = mMusicGenerator.InstrumentSet.Data.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif ?
				mMusicGenerator.InstrumentSet.Data.NumLeitmotifMeasures :
				mMusicGenerator.InstrumentSet.Data.RepeatMeasuresNum;
			mRepeatLength.Initialize( ( value ) =>
				{
					if ( mMusicGenerator.InstrumentSet.Data.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif )
					{
						mMusicGenerator.InstrumentSet.Data.NumLeitmotifMeasures = ( int )value;
					}
					else
					{
						mMusicGenerator.InstrumentSet.Data.RepeatMeasuresNum = ( int )value;
					}

					mRepeatLength.Text.text = $"{value}";
					// changing this while playing will bork our repeat notes.
					mUIManager.UIKeyboard.Stop();
				},
				initialValue: repeatLength );

			mNewThemeOdds.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.SetThemeOdds = value;
					mNewThemeOdds.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.SetThemeOdds );

			mRepeatThemeOdds.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.PlayThemeOdds = value;
					mRepeatThemeOdds.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.PlayThemeOdds );

			mProgressionRate.Initialize( ( value ) =>
				{
					mMusicGenerator.InstrumentSet.SetInverseProgressionRate( ( int )value );
					mProgressionRate.Text.text = $"{mMusicGenerator.InstrumentSet.Data.ProgressionRate}";
					mUIManager.DirtyEditorDisplays();
				},
				initialValue: mMusicGenerator.InstrumentSet.GetProgressionRateIndex( mMusicGenerator.InstrumentSet.Data.ProgressionRate ) );

			mProgressionChangeOdds.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.ProgressionChangeOdds = value;
					mProgressionChangeOdds.Text.text = $"{value}%";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.ProgressionChangeOdds );

			mKeyChangeOdds.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.KeyChangeOdds = value;
					mKeyChangeOdds.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.KeyChangeOdds );

			mOverallGroupOdds.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.OverallGroupChangeOdds = value;
					mOverallGroupOdds.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.OverallGroupChangeOdds );

			mGroupOdds1.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.GroupOdds[0] = value;
					mGroupOdds1.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.GroupOdds[0] );

			mGroupOdds2.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.GroupOdds[1] = value;
					mGroupOdds2.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.GroupOdds[1] );

			mGroupOdds3.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.GroupOdds[2] = value;
					mGroupOdds3.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.GroupOdds[2] );

			mGroupOdds4.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.GroupOdds[3] = value;
					mGroupOdds4.Text.text = $"{value}%";
				},
				initialValue: mMusicGenerator.ConfigurationData.GroupOdds[3] );

			mDynamicStyle.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.DynamicStyle = value ? DynamicStyle.Random : DynamicStyle.Linear;
					mDynamicStyle.Text.text = value ? "Dynamic Style: Random" : "Dynamic Style: Linear";
				},
				initialValue: mMusicGenerator.ConfigurationData.DynamicStyle == DynamicStyle.Random );
		}

		#endregion protected

		#region private

		[SerializeField, Tooltip( "Reference to our options group" )]
		private VerticalLayoutGroup mOptionsGroup;

		[SerializeField, Tooltip( "Reference to our Options rect" )]
		private RectTransform mOptionsRect;

		[SerializeField, Tooltip( "Reference to our content rect" )]
		private RectTransform mContentRect;

		[SerializeField, Tooltip( "Reference to our mode slider" )]
		private UISlider mMode;

		[SerializeField, Tooltip( "Reference to our tempo slider" )]
		private UISlider mTempo;

		[SerializeField, Tooltip( "Reference to our Volume slider" )]
		private UISlider mMasterVolume;

		[SerializeField, Tooltip( "Reference to our FMOD Volume slider" )]
		private UISlider mFmodMasterFader;

		[SerializeField, Tooltip( "Reference to our FMOD Gain slider" )]
		private UISlider mFmodMasterGain;

		[SerializeField, Tooltip( "Reference to our scale slider" )]
		private UISlider mScale;

		[SerializeField, Tooltip( "Reference to our key slider" )]
		private UISlider mKey;

		[SerializeField, Tooltip( "Reference to our layout group for repeat options" )]
		private VerticalLayoutGroup mRepeatGroup;

		[SerializeField, Tooltip( "Reference to our rect transform for repeat options" )]
		private RectTransform mRepeatGroupRect;

		[SerializeField, Tooltip( "Reference to our height of our elements" )]
		private float mElementHeight;

		[SerializeField, Tooltip( "Reference to our repeat theme options slider" )]
		private UISlider mRepeatThemeOptions;

		[SerializeField, Tooltip( "Reference to our Repeat length slider" )]
		private UISlider mRepeatLength;

		[SerializeField, Tooltip( "Reference to our new theme odds slider" )]
		private UISlider mNewThemeOdds;

		[SerializeField, Tooltip( "Reference to our repeat theme odds slider" )]
		private UISlider mRepeatThemeOdds;

		[SerializeField, Tooltip( "Reference to our progression rate slider" )]
		private UISlider mProgressionRate;

		[SerializeField, Tooltip( "Reference to our progression change odds slider" )]
		private UISlider mProgressionChangeOdds;

		[SerializeField, Tooltip( "Reference to our key change odds slider" )]
		private UISlider mKeyChangeOdds;

		[SerializeField, Tooltip( "Reference to our overall group odds slider" )]
		private UISlider mOverallGroupOdds;

		[SerializeField, Tooltip( "Reference to our group odds 1 slider" )]
		private UISlider mGroupOdds1;

		[SerializeField, Tooltip( "Reference to our group odds 2 slider" )]
		private UISlider mGroupOdds2;

		[SerializeField, Tooltip( "Reference to our group odds 3 slider" )]
		private UISlider mGroupOdds3;

		[SerializeField, Tooltip( "Reference to our group odds 4 slider" )]
		private UISlider mGroupOdds4;

		[Tooltip( "Whether groups are chosen randomly or linearly" )]
		[SerializeField] private UIToggle mDynamicStyle;

		[SerializeField, Tooltip( "Reference to our Leitmotif odds Title" )]
		private string mLeitmotifOddsTitle = "Leitmotif Odds";

		[SerializeField, Tooltip( "Reference to our repeat theme odds title" )]
		private string mRepeatThemeOddsTitle = "Repeat Odds";

		/// <summary>
		/// Active number of repeat options
		/// </summary>
		private int mActiveRepeatOptions;

		/// <summary>
		/// Invoked when repeat options are changed. Handles visibility of dependent ui elements
		/// </summary>
		private void OnRepeatOptionsChanged( bool stopPlayer = true )
		{
			switch ( mMusicGenerator.ConfigurationData.ThemeRepeatOptions )
			{
				case ThemeRepeatOptions.Leitmotif:
					mRepeatLength.Option.value = mMusicGenerator.InstrumentSet.Data.NumLeitmotifMeasures;
					mRepeatLength.VisibleObject.SetActive( true );
					mNewThemeOdds.VisibleObject.SetActive( false );
					mRepeatThemeOdds.VisibleObject.SetActive( true );
					mActiveRepeatOptions = 3;
					break;
				case ThemeRepeatOptions.Repeat:
					mRepeatLength.Option.value = mMusicGenerator.InstrumentSet.Data.RepeatMeasuresNum;
					mRepeatLength.VisibleObject.SetActive( true );
					mNewThemeOdds.VisibleObject.SetActive( false );
					mRepeatThemeOdds.VisibleObject.SetActive( false );
					mActiveRepeatOptions = 2;
					break;
				case ThemeRepeatOptions.Theme:
					mRepeatLength.Option.value = mMusicGenerator.InstrumentSet.Data.RepeatMeasuresNum;
					mRepeatLength.VisibleObject.SetActive( true );
					mNewThemeOdds.VisibleObject.SetActive( true );
					mRepeatThemeOdds.VisibleObject.SetActive( true );
					mActiveRepeatOptions = 4;
					break;
				default:
					mRepeatLength.VisibleObject.SetActive( false );
					mNewThemeOdds.VisibleObject.SetActive( false );
					mRepeatThemeOdds.VisibleObject.SetActive( false );
					mActiveRepeatOptions = 1;
					break;
			}

			mRepeatThemeOdds.Title.text = mMusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif ? mLeitmotifOddsTitle : mRepeatThemeOddsTitle;

			if ( stopPlayer && mMusicGenerator.GeneratorState == GeneratorState.Playing )
			{
				mUIManager.UIKeyboard.Stop();
			}
		}

		/// <summary>
		/// Resizes our repeat options based on enabled elements
		/// </summary>
		/// <param name="value"></param>
		private void ResizeRepeatOptions( int value )
		{
			mRepeatGroupRect.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Vertical,
				( mActiveRepeatOptions - 1 ) * mRepeatGroup.spacing +
				mActiveRepeatOptions * mElementHeight +
				mRepeatGroup.padding.top +
				mRepeatGroup.padding.bottom
			);

			var activeOptionsElements = mOptionsRect.childCount;
			activeOptionsElements += value == ( int )ThemeRepeatOptions.None ? 0 : mActiveRepeatOptions - 1;
			mContentRect.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Vertical,
				( activeOptionsElements - 1 ) * mOptionsGroup.spacing +
				activeOptionsElements * mElementHeight +
				mOptionsGroup.padding.top +
				mOptionsGroup.padding.bottom
			);
		}

		#endregion private
	}
}
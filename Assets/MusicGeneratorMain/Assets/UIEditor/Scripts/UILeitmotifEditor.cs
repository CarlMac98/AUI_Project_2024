using System.Collections;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI Panel for leitmotif settings
	/// </summary>
	public class UILeitmotifEditor : UIDisplayEditor
	{
		#region public

		///<inheritdoc/>
		public override Key Key => mUIManager.MusicGenerator.ConfigurationData.Key;

		///<inheritdoc/>
		public override Scale Scale => mUIManager.MusicGenerator.ConfigurationData.Scale;

		///<inheritdoc/>
		public override Mode Mode => mUIManager.MusicGenerator.ConfigurationData.Mode;

		/// <summary>
		/// Our cached leitmotifOdds value.
		/// </summary>
		public float CachedLeitmotifOdds => mCachedLeitmotifOdds;

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			base.Initialize( uiManager, isEnabled );

			mInstrumentSet = uiManager.CurrentInstrumentSet;
			mLeitmotifEditor = uiManager.LeitmotifEditor;
		}

		///<inheritdoc/>
		public override void Pause()
		{
			mLeitmotifEditor.Pause();
		}

		///<inheritdoc/>
		public override void Stop()
		{
			base.Stop();
			mMusicGenerator.ConfigurationData.PlayThemeOdds = mCachedLeitmotifOdds;
			mLeitmotifOdds.Option.value = mCachedLeitmotifOdds;
			mLeitmotifEditor.Stop();
		}

		///<inheritdoc/>
		public override void Play()
		{
			if ( mMusicGenerator.GeneratorState == GeneratorState.Playing )
			{
				return;
			}

			mMusicGenerator.ResetPlayer();
			mLeitmotifEditor.Stop();
			mLeitmotifEnabledButton.Option.isOn = true;
			mCachedLeitmotifOdds = mMusicGenerator.ConfigurationData.PlayThemeOdds;
			mMusicGenerator.ConfigurationData.PlayThemeOdds = 100f;
			mLeitmotifEditor.Play();
		}

		///<inheritdoc/>
		public override void Reset()
		{
			mInstrumentSet.Reset();
			mCurrentMeasure.Option.value = 0;
			mInstrumentSet.RepeatCount = 0;
		}

		///<inheritdoc/>
		public override void Save( string filename )
		{
			// no need to save, it's serialized with the instrument InstrumentSet
		}

		///<inheritdoc/>
		public override void SetPanelActive( bool isActive )
		{
			if ( isActive )
			{
				SetLeitmotifOddsCache( mMusicGenerator.ConfigurationData.PlayThemeOdds );
			}
			else
			{
				RestoreLeitmotifOdds();
			}

			mInstrumentSet = mUIManager.CurrentInstrumentSet;
			base.SetPanelActive( isActive );
			StartCoroutine( LoadPanel( isActive ) );
		}

		/// <summary>
		/// Sets our leitmotifOdds cache. We do this in order to have the leitmotif always play while editing it.
		/// </summary>
		public void SetLeitmotifOddsCache( float leitmotifOdds )
		{
			mCachedLeitmotifOdds = leitmotifOdds;
		}

		/// <summary>
		/// Restores the leitmotif odds
		/// </summary>
		public void RestoreLeitmotifOdds()
		{
			mMusicGenerator.ConfigurationData.PlayThemeOdds = mCachedLeitmotifOdds;
		}

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			if ( IsEnabled == false )
			{
				return;
			}

			base.UpdateUIElementValues();
			mCurrentMeasure.Option.value = mMusicGenerator.InstrumentSet.RepeatCount;
			mLeitmotifOdds.Option.value = mMusicGenerator.ConfigurationData.PlayThemeOdds;
			mLeitmotifEnabledButton.Option.isOn = mMusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif;
			mNumberOfMeasures.Option.value = mMusicGenerator.ConfigurationData.NumLeitmotifMeasures;
			mIgnoreGroupsButton.Option.isOn = mMusicGenerator.ConfigurationData.LeitmotifIgnoresGroups;
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override IEnumerator LoadPanel( bool isActive )
		{
			mState = DisplayEditorState.Initializing;
			yield return mUIManager.EnableLeitmotifEditor( isActive );
			mState = isActive ? DisplayEditorState.Stopped : DisplayEditorState.Inactive;
		}

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			base.InitializeListeners();

			mCurrentMeasure.Initialize( ( value ) =>
				{
					value++;
					mCurrentMeasure.Text.text = $"{value}";

					if ( mNumberOfMeasures.Option.value < value )
					{
						mNumberOfMeasures.Option.value = value;
					}

					mDisplayIsDirty = true;
				},
				initialValue: 0 );

			mLeitmotifOdds.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.PlayThemeOdds = value;
					mLeitmotifOdds.Text.text = $"{value}";
					mCachedLeitmotifOdds = value;
				},
				initialValue: mMusicGenerator.ConfigurationData.PlayThemeOdds );

			mNumberOfMeasures.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.NumLeitmotifMeasures = ( int )value;

					if ( mCurrentMeasure.Option.value >= value )
					{
						mCurrentMeasure.Option.value = value - 1;
						mDisplayIsDirty = true;
					}

					mNumberOfMeasures.Text.text = $"{value}";
				},
				initialValue: mMusicGenerator.ConfigurationData.NumLeitmotifMeasures );

			mLeitmotifEnabledButton.Initialize( ( value ) =>
				{
					if ( value )
					{
						mMusicGenerator.ConfigurationData.ThemeRepeatOptions = ThemeRepeatOptions.Leitmotif;
					}

					mLeitmotifEnabledButton.Text.text = value ? "Leitmotif is enabled" : "Leitmotif is disabled";
				},
				initialValue: mMusicGenerator.ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif );

			mIgnoreGroupsButton.Initialize( ( value ) => { mMusicGenerator.ConfigurationData.LeitmotifIgnoresGroups = value; },
				initialValue: mMusicGenerator.ConfigurationData.LeitmotifIgnoresGroups );
		}

		#endregion protected

		#region private

		[SerializeField, Tooltip( "Reference to our leitmotif odds" )]
		private UISlider mLeitmotifOdds;

		[SerializeField, Tooltip( "Reference to our whether leitmotif is enabled button" )]
		private UIToggle mLeitmotifEnabledButton;

		[SerializeField, Tooltip( "Reference to our ignore groups toggle" )]
		private UIToggle mIgnoreGroupsButton;

		/// <summary>
		/// Reference to the leitmotif editor
		/// </summary>
		private LeitmotifEditor mLeitmotifEditor;

		/// <summary>
		/// Our cached leitmotif odds. We save these, as during the leitmotif editor, we need to force 100% the leitmotif odds in order to playback consistently
		/// </summary>
		private float mCachedLeitmotifOdds;

		#endregion private
	}
}
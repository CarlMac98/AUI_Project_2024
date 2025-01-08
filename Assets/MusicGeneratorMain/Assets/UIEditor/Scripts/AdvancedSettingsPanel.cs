using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI Panel to control advanced settings for the Music Generator
	/// </summary>
	public class AdvancedSettingsPanel : UIPanel
	{
		#region public

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			mTonicInfluence.Option.value = mMusicGenerator.ConfigurationData.TonicInfluence;
			mSubdominantInfluence.Option.value = mMusicGenerator.ConfigurationData.SubdominantInfluence;
			mDominantInfluence.Option.value = mMusicGenerator.ConfigurationData.DominantInfluence;
			mTritoneSubInfluence.Option.value = mMusicGenerator.ConfigurationData.TritoneSubInfluence;
			mAscendDescendKey.Option.value = mMusicGenerator.ConfigurationData.KeyChangeAscendDescend;
			mGroupRate.Option.isOn = mMusicGenerator.ConfigurationData.GroupRate == GroupRate.Progression;
			mVolumeFadeRate.Option.value = mMusicGenerator.ConfigurationData.VolFadeRate;

			for ( var i = 0; i < mExcludedSteps.Count; i++ )
			{
				var index = i;
				mExcludedSteps[index].Option.isOn = mMusicGenerator.ConfigurationData.ExcludedProgSteps[index];
			}
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override void InitializeListeners()
		{
			mTonicInfluence.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.TonicInfluence = value;
					mTonicInfluence.Text.text = $"{value}%";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.TonicInfluence );

			mSubdominantInfluence.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.SubdominantInfluence = value;
					mSubdominantInfluence.Text.text = $"{value}%";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.SubdominantInfluence );

			mDominantInfluence.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.DominantInfluence = value;
					mDominantInfluence.Text.text = $"{value}%";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.DominantInfluence );

			mTritoneSubInfluence.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.TritoneSubInfluence = value;
					mTritoneSubInfluence.Text.text = $"{value}%";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.TritoneSubInfluence );

			mAscendDescendKey.Initialize( value =>
				{
					mMusicGenerator.ConfigurationData.KeyChangeAscendDescend = value;
					mAscendDescendKey.Text.text = $"{value}%";
				},
				initialValue: ( int )mMusicGenerator.ConfigurationData.KeyChangeAscendDescend );

			for ( var i = 0; i < mExcludedSteps.Count; i++ )
			{
				var index = i;
				mExcludedSteps[index].Initialize( ( value ) =>
					{
						mMusicGenerator.ConfigurationData.ExcludedProgSteps[index] = value;
						CheckAvoidSteps();
					},
					initialValue: mMusicGenerator.ConfigurationData.ExcludedProgSteps[index] );
			}

			mGroupRate.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.GroupRate = value ? GroupRate.Progression : GroupRate.Measure;
					mGroupRate.Text.text = value ? "Group Rate: Progression" : "Group Rate: Measure";
				},
				initialValue: mMusicGenerator.ConfigurationData.GroupRate == GroupRate.Progression );

			mVolumeFadeRate.Initialize( ( value ) =>
				{
					//TODO: should have this be in seconds/ms or something. This multiplier is weird.
					mMusicGenerator.SetVolFadeRate( value );
					mVolumeFadeRate.Text.text = $"{value}";
				},
				initialValue: mMusicGenerator.ConfigurationData.VolFadeRate );

			mFadeIn.onClick.AddListener( mMusicGenerator.VolumeFadeIn );
			mFadeOut.onClick.AddListener( mMusicGenerator.VolumeFadeOut );
		}

		///<inheritdoc/>
		protected override void OnDestroy()
		{
			mFadeIn.onClick.RemoveAllListeners();
			mFadeOut.onClick.RemoveAllListeners();
			base.OnDestroy();
		}

		#endregion protected

		#region private

		[Tooltip( "Slider to control tonic influence" )]
		[SerializeField] private UISlider mTonicInfluence;

		[Tooltip( "Slider to control subdominant influence" )]
		[SerializeField] private UISlider mSubdominantInfluence;

		[Tooltip( "Slider to control dominant influence" )]
		[SerializeField] private UISlider mDominantInfluence;

		[Tooltip( "Slider to control tritone substitution odds" )]
		[SerializeField] private UISlider mTritoneSubInfluence;

		[Tooltip( "Slider to control Whether the generator ascends or descends around the circle of 5ths" )]
		[SerializeField] private UISlider mAscendDescendKey;

		[Tooltip( "Toggles for excluded tonal steps in the chord progression" )]
		[SerializeField] private List<UIToggle> mExcludedSteps = new List<UIToggle>();

		[Tooltip( "Rate at which we choose which groups are playing (end of measure or end of progression" )]
		[SerializeField] private UIToggle mGroupRate;

		[Tooltip( "Slider to control rate of fading" )]
		[SerializeField] private UISlider mVolumeFadeRate;

		[Tooltip( "Button to test Fading in" )]
		[SerializeField] private Button mFadeIn;

		[Tooltip( "Button to test  Fading out" )]
		[SerializeField] private Button mFadeOut;

		/// <summary>
		/// Checks the avoid steps. Hacky fix to make sure the user hasn't excluded an entire tonal type:
		/// </summary>
		private void CheckAvoidSteps()
		{
			CheckTonalChords( MusicConstants.TonicChords );
			CheckTonalChords( MusicConstants.SubdominantChords );
			CheckTonalChords( MusicConstants.DominantChords );
		}

		/// <summary>
		/// Enforces the lowest tonal chord to be allowed
		/// </summary>
		/// <param name="tonalChords"></param>
		private void CheckTonalChords( int[] tonalChords )
		{
			foreach ( var tonalChord in tonalChords )
			{
				if ( mMusicGenerator.ConfigurationData.ExcludedProgSteps[tonalChord] == false )
				{
					return;
				}
			}

			mMusicGenerator.ConfigurationData.ExcludedProgSteps[tonalChords[0]] = false;
			mExcludedSteps[tonalChords[0]].Option.isOn = false;
		}

		#endregion private
	}
}
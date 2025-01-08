using System.Collections;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI Panel controller for our percussion editor
	/// </summary>
	public class UIPercussionEditor : UIDisplayEditor
	{
		#region public

		///<inheritdoc/>
		public override Key Key => mUIManager.MusicGenerator.ConfigurationData.Key;

		///<inheritdoc/>
		public override Scale Scale => mUIManager.MusicGenerator.ConfigurationData.Scale;

		///<inheritdoc/>
		public override Mode Mode => mUIManager.MusicGenerator.ConfigurationData.Mode;

		///<inheritdoc/>
		public override void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			base.Initialize( uiManager, isEnabled );

			mInstrumentSet = uiManager.CurrentInstrumentSet;
		}

		///<inheritdoc/>
		public override void Pause()
		{
			mPercussionEditor.Pause();
		}

		///<inheritdoc/>
		public override void Stop()
		{
			base.Stop();
			mPercussionEditor.Stop();
		}

		///<inheritdoc/>
		public override void Play()
		{
			mPercussionEditor.Play();
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
			mInstrumentSet = mUIManager.CurrentInstrumentSet;
			base.SetPanelActive( isActive );
			StartCoroutine( LoadPanel( isActive ) );

			SetMaxGroupOdds( isActive );
			if ( isActive == false )
			{
				mMusicGenerator.InstrumentSet.Reset();
			}

			mMusicGenerator.GroupsAreTemporarilyOverriden = isActive;
			mMusicGenerator.LeitmotifIsTemporarilySuspended = isActive;
		}

		///<inheritdoc/>
		public override void UpdateUIElementValues()
		{
			if ( IsEnabled == false ||
			     mState == DisplayEditorState.Initializing )
			{
				return;
			}

			base.UpdateUIElementValues();
			if ( mMusicGenerator == false )
			{
				return;
			}

			mCurrentMeasure.Option.value = mMusicGenerator.InstrumentSet.PercussionRepeatCount;
			mNumberOfMeasures.Option.value = mMusicGenerator.ConfigurationData.NumForcedPercussionMeasures;
		}

		#endregion public

		#region protected

		///<inheritdoc/>
		protected override IEnumerator LoadPanel( bool isActive )
		{
			mState = DisplayEditorState.Initializing;
			yield return mUIManager.EnablePercussionEditor( isActive );
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

			mNumberOfMeasures.Initialize( ( value ) =>
				{
					mMusicGenerator.ConfigurationData.NumForcedPercussionMeasures = ( int )value;
					if ( mCurrentMeasure.Option.value >= value )
					{
						mCurrentMeasure.Option.value = value - 1;
						mDisplayIsDirty = true;
					}

					mNumberOfMeasures.Text.text = $"{value}";
				},
				initialValue: mMusicGenerator.ConfigurationData.NumForcedPercussionMeasures );
		}

		#endregion protected

		#region private

		[SerializeField, Tooltip( "Reference to the percussion editor" )]
		private PercussionEditor mPercussionEditor;

		/// <summary>
		/// Sets max group odds. This is forced so all percussion instruments play in the editor.
		/// </summary>
		private void SetMaxGroupOdds( bool isEnabled )
		{
			mMusicGenerator.GroupsAreTemporarilyOverriden = isEnabled;
			mMusicGenerator.LeitmotifIsTemporarilySuspended = isEnabled;
			for ( var index = 0; index < mMusicGenerator.ConfigurationData.GroupOdds.Length; index++ )
			{
				mMusicGenerator.InstrumentSet.OverrideGroupIsPlaying( index, isEnabled );
			}

			if ( isEnabled == false )
			{
				mUIManager.InstrumentListPanelUI.PercussionScrollView.DisableAllManualGroups();
				mUIManager.InstrumentListPanelUI.InstrumentScrollView.DisableAllManualGroups();
			}
		}

		#endregion private
	}
}
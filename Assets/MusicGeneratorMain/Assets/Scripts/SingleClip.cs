using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// A single Clip. Used for playing a specific InstrumentSet of instruments/notes. Create via the Measure Editor tab in the ui editor
	/// </summary>
	public class SingleClip : MonoBehaviour
	{
		#region public

		[UsedImplicitly]
		public UnityEvent OnClipFinished { get; } = new UnityEvent();

		/// <summary>
		/// Reference to our configuration data for this clip.
		/// </summary>
		/// <value></value>
		public ConfigurationData ConfigurationData { get; private set; }

		/// <summary>
		/// Instrument InstrumentSet for this clip (is separate from the general music generator instrument InstrumentSet)
		/// </summary>
		/// <value></value>
		public InstrumentSet InstrumentSet { get; private set; }

		/// <summary>
		/// State of this clip
		/// </summary>
		public ClipState ClipState => mClipState;

		/// <summary>
		/// Plays the single clip
		/// </summary>
		public void Play()
		{
			Reset();
			SetState( ClipState.Play );
		}

		/// <summary>
		/// Pauses the single clip
		/// </summary>
		public void Pause()
		{
			SetState( ClipState.Pause );
		}

		/// <summary>
		/// Stops the single clip
		/// </summary>
		public void Stop()
		{
			SetState( ClipState.Stop );
		}

		/// <summary>
		/// Resets the clip values
		/// </summary>
		public void Reset()
		{
			ClipMeasure.ResetMeasure( InstrumentSet, null, true );
			InstrumentSet.SixteenthStepsTaken = 0;
			InstrumentSet.SixteenthRepeatCount = 0;
			InstrumentSet.RepeatCount = 0;
		}

		/// <summary>
		/// Loads the clip configuration
		/// </summary>
		/// <param name="configurationName"></param>
		/// <param name="musicGenerator"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		public IEnumerator LoadConfiguration( string configurationName, MusicGenerator musicGenerator, Action onComplete = null )
		{
			mMusicGenerator = musicGenerator;
			InstrumentSet = new InstrumentSet( mMusicGenerator );
			mMusicGenerator.RegisterInstrumentSet( InstrumentSet );
			mMusicGenerator.RepeatedMeasureExited.RemoveListener( OnMeasureExited );
			mMusicGenerator.RepeatedMeasureExited.AddListener( OnMeasureExited );
			ConfigurationData data = null;
			ConfigurationData.LoadClipConfigurationData( configurationName, ( config ) => { data = config; } );
			yield return new WaitUntil( () => data != null );
			ConfigurationData = data;
			InstrumentSet.SetData( ConfigurationData );
			yield return LoadInstruments();
			onComplete?.Invoke();
		}

		/// <summary>
		/// Saves the clip configuration
		/// </summary>
		/// <param name="configurationName"></param>
		public void SaveConfiguration( string configurationName )
		{
			ConfigurationData.ConfigurationName = configurationName;
			ConfigurationData.SaveClipConfigurationData( ConfigurationData );
		}

		#endregion public

		#region private

		/// <summary>
		/// Reference to our music generator
		/// </summary>
		private MusicGenerator mMusicGenerator;

		/// <summary>
		/// Measure handler for this clip
		/// </summary>
		/// <returns></returns>
		private ClipMeasure ClipMeasure { get; } = new ClipMeasure();

		/// <summary>
		/// Current clip state
		/// </summary>
		/// <value></value>
		private ClipState mClipState;

		/// <summary>
		/// Loads the instruments for this clip
		/// </summary>
		/// <returns></returns>
		private IEnumerator LoadInstruments()
		{
			foreach ( var instrument in ConfigurationData.Instruments )
			{
#if FMOD_ENABLED
				if ( instrument.IsSynth )
				{
					continue;
				}
#endif
				yield return StartCoroutine( mMusicGenerator.AddInstrument( InstrumentSet, instrument ) );
			}
		}

		/// <summary>
		/// Sets our clip state
		/// </summary>
		/// <param name="state"></param>
		private void SetState( ClipState state )
		{
			mClipState = state;
			switch ( mClipState )
			{
				case ClipState.Pause:
					break;
				case ClipState.Play:
					break;
				case ClipState.Stop:
					Reset();
					break;
			}
		}

		/// <summary>
		/// update.
		/// </summary>
		private void Update()
		{
			switch ( mClipState )
			{
				case ClipState.Play:
					ClipMeasure.PlayMeasure( InstrumentSet );
					break;
			}
		}

		/// <summary>
		/// Handles checking for auto-stop of the clip
		/// </summary>
		private void OnMeasureExited( GeneratorState state )
		{
			if ( mClipState == ClipState.Stop )
			{
				return;
			}

			if ( InstrumentSet.RepeatCount >= InstrumentSet.Data.RepeatMeasuresNum - 1 && InstrumentSet.Data.SingleClipIsRepeating == false )
			{
				FinishClip();
			}
		}

		/// <summary>
		/// Finishes playing our clip
		/// </summary>
		private void FinishClip()
		{
			mClipState = ClipState.Finished;
			OnClipFinished.Invoke();
		}

		/// <summary>
		/// Unregisters our instrument set
		/// </summary>
		private void OnDestroy()
		{
			if ( mMusicGenerator != null && InstrumentSet != null )
			{
				mMusicGenerator.UnregisterInstrumentSet( InstrumentSet );
			}
		}

		#endregion private
	}
}

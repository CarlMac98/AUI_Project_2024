using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
#if FMOD_ENABLED
using FMODUnity;

#endif

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Music Generator:
	/// See the included setup documentation file.
	/// Handles the state logic and other top level functions for the entire player. Loading assets, etc.
	/// </summary>
	public sealed class MusicGenerator : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Getter for our addressable manager
		/// </summary>
		public AddressableManager AddressableManager => mAddressableManager;

		/// <summary>
		/// Getter for our Configuration Data
		/// </summary>
		public ConfigurationData ConfigurationData => mConfigurationData;

		/// <summary>
		/// Whether we currently autoplay the generator
		/// </summary>
		public bool AutoPlay => mAutoPlay;

		/// <summary>
		/// Sets our configuration data (as a user, one should generally use the LoadConfiguration method unless an exception to that is needed)
		/// </summary>
		/// <param name="data"></param>
		public void SetConfigurationData( ConfigurationData data )
		{
			mConfigurationData = data;
		}

		/// <summary>
		/// Getter for our default configuration name
		/// </summary>
		public string DefaultConfigurationName => mDefaultConfig;

		/// <summary>
		/// Getter for our audio handler
		/// </summary>
		public PMGAudioHandler AudioHandler => mAudioHandler;

		/// <summary>
		/// Reference to our mods.
		/// </summary>
		public GeneratorMod[] Mods => mMods;

		/// <summary>
		/// Dictionary of audio clips
		/// </summary>
		public IReadOnlyDictionary<string, InstrumentAudio> InstrumentAudio => mInstrumentAudio;

		/// <summary>
		/// Music Generator state
		/// </summary>
		public GeneratorState GeneratorState { get; private set; } = GeneratorState.None;

		///<summary>
		/// ref to our instrument instrumentSet
		/// </summary>
		public InstrumentSet InstrumentSet { get; private set; }

		///<summary>
		/// Set in the editor. Possible instrument paths. While this is public, you probably ought not use it...
		/// </summary>
		public List<string> BaseInstrumentPaths => mBaseInstrumentPaths;

		///<summary>
		/// our currently played chord progression
		/// </summary>
		public IReadOnlyList<int> CurrentChordProgression
		{
			get => ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Leitmotif && mRegularMeasure.IsUsingLeitmotif ?
				InstrumentSet.Data.LeitmotifProgression :
				mCurrentChordProgression;
			set
			{
				if ( value.Count == MusicConstants.MaxFullstepsTaken )
				{
					mCurrentChordProgression = value;
				}
			}
		}

		/// <summary>
		/// Whether groups are temporarily overridden (as opposed to always manually managed as set in configuration data
		/// </summary>
		public bool GroupsAreTemporarilyOverriden { get; set; }

		/// <summary>
		/// Whether leitmotif is temporarily suspended.
		/// </summary>
		public bool LeitmotifIsTemporarilySuspended { get; set; }

		/// <summary>
		/// Returns whether we currently have a specific instrument type loaded
		/// </summary>
		/// <param name="instrumentName"></param>
		/// <returns></returns>
		public bool HasLoadedInstrument( string instrumentName )
		{
			return mInstrumentAudio.ContainsKey( instrumentName );
		}

		/// <summary>
		/// Loads a new configuration (song) to play.
		/// </summary>
		/// <param name="configurationName"></param>
		/// <param name="continueState"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		public IEnumerator LoadConfiguration( string configurationName,
			GeneratorState continueState = GeneratorState.Ready,
			Action onComplete = null )
		{
			if ( GeneratorState == GeneratorState.Initializing )
			{
				yield break;
			}

			DisableMods();
			ResetPlayer();
			SetState( GeneratorState.Initializing );
			ClearInstruments( InstrumentSet );
			ConfigurationData data = null;
			ConfigurationData.LoadConfigurationData( configurationName, ( config ) => { data = config; } );
			yield return new WaitUntil( () => data != null );
			mConfigurationData = data;
			InstrumentSet.SetTimeSignature( data.TimeSignature );
			var instrumentsAdded = 0;
			foreach ( var instrument in ConfigurationData.Instruments )
			{
#if FMOD_ENABLED
				if ( instrument.IsSynth )
				{
					instrumentsAdded++;
					continue;
				}
#endif
				StartCoroutine( AddInstrument( InstrumentSet, instrument, ( _ ) => { instrumentsAdded++; } ) );
			}

			yield return new WaitUntil( () => instrumentsAdded == ConfigurationData.Instruments.Count );
			LoadGeneratorData();
			CleanupUnusedInstruments();
			EnableMods();
			SetState( continueState );

			onComplete?.Invoke();
		}

		/// <summary>
		/// Sets variables from save file.
		/// </summary>
		private void LoadGeneratorData()
		{
			InstrumentSet.SetData( ConfigurationData );
			mAudioHandler.InitializeGlobalEffects();
		}

		/// <summary>
		/// Disables our mods
		/// </summary>
		private void DisableMods()
		{
			foreach ( var mod in mMods )
			{
				mod.DisableMod();
			}
		}

		/// <summary>
		/// Enables any mods found in the configuration data.
		/// </summary>
		private void EnableMods()
		{
			foreach ( var configurationMod in ConfigurationData.Mods )
			{
				foreach ( var generatorMod in mMods )
				{
					if ( generatorMod.ModName == configurationMod )
					{
						generatorMod.EnableMod( this );
						generatorMod.LoadData();
					}
				}
			}
		}

		/// <summary>
		/// Saves our mod data
		/// </summary>
		private void SaveModData()
		{
			ConfigurationData.Mods.Clear();
			foreach ( var mod in mMods )
			{
				if ( mod.enabled )
				{
					ConfigurationData.Mods.Add( mod.ModName );
					mod.SaveData();
				}
			}
		}

		/// <summary>
		/// Restores an already loaded configuration. (in cases where you've temporarily pointed the data elsewhere.)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="continueState"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		public IEnumerator RestoreConfiguration( ConfigurationData data,
			GeneratorState continueState = GeneratorState.Ready,
			Action onComplete = null )
		{
			ResetPlayer();
			SetState( GeneratorState.Initializing );
			InstrumentSet.Instruments.Clear();

			mConfigurationData = data;

			foreach ( var instrument in ConfigurationData.Instruments )
			{
#if FMOD_ENABLED
				if ( instrument.IsSynth )
				{
					continue;
				}
#endif
				yield return AddInstrument( InstrumentSet, instrument );
			}

			LoadGeneratorData();

			SetState( continueState );
			onComplete?.Invoke();
		}

		/// <summary>
		/// Fades out the music before async loading a new configuration and fading back in.
		/// </summary>
		/// <param name="configurationName"></param>
		/// <returns></returns>
		public IEnumerator FadeLoadConfiguration( string configurationName )
		{
			//TODO: Test me, it's been awhile :P
			mStateTimer = 0.0f;
			VolumeFadeOut();
			var maxWaitTime = 10.0f;
			yield return new WaitUntil( () => mAudioHandler.VolumeState == VolumeState.Idle || mStateTimer > maxWaitTime );

			Stop();

			//Debug.Log( "Fade load configuration finished" );
			yield return LoadConfiguration( configurationName, GeneratorState.Playing );
		}

		/// <summary>
		/// pauses the main music generator:
		/// </summary>
		public void Pause()
		{
			SetState( GeneratorState.Paused );
		}

		/// <summary>
		/// plays the main music generator:
		/// </summary>
		public void Play()
		{
			SetState( GeneratorState.Playing );
		}

		/// <summary>
		/// stops the main music generator:
		/// </summary>
		public void Stop()
		{
			SetState( GeneratorState.Stopped );
		}

		/// <summary>
		/// Set the music generator state:
		/// </summary>
		/// <param name="state"></param>
		public void SetState( GeneratorState state )
		{
			if ( state == GeneratorState )
			{
				return;
			}

			mStateTimer = 0.0f;
			GeneratorState = state;

			StateSet.Invoke( GeneratorState );

			switch ( GeneratorState )
			{
				case GeneratorState.Stopped:
				{
					StopGenerator.Invoke();
					ResetPlayer();
					CleanupUnusedInstruments();
					break;
				}
				case GeneratorState.Initializing:
					break;
				case GeneratorState.Ready:
					Ready.Invoke();
					mCurrentChordProgression = mChordProgressions.GenerateProgression( ConfigurationData.Mode, ConfigurationData.Scale, 0 );
					break;
				case GeneratorState.Playing:
				case GeneratorState.ManualPlay:
					//ResetPlayer();
					PlayGenerator.Invoke();
					mCurrentChordProgression = mChordProgressions.GenerateProgression( ConfigurationData.Mode, ConfigurationData.Scale, 0 );
					break;
				case GeneratorState.Paused:
					PauseGenerator.Invoke();
					break;
			}
		}

		/// <summary>
		/// fades Volume out
		/// </summary>
		public void VolumeFadeOut()
		{
			mAudioHandler.VolumeFadeOut();
		}

		/// <summary>
		/// fades Volume in
		/// </summary>
		public void VolumeFadeIn()
		{
			mAudioHandler.VolumeFadeIn();
		}

		/// <summary>
		/// plays an audio clip:
		/// Look for an available clip that's not playing anything, creates a new one if necessary
		/// resets its properties  (Volume, pan, etc) to match our new clip.
		/// </summary>
		/// <param name="set"></param>
		/// <param name="instrument"></param>
		/// <param name="note"></param>
		/// <param name="volume"></param>
		/// <param name="instrumentIndex"></param>
		public void PlayAudioClip( InstrumentSet set, string instrument, int note, float volume, int instrumentIndex )
		{
			NotePlayedArg.InstrumentSet = set;
			NotePlayedArg.InstrumentName = instrument;
			NotePlayedArg.Note = note;
			NotePlayedArg.Volume = volume;
			NotePlayedArg.InstrumentIndex = instrumentIndex;

			// Override for external use and return false for MIDI, etc to suppress playing the note here
			if ( OnNotePlayed( NotePlayedArg ) == false )
			{
				return;
			}

			PlayNote( set, volume, instrument, note, instrumentIndex );
		}

		/// <summary>
		/// Plays a note
		/// </summary>
		/// <param name="set"></param>
		/// <param name="volume"></param>
		/// <param name="instrumentName"></param>
		/// <param name="note"></param>
		/// <param name="instrumentIndex"></param>
		public void PlayNote( InstrumentSet set, float volume, string instrumentName, int note, int instrumentIndex )
		{
			if ( mInstrumentAudio.TryGetValue( instrumentName, out var instrumentAudio ) == false )
			{
				return;
			}

			var isSynth = ConfigurationData.Instruments[instrumentIndex].IsSynth;
			var instrumentSubIndex = isSynth ? 0 : UnityEngine.Random.Range( 0, instrumentAudio.Instruments.Count );

			AudioClipPlayed.Invoke( set, volume, note, instrumentIndex );
			mAudioHandler.PlayNote( set, note, instrumentIndex, instrumentSubIndex );
		}

		/// <summary>
		/// setter for fade rate. This must be positive parameterValue.
		/// </summary>
		/// <param name="value"></param>
		public void SetVolFadeRate( float value )
		{
			ConfigurationData.VolFadeRate = Math.Abs( value );
		}


		/// <summary>
		/// resets all player settings.
		/// reset player is called on things like loading new configurations, loading new levels, etc.
		/// sets all timing values and other settings back to the start
		/// </summary>
		public void ResetPlayer()
		{
			if ( GeneratorState <= GeneratorState.Initializing || mChordProgressions == null )
			{
				return;
			}

			InstrumentSet.Reset();
			mRegularMeasure.ResetMeasure( InstrumentSet, setThemeRepeat: null, hardReset: true );
			mRepeatingMeasure.ResetMeasure( InstrumentSet, setThemeRepeat: null, hardReset: true );

			mCurrentChordProgression =
				mChordProgressions.GenerateProgression( ConfigurationData.Mode, ConfigurationData.Scale, ConfigurationData.KeySteps );

			NormalMeasureExited.Invoke();
			InstrumentSet.RepeatCount = 0;
			InstrumentSet.PercussionRepeatCount = 0;
			InstrumentSet.ResetProgressionSteps();
			InstrumentSet.ResetGroups();
		}

		/// <summary>
		/// Adds an instrument to our list. sets its instrument index
		/// </summary>
		/// <param name="set"></param>
		/// <param name="instrumentData"></param>
		/// <param name="callback"></param>
		/// <param name="isPercussion"></param>
		/// <returns></returns>
		public IEnumerator AddInstrument( InstrumentSet set,
			ConfigurationData.InstrumentData instrumentData = null,
			Action<Instrument> callback = null,
			bool isPercussion = false )
		{
			var instrument = new Instrument();
			set.Instruments.Add( instrument );
			var index = set.Instruments.Count - 1;
			set.Instruments[index].Initialize( this, set.Instruments.Count - 1, instrumentData );

			if ( instrumentData == null )
			{
				if ( isPercussion )
				{
					instrument.InstrumentData.InstrumentType = mBasePercussionType;
				}

				ConfigurationData.Instruments.Add( instrument.InstrumentData );
			}

			yield return LoadBaseClips( instrument,
				( wasSuccessful ) =>
				{
					if ( wasSuccessful == false )
					{
						ConfigurationData.Instruments.Remove( instrument.InstrumentData );
					}
					else
					{
						SetInstrumentEffectsValues( index, instrument.InstrumentData );
					}

					callback?.Invoke( wasSuccessful ? instrument : null );
				} );

			InstrumentAdded.Invoke( instrument );
		}

		/// <summary>
		/// Deletes all instruments:
		/// </summary>
		/// <param name="instrumentSet"></param>
		public void ClearInstruments( InstrumentSet instrumentSet )
		{
			InstrumentsCleared.Invoke();
			if ( instrumentSet.Instruments.Count == 0 )
			{
				return;
			}

			for ( var index = instrumentSet.Instruments.Count - 1; index >= 0; index-- )
			{
				RemoveInstrument( index, instrumentSet );
			}

			instrumentSet.Instruments.Clear();
		}

		/// <summary>
		/// Removes the instrument from our list. Fixes instrument indices.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="instrumentSet"></param>
		public void RemoveInstrument( int index, InstrumentSet instrumentSet )
		{
			if ( index >= instrumentSet.Instruments.Count )
			{
				return;
			}

			var instrumentName = instrumentSet.Instruments[index].InstrumentData.InstrumentType;
			InstrumentWillBeRemoved.Invoke( instrumentSet.Instruments[index] );
			instrumentSet.Instruments[index] = null;
			ConfigurationData.Instruments.RemoveAt( index );
			instrumentSet.Instruments.RemoveAt( index );

			for ( var instrumentIndex = 0; instrumentIndex < instrumentSet.Instruments.Count; instrumentIndex++ )
			{
				instrumentSet.Instruments[instrumentIndex].InstrumentIndex = instrumentIndex;
			}

			InstrumentWasRemoved.Invoke();
			RemoveBaseClip( instrumentName );
		}

		/// <summary>
		/// Removes any instruments not used by our music generator
		/// This does not include any single clips, which will need to be managed by the user.
		/// </summary>
		public void CleanupUnusedInstruments()
		{
			var unusedInstruments = new List<string>();
			foreach ( var instrument in mInstrumentAudio )
			{
				var key = instrument.Key;
				if ( InstrumentIsUsed( key ) == false )
				{
					unusedInstruments.Add( key );
				}
			}

			foreach ( var instrumentKey in unusedInstruments )
			{
				RemoveBaseClip( instrumentKey );
			}
		}

		/// <summary>
		/// Removes a base clip if there are no instruments using it.
		/// </summary>
		/// <param name="instrumentKey"></param>
		public void RemoveBaseClip( string instrumentKey )
		{
			if ( instrumentKey == MusicConstants.SynthID ||
			     mInstrumentAudio.TryGetValue( instrumentKey, out var instrumentAudio ) == false )
			{
				return;
			}

			if ( InstrumentIsUsed( instrumentKey ) == false )
			{
				//unload any handler sounds prior to unloading the audio assets:
				mAudioHandler.UnloadInstrument( instrumentKey, instrumentAudio );
				instrumentAudio.UnloadAudioClips();
				Destroy( instrumentAudio.gameObject );
				mInstrumentAudio.Remove( instrumentKey );
				mLoadedInstrumentNames.Remove( instrumentKey );
			}
		}

		/// <summary>
		/// returns whether an instrument set uses an instrument.
		/// </summary>
		/// <param name="instrumentKey"></param>
		/// <returns></returns>
		private bool InstrumentIsUsed( string instrumentKey )
		{
			foreach ( var instrumentSet in mRegisteredInstrumentSets )
			{
				foreach ( var instrument in instrumentSet.Instruments )
				{
					if ( instrument.InstrumentData.InstrumentType == instrumentKey )
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Sets the Volume.
		/// </summary>
		/// <param name="volume"></param>
		public void SetVolume( float volume )
		{
			mAudioHandler.SetVolume( volume );
		}

		/// <summary>
		/// Saves the currently loaded configuration
		/// </summary>
		/// <param name="configurationName"></param>
		public void SaveCurrentConfiguration( string configurationName )
		{
			ConfigurationData.ConfigurationName = configurationName;
			SaveModData();
			ConfigurationData.SaveConfigurationData( mConfigurationData );
		}

		/// <summary>
		/// Loads the instrument clips from asset reference
		/// </summary>
		/// <param name="instrument"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public IEnumerator LoadBaseClips( Instrument instrument, Action<bool> callback = null )
		{
			var instrumentType = instrument.InstrumentData.InstrumentType;

			// just return the correct index if we've already loaded these.
			if ( mLoadedInstrumentNames.Contains( instrumentType ) )
			{
				callback?.Invoke( true );
				yield break;
			}

			var isSynth = instrumentType == MusicConstants.SynthID;
#if FMOD_ENABLED
			if ( isSynth )
			{
				callback?.Invoke( false );
				yield break;
			}
#endif
			mLoadedInstrumentNames.Add( instrumentType );

			if ( isSynth )
			{
				mInstrumentAudio[MusicConstants.SynthID] = null;
				callback?.Invoke( true );
				yield break;
			}

			var fileLoaded = false;
			var failed = false;
			mAddressableManager.SpawnAddressableInstance( instrumentType,
				new AddressableSpawnRequest(
					Vector3.zero,
					Quaternion.identity,
					( result ) =>
					{
						if ( result )
						{
							var instrumentAudio = result.GetComponent<InstrumentAudio>();
							mInstrumentAudio.Add( instrumentType, instrumentAudio );
							mAudioHandler?.LoadInstrument( instrumentType, instrumentAudio );

							fileLoaded = true;
						}
						else
						{
							failed = true;
							Debug.LogError(
								$"Failed to load addressable assets for {instrumentType}. Please ensure that the assets exist and addressables have been built for your current platform." );
						}
					},
					transform
				) );
			yield return new WaitUntil( () => fileLoaded || failed );
			callback?.Invoke( failed == false );
		}

		/// <summary>
		/// Checks for a key change and starts setup if needed.
		/// </summary>
		public void CheckKeyChange()
		{
			if ( InstrumentSet.ProgressionStepsTaken == 0 )
			{
				KeyChangeSetup();
			}
		}

		/// <summary>
		/// Generates a new chord progression
		/// </summary>
		public void GenerateNewProgression()
		{
			if ( InstrumentSet.ProgressionStepsTaken < CurrentChordProgression.Count - 1 )
			{
				return;
			}

			SetKeyChange();

			if ( UnityEngine.Random.Range( 0, 100 ) < ConfigurationData.ProgressionChangeOdds || mKeyChangeNextMeasure )
			{
				mCurrentChordProgression =
					mChordProgressions.GenerateProgression( ConfigurationData.Mode, ConfigurationData.Scale, ConfigurationData.KeySteps );
				ProgressionGenerated.Invoke();
			}
		}

		/// <summary>
		/// Sets theme / repeat variables.
		/// </summary>
		public void SetThemeRepeat()
		{
			if ( InstrumentSet.RepeatCount < InstrumentSet.Data.RepeatMeasuresNum )
			{
				return;
			}

			var isRepeatType = ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Repeat ||
			                   ( ConfigurationData.ThemeRepeatOptions == ThemeRepeatOptions.Theme &&
			                     UnityEngine.Random.Range( 0, 100.0f ) < ConfigurationData.PlayThemeOdds );

			if ( isRepeatType && GeneratorState == GeneratorState.Playing )
			{
				SetState( GeneratorState.Repeating );
			}

			var newInstrumentDetected = false;

			foreach ( var instrument in InstrumentSet.Instruments )
			{
				if ( instrument.NeedsTheme )
				{
					newInstrumentDetected = true;
				}
			}

			if ( UnityEngine.Random.Range( 0, 100.0f ) < ConfigurationData.SetThemeOdds || newInstrumentDetected )
			{
				foreach ( var instrument in InstrumentSet.Instruments )
				{
					instrument.SetThemeNotes();
				}
			}
		}

		/// <summary>
		/// Registers an instrumentSet as in use
		/// </summary>
		/// <param name="instrumentSet"></param>
		public void RegisterInstrumentSet( InstrumentSet instrumentSet )
		{
			if ( instrumentSet == null )
			{
				return;
			}

			if ( mRegisteredInstrumentSets.Contains( instrumentSet ) == false )
			{
				mRegisteredInstrumentSets.Add( instrumentSet );
			}
		}

		/// <summary>
		/// Removes an instrument set from being considered in use.
		/// </summary>
		/// <param name="instrumentSet"></param>
		public void UnregisterInstrumentSet( InstrumentSet instrumentSet )
		{
			if ( instrumentSet == null )
			{
				return;
			}

			if ( mRegisteredInstrumentSets.Contains( instrumentSet ) )
			{
				mRegisteredInstrumentSets.Remove( instrumentSet );
			}
		}

		#region public.events

		#region public.events.types

		/// <summary>
		/// Unity event with type Int
		/// </summary>
		public class IntEvent : UnityEvent<int> { }

		/// <summary>
		/// Unity event with type GeneratorState
		/// </summary>
		public class StateEvent : UnityEvent<GeneratorState> { }

		/// <summary>
		/// Event with type: InstrumentSet set, float volume, int note, int instrumentIndex
		/// Invoked when audio clip plays
		/// </summary>
		public class AudioClipPlayedEvent : UnityEvent<InstrumentSet, float, int, int> { }

		/// <summary>
		/// Unity event with type Float
		/// </summary>
		public class FloatEvent : UnityEvent<float> { };

		/// <summary>
		/// Unity Event with type Instrument
		/// </summary>
		public class InstrumentEvent : UnityEvent<Instrument> { }

		#endregion public.events.types

		/// <summary>
		/// Event for Generator state instrumentSet:
		/// </summary>
		public StateEvent StateSet { get; } = new StateEvent();

		///<summary>
		/// On manager start event:
		/// </summary>
		public UnityEvent Started { get; } = new UnityEvent();

		///<summary>
		/// Music generator is fully initialized and ready
		/// </summary>
		public UnityEvent Ready { get; } = new UnityEvent();

		///<summary>
		/// Event for state Update()
		/// </summary>
		public StateEvent StateUpdated { get; } = new StateEvent();

		/// <summary>
		/// Event Handler for fading Volume
		/// </summary>
		public FloatEvent VolumeFaded { get; } = new FloatEvent();

		///<summary>
		/// Event Handler for Generates a chord progression:
		/// </summary>
		public UnityEvent ProgressionGenerated { get; } = new UnityEvent();

		/// <summary>
		/// Event handler for choosing which groups are currently playing.
		/// </summary>
		/// <returns></returns>
		public UnityEvent GroupsWereChosen { get; } = new UnityEvent();

		///<summary>
		/// Event handler for clear instruments:
		/// </summary>
		public UnityEvent InstrumentsCleared { get; } = new UnityEvent();

		/// <summary>
		/// Event for instrument being added
		/// </summary>
		/// <returns></returns>
		public InstrumentEvent InstrumentAdded { get; } = new InstrumentEvent();

		/// <summary>
		/// Event for impending instrument removal
		/// </summary>
		/// <returns></returns>
		public InstrumentEvent InstrumentWillBeRemoved { get; } = new InstrumentEvent();

		/// <summary>
		/// Event for impending instrument removal
		/// </summary>
		/// <returns></returns>
		public UnityEvent InstrumentWasRemoved { get; } = new UnityEvent();

		/// <summary>
		/// Event Handler for exiting normal measure
		/// </summary>
		public UnityEvent NormalMeasureExited { get; } = new UnityEvent();

		///<summary>
		/// Event Handler for impending key change:
		/// </summary>
		public IntEvent KeyChanged { get; } = new IntEvent();

		///<summary>
		/// Event for exiting the repeating measure.
		/// </summary>
		public StateEvent RepeatedMeasureExited { get; } = new StateEvent();

		///<summary>
		/// Event for play()
		/// </summary>
		public UnityEvent PlayGenerator { get; } = new UnityEvent();

		///<summary>
		/// Event for stop()
		/// </summary>
		public UnityEvent StopGenerator { get; } = new UnityEvent();

		///<summary>
		/// Event for pause()
		/// </summary>
		public UnityEvent PauseGenerator { get; } = new UnityEvent();

		public AudioClipPlayedEvent AudioClipPlayed { get; } = new AudioClipPlayedEvent();

		public UnityEvent BeatWillPlay { get; } = new UnityEvent();
		public UnityEvent BeatDidPlay { get; } = new UnityEvent();

		///<summary>
		/// Event Handler for UI manager detection:
		/// This is a bit hacky. Please don't listen/return anything for this.
		/// It's used to detect UI states without being coupled to them.
		/// </summary>
		public delegate bool HasVisiblePlayerEventHandler( object source, EventArgs args );

		/// <summary>
		/// Whether there is a visible player event
		/// </summary>
		public event HasVisiblePlayerEventHandler HasVisiblePlayer;

		/// <summary>
		/// OnVisiblePlayer Event
		/// </summary>
		/// <returns></returns>
		private bool OnHasVisiblePlayer()
		{
			return HasVisiblePlayer != null && HasVisiblePlayer( this, EventArgs.Empty );
		}

		/// <summary>
		/// Event handler for played notes. If using MIDI or some other player, override
		/// this and return false to suppress the playing of the music generator audio clip.
		/// </summary>
		public delegate bool NotePlayedEventHandler( object source, NotePlayedArgs args );

		public event NotePlayedEventHandler NotePlayed;
		public NotePlayedArgs NotePlayedArg { get; } = new NotePlayedArgs( null, null, 0, 0, 0 );

		private bool OnNotePlayed( NotePlayedArgs args )
		{
			return NotePlayed == null || NotePlayed( this, args );
		}

		/// <summary>
		/// Event handler for generated notes. If overriding generator behavior for note generation, here is a good foothold
		/// Return desired notes
		/// </summary>
		public delegate bool NotesGeneratedEventHandler( object source, NotesGeneratedArgs args );

		public event NotesGeneratedEventHandler NotesGenerated;

		public NotesGeneratedArgs NotesGeneratedArgs { get; } = new NotesGeneratedArgs( null,
			null,
			new int[]
				{ },
			0,
			0 );

		public bool OnNotesGenerated( NotesGeneratedArgs args )
		{
			return NotesGenerated == null || NotesGenerated( this, args );
		}

		#endregion public.events

		#endregion public

		#region private

		[SerializeField, Tooltip( "Whether the generator automatically plays on start" )]
		private bool mAutoPlay;

		/// <summary>
		/// Our music generator configuration data.
		/// </summary>
		/// <returns></returns>
		[SerializeField, Tooltip( "Our configuration data (will be overwritten at runtime" )]
		private ConfigurationData mConfigurationData = new ConfigurationData();

		/// <summary>
		/// Reference to our UnityAudioHandler
		/// </summary>
		private PMGAudioHandler mAudioHandler;

		/// <summary>
		/// Reference to our FMOD Audio Handler
		/// </summary>
		[SerializeField, Tooltip( "Reference to our FMOD Audio Handler" ), UsedImplicitly]
		private FmodAudioHandler mFmodAudioHandler;

		[SerializeField, Tooltip( "Reference to our Unity Audio Handler" )]
		private UnityAudioHandler mUnityAudioHandler;

		/// <summary>
		/// Container of our mods. 
		/// </summary>
		private GeneratorMod[] mMods;

		///<summary>
		/// Dictionary of audio clips.
		/// </summary>
		private readonly Dictionary<string, InstrumentAudio> mInstrumentAudio = new Dictionary<string, InstrumentAudio>();

		///<summary>
		/// chord progression logic
		/// </summary>
		private ChordProgressions mChordProgressions;

		/// <summary>
		/// Internal state timer
		/// </summary>
		private float mStateTimer;

		/// <summary>
		/// Our list of instrument sets in use. Audio clip loading/unloading is dependent on this.
		/// </summary>
		private readonly List<InstrumentSet> mRegisteredInstrumentSets = new List<InstrumentSet>();

		/// <summary>
		/// Name of our base percussion type to spawn
		/// </summary>
		[SerializeField, Tooltip( "Name of our base percussion type to spawn" )]
		private string mBasePercussionType = "P_AnvilHit";

		/// <summary>
		/// Reference to our addressable manager
		/// </summary>
		[SerializeField, Tooltip( "Reference to our addressable manager" )]
		private AddressableManager mAddressableManager;

		/// <summary>
		/// default config loaded on start.
		/// </summary>
		[SerializeField, Tooltip( "Default configuration loaded on start" )]
		private string mDefaultConfig = "Default";

		/// <summary>
		/// Base Instrument Names. Please edit _carefully_
		/// </summary>
		[SerializeField, Tooltip( "Base Instrument Names. Please edit _carefully_" )]
		private List<string> mBaseInstrumentPaths;

		/// <summary>
		/// Our repeating measure logic
		/// </summary>
		private Measure mRepeatingMeasure;

		///<summary>
		/// Our regular measure logic
		/// </summary>
		private Measure mRegularMeasure;

		/// <summary>
		/// our currently played chord progression
		/// </summary>
		private IReadOnlyList<int> mCurrentChordProgression = new[]
		{
			1, 4, 4, 5
		};

		/// <summary>
		/// whether we'll change key next measure
		/// </summary>
		private bool mKeyChangeNextMeasure;

		/// <summary>
		/// loaded instrument paths for the current configuration
		/// </summary>
		private readonly List<string> mLoadedInstrumentNames = new List<string>();

		private void Awake()
		{
			DontDestroyOnLoad( gameObject );
#if FMOD_ENABLED
			mAudioHandler = mFmodAudioHandler;
#else
			mAudioHandler = mUnityAudioHandler;
#endif // FMOD_ENABLED

			mChordProgressions = new ChordProgressions( this );

			mAddressableManager.Initialize();
			InstrumentSet = new InstrumentSet( this );
			RegisterInstrumentSet( InstrumentSet );
			mRegularMeasure = new RegularMeasure();
			mRepeatingMeasure = new RepeatMeasure();
			mMods = GetComponentsInChildren<GeneratorMod>();
			if ( mAudioHandler != null )
			{
				mAudioHandler.Initialize( this );
			}
			else
			{
				Debug.LogError( "No Audio Handler set on the Music Generator prefab. Please set the appropriate handler before play" );
			}
		}

		/// <summary>
		/// Sets the mixer effects parameterValue for a particular instrument.
		/// Each instrument has its own channel
		/// </summary>
		/// <param name="index"></param>
		/// <param name="instrumentData"></param>
		private void SetInstrumentEffectsValues( int index, ConfigurationData.InstrumentData instrumentData )
		{
			mAudioHandler?.SetAllInstrumentParameters( index, instrumentData );
		}

		/// <summary>
		/// Loads the initial configuration.
		/// </summary>
		private void Start()
		{
			Started.Invoke();

			if ( OnHasVisiblePlayer() == false ) //without the UI, we load manually, as the UI panel is not going to do it, and we don't need ui initialized.
			{
				StartCoroutine( LoadConfiguration( mDefaultConfig, mAutoPlay ? GeneratorState.Playing : GeneratorState.Ready ) );
			}
		}

		/// <summary>
		/// State update:
		/// </summary>
		/// <returns></returns>
		private void Update()
		{
			UpdateState( Time.deltaTime );
		}

		/// <summary>
		/// Updates our generator state
		/// </summary>
		/// <param name="deltaT"></param>
		private void UpdateState( float deltaT )
		{
			mStateTimer += deltaT;

			switch ( GeneratorState )
			{
				case GeneratorState.Ready:
					break;
				case GeneratorState.Playing:
				{
					mRegularMeasure.PlayMeasure( InstrumentSet );
					break;
				}
				case GeneratorState.Repeating:
				{
					mRepeatingMeasure.PlayMeasure( InstrumentSet );
					break;
				}
			}

			mAudioHandler?.UpdateVolumeState( deltaT );

			StateUpdated.Invoke( GeneratorState );
		}

		/// <summary>
		/// sets up whether we'll change keys next measure:
		/// </summary>
		private void KeyChangeSetup()
		{
			if ( mKeyChangeNextMeasure == false )
			{
				return;
			}

			ConfigurationData.Key += ConfigurationData.KeySteps;
			ConfigurationData.Key = (Key) MusicConstants.SafeLoop( (int) ConfigurationData.Key, 0, MusicConstants.OctaveSize );

			mKeyChangeNextMeasure = false;
			KeyChanged.Invoke( (int) ConfigurationData.Key );
		}

		/// <summary>
		/// changes the key for the current instrument instrumentSet:
		/// alters the current chord progression to allow a smooth transition to
		/// the new key
		/// </summary>
		private void SetKeyChange()
		{
			if ( UnityEngine.Random.Range( 0.0f, 100.0f ) < ConfigurationData.KeyChangeOdds )
			{
				mKeyChangeNextMeasure = true;
				ConfigurationData.KeySteps = ( UnityEngine.Random.Range( 0, 100 ) < ConfigurationData.KeyChangeAscendDescend ) ?
					-MusicConstants.ScaleLength :
					MusicConstants.ScaleLength;
			}
			else
			{
				ConfigurationData.KeySteps = 0;
			}
		}

		[SerializeField]
		private bool mPauseOnLoseFocus;

		private bool mHasFocus;
		private GeneratorState mUnfocusedState;

		private void OnApplicationFocus( bool hasFocus )
		{
			if ( mPauseOnLoseFocus == false )
			{
				return;
			}

			mHasFocus = hasFocus;
			if ( mHasFocus && mUnfocusedState != GeneratorState.None )
			{
				SetState( mUnfocusedState );
				mUnfocusedState = GeneratorState.None;
			}
			else if ( mHasFocus == false )
			{
				mUnfocusedState = GeneratorState;
				Pause();
			}
		}

		#endregion private
	}
}

using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Just an example mod. This isn't terribly useful, it just re-rolls all instruments for whether they're using leitmotif when groups are chosen.
	/// You may want to consider adding a custom inspector depending on the complexity of what you're trying to do but for example, we will not.
	/// </summary>
	public class ExampleMod : GeneratorMod
	{
		///<inheritdoc/>
		public override string ModName => "ExampleMod";

		///<inheritdoc/>
		public override string Description => "Just an example";

		///<inheritdoc/>
		public override void SaveData()
		{
			var configurationName = $"{mMusicGenerator.ConfigurationData.ConfigurationName}_{ModName}";
			var persistentModDataPath = MusicConstants.ConfigurationPersistentModDataPath;
			Debug.Log( $"saving configuration {configurationName} to {persistentModDataPath}" );

			if ( Directory.Exists( persistentModDataPath ) == false )
			{
				Directory.CreateDirectory( persistentModDataPath );
			}

			try
			{
				var path = Path.Combine( persistentModDataPath, $"{configurationName}.txt" );
				File.WriteAllText( path, JsonUtility.ToJson( mLeitmotifOddsData, prettyPrint: true ) );
				Debug.Log( $"{configurationName} was successfully written to file" );
			}
			catch ( IOException e )
			{
				Debug.Log( $"{configurationName} failed to write to file with exception {e}" );
			}
		}

		///<inheritdoc/>
		public override void LoadData()
		{
			var configurationName = $"{mMusicGenerator.ConfigurationData.ConfigurationName}_{ModName}";
			ConfigurationData.LoadModData<LeitmotifOddsData>( configurationName,
				( data ) => { mLeitmotifOddsData = data; } );
		}

		///<inheritdoc/>
		public override void EnableMod( MusicGenerator generator )
		{
			mMusicGenerator = generator;
			mUIManager = FindObjectOfType<UIManager>();
			enabled = true;
			mLeitmotifOddsData = new LeitmotifOddsData( 50 );
		}

		///<inheritdoc/>
		public override void DisableMod()
		{
			enabled = false;
		}

		/// <summary>
		/// Reference to our music generator
		/// </summary>
		private MusicGenerator mMusicGenerator;

		[Serializable]
		private class LeitmotifOddsData
		{
			public LeitmotifOddsData( float odds )
			{
				Odds = odds;
			}

			[InspectorNameAttribute( "Individual instrument leitmotif odds" )]
			public float Odds;
		}

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Our data for this mod
		/// </summary>
		[SerializeField]
		private LeitmotifOddsData mLeitmotifOddsData;

		/// <summary>
		/// Invoked when groups are chosen
		/// </summary>
		private void OnGroupsChosen()
		{
			foreach ( var instrument in mMusicGenerator.InstrumentSet.Instruments )
			{
				instrument.InstrumentData.Leitmotif.IsEnabled =
					Random.Range( 0, 100 ) <= mLeitmotifOddsData.Odds;
			}

			if ( mUIManager != false )
			{
				// if we're in editor, dirty displays to update leitmotif toggles
				mUIManager.DirtyEditorDisplays();
			}
		}

		/// <summary>
		/// Invoked when an instrument is added
		/// </summary>
		/// <param name="instrument"></param>
		private void InstrumentAdded( Instrument instrument )
		{
			// we don't actually do anything for this mod here, just giving an example. It's often useful to grab the new instruments to change data
		}

		/// <summary>
		/// Invoked when an instrument is removed
		/// </summary>
		/// <param name="instrument"></param>
		private void InstrumentWillBeRemoved( Instrument instrument )
		{
			// we don't actually do anything for this mod here, just giving an example. It's often useful to grab the new instruments
		}

		/// <summary>
		/// Invoked when an instrument is removed
		/// </summary>
		private void InstrumentWasRemoved()
		{
			// we don't actually do anything for this mod here, just giving an example. Though, You'll usually want to dirty the uiManager at this point.
		}

		/// <summary>
		/// Invoked when this mod is enabled
		/// </summary>
		private void OnEnable()
		{
			mMusicGenerator.GroupsWereChosen.AddListener( OnGroupsChosen );
			mMusicGenerator.InstrumentAdded.AddListener( InstrumentAdded );
			mMusicGenerator.InstrumentWillBeRemoved.AddListener( InstrumentWillBeRemoved );
			mMusicGenerator.InstrumentWasRemoved.AddListener( InstrumentWasRemoved );
		}

		/// <summary>
		/// Invoked when this mod is disabled
		/// </summary>
		private void OnDisable()
		{
			mMusicGenerator.GroupsWereChosen.RemoveListener( OnGroupsChosen );
			mMusicGenerator.InstrumentAdded.RemoveListener( InstrumentAdded );
			mMusicGenerator.InstrumentWillBeRemoved.RemoveListener( InstrumentWillBeRemoved );
			mMusicGenerator.InstrumentWasRemoved.RemoveListener( InstrumentWasRemoved );
		}
	}
}

using Random = UnityEngine.Random;

namespace ProcGenMusic
{
	/// <summary>
	/// This mod enables you to use the random Groups option, while still guaranteeing that _one_ of them is enabled.
	/// Effectively, if none are naturally chosen, each has a 25% percent chance of playing.
	/// </summary>
	public class GuaranteedGroupMod : GeneratorMod
	{
		///<inheritdoc/>
		public override string ModName => "GuaranteedGroupMod";

		///<inheritdoc/>
		public override string Description => "When enabled, guarantees that a group will always be playing";

		///<inheritdoc/>
		public override void SaveData()
		{
			// n/a
		}

		///<inheritdoc/>
		public override void LoadData()
		{
			// n/a
		}

		///<inheritdoc/>
		public override void EnableMod( MusicGenerator generator )
		{
			mMusicGenerator = generator;
			mUIManager = FindObjectOfType<UIManager>();
			enabled = true;
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

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Invoked when groups are chosen
		/// </summary>
		private void OnGroupsChosen()
		{
			if ( mMusicGenerator.ConfigurationData.ManualGroupOdds || mMusicGenerator.GroupsAreTemporarilyOverriden )
			{
				return;
			}

			foreach ( var group in mMusicGenerator.InstrumentSet.GroupIsPlaying )
			{
				if ( group )
				{
					return;
				}
			}

			mMusicGenerator.InstrumentSet.GroupIsPlaying[Random.Range( 0, 3 )] = true;

			if ( mUIManager != false )
			{
				mUIManager.DirtyEditorDisplays();
			}
		}

		/// <summary>
		/// OnEnable
		/// </summary>
		private void OnEnable()
		{
			if ( mMusicGenerator.ConfigurationData.Mods.Contains( ModName ) == false )
			{
				mMusicGenerator.ConfigurationData.Mods.Add( ModName );
			}

			mMusicGenerator.GroupsWereChosen.AddListener( OnGroupsChosen );
		}

		/// <summary>
		/// OnDisable
		/// </summary>
		private void OnDisable()
		{
			mMusicGenerator.ConfigurationData.Mods.Remove( ModName );
			mMusicGenerator.GroupsWereChosen.RemoveListener( OnGroupsChosen );
		}
	}
}

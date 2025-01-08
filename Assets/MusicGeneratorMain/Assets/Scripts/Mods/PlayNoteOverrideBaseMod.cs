namespace ProcGenMusic
{
	/// <summary>
	/// This mod enables you to use the random Groups option, while still guaranteeing that _one_ of them is enabled.
	/// Effectively, if none are naturally chosen, each has a 25% percent chance of playing.
	/// </summary>
	public class PlayNoteOverrideBaseMod : GeneratorMod
	{
		///<inheritdoc/>
		public override string ModName => "PlayNoteOverrideBaseMod";

		///<inheritdoc/>
		public override string Description => "Catches played notes";

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
			if ( mUIManager != false )
			{
				mUIManager.UIKeyboard.ToggleKeyboardMute( true );
				mCachedUIEditorStyle = mUIManager.FXSettings.UIStyle;
				mUIManager.FXSettings.UIStyle = mUIEditorStyle;
				mUIManager.UIKeyboard.UpdateStyle();
			}

			enabled = true;
		}

		///<inheritdoc/>
		public override void DisableMod()
		{
			enabled = false;
			if ( mUIManager != false )
			{
				mUIManager.UIKeyboard.ToggleKeyboardMute( false );
				mUIManager.FXSettings.UIStyle = mCachedUIEditorStyle;
				mUIManager.UIKeyboard.UpdateStyle();
			}
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
		/// We disable the ui style during play as it will play immediately
		/// </summary>
		private UIEditorFXSettings.UIEditorStyle mUIEditorStyle = UIEditorFXSettings.UIEditorStyle.None;

		/// <summary>
		/// UI style prior to enabling mod
		/// </summary>
		private UIEditorFXSettings.UIEditorStyle mCachedUIEditorStyle = UIEditorFXSettings.UIEditorStyle.PianoRoll;

		/// <summary>
		/// OnEnable
		/// </summary>
		private void OnEnable()
		{
			if ( mMusicGenerator.ConfigurationData.Mods.Contains( ModName ) == false )
			{
				mMusicGenerator.ConfigurationData.Mods.Add( ModName );
			}

			mMusicGenerator.NotePlayed += OnNotePlayed;
		}

		/// <summary>
		/// OnDisable
		/// </summary>
		private void OnDisable()
		{
			mMusicGenerator.ConfigurationData.Mods.Remove( ModName );
			mMusicGenerator.NotePlayed -= OnNotePlayed;
		}

		private bool OnNotePlayed( object source, NotePlayedArgs args )
		{
			// Do something with args here:
			var noteIndex = args.Note; //< so, 0='C' 1='C#' 2 = 'D' etc.
			var instrumentIndex = args.InstrumentIndex; //< you'll have to reference your setup to know which instrument this refers to
			var volume = args.Volume;
			var set = args.InstrumentSet; //< this has almost _all_ the other data.
			var instrument = set.Instruments[args.InstrumentIndex]; //< some instrument data
			var instrumentData = instrument.InstrumentData; //< all the relevant instrument data

			// Send whatever data you want to your other library:

			/* If you're trying to do everything in a single method, I'd just feed in this data to a data container with the relevant info, and in a LateUpdate
			 process the data (i'm uncertain which data you need for your use case, so can't offer much pseudo code)
			 you may want/need to add a new unityEvent in MusicGenerator::UpdateState() so you know each 'tick'. or better yet, add an event in
			 MusicGenerator::Measure that is invoked upon 'TakeStep' (all the notes will be played during that step)
			 Unfortunately, not everything is played in a single chunk to just override and get all the note data at once.*/

			// You can still play the note here by returning true

			return false; //< return false in order to keep the generator from playing the note. If, say you're having an external library play it
		}
	}
}

using UnityEngine;

namespace ProcGenMusic
{
	/// <summary>
	/// class to be used for enabling, loading, saving mods
	/// </summary>
	public abstract class GeneratorMod : MonoBehaviour
	{
		/// <summary>
		/// To be used to save configuration data for this mod
		/// </summary>
		public abstract void SaveData();

		/// <summary>
		/// To be used to load data for this mod
		/// </summary>
		public abstract void LoadData();

		/// <summary>
		/// To be used when enabling this mod
		/// </summary>
		public abstract void EnableMod( MusicGenerator generator );

		/// <summary>
		/// To be used when disabling this mod
		/// </summary>
		public abstract void DisableMod();

		/// <summary>
		/// Mod name
		/// </summary>
		public abstract string ModName { get; }

		public abstract string Description { get; }

		protected virtual void Start()
		{
			// because unity is weird and won't surface the 'enabled' tickbox without this
		}
	}
}

using System;
using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
	/*
	 * This class is mostly irrelevant and any/all calls can be made directly to the music generator. Just grouping together here for the example clarity.
	 */
	public class MusicGeneratorHandler : MonoBehaviour
	{
		/// <summary>
		/// Loops through our configurations for this example scene
		/// </summary>
		/// <param name="onComplete"></param>
		public void ChangeConfiguration( Action onComplete = null )
		{
			// just looping through our example scene configuration names:
			mConfigurationIndex = mConfigurationIndex + 1 >= mConfigurationNames.Length ? 0 : mConfigurationIndex + 1;

			// Setting the continue state here is relevant. Since we want it to autoplay, we pass in 'GeneratorState.Playing'.
			StartCoroutine( mMusicGenerator.LoadConfiguration( mConfigurationNames[mConfigurationIndex], continueState: GeneratorState.Playing, onComplete: onComplete ) );
		}

		/// <summary>
		/// Changes the Mode of our currently loaded configuration
		/// </summary>
		/// <param name="mode"></param>
		public void ChangeMode( Mode mode )
		{
			mMusicGenerator.InstrumentSet.Data.Mode = mode;
		}

		/// <summary>
		/// Changes the Scale of our currently loaded configuration
		/// </summary>
		/// <param name="scale"></param>
		public void ChangeScale( Scale scale )
		{
			mMusicGenerator.InstrumentSet.Data.Scale = scale;
		}

		[SerializeField]
		private MusicGenerator mMusicGenerator;

		[SerializeField]
		private string[] mConfigurationNames;

		[SerializeField]
		private int mConfigurationIndex;
	}
}
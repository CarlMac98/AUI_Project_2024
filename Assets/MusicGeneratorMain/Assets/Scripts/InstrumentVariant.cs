using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Instrument Variant contains the audio clips for 3 octaves for an instrument
	/// </summary>
	[Serializable]
	public class InstrumentVariant
	{
		/// <summary>
		/// Our list of audio clips for this instrument
		/// </summary>
		public IReadOnlyList<AudioClip> Notes => mNotes;

		/// <summary>
		/// Loads our audio clips into memory
		/// </summary>
		public void LoadAudioClips( string instrumentName )
		{
			foreach ( var audioClip in mNotes )
			{
				if ( audioClip != null )
				{
					audioClip.LoadAudioData();
				}
				else
				{
					Debug.LogError( $"Audio file is missing for {instrumentName}. Please reimport the prefab" );
				}
			}
		}

		/// <summary>
		/// Unloads our audio clips from memory
		/// </summary>
		public void UnloadAudioClips()
		{
			foreach ( var audioClip in mNotes )
			{
				audioClip.UnloadAudioData();
			}
		}

		/// <summary>
		/// Sets our audio clips. Mostly for scripting use. Prefer using unity Editor
		/// </summary>
		/// <param name="audioClips"></param>
		public void SetAudioClips( List<AudioClip> audioClips )
		{
			mNotes = audioClips;
		}

		[SerializeField, Tooltip( "Our container of audio clips" )]
		private List<AudioClip> mNotes;
	}
}

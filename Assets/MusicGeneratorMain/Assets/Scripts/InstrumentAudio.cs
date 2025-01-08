using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Instrument Audio is simply a container of variations of an instrument
	/// </summary>
	[Serializable]
	public class InstrumentAudio : MonoBehaviour
	{
		/// <summary>
		/// Our reference to the InstrumentVariants of the instrument
		/// </summary>
		public IReadOnlyList<InstrumentVariant> Instruments => mInstrumentVariants;

		/// <summary>
		/// Loads audio clips for each instrument variant
		/// </summary>
		public void LoadAudioClips()
		{
			foreach ( var instrumentVariant in mInstrumentVariants )
			{
				instrumentVariant.LoadAudioClips( gameObject.name );
			}
		}

		/// <summary>
		/// Unloads the audio clips for each instrument variant
		/// </summary>
		public void UnloadAudioClips()
		{
			foreach ( var instrumentVariant in mInstrumentVariants )
			{
				instrumentVariant.UnloadAudioClips();
			}
		}

		/// <summary>
		/// Adds and instrument variant. mostly for scripting use. Prefer using Unity Editor.
		/// </summary>
		public void AddInstrument()
		{
			mInstrumentVariants.Add( new InstrumentVariant() );
		}

		[SerializeField, Tooltip( "Reference to our instrument variants" )]
		private List<InstrumentVariant> mInstrumentVariants = new List<InstrumentVariant>();
	}
}

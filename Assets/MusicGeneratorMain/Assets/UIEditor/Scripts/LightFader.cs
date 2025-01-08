using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles fading light objects in/out
	/// </summary>
	public class LightFader : Fader
	{
		///<inheritdoc/>
		protected override void UpdateFadeState()
		{
			base.UpdateFadeState();
			mLight.intensity = mFadeValue;
		}

		[SerializeField, Tooltip( "Reference to our light" )]
		private Light mLight;
	}
}
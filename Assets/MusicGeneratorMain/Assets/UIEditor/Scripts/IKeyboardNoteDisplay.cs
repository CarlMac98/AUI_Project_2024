using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Interface for keyboard note displays
	/// </summary>
	public interface IKeyboardNoteDisplay
	{
		/// <summary>
		/// Plays the keyboard note
		/// </summary>
		/// <param name="position"></param>
		/// <param name="color"></param>
		/// <param name="particlesEnabled"></param>
		void Play( Vector3 position, Color color, bool particlesEnabled );

		/// <summary>
		/// Stops the Keyboard Note Display
		/// </summary>
		void Stop();

		/// <summary>
		/// Updates color for keyboard note display
		/// </summary>
		/// <param name="color"></param>
		void UpdateColor( Color color );

		/// <summary>
		/// Reference for transform for keyboard note display
		/// </summary>
		Transform Transform { get; }
	}
}
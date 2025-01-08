using System;

namespace ProcGenMusic.ExampleScene
{
	/// <summary>
	/// Sets the configuration scale (based on handle angle in scene)
	/// </summary>
	public class ScaleHandle : MusicHandle
	{
		protected override int HandleTypeLength => Enum.GetNames(typeof(Scale)).Length;

		protected override string GetHandleText(int handleType)
		{
			return ScaleNames[handleType];
		}

		protected override void UpdateHandleType(int handleType)
		{
			mMusicGeneratorHandler.ChangeScale((Scale)handleType);
		}

		protected override float GetInitialHandleAngle()
		{
			return -mRotationMax + (int)mMusicGenerator.InstrumentSet.Data.Scale * (mRotationMax * 2 / mHandleTypeLength);
		}

		protected override string GetInitialHandleText()
		{
			return mMusicGenerator.InstrumentSet.Data.Scale.ToString();
		}

		private string[] ScaleNames =
		{
			"Major",
			"Natural Minor",
			"Melodic Minor",
			"Harmonic Minor",
			"Harmonic Major"
		};
	}
}
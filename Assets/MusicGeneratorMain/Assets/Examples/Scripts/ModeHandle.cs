using System;

namespace ProcGenMusic.ExampleScene
{
	/// <summary>
	/// Sets the current mode of the configuration (based on scene handle angle)
	/// </summary>
	public class ModeHandle : MusicHandle
	{
		protected override int HandleTypeLength => Enum.GetNames(typeof(Mode)).Length;

		protected override string GetHandleText(int handleType)
		{
			return $"{(Mode)handleType}";
		}

		protected override void UpdateHandleType(int handleType)
		{
			mMusicGeneratorHandler.ChangeMode((Mode)handleType);
		}

		protected override float GetInitialHandleAngle()
		{
			return -mRotationMax + (int)mMusicGenerator.InstrumentSet.Data.Mode * (mRotationMax * 2 / mHandleTypeLength);
		}

		protected override string GetInitialHandleText()
		{
			return mMusicGenerator.InstrumentSet.Data.Mode.ToString();
		}
	}
}
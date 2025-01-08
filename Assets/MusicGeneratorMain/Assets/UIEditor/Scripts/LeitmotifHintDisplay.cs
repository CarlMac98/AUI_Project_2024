using System.Collections.Generic;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Hint overlay for the leitmotif editor
	/// </summary>
	public class LeitmotifHintDisplay : EditorHintDisplay
	{
		protected override IReadOnlyList<int> CurrentProgression => mUIManager.MusicGenerator.InstrumentSet.Data.LeitmotifProgression;
	}
}
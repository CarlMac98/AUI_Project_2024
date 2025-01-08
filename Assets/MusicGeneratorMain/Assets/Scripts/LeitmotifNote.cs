using System;

#pragma warning disable 0649

namespace ProcGenMusic
{
	[Serializable]
	public class LeitmotifNote : IEquatable<LeitmotifNote>
	{
		public LeitmotifNote( int scaledNote = -1, int sharpFlat = 0 )
		{
			ScaledNote = scaledNote;
			Accidental = sharpFlat;
		}

		public override bool Equals( object obj )
		{
			var other = obj as LeitmotifNote;
			if ( other == null ) return false;

			return Equals( other );
		}

		public override int GetHashCode()
		{
			return ( ScaledNote * 10 ) + Accidental;
		}

		public bool Equals( LeitmotifNote other )
		{
			if ( other == null )
			{
				return false;
			}

			if ( ReferenceEquals( this, other ) )
			{
				return true;
			}

			return other.ScaledNote == ScaledNote && other.Accidental == Accidental;
		}

		public int ScaledNote;
		public int Accidental;
	}
}

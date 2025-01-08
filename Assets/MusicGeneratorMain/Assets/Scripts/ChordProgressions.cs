using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Generates chord progressions
	/// </summary>
	public class ChordProgressions
	{
		#region public

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="musicGenerator"></param>
		public ChordProgressions( MusicGenerator musicGenerator )
		{
			mMusicGenerator = musicGenerator;
		}

		/// <summary>
		/// Generates a new progression.
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="scale"></param>
		/// <param name="keyChange"></param>
		/// <returns></returns>
		public IReadOnlyList<int> GenerateProgression( Mode mode, Scale scale, int keyChange )
		{
			// here we decide which chord step we'll use based on tonal influences and whether we'll change keys:
			// this is a bit mangled, but it works :P
			for ( var index = 0; index < MusicConstants.MaxFullstepsTaken; index++ )
			{
				switch ( index )
				{
					case 0:
						mCurrentChords = Random.Range( 0, 100 ) < Data.TonicInfluence ? TonicChords : SubdominantChords;
						break;
					case 1:
						mCurrentChords = Random.Range( 0, 100 ) < Data.SubdominantInfluence ? SubdominantChords : TonicChords;
						break;
					case 2:
						mCurrentChords = Random.Range( 0, 100 ) < Data.SubdominantInfluence ? SubdominantChords : DominantChords;
						break;
					case 3:
						if ( Random.Range( 0, 100 ) < Data.DominantInfluence )
							mCurrentChords = DominantChords;
						else if ( Random.Range( 0, 100 ) < Data.SubdominantInfluence )
							mCurrentChords = SubdominantChords;
						else
							mCurrentChords = TonicChords;
						break;
				}

				var triTone = ( mCurrentChords == DominantChords && Random.Range( 0, 100 ) < Data.TritoneSubInfluence ) ? -1 : 1;
				mProgression[index] = triTone * GetProgressionSteps( mCurrentChords, mode, scale, keyChange );
			}

			return mProgression;
		}

		#endregion public

		#region private

		/// <summary>
		/// Reference to our music generator
		/// </summary>
		private readonly MusicGenerator mMusicGenerator;

		/// <summary>
		/// Reference to our configuration data
		/// </summary>
		private ConfigurationData Data => mMusicGenerator == null ? null : mMusicGenerator.ConfigurationData;

		/// <summary>
		/// which steps belong to the tonic:
		/// </summary>
		private static readonly int[] TonicChords =
		{
			1, 3, 6
		};

		/// <summary>
		/// which steps belong to the subdominant:
		/// </summary>
		private static readonly int[] SubdominantChords =
		{
			4, 2
		};

		/// <summary>
		/// which steps belong to the dominant:
		/// </summary>
		private static readonly int[] DominantChords =
		{
			5, 7
		};

		/// <summary>
		/// Returns whether our tonal chords contain a value.
		/// </summary>
		/// <param name="chords"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TonalChordsContain( int[] chords, int value )
		{
			foreach ( var chord in chords )
			{
				if ( chord == value )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Our currently generated chords
		/// </summary>
		/// <value></value>
		private int[] mCurrentChords;

		/// <summary>
		/// our current chord progression
		/// </summary>
		private readonly int[] mProgression =
		{
			1, 4, 4, 5
		};

		///<summary>
		/// Cached array to reduce GC
		/// </summary>
		private ListArrayInt mTempArray = new ListArrayInt( 10 );

		/// <summary>
		/// returns the chord interval.
		/// </summary>
		/// <param name="chords"></param>
		/// <param name="mode"></param>
		/// <param name="isMajorScale"></param>
		/// <param name="keyChange"></param>
		/// <returns></returns>
		private int GetProgressionSteps( int[] chords, Mode mode, Scale isMajorScale, int keyChange )
		{
			mTempArray.Clear();
			//create a new array of possible chord steps, excluding the steps we'd like to avoid:
			foreach ( var t in chords )
			{
				/* we're going to ignore excluded steps when changing keys, if it's not an avoid note.
				 * it's too likely that the note that's excluded is the only available note that's shared between
				 * the two keys for that chord type (like, if V is excluded, VII is never shared in major key ascending fifth step up)*/
				if ( ( keyChange != 0 && CheckKeyChangeAvoid( isMajorScale, keyChange, t, mode ) ) ||
				     Data.ExcludedProgSteps[t - 1] != true )
				{
					mTempArray.Add( t );
				}
			}

			if ( mTempArray.Count == 0 )
			{
				Debug.Log( "progression steps == 0" );
			}

			return mTempArray[Random.Range( 0, mTempArray.Count )];
		}

		/// <summary>
		/// Checks for notes to avoid before a key change
		/// </summary>
		/// <param name="scale"></param>
		/// <param name="keyChange"></param>
		/// <param name="chord"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		private static bool CheckKeyChangeAvoid( Scale scale, int keyChange, int chord, Mode mode )
		{
			// Musically, this could be more robust, but essentially checks to make sure a given chord will not sound
			// bad when changing keys. We change the key early in the generator, so, for example, we don't
			// want to play the 4th chord in the new key if we're descending, that chord is not shared
			// between the two keys.
			// TODO: more intelligent key changes :P
			var modeIndex = (int) mode;

			// if we're not changing keys, there's nothing to avoid:
			if ( keyChange == 0 ) return true;

			var isNotAvoidNote = true;
			if ( scale == Scale.Major || scale == Scale.HarmonicMajor )
			{
				if ( ( keyChange > 0 && chord == 7 - modeIndex ) ||
				     ( keyChange < 0 && chord == 4 - modeIndex ) )
					isNotAvoidNote = false;
			}
			else
			{
				if ( ( keyChange > 0 && chord == 2 - modeIndex ) ||
				     ( keyChange < 0 && chord == 6 - modeIndex ) )
					isNotAvoidNote = false;
			}

			return isNotAvoidNote;
		}

		#endregion private
	}
}

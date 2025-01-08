using System;
using System.Collections;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Probably a misguided attempt at avoiding generated garbage in the generator updates.
	/// Is a fixed array we can treat like a list to some extent. Limited functionality.
	/// Mostly legacy code I regret, but alas, I should have become a baby-goat herder instead of a programmer.
	/// </summary>
	[Serializable]
	public struct ListArray<T> : IEnumerable
	{
		public ListArray( int size )
		{
			Count = 0;
			mArray = new T[size];
		}

		public ListArray( T[] contents )
		{
			Count = contents.Length;
			mArray = contents;
		}

		public bool Equals( ListArray<T> other )
		{
			return mArray == other.mArray;
		}

		public T this[ int index ]
		{
			private get => mArray[index];
			set => mArray[index] = value;
		}

		public void Clear()
		{
			Count = 0;
		}

		public void Add( T a )
		{
			mArray[Count] = a;
			Count++;
		}

		public bool Contains( T value )
		{
			for ( var index = 0; index < Count; index++ )
			{
				if ( mArray[index].Equals( value ) )
				{
					return true;
				}
			}

			return false;
		}

		public T[] mArray;
		public int Count { get; private set; }

		public IEnumerator GetEnumerator()
		{
			return mArray.GetEnumerator();
		}
	}

	[Serializable]
	public struct ListArrayInt
	{
		public ListArrayInt( int size )
		{
			Count = 0;
			mArray = new int[size];
		}

		public ListArrayInt( int[] contents )
		{
			Count = contents.Length;
			mArray = new int[contents.Length];

			for ( var index = 0; index < contents.Length; index++ )
			{
				mArray[index] = contents[index];
			}
		}

		public ListArrayInt Clone()
		{
			var clone = new ListArrayInt( mArray )
			{
				Count = Count
			};
			return clone;
		}

		public bool Equals( ListArrayInt other )
		{
			return mArray == other.mArray;
		}

		public int this[ int index ]
		{
			get => mArray[index];
			set => mArray[index] = value;
		}

		public void Clear()
		{
			Count = 0;
		}

		public void Add( int a )
		{
			mArray[Count] = a;
			Count++;
		}

		public bool Contains( int value )
		{
			for ( var index = 0; index < Count; index++ )
			{
				if ( mArray[index].Equals( value ) )
				{
					return true;
				}
			}

			return false;
		}

		[SerializeField]
		public int[] mArray;

		[SerializeField]
		public int Count;
	}

	[Serializable]
	public struct ListArrayFloat
	{
		public ListArrayFloat( int size )
		{
			Count = 0;
			mArray = new float[size];
		}

		public ListArrayFloat( float[] contents )
		{
			Count = contents.Length;
			mArray = contents;
		}

		public bool Equals( ListArrayFloat other )
		{
			return mArray == other.mArray;
		}

		public float this[ int index ]
		{
			get => mArray[index];
			set => mArray[index] = value;
		}

		public void Clear()
		{
			Count = 0;
		}

		public void Add( float a )
		{
			mArray[Count] = a;
			Count++;
		}

		public bool Contains( float value )
		{
			for ( var index = 0; index < Count; index++ )
			{
				if ( mArray[index].Equals( value ) )
				{
					return true;
				}
			}

			return false;
		}

		[SerializeField, Range( 0f, 100f )]
		private float[] mArray;

		public float[] FloatArray
		{
			get => mArray;
			set
			{
				for ( var index = 0; index < value.Length; index++ )
				{
					value[index] = Mathf.Clamp( value[index], 0f, 100f );
				}
			}
		}

		public int Count { get; private set; }
	}

	[Serializable]
	public struct ListArrayBool
	{
		public ListArrayBool( int size )
		{
			Count = 0;
			mArray = new bool[size];
		}

		public ListArrayBool( bool[] contents )
		{
			Count = contents.Length;
			mArray = contents;
		}

		public bool Equals( ListArrayBool other )
		{
			return mArray == other.mArray;
		}

		public bool this[ int index ]
		{
			get => mArray[index];
			set => mArray[index] = value;
		}

		public void Clear()
		{
			Count = 0;
		}

		public void Add( bool a )
		{
			mArray[Count] = a;
			Count++;
		}

		public bool Contains( bool value )
		{
			for ( var index = 0; index < Count; index++ )
			{
				if ( mArray[index].Equals( value ) )
				{
					return true;
				}
			}

			return false;
		}

		public bool[] mArray;
		public int Count { get; private set; }
	}
}

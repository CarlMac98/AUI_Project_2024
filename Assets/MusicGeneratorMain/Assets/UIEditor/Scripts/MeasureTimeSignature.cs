using System;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Measure Time signature display (the literal display of editor measures for a time signature in the measure editor and variants)
	/// Handles returning the offset values for notes within the measures
	/// </summary>
	public class MeasureTimeSignature : MonoBehaviour
	{
		[Serializable]
		private class Measure
		{
			/// <summary>
			/// Collider for the entirety of the time signature
			/// </summary>
			public Collider2D Collider => mCollider;

			/// <summary>
			/// Returns how many steps are in this time signature (i.e. 3 for 3/4, 4 for 4/4)
			/// </summary>
			public int StepLength => mStepTransforms.Length;

			/// <summary>
			/// returns vertical offset of the step transform
			/// </summary>
			/// <param name="index"></param>
			/// <returns></returns>
			public float GetNoteVerticalOffset( int index )
			{
				return mStepTransforms[index].position.y;
			}

			/// <summary>
			/// Returns vertical offset of the step transform
			/// </summary>
			/// <param name="position"></param>
			/// <param name="yIndex"></param>
			/// <returns></returns>
			public float GetNoteVerticalOffset( Vector2 position, out int yIndex )
			{
				var closestDistance = float.MaxValue;
				float closestPosition = 0;
				yIndex = 0;
				for ( var stepIdx = 0; stepIdx < mStepTransforms.Length; stepIdx++ )
				{
					var distance = Mathf.Abs( mStepTransforms[stepIdx].position.y - position.y );
					if ( distance < closestDistance )
					{
						closestPosition = mStepTransforms[stepIdx].position.y;
						closestDistance = distance;
						yIndex = stepIdx;
					}
				}

				return closestPosition;
			}

			[SerializeField, Tooltip( "Reference to our Collider for the measure time signature" )]
			private Collider2D mCollider;

			[SerializeField, Tooltip( "Reference to our transforms for each step" )]
			private Transform[] mStepTransforms;
		}

		/// <summary>
		/// Returns vertical offset of a note
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public float GetStepVerticalOffset( Vector2Int index )
		{
			if ( index.x >= mMeasures.Length || index.y >= mMeasures[index.x].StepLength )
			{
				return -1;
			}

			return mMeasures[index.x].GetNoteVerticalOffset( index.y );
		}

		/// <summary>
		/// Returns vertical offset of a note
		/// </summary>
		/// <param name="position"></param>
		/// <param name="beat"></param>
		/// <returns></returns>
		public float GetStepVerticalOffset( Vector2 position, out Vector2Int beat )
		{
			beat = Vector2Int.zero;
			Vector3 zAlignedPosition = position;
			for ( var measureIdx = 0; measureIdx < mMeasures.Length; measureIdx++ )
			{
				zAlignedPosition.z = mMeasures[measureIdx].Collider.bounds.center.z;
				if ( mMeasures[measureIdx].Collider.bounds.Contains( zAlignedPosition ) )
				{
					var yOffset = mMeasures[measureIdx].GetNoteVerticalOffset( zAlignedPosition, out var yIndex );
					beat = new Vector2Int( measureIdx, yIndex );
					return yOffset;
				}
			}

			return 0f;
		}

		/// <summary>
		/// Hides the measure time signature
		/// </summary>
		public void Hide()
		{
			foreach ( var spriteFade in mSpriteFades )
			{
				spriteFade.Hide();
			}
		}

		/// <summary>
		/// Shows the measure time signatures
		/// </summary>
		public void Show()
		{
			foreach ( var spriteFade in mSpriteFades )
			{
				spriteFade.Show();
			}
		}

		/// <summary>
		/// Fades the measure time signatures in
		/// </summary>
		public void FadeIn()
		{
			foreach ( var spriteFade in mSpriteFades )
			{
				spriteFade.FadeIn();
			}
		}

		/// <summary>
		/// Fades the measure time signatures out.
		/// </summary>
		public void FadeOut()
		{
			foreach ( var spriteFade in mSpriteFades )
			{
				spriteFade.FadeOut();
			}
		}

		[SerializeField, Tooltip( "Reference to our Measures" )]
		private Measure[] mMeasures;

		[SerializeField, Tooltip( "Reference to our Sprite fade objects for the measures" )]
		private SpriteFade[] mSpriteFades;
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class OutputVisualizer : MonoBehaviour
	{
		/// <summary>
		/// Sets our visualizer line color
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( Color color )
		{
			mAlphaVector.z = mOutputRenderer.startColor.a;
			mOutputRenderer.startColor = color * mAlphaVector;
			mOutputRenderer.endColor = color * mAlphaVector;
		}

		public void SetWidth( float width )
		{
			mOutputRenderer.startWidth = width;
			mOutputRenderer.endWidth = width;
		}

		/// <summary>
		/// Toggles our visibility
		/// </summary>
		/// <param name="isOn"></param>
		public void ToggleFadeState( bool isOn )
		{
			if ( isOn )
			{
				mLineFader.FadeIn();
			}
			else
			{
				mLineFader.FadeOut();
			}
		}

		[SerializeField, Tooltip( "Reference to our output line renderer" )]
		private LineRenderer mOutputRenderer;

		[SerializeField, Range( .1f, 1f ), Tooltip( "Width multiplier of our line renderer" )]
		private float mOutputWidthScale = .29f;

		[SerializeField, Range( 1f, 100f ), Tooltip( "Height multiplier of our line renderer" )]
		private float mOutputHeightScale = 30f;

		private Vector4 mAlphaVector = Vector4.one;

		[SerializeField, Tooltip( "Reference to our line renderer" )]
		private LineFader mLineFader;

		[SerializeField]
		private float mDataScale = .2f;

		private readonly Vector3[] mEmptyPositions = { Vector3.zero, new Vector3( 1f, 1f, 1f ) };

		/// <summary>
		/// Current position cache
		/// </summary>
		private readonly Vector3[] mPositions = new Vector3[AudioVisualizer.SAMPLE_SIZE];

		/// <summary>
		/// current scale cache
		/// </summary>
		private float[] mScaleValues;

		/// <summary>
		/// Sample size
		/// </summary>
		private readonly int mSampleSize = AudioVisualizer.SAMPLE_SIZE;

		/// <summary>
		/// Awake
		/// </summary>
		private void Awake()
		{
			mScaleValues = new float[mSampleSize];
		}

		/// <summary>
		/// Update
		/// </summary>
		private void Update()
		{
			mScaleValues = AudioVisualizer.OutputData;

			if ( mScaleValues == null || mScaleValues.Length == 0 )
			{
				mOutputRenderer.positionCount = mEmptyPositions.Length;
				mOutputRenderer.SetPositions( mEmptyPositions );
				return;
			}

			var scale = mDataScale;
			var dataMin = mScaleValues.Min();
			var dataMax = mScaleValues.Max();
			var shouldScale = dataMax > scale || dataMin < -scale;

			if ( Math.Abs( dataMax - dataMin ) < .0001f )
			{
				scale = 0f;
			}

			if ( shouldScale )
			{
				mScaleValues = NormalizeData( mScaleValues, -scale, scale );
			}

			for ( var index = 0; index < mSampleSize; index++ )
			{
				float value;
				if ( shouldScale == false )
				{
					value = mScaleValues[index] * 10f;
				}
				else
				{
					value = mScaleValues[index] * 10f;
				}

				mPositions[index] = new Vector3( index * mOutputWidthScale, value * mOutputHeightScale, 1 );
			}

			mOutputRenderer.positionCount = mPositions.Length;
			mOutputRenderer.SetPositions( mPositions );
		}

		private static float[] NormalizeData( IEnumerable<float> data, float min, float max )
		{
			var enumerable = data as float[] ?? data.ToArray();
			var dataMax = enumerable.Max();
			var dataMin = enumerable.Min();
			var range = dataMax - dataMin;

			return enumerable
				.Select( d => ( d - dataMin ) / range )
				.Select( n => ( 1f - n ) * min + n * max )
				.ToArray();
		}
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

#pragma warning disable 0649
#pragma warning disable 0414

namespace ProcGenMusic
{
	public class SpectrumVisualizer : MonoBehaviour
	{
		/// <summary>
		/// Sets the color of our spectrum visualizer
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( Color color )
		{
			mColor = color;
			if ( mHasInitialized == false )
			{
				Initialize( );
			}
			else
			{
				mBarSharedMaterial.color = color;
				mBarSharedMaterial.SetColor( mColorID, mColor * Mathf.LinearToGammaSpace( mEmissionMultiplier ) );
			}
		}

		/// <summary>
		/// Toggles our visibility
		/// </summary>
		/// <param name="isOn"></param>
		public void ToggleFadeState( bool isOn )
		{
			if ( isOn )
			{
				foreach ( var fader in mMeshFaders )
				{
					fader.SetEmissionIntensity( mUIManager.FXSettings.VisualizerEmissiveIntensity );
					fader.FadeIn( );
				}
			}
			else
			{
				foreach ( var fader in mMeshFaders )
				{
					fader.SetEmissionIntensity( mUIManager.FXSettings.VisualizerEmissiveIntensity );
					fader.FadeOut( );
				}
			}
		}

		/// <summary>
		/// Sets the brightness of our spectrum visualizer
		/// </summary>
		/// <param name="emissionMultiplier"></param>
		public void SetBrightness( float emissionMultiplier )
		{
			mEmissionMultiplier = emissionMultiplier;
		}

		[SerializeField, Range( 1f, 100f ), Tooltip( "Scale multiplier of our visualizer bars" )]
		private float mScale = 1;

		[SerializeField, Range( 1f, 300f ), Tooltip( "Radius of our bar layout" )]
		private float mRadius;

		[SerializeField, Range( 1f, 100f ), Tooltip( "Bar scale multiplier" )]
		private float mMultiplier = 2f;

		[SerializeField, Tooltip( "Asset Reference to our visual image" )]
		private GameObject mBarReference;

		[SerializeField, Tooltip( "Reference to the music generator" )]
		private MusicGenerator mMusicGenerator;

		[SerializeField, Tooltip( "Reference to the ui manager" )]
		private UIManager mUIManager;

		[SerializeField, Tooltip( "Which color material index to use" )]
		public ColorFieldType mColorFieldType;

		[SerializeField, Range( 1f, 300f ), Tooltip( "Fall rate of our visual bars" )]
		private float mFallRate = 10f;

		/// <summary>
		/// How many bars to display
		/// </summary>
		private const int NumBars = 16;

		/// <summary>
		/// Current color
		/// </summary>
		private Color mColor;

		/// <summary>
		/// Cache of our spawned bars
		/// </summary>
		private readonly List<GameObject> mBars = new List<GameObject>( );

		/// <summary>
		/// Reference to our bar shared material
		/// </summary>
		private Material mBarSharedMaterial;

		/// <summary>
		/// Scale cache
		/// </summary>
		private float[] mBarScaleValues;

		/// <summary>
		/// Emission color id
		/// </summary>
		private static readonly int mColorID = Shader.PropertyToID( "_BaseColor" );

		/// <summary>
		/// Our current emission multiplier
		/// </summary>
		private float mEmissionMultiplier = 1f;

		private bool mHasInitialized;
		private List<MeshFader> mMeshFaders = new List<MeshFader>( );

		/// <summary>
		/// Awake
		/// </summary>
		private void Awake( )
		{
			if ( mHasInitialized == false )
			{
				Initialize( );
			}
		}

		/// <summary>
		/// Instantiates our visual bars
		/// </summary>
		private void Initialize( )
		{
			mColor = mUIManager.CurrentColors[( int )mColorFieldType].Color;
			for ( var index = 0; index < NumBars; index++ )
			{
				var angle = ( ( float )index / ( NumBars - 1 ) / 2 ) * Mathf.PI * 2f;
				var x = Mathf.Cos( angle ) * -mRadius;
				var y = +Mathf.Sin( angle ) * mRadius;
				var position = new Vector3( x, y, 0 );
				if ( mBarReference == null )
				{
					break;
				}

				var bar = Instantiate( mBarReference, Vector3.zero, Quaternion.identity, transform );

				bar.transform.SetParent( transform );
				bar.transform.rotation = Quaternion.LookRotation( Vector3.forward, position );
				bar.transform.localPosition = position;
				bar.transform.localScale = Vector3.one * mScale;
				mBars.Add( bar );
				mMeshFaders.Add( bar.GetComponentInChildren<MeshFader>( ) );
				mBarSharedMaterial = bar.GetComponentInChildren<MeshRenderer>( ).sharedMaterial;
				mBarSharedMaterial.SetColor( mColorID, mColor * Mathf.LinearToGammaSpace( mEmissionMultiplier ) );
			}

			mHasInitialized = true;
			mBarScaleValues = new float[NumBars];
		}

		/// <summary>
		/// Update.
		/// TODO: revisit this, it's not super great :P
		/// </summary>
		private void Update( )
		{
#if FMOD_ENABLED == false
			if ( mBars.Count < NumBars )
			{
				return;
			}

			var sum = 0f;
			var chunk = 4f;
			var chunkIndex = 0;
			var data = AudioVisualizer.SpectrumData;
			var min = float.MaxValue;
			var max = 0f;
			var scaleIndex = 0;

			foreach ( var element in data )
			{
				if ( scaleIndex >= mBars.Count )
				{
					break;
				}

				sum += element;
				if ( chunkIndex >= chunk )
				{
					var value = sum;
					min = value < min ? value : min;
					max = value > max ? value : max;

					mBarScaleValues[scaleIndex] = sum;
					sum = 0;
					chunk *= 1.31f;
					chunkIndex = 0;
					scaleIndex++;
				}
				else
				{
					chunkIndex++;
				}
			}

			if ( max != 0 )
			{
				for ( var index = 0; index < mBarScaleValues.Length; index++ )
				{
					var value = ( mBarScaleValues[index] - min ) / max;

					var newScale = new Vector3( mScale, mScale, mScale ) + Vector3.up * ( value * mMultiplier );
					if ( newScale.y < mBars[index].transform.localScale.y )
					{
						newScale.y = Mathf.Max( 0, mBars[index].transform.localScale.y - ( mFallRate * Time.deltaTime ) );
					}

					mBars[index].transform.localScale = newScale;
				}
			}
			
#endif //FMOD_ENABLED == false
		}
	}
}
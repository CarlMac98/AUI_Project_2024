using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Handles visual representation of percussion instruments on the keyboard/piano roller
	/// </summary>
	public class KeyboardPercussionInstrument : MonoBehaviour, IKeyboardNoteDisplay
	{
#region public

		///<inheritdoc/>
		public Transform Transform { get; private set; }

		/// <summary>
		/// Initializes the KeyboardPercussionInstrument, setting color and injecting references
		/// </summary>
		/// <param name="uiKeyboard"></param>
		/// <param name="uiManager"></param>
		/// <param name="color"></param>
		public void Initialize( UIKeyboard uiKeyboard, UIManager uiManager, Color color )
		{
			mUIKeyboard = uiKeyboard;
			mUIManager = uiManager;
			UpdateColor( color );
			Transform = transform;
		}

		///<inheritdoc/>
		public void UpdateColor( Color color )
		{
			mColor = color;
		}

		/// <summary>
		/// Updates the size and position of the Percussion Instrument
		/// </summary>
		/// <param name="position"></param>
		/// <param name="size"></param>
		public void UpdateSizeAndPosition( Vector3 position, Vector2 size )
		{
			Transform.localPosition = position;
			mSpriteRenderer.size = size;
		}

		///<inheritdoc/>
		public void Stop()
		{
			mEmissionMultiplier = mUIManager.FXSettings.FallingNoteEmissionIntensityFloor;
			mSpriteRenderer.material.SetColor( mColorID, mColor * Mathf.LinearToGammaSpace( mEmissionMultiplier ) );
		}

		///<inheritdoc/>
		public void Play( Vector3 position, Color color, bool particlesEnabled )
		{
		}

		public void PlayBeat( float duration )
		{
			mDuration = duration;
			mEmissionMultiplier = mUIKeyboard.EmissionPulseIntensity;
		}

#endregion public

#region private

		[SerializeField, Tooltip( "Reference to our sprite renderer" )]
		private SpriteRenderer mSpriteRenderer;

		private static readonly int mColorID = Shader.PropertyToID( "_BaseColor" );
		private float mDuration;

		/// <summary>
		/// Reference to the  UIKeyboard
		/// </summary>
		private UIKeyboard mUIKeyboard;

		/// <summary>
		/// Reference to our emission multiplier
		/// </summary>
		private float mEmissionMultiplier;

		/// <summary>
		/// Reference to the UIManager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// The color of this percussion instrument
		/// </summary>
		private Color mColor;
		
		/// <summary>
		/// Update
		/// </summary>
		private void Update()
		{
			var emissionFloor = mUIManager.FXSettings.FallingNoteEmissionIntensityFloor;
			if ( mEmissionMultiplier > emissionFloor )
			{
				mEmissionMultiplier -= Time.deltaTime * mUIKeyboard.EmissionPulseIntensityFalloff / mDuration;
			}
			else
			{
				mEmissionMultiplier = emissionFloor;
			}
			mSpriteRenderer.material.SetColor( mColorID, mColor * Mathf.LinearToGammaSpace( mEmissionMultiplier ) );
		}

#endregion private
	}
}

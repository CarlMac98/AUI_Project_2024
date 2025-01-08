using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Physical/visual key on the keyboard for the generator's editor
	/// </summary>
	public class UIKey : MonoBehaviour
	{
		/// <summary>
		/// Reference to the UIKey's transform
		/// </summary>
		public Transform Transform { get; private set; }

		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name="uiManager"></param>
		public void Initialize( UIManager uiManager )
		{
			Transform = transform;
			mUIManager = uiManager;
			SetMaterialIDs( );
		}

		/// <summary>
		/// Plays the UIKey's lights/particles
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="strength"></param>
		/// <param name="color"></param>
		public void Play( float duration, int strength, Color color )
		{
			mIsPlaying = true;
			mCurrentColor = color;
			mCurrentColorFadeTime = mColorFadeTime;
			mTimer = 0;

			if ( mLightsAreEnabled )
			{
				mMaterial.SetColor( mShaderColorID, mCurrentColor );
			}

			if ( mParticlesAreEnabled )
			{
				PlayParticles( mCurrentColor );
			}
		}

		private void PlayParticles( Color finalColor )
		{
			var mainModule = mParticles[( int )mUIManager.FXSettings.UIKeysParticleStyle].main;
			mainModule.startColor = finalColor;
			mParticles[( int )mUIManager.FXSettings.UIKeysParticleStyle].time = 0f;

			mParticles[( int )mUIManager.FXSettings.UIKeysParticleStyle].Play( true );
			foreach ( var subParticle in mParticles[( int )mUIManager.FXSettings.UIKeysParticleStyle].GetComponentsInChildren<ParticleSystem>( ) )
			{
				var subModule = subParticle.main;
				subModule.startColor = finalColor;
				subParticle.Play( );
			}
		}

		public void Stop( )
		{
			mParticles[( int )mUIManager.FXSettings.UIKeysParticleStyle].Stop( );
		}

		public void ShowLightHighlight( Color color )
		{
			mIsPlaying = true;
			mCurrentColor = color;
			mCurrentColorFadeTime = mHighlightColorFadeTime;
			mTimer = 0;
			mMaterial.SetColor( mShaderColorID, mCurrentColor );
		}

		/// <summary>
		/// Toggle's whether lights are used for the ui key
		/// </summary>
		/// <param name="lightsAreEnabled"></param>
		public void ToggleLights( bool lightsAreEnabled )
		{
			mLightsAreEnabled = lightsAreEnabled;
			if ( mLightsAreEnabled == false )
			{
				mMaterial.SetColor( mShaderColorID, mBaseColor );
				mIsPlaying = false;
			}
		}

		/// <summary>
		/// Toggles whether particles are used for the ui key
		/// </summary>
		/// <param name="particlesAreEnabled"></param>
		public void ToggleParticles( bool particlesAreEnabled )
		{
			mParticlesAreEnabled = particlesAreEnabled;
		}

		/// <summary>
		/// Manual update loop
		/// </summary>
		/// <param name="deltaTime"></param>
		public void DoUpdate( float deltaTime )
		{
			if ( mLightsAreEnabled == false )
			{
				return;
			}

			if ( mTimer < mCurrentColorFadeTime && mIsPlaying )
			{
				mTimer += deltaTime;

				mMaterial.SetColor( mShaderColorID, Color.Lerp( mCurrentColor, mBaseColor, mTimer / mCurrentColorFadeTime ) );
				//mMaterial.SetColor( mShaderColorID, Color.Lerp( mCurrentColor, mBaseColor, mTimer / mColorFadeTime ) );
			}
			else if ( mIsPlaying )
			{
				mMaterial.SetColor( mShaderColorID, mBaseColor );
				mIsPlaying = false;
			}
		}

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Current timer value
		/// </summary>
		private float mTimer;

		/// <summary>
		/// Current playing state of the ui key
		/// </summary>
		private bool mIsPlaying;

		/// <summary>
		/// Whether lights are enabled
		/// </summary>
		private bool mLightsAreEnabled = true;

		/// <summary>
		/// Whether particles are enabled
		/// </summary>
		private bool mParticlesAreEnabled = true;

		[SerializeField, Tooltip( "Reference to our keyboard material" )]
		private Material mMaterial;

		[SerializeField, Tooltip( "Reference to our ui key's particle systems" )]
		private ParticleSystem[] mParticles;

		[SerializeField, Tooltip( "Color Fade Time" )]
		private float mColorFadeTime = 1.5f;

		[SerializeField]
		private float mHighlightColorFadeTime;

		[SerializeField, Tooltip( "Note index to which this key corresponds" )]
		private int mNoteIndex;

		[SerializeField, Tooltip( "Duration for the light when pressed" )]
		private float mKeyPressLightDuration = .5f;

		[SerializeField, Tooltip( "Strength for the light when pressed" )]
		private int mKeypressLightStrength = 3;

		[SerializeField, Tooltip( "Base key color" )]
		private Color mBaseColor = Color.white;

		private int mShaderColorID;
		private Color mCurrentColor;
		private float mCurrentColorFadeTime;

		/// <summary>
		/// OnMouseDown (we handle the playing of the key/lights/particle manually
		/// </summary>
		private void OnMouseDown( )
		{
			if ( mUIManager.MusicGenerator.GeneratorState == GeneratorState.Playing ||
			     mUIManager.MusicGenerator.GeneratorState == GeneratorState.Repeating ||
			     mUIManager.InstrumentListPanelUI.SelectedInstrument == null )
			{
				return;
			}

			var color = mUIManager.Colors[( int )mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.StaffPlayerColor];
			Play( mKeyPressLightDuration, mKeypressLightStrength, color );
			var instrumentName = mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.InstrumentType;
			var volume = mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.Volume;
			var instrumentIndex = mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentIndex;
			var noteIndex = mUIManager.InstrumentListPanelUI.SelectedInstrument.InstrumentData.IsPercussion ? 0 : mNoteIndex;
			mUIManager.MusicGenerator.PlayAudioClip( mUIManager.CurrentInstrumentSet, instrumentName, noteIndex, volume, instrumentIndex );
		}

		private void SetMaterialIDs( )
		{
			switch ( mNoteIndex )
			{
				case 0:
				case 12:
				case 24:
					mShaderColorID = Shader.PropertyToID( $"_CColor" );
					break;
				case 1:
				case 13:
				case 25:
					mShaderColorID = Shader.PropertyToID( $"_CSharpColor" );
					break;
				case 2:
				case 14:
				case 26:
					mShaderColorID = Shader.PropertyToID( $"_DColor" );
					break;
				case 3:
				case 15:
				case 27:
					mShaderColorID = Shader.PropertyToID( $"_DSharpColor" );
					break;
				case 4:
				case 16:
				case 28:
					mShaderColorID = Shader.PropertyToID( $"_EColor" );
					break;
				case 5:
				case 17:
				case 29:
					mShaderColorID = Shader.PropertyToID( $"_FColor" );
					break;
				case 6:
				case 18:
				case 30:
					mShaderColorID = Shader.PropertyToID( $"_FSharpColor" );
					break;
				case 7:
				case 19:
				case 31:
					mShaderColorID = Shader.PropertyToID( $"_GColor" );
					break;
				case 8:
				case 20:
				case 32:
					mShaderColorID = Shader.PropertyToID( $"_GSharpColor" );
					break;
				case 9:
				case 21:
				case 33:
					mShaderColorID = Shader.PropertyToID( $"_AColor" );
					break;
				case 10:
				case 22:
				case 34:
					mShaderColorID = Shader.PropertyToID( $"_ASharpColor" );
					break;
				case 11:
				case 23:
				case 35:
					mShaderColorID = Shader.PropertyToID( $"_BColor" );
					break;
			}
		}

		private void OnDisable( )
		{
			mMaterial.SetColor( mShaderColorID, mBaseColor );
		}
	}
}
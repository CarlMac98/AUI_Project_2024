using UnityEngine;
using UnityEngine.Serialization;

namespace ProcGenMusic
{
	public class PMGLogo : MonoBehaviour
	{
		[FormerlySerializedAs( "m_logoParticles" )]
		[SerializeField]
		private ParticleSystem mLogoParticles;

		[SerializeField]
		private ParticleSystem mSubparticles;

		[SerializeField]
		private float mPauseDelay = 1f;

		[SerializeField]
		private MusicGenerator mMusicGenerator;

		[SerializeField]
		private UIKeyboard mUIKeyboard;

		[SerializeField]
		private UIManager mUIManager;

		private bool mIsPlaying;
		private float mElapsedTime;
		private LogoState mLogoState;
		private GeneratorState mGeneratorState;
		
		private enum LogoState
		{
			Playing,
			Paused,
			Stopped
		}

		private void Awake()
		{
			mMusicGenerator.StateSet.AddListener( OnStateChanged );
			mLogoParticles.Play();
			mSubparticles.Play();
			mIsPlaying = true;
			mElapsedTime = 0f;
			mLogoState = LogoState.Stopped;
			mUIManager.UIEditorSettings.OnUseLogoChanged.AddListener( OnUseLogoChanged );
		}

		private void Update()
		{
			if ( mIsPlaying && mLogoState != LogoState.Playing )
			{
				mElapsedTime += Time.deltaTime;
				if ( mElapsedTime >= mPauseDelay )
				{
					mLogoParticles.Pause();
					mSubparticles.Pause();
					mIsPlaying = false;
				}
			}
		}

		private void OnStateChanged( GeneratorState state )
		{
			var previousState = mGeneratorState;
			switch ( state )
			{
				case GeneratorState.Paused:
					mLogoState = LogoState.Paused;
					break;
				case GeneratorState.Playing:
					if ( previousState == GeneratorState.Repeating || 
					     previousState == GeneratorState.ManualPlay )
					{
						break;
					}
					var canPlay = mUIKeyboard.CurrentPlayMode != UIKeyboard.PlayMode.Percussion &&
					              mUIKeyboard.CurrentPlayMode != UIKeyboard.PlayMode.LeitmotifPercussion &&
					              mUIKeyboard.CurrentPlayMode != UIKeyboard.PlayMode.ClipPercussion;

					if ( mLogoState != LogoState.Paused && canPlay )
					{
						mLogoParticles.Play();
						mSubparticles.Play();
					}

					mLogoState = LogoState.Playing;
					break;
				case GeneratorState.Stopped:
					mLogoState = LogoState.Stopped;
					mIsPlaying = false;
					mElapsedTime = 0f;
					mLogoParticles.Simulate( mElapsedTime, true, true );

					mSubparticles.Simulate( mElapsedTime, true, true );
					mIsPlaying = true;
					break;
			}

			mGeneratorState = state;
		}

		private void OnUseLogoChanged( bool isActive )
		{
			mLogoParticles.gameObject.SetActive( isActive );
		}
	}
}
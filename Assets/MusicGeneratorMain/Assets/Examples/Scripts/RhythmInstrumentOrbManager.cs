using System.Collections.Generic;
using UnityEngine;

namespace ProcGenMusic.ExampleScene
{
    public class RhythmInstrumentOrbManager : MonoBehaviour
    {
        public void Undo()
        {
            var index = mBounceSpawnIndex - 1;

            if ( index >= 0 && index < mBounceSpawn.Count )
            {
                mBounceSpawn[mBounceSpawnIndex - 1].Stop();
                mBounceSpawnIndex = mBounceSpawnIndex-- > 0 ? mBounceSpawnIndex : 0;
            }
        }

        [SerializeField]
        private WaterObject mWaterObject;

        [SerializeField]
        private RhythmInstrumentOrb mBallPrefab;

        [SerializeField]
        private MusicGenerator mMusicGenerator;

        [SerializeField]
        private int mPoolSize = 10;

        [SerializeField]
        private Transform mBounceSpawnParent;

        [SerializeField]
        private InputHandler mInputHandler;

        [SerializeField]
        private LayerMask mWaterLayerMask;

        [SerializeField]
        private int mRightClickInstrumentIndex;

        [SerializeField]
        private int mLeftClickInstrumentIndex;

        [SerializeField]
        private int mMiddleClickInstrumentIndex;

        [SerializeField, ColorUsage( true, true )]
        private Color mLeftClickColor;

        [SerializeField, ColorUsage( true, true )]
        private Color mRightClickColor;

        [SerializeField, ColorUsage( true, true )]
        private Color mMiddleClickColor;

        private int mBounceSpawnIndex;
        private readonly List<RhythmInstrumentOrb> mBounceSpawn = new List<RhythmInstrumentOrb>();

        private void Awake()
        {
            var addressableManager = mMusicGenerator.AddressableManager;
            mInputHandler.LeftClickDown.AddListener( OnLeftClick );
            mInputHandler.RightClickDown.AddListener( OnRightClick );
            mInputHandler.MiddleClickDown.AddListener( OnMiddleClick );

            SpawnBounceClickers( addressableManager );
        }

        private void OnMiddleClick()
        {
            if ( mInputHandler.HitInfo.transform == null || ( mWaterLayerMask & ( 1 << mInputHandler.HitInfo.transform.gameObject.layer ) ) <= 0 )
            {
                return;
            }

            PlayBounce( mMiddleClickInstrumentIndex, mMiddleClickColor );
        }

        private void OnRightClick()
        {
            if ( mInputHandler.HitInfo.transform == null || ( mWaterLayerMask & ( 1 << mInputHandler.HitInfo.transform.gameObject.layer ) ) <= 0 )
            {
                return;
            }

            PlayBounce( mRightClickInstrumentIndex, mRightClickColor );
        }

        private void OnLeftClick()
        {
            if ( mInputHandler.HitInfo.transform == null || ( mWaterLayerMask & ( 1 << mInputHandler.HitInfo.transform.gameObject.layer ) ) <= 0 )
            {
                return;
            }

            PlayBounce( mLeftClickInstrumentIndex, mLeftClickColor );
        }

        private void PlayBounce( int instrumentIndex, Color color )
        {
            if ( mBounceSpawnIndex < mBounceSpawn.Count )
            {
                mBounceSpawn[mBounceSpawnIndex].Play( mInputHandler.HitInfo.point, instrumentIndex, color );
            }

            mBounceSpawnIndex++;
            if ( mBounceSpawnIndex >= mBounceSpawn.Count )
            {
                mBounceSpawnIndex = 0;
            }
        }

        private void SpawnBounceClickers( AddressableManager addressableManager )
        {
            // just spawning under the world to avoid any initial glitch
            var spawnPosition = new Vector3( 0, -100, 0 );

            for ( var index = 0; index < mPoolSize; index++ )
            {
                var result = Instantiate( mBallPrefab, spawnPosition, Quaternion.identity, mBounceSpawnParent );
                if ( result == null )
                {
                    return;
                }

                var clickSpawnBounce = result.GetComponent<RhythmInstrumentOrb>();
                clickSpawnBounce.Initialize( mMusicGenerator, 1, mWaterObject );
                mBounceSpawn.Add( clickSpawnBounce );
            }
        }
    }
}
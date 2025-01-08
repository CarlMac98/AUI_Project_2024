using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class Demo : MonoBehaviour
	{
		[SerializeField] private MusicGenerator mMusicGenerator;
		[SerializeField] private DemoParameters mDemoParameters;
		[SerializeField] private Transform[] mSpawnPoints;
		[SerializeField] private DemoOnClick mTempoUp;
		[SerializeField] private DemoOnClick mTempoDown;
		[SerializeField] private DemoOnClick mModeUp;
		[SerializeField] private DemoOnClick mModeDown;
		[SerializeField] private int mBaseTempoDelta = 5;
		[SerializeField] private Collider mDemoBounds;

		private readonly List<DemoSpawn> mSpawns = new List<DemoSpawn>();
		private Transform mTransform;

		private void Awake()
		{
			mTransform = transform;
			mMusicGenerator.Ready.AddListener( OnMusicGeneratorReady );
			mMusicGenerator.NotePlayed += OnNotePlayed;
			Physics.gravity = new Vector3( 0, mDemoParameters.mGravity, 0 );
		}

		private bool OnNotePlayed( object source, NotePlayedArgs args )
		{
			if ( mDemoBounds.bounds.Contains( mSpawns[args.InstrumentIndex].Transform.position ) )
			{
				mSpawns[args.InstrumentIndex].Select();
			}

			return true;
		}

		private void OnMusicGeneratorReady()
		{
			/* if you want to auto play, you can do it here:
			 * you can also set the 'autoplay' toggle on the music generator prefab in the scene.*/
			// mMusicGenerator.Play();

			// However, we want to spawn some objects in the scene first
			StartCoroutine( SpawnObjects() );
		}

		private IEnumerator SpawnObjects()
		{
			foreach ( var instrument in mMusicGenerator.InstrumentSet.Instruments )
			{
				var result = Instantiate( mDemoParameters.mSpawnReference, mSpawnPoints[instrument.InstrumentIndex].position, Quaternion.identity, mTransform );
				
				//Ignore me, not relevant to demo really.
				var color = mDemoParameters.mColorPalette.ColorFields[(int) instrument.InstrumentData.StaffPlayerColor].Color;

				var demoSpawn = result.GetComponent<DemoSpawn>();
				demoSpawn.Initialize( color, mDemoParameters );
				mSpawns.Add( demoSpawn );
			}

			yield return new WaitUntil( () => mSpawns.Count == mMusicGenerator.InstrumentSet.Instruments.Count );
			mMusicGenerator.Play();

			InitializeBlocks();
		}

		private void InitializeBlocks()
		{
			mTempoUp.Initialize( mDemoParameters.mColorPalette.ColorFields[0].Color, mDemoParameters );
			mTempoUp.OnSelected += OnTempoUpSelected;
			mTempoUp.SetValueText( $"{mMusicGenerator.InstrumentSet.Data.Tempo} bpm" );

			mTempoDown.Initialize( mDemoParameters.mColorPalette.ColorFields[1].Color, mDemoParameters );
			mTempoDown.OnSelected += OnTempoDownSelected;

			mModeUp.Initialize( mDemoParameters.mColorPalette.ColorFields[0].Color, mDemoParameters );
			mModeUp.OnSelected += OnModeUpSelected;
			mModeUp.SetValueText( $"{mMusicGenerator.ConfigurationData.Mode}" );

			mModeDown.Initialize( mDemoParameters.mColorPalette.ColorFields[1].Color, mDemoParameters );
			mModeDown.OnSelected += OnModeDownSelected;
		}

		private void OnDestroy()
		{
			mTempoUp.OnSelected -= OnTempoUpSelected;
			mTempoDown.OnSelected -= OnTempoDownSelected;
			mModeUp.OnSelected -= OnModeUpSelected;
			mModeDown.OnSelected -= OnModeDownSelected;

			if ( mMusicGenerator != false )
			{
				mMusicGenerator.Ready.RemoveListener( OnMusicGeneratorReady );
				mMusicGenerator.NotePlayed -= OnNotePlayed;
			}
		}

		private void OnModeUpSelected()
		{
			ChangeMode( 1 );
			mModeUp.SetValueText( $"{mMusicGenerator.ConfigurationData.Mode}" );
		}

		private void OnModeDownSelected()
		{
			ChangeMode( -1 );
			mModeUp.SetValueText( $"{mMusicGenerator.ConfigurationData.Mode}" );
		}

		private void ChangeMode( int modeDelta )
		{
			var currentMode = (int) mMusicGenerator.ConfigurationData.Mode;
			var nextMode = (Mode) MusicConstants.SafeLoop( currentMode + modeDelta, 0, Enum.GetNames( typeof(Mode) ).Length );
			mMusicGenerator.ConfigurationData.Mode = nextMode;
		}

		private void OnTempoUpSelected()
		{
			ChangeTempo( mBaseTempoDelta );
		}

		private void OnTempoDownSelected()
		{
			ChangeTempo( -mBaseTempoDelta );
		}

		private void ChangeTempo( int tempoDelta )
		{
			var tempo = mMusicGenerator.InstrumentSet.Data.Tempo + tempoDelta;
			tempo = mBaseTempoDelta * (int) Math.Round( tempo / mBaseTempoDelta ); //< rounding for demo purposes.
			mMusicGenerator.InstrumentSet.Data.Tempo = tempo;
			mMusicGenerator.InstrumentSet.UpdateTempo();
			mTempoUp.SetValueText( $"{mMusicGenerator.InstrumentSet.Data.Tempo} bpm" );
		}
	}
}

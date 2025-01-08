using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;

#endif //UNITY_EDITOR

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// This mod will duplicate the played notes from one instrument to another.
	/// Recommended to set the destination instrument odds of playing to zero.
	/// </summary>
	public class DuplicateInstrumentMod : GeneratorMod
	{
#if UNITY_EDITOR

		[CustomEditor( typeof( DuplicateInstrumentMod ) )]
		public class DuplicateInstrumentModEditor : Editor
		{
			private Texture mDivider;
			private AnimBool mInstrumentToggle;
			private DuplicateInstrumentMod mTargetMod;
			private Vector2 mDividerSize = new Vector2( 200f, 1f );
			private const string cNoDuplicateString = "No Duplicate";
			private const float cDropdownWidth = 600f;

			private void OnEnable()
			{
				mInstrumentToggle = new AnimBool( true );
				mInstrumentToggle.valueChanged.AddListener( Repaint );
				mDivider = Resources.Load( "ButtonFlat" ) as Texture;
				mTargetMod = (DuplicateInstrumentMod) target;
			}

			public override void OnInspectorGUI()
			{
				if ( mTargetMod == false ||
				     mTargetMod.mMusicGenerator == false ||
				     mTargetMod.enabled == false ||
				     mTargetMod.mMusicGenerator.Mods.Contains( mTargetMod ) == false )
				{
					return;
				}

				mInstrumentToggle.target = EditorGUILayout.ToggleLeft( "Instruments", mInstrumentToggle.target );
				EditorGUILayout.HelpBox( new GUIContent( "This mod can duplicate the notes from one instrument to another." ) );

				var instrumentNames = CreateInstrumentList();

				using ( var group = new EditorGUILayout.FadeGroupScope( mInstrumentToggle.faded ) )
				{
					if ( group.visible )
					{
						for ( var index = 0; index < mTargetMod.mMusicGenerator.InstrumentSet.Instruments.Count; index++ )
						{
							if ( index >= mTargetMod.mDuplicateInstrumentData.Instruments.Count )
							{
								break;
							}

							mTargetMod.mDuplicateInstrumentData.Instruments[index].ToInstrument =
								GetDropdownIndex( index, instrumentNames ) - 1; //-1 because of the 'none' entry
							CreateDivider();
						}
					}
				}

				// Bypassing base, don't want to draw normally
				//base.OnInspectorGUI();
			}

			private void CreateDivider()
			{
				using ( new EditorGUIUtility.IconSizeScope( mDividerSize ) )
				{
					var dividerStyle = new GUIStyle( EditorStyles.label )
					{
						alignment = TextAnchor.MiddleCenter
					};
					GUILayout.Box( mDivider, dividerStyle );
				}
			}

			private int GetDropdownIndex( int index, string[] instrumentNames )
			{
				var instrument = mTargetMod.mMusicGenerator.InstrumentSet.Instruments[index];
				var colorTextStyle =
					new GUIStyle( EditorStyles.label )
					{
						normal =
						{
							textColor = mTargetMod.mUIManager.Colors[(int) instrument.InstrumentData.StaffPlayerColor]
						}
					};
				EditorGUILayout.BeginHorizontal( "box" );
				GUILayout.Label( instrument.InstrumentData.InstrumentType, colorTextStyle );
				var dropdownStyle = new GUIStyle( EditorStyles.popup )
				{
					alignment = TextAnchor.MiddleLeft,
					fixedWidth = cDropdownWidth
				};
				var toIndex = EditorGUILayout.Popup(
					mTargetMod.mDuplicateInstrumentData.Instruments[index].ToInstrument + 1, // +1 because of the 'none' entry
					instrumentNames,
					dropdownStyle );
				GUILayout.EndHorizontal();

				if ( toIndex != 0 && CheckRecursion( index, toIndex - 1 ) )
				{
					Debug.LogError( "Cannot link duplicate instrument, creates recursive loop" );
					toIndex = 0;
				}

				return toIndex;
			}

			private string[] CreateInstrumentList()
			{
				var instrumentNames = new string[mTargetMod.mMusicGenerator.InstrumentSet.Instruments.Count + 1];
				instrumentNames[0] = cNoDuplicateString;
				for ( var index = 1; index < instrumentNames.Length; index++ )
				{
					instrumentNames[index] = $"duplicated by {index} {mTargetMod.mMusicGenerator.InstrumentSet.Instruments[index - 1].InstrumentData.InstrumentType}";
				}

				return instrumentNames;
			}

			private bool CheckRecursion( int fromInstrument, int toInstrument )
			{
				if ( mTargetMod.mDuplicateInstrumentData.Instruments[toInstrument].ToInstrument == fromInstrument )
				{
					return true;
				}

				if ( mTargetMod.mDuplicateInstrumentData.Instruments[toInstrument].ToInstrument == toInstrument )
				{
					return true;
				}

				if ( mTargetMod.mDuplicateInstrumentData.Instruments[toInstrument].ToInstrument < 0 )
				{
					return false;
				}

				if ( toInstrument < mTargetMod.mDuplicateInstrumentData.Instruments.Count - 1 )
				{
					return CheckRecursion( fromInstrument, mTargetMod.mDuplicateInstrumentData.Instruments[toInstrument].ToInstrument );
				}

				return false;
			}
		}
#endif //UNITY_EDITOR

		///<inheritdoc/>
		public override string ModName => "DuplicateInstrumentMod";

		///<inheritdoc/>
		public override string Description => "Copies the played notes from one instrument to another";

		///<inheritdoc/>
		public override void SaveData()
		{
			var configurationName = $"{mMusicGenerator.ConfigurationData.ConfigurationName}_{ModName}";
			var persistentModDataPath = MusicConstants.ConfigurationPersistentModDataPath;
			Debug.Log( $"saving configuration {configurationName} to {persistentModDataPath}" );

			if ( Directory.Exists( persistentModDataPath ) == false )
			{
				Directory.CreateDirectory( persistentModDataPath );
			}

			try
			{
				var path = Path.Combine( persistentModDataPath, $"{configurationName}.txt" );
				File.WriteAllText( path, JsonUtility.ToJson( mDuplicateInstrumentData, prettyPrint: true ) );
				Debug.Log( $"{configurationName} was successfully written to file" );
			}
			catch ( IOException e )
			{
				Debug.Log( $"{configurationName} failed to write to file with exception {e}" );
			}
		}

		///<inheritdoc/>
		public override void LoadData()
		{
			var configurationName = $"{mMusicGenerator.ConfigurationData.ConfigurationName}_{ModName}";
			ConfigurationData.LoadModData<Data>( configurationName,
				( data ) => { mDuplicateInstrumentData = data; } );
		}

		///<inheritdoc/>
		public override void EnableMod( MusicGenerator generator )
		{
			mMusicGenerator = generator;
			mUIManager = FindObjectOfType<UIManager>();
			mDuplicateInstrumentData.Instruments.Clear();
			foreach ( var instrument in mMusicGenerator.InstrumentSet.Instruments )
			{
				mDuplicateInstrumentData.Instruments.Add( new DuplicateInstrumentData( instrument.InstrumentIndex, -1 ) );
			}

			enabled = true;
		}

		///<inheritdoc/>
		public override void DisableMod()
		{
			enabled = false;
		}

		/// <summary>
		/// Reference to our music generator
		/// </summary>
		private MusicGenerator mMusicGenerator;

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Our data for this mod
		/// </summary>
		[SerializeField]
		private Data mDuplicateInstrumentData = new Data();

		[Serializable]
		private class DuplicateInstrumentData
		{
			public DuplicateInstrumentData( int fromInstrument, int toInstrument )
			{
				FromInstrument = fromInstrument;
				ToInstrument = toInstrument;
			}

			public void SetToInstrumentIndex( int index )
			{
				ToInstrument = index;
			}

			[SerializeField, UsedImplicitly]
			public int FromInstrument;

			[SerializeField]
			public int ToInstrument;
		}

		[Serializable]
		private class Data
		{
			public List<DuplicateInstrumentData> Instruments = new List<DuplicateInstrumentData>();
		}

		/// <summary>
		/// Invoked when an instrument is added
		/// </summary>
		/// <param name="instrument"></param>
		private void InstrumentAdded( Instrument instrument )
		{
			mDuplicateInstrumentData.Instruments.Add( new DuplicateInstrumentData( -1, -1 ) );
		}

		/// <summary>
		/// Invoked when an instrument is removed
		/// </summary>
		/// <param name="instrument"></param>
		private void InstrumentWillBeRemoved( Instrument instrument )
		{
			mDuplicateInstrumentData.Instruments.RemoveAt( instrument.InstrumentIndex );
			foreach ( var instrumentData in mDuplicateInstrumentData.Instruments )
			{
				if ( instrumentData.ToInstrument == instrument.InstrumentIndex )
				{
					instrumentData.ToInstrument = -1;
				}

				if ( instrumentData.ToInstrument > instrument.InstrumentIndex )
				{
					instrumentData.ToInstrument -= 1;
				}
			}
		}

		/// <summary>
		/// Invoked when an instrument is removed
		/// </summary>
		private void InstrumentWasRemoved()
		{
			// we don't actually do anything for this mod here, just giving an example. Though, You'll usually want to dirty the uiManager at this point.
		}

		/// <summary>
		/// Invoked when this mod is enabled
		/// </summary>
		private void OnEnable()
		{
			if ( mMusicGenerator == null )
			{
				return;
			}

			if ( mMusicGenerator.ConfigurationData.Mods.Contains( ModName ) == false )
			{
				mMusicGenerator.ConfigurationData.Mods.Add( ModName );
			}

			mMusicGenerator.AudioClipPlayed.AddListener( OnClipPlayed );
			mMusicGenerator.InstrumentAdded.AddListener( InstrumentAdded );
			mMusicGenerator.InstrumentWillBeRemoved.AddListener( InstrumentWillBeRemoved );
			mMusicGenerator.InstrumentWasRemoved.AddListener( InstrumentWasRemoved );
		}

		private void OnClipPlayed( InstrumentSet set, float volume, int note, int instrumentIndex )
		{
			if ( mDuplicateInstrumentData.Instruments[instrumentIndex].ToInstrument >= 0 )
			{
				var toInstrumentIndex = mDuplicateInstrumentData.Instruments[instrumentIndex].ToInstrument;
				var instrument = mMusicGenerator.InstrumentSet.Instruments[toInstrumentIndex];
				note = instrument.InstrumentData.IsPercussion ? 0 : note;
				mMusicGenerator.PlayNote( set,
					instrument.InstrumentData.Volume,
					instrument.InstrumentData.InstrumentType,
					note,
					instrument.InstrumentIndex );
			}
		}

		/// <summary>
		/// Invoked when this mod is disabled
		/// </summary>
		private void OnDisable()
		{
			if ( mMusicGenerator == null )
			{
				return;
			}

			mMusicGenerator.ConfigurationData.Mods.Remove( ModName );

			mMusicGenerator.AudioClipPlayed.RemoveListener( OnClipPlayed );
			mMusicGenerator.InstrumentAdded.RemoveListener( InstrumentAdded );
			mMusicGenerator.InstrumentWillBeRemoved.RemoveListener( InstrumentWillBeRemoved );
			mMusicGenerator.InstrumentWasRemoved.RemoveListener( InstrumentWasRemoved );
		}
	}
}

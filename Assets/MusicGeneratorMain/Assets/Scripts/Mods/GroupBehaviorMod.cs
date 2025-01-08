using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;

#endif //UNITY_EDITOR

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class GroupBehaviorMod : GeneratorMod
	{
#if UNITY_EDITOR

		[CustomEditor( typeof( GroupBehaviorMod ) )]
		public class GroupBehaviorModEditor : Editor
		{
			private Texture mDivider;
			private AnimBool mInstrumentToggle;
			private GroupBehaviorMod mTargetMod;
			private Vector2 mDividerSize = new Vector2( 200f, 1f );
			private const float cDropdownWidth = 600f;

			private void OnEnable()
			{
				mInstrumentToggle = new AnimBool( true );
				mInstrumentToggle.valueChanged.AddListener( Repaint );
				mDivider = Resources.Load( "ButtonFlat" ) as Texture;
				mTargetMod = (GroupBehaviorMod) target;
			}

			public override void OnInspectorGUI()
			{
				EditorGUILayout.HelpBox( new GUIContent(
					"This mod can enforce behavior for which groups are playing. \n There is no sanity-checking of this, so you can set conflicting data. \n Rules are enforced top to bottom" ) );

				if ( mTargetMod == false ||
				     mTargetMod.mMusicGenerator == false ||
				     mTargetMod.enabled == false ||
				     mTargetMod.mMusicGenerator.Mods.Contains( mTargetMod ) == false )
				{
					return;
				}

				mInstrumentToggle.target = EditorGUILayout.ToggleLeft( "Groups", mInstrumentToggle.target );

				var groupBehaviorNames = GetGroupBehaviorList();

				using ( var group = new EditorGUILayout.FadeGroupScope( mInstrumentToggle.faded ) )
				{
					if ( group.visible )
					{
						for ( var index = 0; index < MusicConstants.NumGroups; index++ )
						{
							if ( index >= mTargetMod.mGroupBehaviorData.Groups.Count )
							{
								break;
							}

							EditorGUILayout.BeginHorizontal( "box" );
							EditorGUILayout.LabelField( $"If Group {index + 1}:" );
							var dropdownStyle = new GUIStyle( EditorStyles.popup ) { alignment = TextAnchor.MiddleLeft, fixedWidth = cDropdownWidth };
							var groupCondition = EditorGUILayout.Popup(
								(int) mTargetMod.mGroupBehaviorData.Groups[index].GroupState,
								groupBehaviorNames, dropdownStyle );
							mTargetMod.mGroupBehaviorData.Groups[index].GroupState = (GroupBehavior) groupCondition;
							EditorGUILayout.EndHorizontal();
							if ( groupCondition == (int) GroupBehavior.IsDefaultBehavior )
							{
								CreateDivider();
								continue;
							}

							for ( var groupBehaviorIndex = 0; groupBehaviorIndex < MusicConstants.NumGroups; groupBehaviorIndex++ )
							{
								if ( groupBehaviorIndex == mTargetMod.mGroupBehaviorData.Groups[index].GroupIndex )
								{
									continue;
								}

								mTargetMod.mGroupBehaviorData.Groups[index].OtherGroups[groupBehaviorIndex] =
									GetDropdownIndex( index, groupBehaviorIndex, groupBehaviorNames );
							}

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
					var dividerStyle = new GUIStyle( EditorStyles.label ) { alignment = TextAnchor.MiddleCenter };
					GUILayout.Box( mDivider, dividerStyle );
				}
			}

			private GroupBehavior GetDropdownIndex( int index, int otherGroupIndex, string[] instrumentNames )
			{
				EditorGUILayout.BeginHorizontal( "box" );
				GUILayout.Label( $"Then group {otherGroupIndex + 1}" );
				var dropdownStyle = new GUIStyle( EditorStyles.popup ) { alignment = TextAnchor.MiddleLeft, fixedWidth = cDropdownWidth };
				var toIndex = EditorGUILayout.Popup(
					(int) mTargetMod.mGroupBehaviorData.Groups[index].OtherGroups[otherGroupIndex],
					instrumentNames, dropdownStyle );
				GUILayout.EndHorizontal();

				if ( toIndex != 0 && CheckRecursion( index, toIndex - 1 ) )
				{
					Debug.LogError( "Cannot link duplicate instrument, creates recursive loop" );
					toIndex = 0;
				}

				return (GroupBehavior) toIndex;
			}

			private string[] GetGroupBehaviorList()
			{
				var groupBehaviorLength = Enum.GetNames( typeof( GroupBehavior ) ).Length;
				var groupBehaviorNames = new string[groupBehaviorLength];
				for ( var index = 0; index < groupBehaviorLength; index++ )
				{
					groupBehaviorNames[index] = $"{(GroupBehavior) index}";
				}

				return groupBehaviorNames;
			}

			private bool CheckRecursion( int fromInstrument, int toInstrument )
			{
				return false;
			}
		}
#endif //UNITY_EDITOR

		///<inheritdoc/>
		public override string ModName => "GroupBehaviorMod";

		///<inheritdoc/>
		public override string Description => "Sets behavior for groups based on which are playing";

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
				File.WriteAllText( path, JsonUtility.ToJson( mGroupBehaviorData, prettyPrint: true ) );
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
				( data ) => { mGroupBehaviorData = data; } );
		}

		///<inheritdoc/>
		public override void EnableMod( MusicGenerator generator )
		{
			mMusicGenerator = generator;
			mUIManager = FindObjectOfType<UIManager>();
			mGroupBehaviorData.Groups.Clear();
			for ( var index = 0; index < MusicConstants.NumGroups; index++ )
			{
				mGroupBehaviorData.Groups.Add( new GroupBehaviorData( index ) );
			}

			enabled = true;
		}

		///<inheritdoc/>
		public override void DisableMod()
		{
			enabled = false;
		}

		/// <summary>
		/// Available time signatures.
		/// </summary>
		[Serializable] public enum GroupBehavior
		{
			IsDefaultBehavior = 0,
			IsPlaying = 1,
			IsNotPlaying = 2
		}

		[Serializable]
		private class GroupBehaviorData
		{
			public GroupBehaviorData( int groupIndex )
			{
				GroupIndex = groupIndex;
				OtherGroups = new GroupBehavior[4];
			}

			[SerializeField]
			public int GroupIndex;

			public GroupBehavior GroupState;

			[SerializeField]
			public GroupBehavior[] OtherGroups;
		}

		[Serializable]
		private class Data
		{
			public List<GroupBehaviorData> Groups = new List<GroupBehaviorData>();
		}

		/// <summary>
		/// Our data for this mod
		/// </summary>
		[SerializeField]
		private Data mGroupBehaviorData = new Data();

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Reference to our music generator
		/// </summary>
		private MusicGenerator mMusicGenerator;

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

			mMusicGenerator.GroupsWereChosen.AddListener( OnGroupsChosen );
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

			mMusicGenerator.GroupsWereChosen.RemoveListener( OnGroupsChosen );
		}

		private void OnGroupsChosen()
		{
			// forcing guaranteed group behavior:
			var noGroupsPlaying = true;
			foreach ( var groupIsPlaying in mMusicGenerator.InstrumentSet.GroupIsPlaying )
			{
				if ( groupIsPlaying )
				{
					noGroupsPlaying = false;
					break;
				}
			}

			if ( noGroupsPlaying )
			{
				mMusicGenerator.InstrumentSet.GroupIsPlaying[Random.Range( 0, 3 )] = true;
			}

			// 
			for ( var index = 0; index < mGroupBehaviorData.Groups.Count; index++ )
			{
				var isPlaying = mMusicGenerator.InstrumentSet.GroupIsPlaying[index];
				var matchesStateCondition = ( mGroupBehaviorData.Groups[index].GroupState == GroupBehavior.IsPlaying && isPlaying ) ||
				                            ( mGroupBehaviorData.Groups[index].GroupState == GroupBehavior.IsNotPlaying && isPlaying == false );
				if ( matchesStateCondition )
				{
					for ( var otherIndex = 0; otherIndex < mGroupBehaviorData.Groups[index].OtherGroups.Length; otherIndex++ )
					{
						if ( otherIndex == index )
						{
							continue;
						}

						var groupState = mGroupBehaviorData.Groups[index].OtherGroups[otherIndex];
						if ( groupState == GroupBehavior.IsPlaying )
						{
							mMusicGenerator.InstrumentSet.GroupIsPlaying[otherIndex] = true;
						}
						else if ( groupState == GroupBehavior.IsNotPlaying )
						{
							mMusicGenerator.InstrumentSet.GroupIsPlaying[otherIndex] = false;
						}
					}
				}
			}

			if ( mUIManager )
			{
				mUIManager.DirtyEditorDisplays();
			}
		}
	}
}

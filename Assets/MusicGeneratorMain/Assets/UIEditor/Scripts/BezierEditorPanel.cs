using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ProcGenMusic
{
	public class BezierEditorPanel : MonoBehaviour
	{
		public Instrument Instrument => mInstrument;

		public BezierType BezierEditorType { get; private set; }

		public enum BezierType
		{
			None = 0,
			Waveform = 1,
			Envelope = 2,
			Panvelope = 3,
		}

		public void InitializeNewSynth( Instrument instrument )
		{
			var cachedType = BezierEditorType;
			mInstrument = instrument;
			mInstrumentIsValid = mInstrument != null && mInstrument.InstrumentData.IsSynth && mInstrument.InstrumentData.IsCustomWaveform;

			SetBezierType( BezierType.Waveform );
			SpawnFromData();
			OnDataUpdated.Invoke();

			SetBezierType( BezierType.Envelope );
			SpawnFromData();
			OnDataUpdated.Invoke();

			SetBezierType( BezierType.Panvelope );
			SpawnFromData();
			OnDataUpdated.Invoke();

			SetBezierType( cachedType );
		}

		public void SetPanelActive( bool isActive )
		{
			mIsActive = isActive;
			if ( isActive == false )
			{
				mCurrentControlData.Clear();
				foreach ( var bezierControl in mBezierPool )
				{
					bezierControl.SetInactive();
				}

				mBezierControls.Clear();
			}

			mVisibleObject.SetActive( isActive ); // && mInstrumentIsValid );
		}

		public void SetBezierType( BezierType bezierType )
		{
			mCurrentControlData.Clear();

			switch ( bezierType )
			{
				case BezierType.Envelope:
					mCurrentDefaultControls = mDefaultEnvelopeControls;
					if ( mInstrument != null )
					{
						mCurrentControlData.Clear();
						foreach ( var data in mInstrument.InstrumentData.EnvelopeData )
						{
							mCurrentControlData.Add( data );
						}
					}

					mCeilingText.SetText( "1" );
					mFloorText.SetText( "0" );
					break;
				case BezierType.Waveform:
					mCurrentDefaultControls = mDefaultWaveformControls;
					if ( mInstrument != null )
					{
						mCurrentControlData.Clear();
						foreach ( var data in mInstrument.InstrumentData.WaveformData )
						{
							mCurrentControlData.Add( data );
						}
					}

					mCeilingText.SetText( "1" );
					mFloorText.SetText( "-1" );
					break;
				case BezierType.Panvelope:
					mCurrentDefaultControls = mDefaultPanvelopeControls;
					if ( mInstrument != null )
					{
						mCurrentControlData.Clear();
						foreach ( var data in mInstrument.InstrumentData.PanvelopeData )
						{
							mCurrentControlData.Add( data );
						}
					}

					mCeilingText.SetText( "R" );
					mFloorText.SetText( "L" );
					break;
			}

			BezierEditorType = bezierType;
		}

		public GameObject VisibleObject => mVisibleObject;
		public IEnumerable<BezierControl> BezierControls => mBezierControls;
		public LineRenderer LineRenderer => mLineRenderer;
		public Action OnDataUpdated;
		public float Ceiling => mCeiling.position.y;
		public float Floor => mFloor.position.y;

		public void UpdateUIElements( Instrument instrument )
		{
			if ( mIsActive == false )
			{
				return;
			}

			if ( instrument == null )
			{
				mVisibleObject.SetActive( false );
				SetVisibility();
				return;
			}

			if ( instrument.InstrumentData.IsCustomWaveform &&
			     instrument.InstrumentData.IsSynth )
			{
				UpdateModal( mInstrumentIsValid == false );
				mInstrument = instrument;
				mInstrumentIsValid = instrument.InstrumentData.IsSynth && instrument.InstrumentData.IsCustomWaveform;
				mVisibleObject.SetActive( mInstrumentIsValid );
				SetBezierType( BezierEditorType );
				SpawnFromData();
			}
			else
			{
				mInstrument = instrument;
				SetVisibility();
			}
		}

		[SerializeField]
		private Animator mSynthNotSelectedDisplay;

		[SerializeField]
		private int mBezierPoolSize = 25;

		[SerializeField]
		private string mSelectSynthBoolName = "IsVisible";

		[SerializeField]
		private GameObject mVisibleObject;

		[SerializeField]
		private LineRenderer mLineRenderer;

		[SerializeField]
		private Collider mBackgroundCollider;

		[SerializeField]
		private InputHandler mInputHandler;

		[SerializeField]
		private Transform mControlPointsParent;

		[SerializeField]
		private BezierControl mBezierControlPrefab;

		[SerializeField]
		private List<BezierControl> mBezierControls = new List<BezierControl>();

		[SerializeField]
		private Transform mCeiling;

		[SerializeField]
		private Transform mFloor;

		[SerializeField]
		private TMP_Text mCeilingText;

		[SerializeField]
		private TMP_Text mFloorText;

		[SerializeField]
		private List<BezierControl> mDefaultWaveformControls;

		[SerializeField]
		private List<BezierControl> mDefaultEnvelopeControls;

		[SerializeField]
		private List<BezierControl> mDefaultPanvelopeControls;

		[SerializeField]
		private ParticleSystem mDestroyParticles;

		[SerializeField]
		private Transform mDestroyParticlesParentTransform;

		private bool mIsDirty;
		private readonly List<Vector3> mSortedPoints = new List<Vector3>();
		private int mCurveCount;
		private const int SEGMENT_COUNT = 1000;
		private Instrument mInstrument;
		private bool mInstrumentIsValid;
		private bool mIsActive;
		private Coroutine mSpawnRoutine;
		private List<BezierControl> mCurrentDefaultControls;
		private List<BezierControlData> mCurrentControlData = new List<BezierControlData>();
		private int mSetSynthModalAnimID;
		private readonly List<BezierControl> mBezierPool = new List<BezierControl>();

		private void Awake()
		{
			CreateBezierPool();
			mSetSynthModalAnimID = Animator.StringToHash( mSelectSynthBoolName );
			mInputHandler.DoubleLeftClick.AddListener( OnDoubleClick );
			mInputHandler.RightClickDown.AddListener( OnRightClick );
			mInputHandler.LeftClickUp.AddListener( OnLeftClickUp );
		}

		private void CreateBezierPool()
		{
			if ( mBezierControlPrefab == null )
			{
				return;
			}

			for ( var index = 0; index < mBezierPoolSize; index++ )
			{
				var bezierControl = Instantiate( mBezierControlPrefab, Vector3.zero, Quaternion.identity, mControlPointsParent );
				bezierControl.gameObject.SetActive( false );
				mBezierPool.Add( bezierControl );
			}
		}

		private void OnLeftClickUp()
		{
			if ( mIsDirty )
			{
				SetData();
				mIsDirty = false;
			}
		}

		private void OnRightClick()
		{
			var hitTransform = mInputHandler.HitInfo.transform;
			BezierControl toRemove = null;
			if ( hitTransform )
			{
				foreach ( var bezierControl in mBezierControls )
				{
					var didHitMainPoint = bezierControl.MainControl.transform == hitTransform;
					var didHitInPoint = bezierControl.InControl != null && bezierControl.InControl.transform == hitTransform;
					var didHitOutPoint = bezierControl.OutControl != null && bezierControl.OutControl.transform == hitTransform;

					var didHitTransform = didHitMainPoint ||
					                      didHitInPoint ||
					                      didHitOutPoint;

					if ( didHitTransform &&
					     bezierControl.IsEndcap == false )
					{
						toRemove = bezierControl;
						break;
					}
				}
			}

			if ( toRemove == null )
			{
				return;
			}

			mDestroyParticlesParentTransform.position = toRemove.MainControl.transform.position;
			mDestroyParticles.Play();
			toRemove.SetInactive();
			mBezierControls.Remove( toRemove );
			SortBeziers();
			UpdateSiblings();
			UpdatePositions();
			DrawCurve();
			SetData();
		}

		private void Update()
		{
			if ( mVisibleObject.activeInHierarchy == false ||
			     mInstrument == null ||
			     mInstrument.InstrumentData.IsSynth == false ||
			     mSpawnRoutine != null )
			{
				return;
			}

			if ( mCurrentControlData == null || mCurrentControlData.Count == 0 )
			{
				return;
			}

			if ( mBezierControls.Any( bezierControl => bezierControl.IsDirty ) == false )
			{
				return;
			}

			mIsDirty = true;
			UpdatePositions();
			DrawCurve();

			foreach ( var bezierControl in mBezierControls )
			{
				bezierControl.Clean();
			}
		}

		private void UpdatePositions()
		{
			mSortedPoints.Clear();
			foreach ( var control in mBezierControls )
			{
				if ( control.InControl != null && control.IsStartPoint == false )
				{
					mSortedPoints.Add( control.InControl.Transform.position );
				}

				if ( control.MainControl != null && control.MainControl.gameObject.activeSelf )
				{
					mSortedPoints.Add( control.MainControl.transform.position );
				}

				if ( control.OutControl != null && control.IsEndPoint == false )
				{
					mSortedPoints.Add( control.OutControl.transform.position );
				}
			}

			mCurveCount = mSortedPoints.Count / 3;
		}

		private void OnDoubleClick()
		{
			if ( mInputHandler.HitInfo.transform == null ||
			     mInputHandler.HitInfo.transform != mBackgroundCollider.transform )
			{
				return;
			}

			var unusedBezierControl = mBezierPool.First( control => control.gameObject.activeSelf == false );

			unusedBezierControl.gameObject.SetActive( true );
			unusedBezierControl.SetEndPoint( false );
			unusedBezierControl.SetStartPoint( false );
			mBezierControls.Add( unusedBezierControl );
			var hitPoint = mInputHandler.HitInfo.point;
			var transform1 = unusedBezierControl.MainControl.transform;
			hitPoint.z = transform1.position.z;
			transform1.position = hitPoint;
			unusedBezierControl.InControl.transform.position = mInputHandler.HitInfo.point + new Vector3( -50, 0, 0 );
			unusedBezierControl.OutControl.transform.position = mInputHandler.HitInfo.point + new Vector3( 50, 0, 0 );

			SortBeziers();
			UpdatePositions();
			UpdateSiblings();
			//poolObject.transform.position = Vector3.zero;

			unusedBezierControl.Initialize( mInputHandler, mCeiling.position, mFloor.position, BezierEditorType );
			unusedBezierControl.UpdateControl( unusedBezierControl.MainControl );
			DrawCurve();
			SetData();

			foreach ( var control in mBezierControls )
			{
				control.UpdateControl( control.MainControl );
			}
		}

		private void SortBeziers()
		{
			var sortedBeziers = from control in mBezierControls orderby control.MainControl.Transform.position.x select control;
			mBezierControls = sortedBeziers.ToList();
		}

		private void UpdateSiblings()
		{
			if ( mBezierControls.Count < 2 )
			{
				return;
			}

			for ( var index = 0; index < mBezierControls.Count; index++ )
			{
				if ( index == 0 )
				{
					mBezierControls[index].UpdateSiblings( null, mBezierControls[index + 1] );
				}
				else if ( index == mBezierControls.Count - 1 )
				{
					mBezierControls[index].UpdateSiblings( mBezierControls[index - 1], null );
				}
				else
				{
					mBezierControls[index].UpdateSiblings( mBezierControls[index - 1], mBezierControls[index + 1] );
				}
			}
		}

		private void DrawCurve()
		{
			if ( mCurveCount <= 0 )
			{
				return;
			}

			var curveSegmentCount = SEGMENT_COUNT / mCurveCount;
			for ( var curveIndex = 0; curveIndex < mCurveCount; curveIndex++ )
			{
				for ( var segmentIndex = 1; segmentIndex <= curveSegmentCount; segmentIndex++ )
				{
					var t = segmentIndex / ( float )curveSegmentCount;
					var nodeIndex = curveIndex * 3;
					var pixel = CalculateCubicBezierPoint( t,
						mSortedPoints[nodeIndex],
						mSortedPoints[nodeIndex + 1],
						mSortedPoints[nodeIndex + 2],
						mSortedPoints[nodeIndex + 3] );
					mLineRenderer.positionCount = ( ( curveIndex * curveSegmentCount ) + segmentIndex );
					mLineRenderer.SetPosition( ( curveIndex * curveSegmentCount ) + ( segmentIndex - 1 ), pixel );
				}
			}
		}

		private static Vector3 CalculateCubicBezierPoint( float index, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3 )
		{
			var x = 1 - index;
			var y = index * index;
			var z = x * x;
			var w = z * x;
			var v = y * index;

			var p = w * p0;
			p += 3 * z * index * p1;
			p += 3 * x * y * p2;
			p += v * p3;

			return p;
		}

		private void CreateBeziers( Action onComplete )
		{
			ClearBeziers();

			foreach ( var controlData in mCurrentControlData )
			{
				foreach ( var poolObject in mBezierPool )
				{
					if ( poolObject.gameObject.activeSelf == false )
					{
						poolObject.MainControl.transform.position = controlData.MainControlPoint;

						var isStartPoint = controlData.IsStartPoint;
						poolObject.SetStartPoint( isStartPoint );
						if ( isStartPoint == false )
						{
							poolObject.InControl.transform.position = controlData.InControlPoint;
						}

						var isEndPoint = controlData.IsEndPoint;
						poolObject.SetEndPoint( isEndPoint );
						if ( isEndPoint == false )
						{
							poolObject.OutControl.transform.position = controlData.OutControlPoint;
						}

						poolObject.Initialize( mInputHandler, mCeiling.position, mFloor.position, BezierEditorType );
						mBezierControls.Add( poolObject );
						break;
					}
				}
			}

			SortBeziers();
			mBezierControls[0].SetEndcapSibling( mBezierControls[mBezierControls.Count - 1] );
			mBezierControls[mBezierControls.Count - 1].SetEndcapSibling( mBezierControls[0] );
			UpdatePositions();
			UpdateSiblings();
			foreach ( var control in mBezierControls )
			{
				control.Initialize( mInputHandler, mCeiling.position, mFloor.position, BezierEditorType );
			}

			mCurveCount = mSortedPoints.Count / 3;
			DrawCurve();
			onComplete.Invoke();
			mSpawnRoutine = null;
		}

		private void ClearBeziers()
		{
			foreach ( var bezierControl in mBezierControls )
			{
				bezierControl.SetInactive();
			}

			mBezierControls.Clear();
		}

		private void SetData()
		{
			if ( mIsActive == false || mInstrumentIsValid == false )
			{
				return;
			}

			OnDataUpdated.Invoke();
		}

		private void CheckWaveformDefaults()
		{
			if ( mCurrentControlData != null &&
			     mCurrentControlData.Count != 0 )
			{
				return;
			}

			mCurrentControlData = new List<BezierControlData>();
			foreach ( var bezierControl in mCurrentDefaultControls )
			{
				mCurrentControlData.Add( new BezierControlData()
				{
					MainControlPoint = bezierControl.MainControl.transform.position,
					InControlPoint = bezierControl.IsStartPoint ? Vector3.zero : bezierControl.InControl.transform.position,
					OutControlPoint = bezierControl.IsEndPoint ? Vector3.zero : bezierControl.OutControl.transform.position,
					IsStartPoint = bezierControl.IsStartPoint,
					IsEndPoint = bezierControl.IsEndPoint,
					BezierType = bezierControl.BezierType
				} );
			}
		}

		private void SetVisibility()
		{
			mInstrumentIsValid = mInstrument != null && mInstrument.InstrumentData.IsSynth && mInstrument.InstrumentData.IsCustomWaveform;
			var isActive = mInstrumentIsValid && mIsActive;
			foreach ( var bezierControl in mBezierControls )
			{
				bezierControl.gameObject.SetActive( isActive );
			}

			mLineRenderer.gameObject.SetActive( isActive );

			if ( mIsActive )
			{
				UpdateModal( isActive == false );
			}

			if ( isActive == false )
			{
				return;
			}

			SortBeziers();
			UpdatePositions();
			UpdateSiblings();
			mCurveCount = mSortedPoints.Count / 3;
			SetData();
		}

		private void UpdateModal( bool isVisible )
		{
			mSynthNotSelectedDisplay.SetBool( mSetSynthModalAnimID, isVisible );
		}

		private void SpawnFromData()
		{
			if ( mInstrumentIsValid == false )
			{
				return;
			}

			CheckWaveformDefaults();
			CreateBeziers( SetVisibility );
		}
	}
}
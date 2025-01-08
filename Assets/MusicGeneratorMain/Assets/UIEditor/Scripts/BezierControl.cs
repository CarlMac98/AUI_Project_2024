using UnityEngine;

namespace ProcGenMusic
{
	public class BezierControl : MonoBehaviour
	{
		public bool IsDirty { get; private set; }
		public BezierControlPoint MainControl => mMainPoint;
		public BezierControlPoint InControl => mInPoint;
		public BezierControlPoint OutControl => mOutPoint;
		public bool IsEndcap => mIsStartPoint || mIsEndPoint;
		public bool IsStartPoint => mIsStartPoint;
		public bool IsEndPoint => mIsEndPoint;
		public BezierControl mEndcapSibling;
		public BezierEditorPanel.BezierType BezierType => mBezierType;

		public void SetEndcapSibling( BezierControl endcapSibling )
		{
			mEndcapSibling = endcapSibling;
		}

		public BezierControlData GetData()
		{
			UpdateData();
			return mData;
		}

		public void SetStartPoint( bool isStartPoint )
		{
			if ( mInPoint == null )
			{
				return;
			}

			mInPoint.gameObject.SetActive( isStartPoint == false ); // = null;
			mInLine.gameObject.SetActive( isStartPoint == false );
			mIsStartPoint = isStartPoint;
		}

		public void UpdateControl( BezierControlPoint point )
		{
			mCurrentBezierControlPoint = point;
			var position = ClampControl( point.transform.position );
			UpdateMainPoint( position );
			mCurrentBezierControlPoint = null;
		}
		
		public void SetEndPoint( bool isEndPoint )
		{
			if ( mOutPoint == null )
			{
				return;
			}

			mOutPoint.gameObject.SetActive( isEndPoint == false ); // = null;
			mOutLine.gameObject.SetActive( isEndPoint == false );
			mIsEndPoint = isEndPoint;
		}

		public void Clean()
		{
			IsDirty = false;
		}

		public void SetInactive()
		{
			mIsEndPoint = false;
			mIsStartPoint = false;
			gameObject.SetActive( false );
			mEndcapSibling = null;
		}

		public void Initialize( InputHandler inputHandler, Vector3 ceiling, Vector3 floor, BezierEditorPanel.BezierType bezierType )
		{
			mBezierType = bezierType;
			mCeiling = ceiling;
			mFloor = floor;
			if ( mInputHandler == null )
			{
				mInputHandler = inputHandler;
				mInputHandler.LeftClickDown.AddListener( OnLeftClick );
				mInputHandler.LeftClickUp.AddListener( OnLeftClickUp );
			}

			IsDirty = true;
			if ( IsStartPoint )
			{
				mInLine.gameObject.SetActive( false );
			}

			if ( IsEndPoint )
			{
				mOutLine.gameObject.SetActive( false );
			}

			gameObject.SetActive( true );

			mCurrentBezierControlPoint = mMainPoint;
			//var position = mMainPoint.Transform.position;
			//position = ClampControl( position );
			//UpdateMainPoint( position );
			//mCurrentBezierControlPoint = null;
			UpdateLines();
			UpdateData();
			mCurrentBezierControlPoint = null;
		}

		public void UpdateSiblings( BezierControl inSibling, BezierControl outSibling )
		{
			mInSibling = inSibling;
			mOutSibling = outSibling;
		}

		[SerializeField]
		private BezierControlPoint mInPoint;

		[SerializeField]
		private BezierControlPoint mMainPoint;

		[SerializeField]
		private BezierControlPoint mOutPoint;

		[SerializeField]
		private LineRenderer mInLine;

		[SerializeField]
		private LineRenderer mOutLine;

		[SerializeField]
		private InputHandler mInputHandler;

		[SerializeField]
		private bool mIsStartPoint;

		[SerializeField]
		private bool mIsEndPoint;

		private BezierControlPoint mCurrentBezierControlPoint;
		private BezierControl mInSibling;
		private BezierControl mOutSibling;
		private Vector3 mCeiling;
		private Vector3 mFloor;
		private BezierControlData mData;
		private BezierEditorPanel.BezierType mBezierType;

		private void OnLeftClick()
		{
			if ( mInputHandler.HitInfo.transform != null )
			{
				var hitTransform = mInputHandler.HitInfo.transform;
				if ( mIsStartPoint == false && mInPoint != null && hitTransform == mInPoint.Transform )
				{
					mCurrentBezierControlPoint = mInPoint;
				}
				else if ( mMainPoint != null && hitTransform == mMainPoint.transform )
				{
					if ( IsEndcap && mBezierType == BezierEditorPanel.BezierType.Envelope )
					{
						return;
					}

					mCurrentBezierControlPoint = mMainPoint;
				}
				else if ( mIsEndPoint == false && mOutPoint != null && hitTransform == mOutPoint.transform )
				{
					mCurrentBezierControlPoint = mOutPoint;
				}
				else
				{
					mCurrentBezierControlPoint = null;
				}
			}
		}

		private void OnLeftClickUp()
		{
			mCurrentBezierControlPoint = null;
		}

		private void Update()
		{
			if ( mCurrentBezierControlPoint == null )
			{
				return;
			}

			var pos = mInputHandler.MouseWorldPos;
			pos.z = mCurrentBezierControlPoint.Transform.position.z;
			if ( IsEndcap && mCurrentBezierControlPoint.ControlType == BezierControlPoint.BezierControlType.Main )
			{
				pos.x = mMainPoint.Transform.position.x;
			}

			pos = ClampControl( pos );

			switch ( mCurrentBezierControlPoint.ControlType )
			{
				case BezierControlPoint.BezierControlType.Main:
					UpdateMainPoint( pos );
					break;
				case BezierControlPoint.BezierControlType.In:
					pos.x = Mathf.Min( pos.x, mMainPoint.transform.position.x );
					break;
				case BezierControlPoint.BezierControlType.Out:
					pos.x = Mathf.Max( pos.x, mMainPoint.transform.position.x );
					break;
			}

			pos.y = Mathf.Clamp( pos.y, mFloor.y, mCeiling.y );

			UpdateLines();
			mCurrentBezierControlPoint.Transform.position = pos;
			UpdateData();
			IsDirty = true;
		}

		private void UpdateSibling( Vector3 position )
		{
			mCurrentBezierControlPoint = MainControl;
			position = ClampControl( position );
			UpdateMainPoint( position, false );
			mCurrentBezierControlPoint = null;
		}

		private void UpdateMainPoint( Vector3 pos, bool updateSibling = true )
		{
			var delta = pos - mMainPoint.Transform.position;
			if ( mIsStartPoint == false && mInPoint != null )
			{
				var inPoint = mInPoint.Transform.position + delta;
				if ( mInSibling != null )
				{
					inPoint.x = Mathf.Max( mInSibling.MainControl.Transform.position.x + 1, inPoint.x );
					inPoint.y = Mathf.Clamp( inPoint.y, mFloor.y, mCeiling.y );
				}

				mInPoint.Transform.position = inPoint;
			}

			if ( mIsEndPoint == false && mOutPoint != null )
			{
				var outPoint = mOutPoint.Transform.position + delta;
				if ( mOutSibling != null )
				{
					outPoint.x = Mathf.Min( mOutSibling.MainControl.Transform.position.x - 1, outPoint.x );
					outPoint.y = Mathf.Clamp( outPoint.y, mFloor.y, mCeiling.y );
				}

				mOutPoint.Transform.position = outPoint;
			}

			if ( IsEndcap && updateSibling && mCurrentBezierControlPoint.ControlType == BezierControlPoint.BezierControlType.Main )
			{
				var siblingTransform = mEndcapSibling.MainControl.transform;
				var siblingPos = siblingTransform.position;
				siblingPos.y = pos.y;
				mEndcapSibling.UpdateSibling( siblingPos );
			}

			mMainPoint.transform.position = pos;
			UpdateLines();
		}

		private Vector3 ClampControl( Vector3 pos )
		{
			switch ( mCurrentBezierControlPoint.ControlType )
			{
				case BezierControlPoint.BezierControlType.Main:
					if ( mInSibling != null )
					{
						pos.x = Mathf.Max( mInSibling.OutControl.Transform.position.x + 1f, pos.x );
					}

					if ( mOutSibling != null )
					{
						pos.x = Mathf.Min( mOutSibling.InControl.Transform.position.x - 1f, pos.x );
					}

					break;
				case BezierControlPoint.BezierControlType.In:
				case BezierControlPoint.BezierControlType.Out:
					if ( mInSibling != null )
					{
						pos.x = Mathf.Max( mInSibling.MainControl.Transform.position.x + 1f, pos.x );
					}

					if ( mOutSibling != null )
					{
						pos.x = Mathf.Min( mOutSibling.MainControl.Transform.position.x - 1f, pos.x );
					}

					break;
			}

			pos.y = Mathf.Clamp( pos.y, mFloor.y, mCeiling.y );
			return pos;
		}

		private void UpdateLines()
		{
			var mainPos = mMainPoint.transform.position;
			if ( mIsStartPoint == false )
			{
				mInLine.SetPosition( 0, mainPos );
				mInLine.SetPosition( 1, mInPoint.Transform.position );
			}

			if ( mIsEndPoint == false )
			{
				mOutLine.SetPosition( 0, mOutPoint.Transform.position );
				mOutLine.SetPosition( 1, mainPos );
			}
		}

		private void UpdateData()
		{
			mData.InControlPoint = mIsStartPoint ? Vector3.zero : mInPoint.Transform.position;
			mData.MainControlPoint = mMainPoint.Transform.position;
			mData.OutControlPoint = IsEndPoint ? Vector3.zero : mOutPoint.Transform.position;
			mData.IsStartPoint = mIsStartPoint;
			mData.IsEndPoint = mIsEndPoint;
			mData.BezierType = mBezierType;
		}
	}
}
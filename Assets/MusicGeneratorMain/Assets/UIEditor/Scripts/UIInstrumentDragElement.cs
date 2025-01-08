using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// This handles dragging/parenting ui instrument objects dragging behavior
	/// </summary>
	public class UIInstrumentDragElement : MonoBehaviour,
		IPanelOption<int>,
		IPointerDownHandler,
		IPointerUpHandler
	{
		#region public

		/// <summary>
		/// Initialization of the Instrument Drag element
		/// </summary>
		/// <param name="action"></param>
		/// <param name="uiManager"></param>
		/// <param name="instrumentObject"></param>
		/// <param name="scrollView"></param>
		public void Initialize( UnityAction<int> action,
			UIManager uiManager,
			InstrumentListUIObject instrumentObject,
			UIInstrumentListScrollView scrollView )
		{
			mUIManager = uiManager;
			mInstrumentObject = instrumentObject;
			mScrollView = scrollView;
			mOnValueChanged.RemoveAllListeners();
			mOnValueChanged.AddListener( action );
			mOption = instrumentObject.Instrument.InstrumentData.Group;
			action.Invoke( mOption );
		}

		/// <summary>
		/// event handler for pointer down
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerDown( PointerEventData eventData )
		{
			mUIManager.InstrumentListPanelUI.StartDragInstrument( mInstrumentObject );
			mIsDragging = true;
		}

		/// <summary>
		/// Event handler for pointer up
		/// </summary>
		/// <param name="eventData"></param>
		public void OnPointerUp( PointerEventData eventData )
		{
			mIsDragging = false;

			var hoveredGroup = mScrollView.IsOverGroup( mTransform.position );
			if ( hoveredGroup >= 0 )
			{
				mOption = hoveredGroup;
			}

			mOnValueChanged.Invoke( mOption );
		}

		///<inheritdoc/>
		public int Option => mOption;

		///<inheritdoc/>
		public TMP_Text Text => mText;

		///<inheritdoc/>
		public TMP_Text Title => mTitle;

		///<inheritdoc/>
		public GameObject VisibleObject => mVisibleObject;

		///<inheritdoc/>
		public Tooltip Tooltip => mTooltip;

		#endregion public

		#region protected

		[SerializeField, Tooltip( "Reference to our drag element's option" )]
		protected int mOption;

		[SerializeField, Tooltip( "Reference to our drag element's Text" )]
		protected TMP_Text mText;

		[SerializeField, Tooltip( "Reference to our drag element's Title" )]
		protected TMP_Text mTitle;

		[SerializeField, Tooltip( "Reference to our drag element's Visible Object" )]
		protected GameObject mVisibleObject;

		[SerializeField, Tooltip( "Reference to our drag element's Tooltip" )]
		protected Tooltip mTooltip;

		[SerializeField, Tooltip( "Reference to our drag element's Transform" )]
		protected Transform mTransform;

		#endregion protected

		#region private

		/// <summary>
		/// Drag event
		/// </summary>
		private class UIDragEvent : UnityEvent<int> { }

		/// <summary>
		/// UI Instrument that will be dragged by this element
		/// </summary>
		private InstrumentListUIObject mInstrumentObject;

		/// <summary>
		/// Drag event invoked when value changes
		/// </summary>
		private readonly UIDragEvent mOnValueChanged = new UIDragEvent();

		/// <summary>
		/// Whether we're currently dragging this element
		/// </summary>
		private bool mIsDragging;

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		private UIManager mUIManager;

		/// <summary>
		/// Reference to the scroll view in which this will drag
		/// </summary>
		private UIInstrumentListScrollView mScrollView;

		/// <summary>
		/// Update
		/// </summary>
		private void Update()
		{
			if ( mIsDragging == false )
			{
				return;
			}

			Vector3 mousePos = mUIManager.MouseScreenPoint;
			mousePos.y = mTransform.position.y;
			mousePos.z = transform.position.z;
			mTransform.position = mousePos;
		}

		#endregion private
	}
}
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Draggable ui element
	/// </summary>
	public class UIWorldDragElement : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name="action"></param>
		/// <param name="uiManager"></param>
		public void Initialize( UnityAction action, UIManager uiManager )
		{
			mUIManager = uiManager;
			mOnValueChanged.RemoveAllListeners();
			mOnValueChanged.AddListener( action );
		}

		/// <summary>
		/// Begins drag
		/// </summary>
		public void OnMouseDown()
		{
			mIsDragging = true;
		}

		/// <summary>
		/// Ends Drag
		/// </summary>
		public void OnMouseUp()
		{
			mIsDragging = false;
			mOnValueChanged.Invoke();
		}

		#endregion public

		#region private

		[SerializeField, Tooltip( "Reference to our transform" )]
		private Transform mTransform;

		/// <summary>
		/// On value changed event (for drag elements, this is invoked if being dragged)
		/// </summary>
		private readonly UnityEvent mOnValueChanged = new UnityEvent();

		/// <summary>
		/// Current dragging state
		/// </summary>
		private bool mIsDragging;

		/// <summary>
		/// Reference to the ui manager
		/// </summary>
		private UIManager mUIManager;

		private void DoUpdate()
		{
			if ( mIsDragging == false )
			{
				return;
			}

			mOnValueChanged.Invoke();
			Vector3 mousePos = mUIManager.MouseWorldPoint;
			var currentPosition = mTransform.position;
			mousePos.y = currentPosition.y;
			mousePos.z = currentPosition.z;
			mTransform.position = mousePos;
		}

		/// <summary>
		/// Update loop
		/// </summary>
		private void Update()
		{
			DoUpdate();
		}

		#endregion private
	}
}
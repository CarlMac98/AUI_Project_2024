using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	///  UI Panel for our int scrollers
	/// </summary>
	public class UIIntScroller : MonoBehaviour, IPanelOption<UIScrollElement<int>>
	{
		[Serializable] private class IntScrollOption : PanelOption<UIIntScrollElement> { }

		///<inheritdoc/>
		public UIScrollElement<int> Option => mOptions.Option;

		///<inheritdoc/>
		public TMP_Text Text => mOptions.Text;

		///<inheritdoc/>
		public TMP_Text Title => mOptions.Title;

		///<inheritdoc/>
		public GameObject VisibleObject => mOptions.VisibleObject;

		///<inheritdoc/>
		public Tooltip Tooltip => mOptions.Tooltip;

		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name="action"></param>
		/// <param name="initialIndex"></param>
		/// <param name="elements"></param>
		public void Initialize( UnityAction<int> action, int initialIndex, int[] elements )
		{
			mOptions.Option.Initialize( elements, initialIndex );
			Option.OnValueChanged.RemoveAllListeners();
			Option.OnValueChanged.AddListener( action );
			action.Invoke( Option.Value );
		}

		[SerializeField, Tooltip( "Reference to our int scroll option" )]
		private IntScrollOption mOptions;

		/// <summary>
		/// OnDestroy
		/// </summary>
		private void OnDestroy()
		{
			Option.OnValueChanged.RemoveAllListeners();
		}
	}
}
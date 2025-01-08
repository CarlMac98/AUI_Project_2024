using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Workaround class as I can't expose abstract classes or interfaces to the unity editor
	/// </summary>
	[Serializable]
	public class ToggleOption : PanelOption<Toggle> { }

	/// <summary>
	/// UI Toggle Element
	/// </summary>
	public class UIToggle : MonoBehaviour, IPanelOption<Toggle>
	{
		///<inheritdoc/>
		public Toggle Option => mOptions.Option;

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
		/// <param name="initialValue"></param>
		public void Initialize( UnityAction<bool> action, bool initialValue )
		{
			Option.onValueChanged.RemoveAllListeners();
			Option.onValueChanged.AddListener( action );
			Option.isOn = initialValue;
			action.Invoke( initialValue );
		}

		[SerializeField, Tooltip( "Reference to our Toggle Option object" )]
		private ToggleOption mOptions;

		/// <summary>
		/// OnDestroy
		/// </summary>
		private void OnDestroy()
		{
			Option.onValueChanged.RemoveAllListeners();
		}
	}
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	[Serializable]
	public class TMPDropdownOption : PanelOption<UITMPDropdownWrapper> { }

	/// <summary>
	/// UI Element for dropdown panels
	/// </summary>
	public class UITMPDropdown : MonoBehaviour, IPanelOption<UITMPDropdownWrapper>
	{
		public UnityEvent<int> OnRemoveElementTapped = new UnityEvent<int>();

		[SerializeField]
		private bool mShouldUpdatePosition = true;

		///<inheritdoc/>
		public UITMPDropdownWrapper Option => mOptions.Option;

		///<inheritdoc/>
		public TMP_Text Text => mOptions.Text;

		///<inheritdoc/>
		public TMP_Text Title => mOptions.Title;

		///<inheritdoc/>
		public GameObject VisibleObject => mOptions.VisibleObject;

		///<inheritdoc/>
		public Tooltip Tooltip => mOptions.Tooltip;

		public Button mRemoveButton;

		/// <summary>
		/// Initializes the UI Dropdown
		/// </summary>
		/// <param name="action"></param>
		/// <param name="initialValue"></param>
		public void Initialize( UnityAction<int> action, int? initialValue = null )
		{
			Option.ToggleShouldUpdatePosition( mShouldUpdatePosition );
			Option.onValueChanged.RemoveAllListeners();
			Option.onValueChanged.AddListener( action );
			if ( initialValue.HasValue )
			{
				Option.value = initialValue.Value;
				action.Invoke( initialValue.Value );
			}
		}

		public void OnEnable()
		{
			if ( mRemoveButton )
			{
				mRemoveButton.onClick.AddListener( RemoveElement );
			}
		}

		public void OnDisable()
		{
			if ( mRemoveButton )
			{
				mRemoveButton.onClick.RemoveListener( RemoveElement );
			}
		}

		[SerializeField]
		private TMPDropdownOption mOptions;

		/// <summary>
		/// OnDestroy
		/// </summary>
		private void OnDestroy()
		{
			Option.onValueChanged.RemoveAllListeners();
		}

		private void RemoveElement()
		{
			if ( Option.options.Count == 0 )
			{
				return;
			}

			OnRemoveElementTapped?.Invoke( Option.value );
			Option.options.RemoveAt( Option.value );
		}
	}
}
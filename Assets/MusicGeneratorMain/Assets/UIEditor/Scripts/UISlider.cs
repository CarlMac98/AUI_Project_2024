using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#pragma warning disable 0649

namespace ProcGenMusic
{
	#region public

	/// <summary>
	/// Workaround class as I can't expose abstract classes or interfaces to the unity editor
	/// </summary>
	[Serializable] public class SliderOption : PanelOption<PMGSlider> { }

	/// <summary>
	/// UI Slider element
	/// </summary>
	public class UISlider : MonoBehaviour, IPanelOption<PMGSlider>
	{
		///<inheritdoc/>
		public PMGSlider Option => mOptions.Option;

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
		/// <param name="resetValue"></param>
		/// <param name="createDividers"></param>
		public void Initialize( UnityAction<float> action, float initialValue, float? resetValue = null, bool createDividers = false )
		{
			Option.onValueChanged.RemoveAllListeners();
			Option.onValueChanged.AddListener( action );
			Option.value = initialValue;
			mResetValue = resetValue;
			action.Invoke( initialValue );

			if ( createDividers )
			{
				CreateDividers();
			}
		}

		/// <summary>
		/// Resets option value (to mResetValue)
		/// </summary>
		public void Reset()
		{
			if ( mResetValue.HasValue )
			{
				Option.value = mResetValue.Value;
			}
		}

		#endregion public

		#region private

		[SerializeField, Tooltip( "Reference to our slider option" )]
		private SliderOption mOptions;

		[SerializeField, Tooltip( "Divider Asset Reference" )]
		private GameObject mDividerReference;

		[SerializeField, Tooltip( "Divider Parent Transform" )]
		private Transform mDividerParent;

		[SerializeField, Tooltip( "Amount to offset the divider" )]
		private float mDividerOffset = .50f;

		/// <summary>
		/// Our currently instantiated sliders
		/// </summary>
		private readonly List<GameObject> mDividers = new List<GameObject>();

		/// <summary>
		/// Value to reset our slider to
		/// </summary>
		private float? mResetValue;

		/// <summary>
		/// On Destroy
		/// </summary>
		private void OnDestroy()
		{
			Option.onValueChanged.RemoveAllListeners();
		}

		/// <summary>
		/// Creates dividers for our slider based on the number of option values
		/// </summary>
		private void CreateDividers()
		{
			if ( mDividerReference == null )
			{
				return;
			}

			DestroyExistingDividers();
			var sliderCount = ( int )( Option.maxValue - Option.minValue );
			for ( var index = 1; index < sliderCount; index++ )
			{
				var xOffset = index * ( Option.GetComponent<RectTransform>().rect.width / sliderCount );
				var divider = Instantiate( mDividerReference, Vector3.zero, Quaternion.identity, mDividerParent );
				var rectTransform = divider.GetComponent<RectTransform>();

				rectTransform.SetInsetAndSizeFromParentEdge( RectTransform.Edge.Bottom,
					0f,
					Option.fillRect.rect.height * mDividerOffset );
				rectTransform.transform.localScale = Vector3.one;
				divider.transform.localPosition = new Vector3( xOffset, Option.fillRect.rect.height * mDividerOffset / 2, 0 );
				mDividers.Add( divider );
			}
		}

		/// <summary>
		/// Destroys existing dividers
		/// </summary>
		private void DestroyExistingDividers()
		{
			for ( var index = mDividers.Count - 1; index >= 0; index-- )
			{
				Destroy( mDividers[index] );
			}

			mDividers.Clear();
		}

		#endregion private
	}
}
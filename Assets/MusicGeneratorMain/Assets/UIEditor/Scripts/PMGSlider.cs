using ProcGenMusic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 0649

public class PMGSlider : Slider, IPointerClickHandler
{
	public void OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.button.Equals( PointerEventData.InputButton.Right ) )
		{
			mInputField.gameObject.SetActive( true );
			mInputField.text = $"{value}";
			mInputField.ActivateInputField();
			UIManager.LockSliderInput();
		}
	}

	[SerializeField, Tooltip( "Reference to our TMP_InputField" )]
	private TMP_InputField mInputField;

	protected override void Awake()
	{
		base.Awake();
		mInputField.onSubmit.AddListener( OnSubmit );
		mInputField.onDeselect.AddListener( OnDeselect );
	}

	private void OnSubmit( string inputValue )
	{
		if ( mInputField.wasCanceled || string.IsNullOrEmpty( inputValue ) )
		{
			mInputField.gameObject.SetActive( false );
			return;
		}

		var floatValue = float.Parse( inputValue );
		if ( floatValue < minValue || floatValue > maxValue )
		{
			mInputField.text = $"{value}";
			return;
		}

		value = floatValue;
		mInputField.gameObject.SetActive( false );
		UIManager.UnlockSlider();
	}

	private void OnDeselect( string inputValue )
	{
		mInputField.gameObject.SetActive( false );
		UIManager.UnlockSlider();
	}
}
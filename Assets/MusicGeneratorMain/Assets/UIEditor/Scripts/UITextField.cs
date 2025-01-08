using System.Linq;
using ProcGenMusic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#pragma warning disable 0649

public class UITextField : MonoBehaviour, IPointerClickHandler
{
	public UnityEvent<string> OnTextWasSet = new UnityEvent<string>();

	public void OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.button.Equals( PointerEventData.InputButton.Right ) )
		{
			mInputField.gameObject.SetActive( true );
			mInputField.text = $"{mText.text}";
			mInputField.ActivateInputField();
			UIManager.LockSliderInput();
		}
	}

	public void SetText( string text )
	{
		mText.SetText( text );
	}

	public void SetColor( Color color )
	{
		mText.color = color;
	}

	public void SetTextAndColor( string text, Color color )
	{
		SetText( text );
		SetColor( color );
	}

	[SerializeField, Tooltip( "Reference to our TMP_InputField" )]
	private TMP_InputField mInputField;

	[SerializeField, Tooltip( "Reference to our main Text object" )]
	private TMP_Text mText;

	private void Awake()
	{
		mInputField.onSubmit.AddListener( OnSubmit );
		mInputField.onDeselect.AddListener( OnDeselect );
	}

	private void OnSubmit( string inputValue )
	{
		if ( mInputField.wasCanceled || string.IsNullOrEmpty( inputValue ) ||
		     inputValue.Any( x => char.IsLetterOrDigit( x ) || x == '_' ) == false )
		{
			mInputField.gameObject.SetActive( false );
			return;
		}

		mText.SetText( mInputField.text );
		OnTextWasSet?.Invoke( mInputField.text );
		mInputField.gameObject.SetActive( false );
		UIManager.UnlockSlider();
	}

	private void OnDeselect( string inputValue )
	{
		mInputField.gameObject.SetActive( false );
		UIManager.UnlockSlider();
	}
}
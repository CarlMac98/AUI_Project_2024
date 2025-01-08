using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	[RequireComponent( typeof( Tooltip ) )]
	public class UIColorScroller : MonoBehaviour, IPanelOption<UIScrollElement<Color>>
	{
		[Serializable] public class ColorScrollOption : PanelOption<UIColorScrollElement> { }

		/// <summary>
		/// Reference to our Scroll Element
		/// </summary>
		public UIScrollElement<Color> Option => Options.Option;

		/// <summary>
		/// Reference to the Color Scroll Element Text
		/// </summary>
		public TMP_Text Text => Options.Text;

		/// <summary>
		/// Reference to the Color Scroll Title
		/// </summary>
		public TMP_Text Title => Options.Title;

		/// <summary>
		/// Reference to the toggleable Game Object
		/// </summary>
		public GameObject VisibleObject => Options.VisibleObject;

		/// <summary>
		/// Reference to our tooltip for the Color Scroller
		/// </summary>
		public Tooltip Tooltip => Options.Tooltip;

		/// <summary>
		/// Initialization 
		/// </summary>
		/// <param name="action"></param>
		/// <param name="initialIndex"></param>
		/// <param name="colorElements"></param>
		public void Initialize( UnityAction<Color> action, int initialIndex, List<Color> colorElements )
		{
			Options.Option.Initialize( colorElements.ToArray(), initialIndex );
			Option.OnValueChanged.RemoveAllListeners();
			Option.OnValueChanged.AddListener( action );
			Option.OnValueChanged.AddListener( UpdateColor );
			mDisplaySprite.color = Option.Value;
			action.Invoke( Option.Value );
		}

		[SerializeField, Tooltip( "Reference to our Color Scroll Option" )]
		private ColorScrollOption Options;

		[SerializeField, Tooltip( "Reference to our Color Scroll Sprite" )]
		private Image mDisplaySprite;

		/// <summary>
		/// On Destroy
		/// </summary>
		private void OnDestroy()
		{
			Option.OnValueChanged.RemoveAllListeners();
		}

		/// <summary>
		/// Updates the sprite color
		/// </summary>
		/// <param name="color"></param>
		private void UpdateColor( Color color )
		{
			mDisplaySprite.color = color;
		}
	}
}
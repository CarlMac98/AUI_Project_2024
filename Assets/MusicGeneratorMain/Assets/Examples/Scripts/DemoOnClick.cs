using System;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public class DemoOnClick : DemoLightObject
	{
		[SerializeField] private TMP_Text mBottomText;
		
		public Action OnSelected;

		public void SetValueText( string text )
		{
			mBottomText.text = text;
		}

		private void OnMouseDown()
		{
			Select();
			OnSelected.Invoke();
		}
	}
}

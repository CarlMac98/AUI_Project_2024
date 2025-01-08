using System;
using TMPro;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Interface for our panel options.
	/// </summary>
	/// <typeparam name="TOptionType"></typeparam>
	public interface IPanelOption<out TOptionType>
	{
		//TODO: These are all unused as interfaces. Due to not being able to use serialize field with interfaces/generics
		TOptionType Option { get; }
		TMP_Text Text { get; }
		TMP_Text Title { get; }
		GameObject VisibleObject { get; }
		Tooltip Tooltip { get; }
	}

	/// <summary>
	/// Panel options are the base of our ui element variants (UISliders, UIDropdowns etc)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class PanelOption<T> : IPanelOption<T>
	{
		public T Option => mOption;
		public TMP_Text Text => mText;
		public TMP_Text Title => mTitle;
		public GameObject VisibleObject => mVisibleObject;
		public Tooltip Tooltip => mTooltip;

		[SerializeField]
		protected T mOption;

		[SerializeField]
		protected TMP_Text mTitle;

		[SerializeField]
		protected TMP_Text mText;

		[SerializeField]
		protected GameObject mVisibleObject;

		[SerializeField]
		protected Tooltip mTooltip;
	}
}

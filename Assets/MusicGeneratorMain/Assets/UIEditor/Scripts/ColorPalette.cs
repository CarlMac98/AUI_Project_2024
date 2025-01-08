using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// Types of color fields.
	/// These names have long since stopped having any real meaning. 
	/// </summary>
	[Serializable] public enum ColorFieldType
	{
		Background,
		UI_1,
		UI_2,
		UI_3,
		UI_4,
		UI_5,
		TEXT_1,
		Background_2,
		Background_3
	}

	/// <summary>
	/// Contains references to color and color field type for a palette. 
	/// </summary>
	[CreateAssetMenu( fileName = "ColorPalette", menuName = "ProcGenMusic/UI/ColorPalette", order = 1 )]
	public class ColorPalette : ScriptableObject
	{
		/// <summary>
		/// List of our UIColor Fields for this palette
		/// </summary>
		public IReadOnlyList<UIColorField> ColorFields => mColorFields;

		/// <summary>
		/// Getter for the palette name
		/// </summary>
		public string PaletteName => mPaletteName;

		/// <summary>
		/// Container with the color/field type.
		/// </summary>
		[Serializable]
		public class UIColorField
		{
			public Color Color => mColor;
			public ColorFieldType Type => mType;

			[SerializeField, Tooltip( "Color for this field" )]
			private Color mColor;

			[SerializeField, Tooltip( "Color Field Type for this field" )]
			private ColorFieldType mType;
		}

		[SerializeField, Tooltip( "Palette Name" )]
		private string mPaletteName;

		[SerializeField, Tooltip( "List of available color fields" )]
		private List<UIColorField> mColorFields;
	}
}
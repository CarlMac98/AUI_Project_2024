using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649

namespace ProcGenMusic
{
	/// <summary>
	/// UI Panel for Editor Visual Settings
	/// </summary>
	public class UIEditorSettings : UIPanel
	{
#region public

		/// <summary>
		/// Event fired when using Keyboard Particles state has changed 
		/// </summary>
		public UnityEvent<bool> OnUseKeyboardParticlesChanged => mOnUseKeyboardParticlesChanged;

		/// <summary>
		/// Event fired when using Keyboard Lights has changed
		/// </summary>
		public UnityEvent<bool> OnUseKeyboardLightsChanged => mOnUseKeyboardLightsChanged;

		/// <summary>
		/// Event fired when using UseLogo value changed
		/// </summary>
		public UnityEvent<bool> OnUseLogoChanged => mOnUseLogoChanged;

		/// <summary>
		/// Reference to our current color palette
		/// </summary>
		public ColorPalette CurrentPalette => mColorPalettes[( int )mPaletteSlider.Option.value];

		///<inheritdoc/>
		public override void UpdateUIElementValues( )
		{
			if ( mUIManager )
			{
				mPaletteSlider.Option.value = mUIManager.FXSettings.ColorPaletteIndex;
			}
		}

#endregion public

#region protected

		///<inheritdoc/>
		protected override void InitializeListeners( )
		{
			mPaletteSlider.Initialize( ( value ) =>
				{
					mUIManager.FXSettings.ColorPaletteIndex = ( int )value;
					UpdatePalette( ( int )value );
					mUIKeyboard.UpdateStyle( );
					mPaletteSlider.Option.maxValue = mColorPalettes.Length - 1;
					mPaletteSlider.Text.text = mColorPalettes[( int )value].PaletteName;
				}, mUIManager.FXSettings.ColorPaletteIndex, resetValue: null,
				createDividers: true );

			mStyleSlider.Initialize( ( value ) =>
				{
					mUIManager.FXSettings.UIStyle = ( UIEditorFXSettings.UIEditorStyle )value;
					mStyleSlider.Text.text = $"{mUIManager.FXSettings.UIStyle}";
					mUIKeyboard.UpdateStyle( );
				}, ( int )mUIManager.FXSettings.UIStyle, resetValue: null,
				createDividers: true );

			mParticleStyleSlider.Initialize( ( value ) =>
				{
					mUIManager.FXSettings.UIKeysParticleStyle = ( UIEditorFXSettings.UIParticleStyle )value;
					mParticleStyleSlider.Text.text = $"{mUIManager.FXSettings.UIKeysParticleStyle}";
					mUIKeyboard.UpdateStyle( );
				}, ( int )mUIManager.FXSettings.UIKeysParticleStyle, resetValue: null,
				createDividers: true );

			mUseKeyboardParticles.Initialize( ( value ) =>
			{
				mUIManager.FXSettings.UseKeyboardParticles = value;
				mOnUseKeyboardParticlesChanged.Invoke( value );
			}, mUIManager.FXSettings.UseKeyboardParticles );

			mUseKeyboardLights.Initialize( ( value ) =>
			{
				mUIManager.FXSettings.UseKeyboardLights = value;
				mOnUseKeyboardLightsChanged.Invoke( value );
			}, mUIManager.FXSettings.UseKeyboardLights );

			mUseLogo.Initialize( ( value ) =>
			{
				mUIManager.FXSettings.UseLogo = value;
				mOnUseLogoChanged.Invoke( value );
			}, mUIManager.FXSettings.UseLogo );

			mUseFallingNotePulse.Initialize( ( value ) => { mUIManager.FXSettings.UseFallingNotePulse = value; },
				mUIManager.FXSettings.UseFallingNotePulse );

			mFallingNoteEmissionFloor.Initialize( ( value ) =>
			{
				mFallingNoteEmissionFloor.Text.text = $"{value:N2}";
				mUIManager.FXSettings.FallingNoteEmissionIntensityFloor = value;
			}, mUIManager.FXSettings.FallingNoteEmissionIntensityFloor );

			mNoteFallSpeed.Initialize( ( value ) =>
			{
				mUIManager.FXSettings.FallingNoteSpeed = value;
				mNoteFallSpeed.Text.text = $"{value}";
			}, mUIManager.FXSettings.FallingNoteSpeed );

			mBloomEnabled.Initialize( ( value ) =>
			{
				mUIManager.FXSettings.BloomIsEnabled = value;
				mUIManager.SetPostProcessingEnabled( value );
				mBloomEnabled.Text.text = value ? BLOOM_ENABLED_TEXT : BLOOM_DISABLED_TEXT;
			}, mUIManager.FXSettings.BloomIsEnabled );
		}

#endregion protected

#region private

		[SerializeField, Tooltip( "Reference to our ui keyboard" )]
		private UIKeyboard mUIKeyboard;

		[SerializeField, Tooltip( "Reference to our Color Palette for the UI Editor Settings Panel" )]
		private ColorPalette[] mColorPalettes;

		[SerializeField, Tooltip( "Reference to our Container of base materials for the UI Editor Settings Panel" )]
		private List<Material> mBasematerials;

		[SerializeField, Tooltip( "Reference to our UI Toggle for falling note pulse for the UI Editor Settings Panel" )]
		private UIToggle mUseFallingNotePulse;

		[SerializeField, Tooltip( "Reference to our UI Toggle for keyboard particles for the UI Editor Settings Panel" )]
		private UIToggle mUseKeyboardParticles;

		[SerializeField, Tooltip( "Reference to our UI Toggle for keyboard lights for the UI Editor Settings Panel" )]
		private UIToggle mUseKeyboardLights;

		[SerializeField, Tooltip( "Reference to our UI Toggle for the Logo for the UI Editor Settings Panel" )]
		private UIToggle mUseLogo;

		[SerializeField, Tooltip( "Reference to our UI Toggle for bloom for the UI Editor Settings Panel" )]
		private UIToggle mBloomEnabled;

		[SerializeField, Tooltip( "Reference to our UI Slider for the palette for the UI Editor Settings Panel" )]
		private UISlider mPaletteSlider;

		[SerializeField, Tooltip( "Reference to our style slider" )]
		private UISlider mStyleSlider;

		[SerializeField, Tooltip( "Reference to our particle style slider" )]
		private UISlider mParticleStyleSlider;

		[SerializeField, Tooltip( "Reference to our UI Slider for falling note emission floor for the UI Editor Settings Panel" )]
		private UISlider mFallingNoteEmissionFloor;

		[SerializeField, Tooltip( "Reference to our UI Slider for Note Fall Speed for the UI Editor Settings Panel" )]
		private UISlider mNoteFallSpeed;

		private class UIEditorToggleChangedEvent : UnityEvent<bool>
		{
		}
		
		/// <summary>
		/// Event for Keyboard Particles Changed
		/// </summary>
		private readonly UIEditorToggleChangedEvent mOnUseKeyboardParticlesChanged = new UIEditorToggleChangedEvent( );

		/// <summary>
		/// Event for keyboard lights changed
		/// </summary>
		private readonly UIEditorToggleChangedEvent mOnUseKeyboardLightsChanged = new UIEditorToggleChangedEvent( );

		/// <summary>
		/// Event for Use Logo changed 
		/// </summary>
		private readonly UIEditorToggleChangedEvent mOnUseLogoChanged = new UIEditorToggleChangedEvent( );

		/// <summary>
		/// Text for bloom enabled 
		/// </summary>
		private const string BLOOM_ENABLED_TEXT = "Bloom is enabled";

		/// <summary>
		/// Text for bloom disabled
		/// </summary>
		private const string BLOOM_DISABLED_TEXT = "Bloom is disabled";

		/// <summary>
		/// accessor string for face color
		/// </summary>
		private readonly int mFaceColor = Shader.PropertyToID( "_FaceColor" );

		/// <summary>
		/// Updates our palette
		/// </summary>
		/// <param name="paletteIndex"></param>
		private void UpdatePalette( int paletteIndex )
		{
			for ( var index = 0; index < mBasematerials.Count; index++ )
			{
				if ( mColorPalettes[paletteIndex].ColorFields[index].Type == ColorFieldType.TEXT_1 )
				{
					mBasematerials[index].SetColor( mFaceColor, mColorPalettes[paletteIndex].ColorFields[index].Color );
				}
				else
				{
					mBasematerials[index].color = mColorPalettes[paletteIndex].ColorFields[index].Color;
				}
			}

			mUIManager.DirtyEditorDisplays( );
		}

#endregion private
	}
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace ProcGenMusic
{
	public abstract class UIPanel : MonoBehaviour
	{
		#region public

		/// <summary>
		/// Button To Exit the UI Panel
		/// </summary>
		public Button ExitButton => mExit;

		/// <summary>
		/// Whether this UIPanel is currently enabled
		/// </summary>
		public bool IsEnabled { get; private set; }

		/// <summary>
		/// Updates our UI Element Values. This can happen when the base data class is changed.
		/// I.e. an instrument panel changes instruments.
		/// </summary>
		public abstract void UpdateUIElementValues();

		/// <summary>
		/// Initializes the UI Panel.
		/// Note: Either this, or the InitializeRoutine can be used, but not both, pretty pleases <3
		/// </summary>
		/// <param name="uiManager"></param>
		/// <param name="isEnabled"></param>
		public virtual void Initialize( UIManager uiManager, bool isEnabled = true )
		{
			if ( mIsInitialized )
			{
				return;
			}

			IsEnabled = isEnabled;
			mUIManager = uiManager;
			mMusicGenerator = mUIManager.MusicGenerator;
			mCanvasGroup.interactable = false;
			mCanvasGroup.blocksRaycasts = false;

			if ( mExit != null )
			{
				mExit.onClick.AddListener( TogglePanel );
			}

			AddTooltips();
			InitializeListeners();
			mIsInitialized = true;
		}

		/// <summary>
		/// Initializes the UIPanel through a Coroutine. Several derived classes require this, and the UI Manager
		/// will need to wait until it completes before initializing downstream panels.
		/// </summary>
		/// <param name="uiManager"></param>
		/// <param name="isEnabled"></param>
		/// <returns></returns>
		public virtual IEnumerator InitializeRoutine( UIManager uiManager, bool isEnabled = true )
		{
			mUIManager = uiManager;
			mMusicGenerator = mUIManager.MusicGenerator;
			mCanvasGroup.interactable = false;
			mCanvasGroup.blocksRaycasts = false;

			if ( mExit != null )
			{
				mExit.onClick.AddListener( TogglePanel );
			}

			AddTooltips();
			InitializeListeners();
			mIsInitialized = true;
			yield return null;
		}

		/// <summary>
		/// Toggles the panel active/inactive
		/// </summary>
		public virtual void TogglePanel()
		{
			if ( mMusicGenerator.GeneratorState == GeneratorState.Initializing )
			{
				return;
			}

			if ( mAnimator != null )
			{
				var isActive = mAnimator.GetBool( mIsVisibleAnimName );
				SetPanelActive( isActive == false );
			}
		}

		/// <summary>
		/// Sets the panel active/inactive
		/// </summary>
		/// <param name="isActive"></param>
		public virtual void SetPanelActive( bool isActive )
		{
			IsEnabled = isActive;
			mCanvasGroup.interactable = isActive;
			mCanvasGroup.blocksRaycasts = isActive;
			if ( mAnimator != null )
			{
				mAnimator.SetBool( mIsVisibleAnimName, isActive );
			}

			if ( isActive )
			{
				UpdateUIElementValues();
			}
		}

		#endregion public

		#region protected

		/// <summary>
		/// Reference to our animator
		/// </summary>
		[SerializeField] protected Animator mAnimator;

		/// <summary>
		/// Name the animator uses for visibility state
		/// </summary>
		[SerializeField] protected string mIsVisibleAnimName = "IsVisible";

		/// <summary>
		/// Reference to our ui manager
		/// </summary>
		protected UIManager mUIManager;

		/// <summary>
		/// Reference to our music generator
		/// </summary>
		protected MusicGenerator mMusicGenerator;

		/// <summary>
		/// Initializes our UI Element listeners. 
		/// </summary>
		protected abstract void InitializeListeners();

		/// <summary>
		/// Whether our UI Panel has been initialized
		/// </summary>
		protected bool mIsInitialized;

		/// <summary>
		/// OnDestroy
		/// </summary>
		protected virtual void OnDestroy()
		{
			if ( mExit != null )
			{
				mExit.onClick.RemoveAllListeners();
			}
		}

		#endregion protected

		#region private

		/// <summary>
		/// Reference to our canvas group this panel belongs to
		/// </summary>
		[SerializeField] private CanvasGroup mCanvasGroup;

		/// <summary>
		/// Reference to our exit button
		/// </summary>
		[SerializeField] private Button mExit;

		/// <summary>
		/// Adds our tooltips to the ui manager
		/// </summary>
		private void AddTooltips()
		{
			foreach ( var tooltip in GetComponentsInChildren<Tooltip>( includeInactive: true ) )
			{
				tooltip.Initialize( mUIManager );
			}
		}

		#endregion private
	}
}
using UnityEngine;

namespace ProcGenMusic
{
	public class MouseCursor : MonoBehaviour
	{
		[SerializeField]
		private Texture2D mMouseTexture;

		private void Awake()
		{
			if (mMouseTexture)
			{
				Cursor.SetCursor(mMouseTexture, Vector2.zero, CursorMode.Auto);
			}
		}
	}
}
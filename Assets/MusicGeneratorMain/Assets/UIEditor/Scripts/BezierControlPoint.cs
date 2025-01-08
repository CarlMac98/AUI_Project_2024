using UnityEngine;

namespace ProcGenMusic
{
	public class BezierControlPoint : MonoBehaviour
	{
		public enum BezierControlType
		{
			In = 0,
			Main = 1,
			Out = 2
		}

		public Transform Transform => mTransform;
		public BezierControlType ControlType => mControlType;

		[SerializeField]
		private BezierControlType mControlType;

		[SerializeField]
		private Transform mTransform;
	}
}
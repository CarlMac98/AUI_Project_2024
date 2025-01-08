using System;
using UnityEngine;

namespace ProcGenMusic
{
	[Serializable]
	public struct BezierControlData
	{
		public Vector3 MainControlPoint;
		public Vector3 InControlPoint;
		public Vector3 OutControlPoint;
		public bool IsStartPoint;
		public bool IsEndPoint;
		public BezierEditorPanel.BezierType BezierType;
	}
}
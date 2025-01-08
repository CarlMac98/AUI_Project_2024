using System.Linq;
using UnityEngine;

namespace ProcGenMusic
{
	public class WaveformEditor : MonoBehaviour
	{
		public void UpdateUIElements( Instrument instrument )
		{
			mBezierEditorPanel.UpdateUIElements( instrument );
		}

		[SerializeField]
		private BezierEditorPanel mBezierEditorPanel;

		private void OnEnable()
		{
			if ( mBezierEditorPanel != null )
			{
				mBezierEditorPanel.OnDataUpdated += SetData;
			}
		}

		private void OnDisable()
		{
			if ( mBezierEditorPanel != null )
			{
				mBezierEditorPanel.OnDataUpdated -= SetData;
			}
		}

		private void SetData()
		{
			var instrument = mBezierEditorPanel.Instrument;

			if ( instrument == null ||
			     mBezierEditorPanel.BezierEditorType != BezierEditorPanel.BezierType.Waveform )
			{
				return;
			}

			instrument.InstrumentData.WaveformData.Clear();
			foreach ( var bezierControl in mBezierEditorPanel.BezierControls )
			{
				instrument.InstrumentData.WaveformData.Add( bezierControl.GetData() );
			}

			instrument.InstrumentData.CustomSynthWaveform = GetWaveform();
		}

		private float[] GetWaveform()
		{
			var positions = new Vector3[mBezierEditorPanel.LineRenderer.positionCount];
			mBezierEditorPanel.LineRenderer.GetPositions( positions );
			var waveForm = ( from sortedPoints in positions select sortedPoints.y ).ToArray();
			var range = mBezierEditorPanel.Ceiling - mBezierEditorPanel.Floor;
			for ( var index = 0; index < waveForm.Length; index++ )
			{
				var finalPoint = mBezierEditorPanel.Ceiling - waveForm[index];
				finalPoint /= range / 2f;
				finalPoint = 1f - finalPoint;
				waveForm[index] = finalPoint;
			}

			return waveForm;
		}
	}
}
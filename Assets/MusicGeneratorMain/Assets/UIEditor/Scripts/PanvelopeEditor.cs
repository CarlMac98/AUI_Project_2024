using UnityEngine;

namespace ProcGenMusic
{
	public class PanvelopeEditor : MonoBehaviour
	{
		public void UpdateUIElements( Instrument instrument )
		{
			mBezierEditorPanel.UpdateUIElements( instrument );
		}

		[SerializeField]
		private BezierEditorPanel mBezierEditorPanel;

		private const int ENVELOPE_SEGMENT_COUNT = 2000;

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
			     mBezierEditorPanel.BezierEditorType != BezierEditorPanel.BezierType.Panvelope )
			{
				return;
			}

			instrument.InstrumentData.PanvelopeData.Clear();
			foreach ( var bezierControl in mBezierEditorPanel.BezierControls )
			{
				instrument.InstrumentData.PanvelopeData.Add( bezierControl.GetData() );
			}

			instrument.InstrumentData.CustomPanvelope = GetEnvelope();
		}

		private float[] GetEnvelope()
		{
			var positions = new Vector3[mBezierEditorPanel.LineRenderer.positionCount];
			mBezierEditorPanel.LineRenderer.GetPositions( positions );

			var start = positions[0].x;
			var end = positions[positions.Length - 1].x;
			var xRange = end - start;

			var increment = xRange / ENVELOPE_SEGMENT_COUNT;
			var xPos = start;
			var envelopeList = new float[ENVELOPE_SEGMENT_COUNT];
			var lastFoundIndex = 0;

			for ( var envelopeIndex = 0; envelopeIndex < ENVELOPE_SEGMENT_COUNT; envelopeIndex++ )
			{
				var foundPosition = false;
				for ( var positionIndex = lastFoundIndex; positionIndex < positions.Length; positionIndex++ )
				{
					if ( positions[positionIndex].x <= xPos )
					{
						continue;
					}

					foundPosition = true;
					lastFoundIndex = positionIndex;
					envelopeList[envelopeIndex] = positions[positionIndex].y;
					break;
				}

				if ( foundPosition == false )
				{
					envelopeList[envelopeIndex] = 0f;
				}

				xPos += increment;
			}

			var range = mBezierEditorPanel.Ceiling - mBezierEditorPanel.Floor;
			for ( var index = 0; index < envelopeList.Length; index++ )
			{
				var finalPoint = mBezierEditorPanel.Ceiling - envelopeList[index];
				finalPoint /= range / 2f;
				envelopeList[index] = ( 1f - finalPoint );
			}

			return envelopeList;
		}
	}
}
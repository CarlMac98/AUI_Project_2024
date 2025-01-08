using UnityEditor;
using UnityEditor.UI;

namespace ProcGenMusic
{
	[CustomEditor(typeof(PMGSlider))]
	public class PMGSliderInspector : SliderEditor
	{
		private SerializedProperty InputField;

		protected override void OnEnable()
		{
			base.OnEnable();
			InputField = serializedObject.FindProperty("mInputField");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.PropertyField(InputField);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
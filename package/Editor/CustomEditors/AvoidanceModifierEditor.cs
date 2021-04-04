using AlchemyBow.Navigation.Simple.Elements;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AvoidanceModifier))]
    public class AvoidanceModifierEditor : UnityEditor.Editor
    {
        private static bool visualize;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            visualize = GUILayout.Toggle(visualize, "Visualize", "Button");
        }

        private void OnSceneGUI()
        {
            if (visualize)
            {
                var modifier = (AvoidanceModifier)target;
                NavigationEditorTools.DrawWireCylinder(modifier.transform.position, modifier.transform.rotation, modifier.Radius, modifier.Height, Color.blue);
            }
        }
    }
}

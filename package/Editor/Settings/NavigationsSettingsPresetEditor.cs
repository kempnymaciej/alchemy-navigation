using AlchemyBow.Navigation.Settings;
using UnityEditor;

namespace AlchemyBow.Navigation.Editor
{
    [CustomEditor(typeof(AlchemyNavigationSettingsPreset))]
    public sealed class NavigationsSettingsPresetEditor : UnityEditor.Editor
    {
        private NavigationSettingsEditingHelper navigationSettingsEditingHelper;

        private void OnEnable()
        {
            navigationSettingsEditingHelper = new NavigationSettingsEditingHelper(serializedObject.FindProperty("settings"), false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            navigationSettingsEditingHelper.DrawSettingsEditor(false);
            serializedObject.ApplyModifiedProperties();
        }
    } 
}

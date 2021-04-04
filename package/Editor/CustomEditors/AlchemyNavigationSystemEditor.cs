using AlchemyBow.Navigation.Settings;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor
{
    [CustomEditor(typeof(AlchemyNavigationSystem))]
    public class AlchemyNavigationSystemEditor : UnityEditor.Editor
    {
        private NavigationSettingsEditingHelper navigationSettingsEditingHelper;
        private SerializedProperty debugUnitProperty;

        private void OnEnable()
        {
            navigationSettingsEditingHelper = new NavigationSettingsEditingHelper(serializedObject.FindProperty("settings"), true);
            debugUnitProperty = serializedObject.FindProperty("debugUnit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Navigation Settings - editing", EditorStyles.boldLabel);
            navigationSettingsEditingHelper.DrawSettingsEditor(false);
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Settings cannot be changed in play mode.", MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Navigation Settings - export / import", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export"))
            {
                ExportSettingsPreset();
            }
            if (GUILayout.Button("Import"))
            {
                ImportSettingsPreset();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(debugUnitProperty);
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("If you change the drawing settings, the changes will not appear until the next BuildingProcess. You can force them using ContextMenu -> ForceGizmosDrawerUpdate.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ExportSettingsPreset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Export Settings Preset", "NavigationSettingsPreset", "asset", "Please enter a file name to save the settings preset to.");
            if (!string.IsNullOrEmpty(path))
            {
                var preset = ScriptableObject.CreateInstance<AlchemyNavigationSettingsPreset>();
                var presetSerializedObject = new SerializedObject(preset);
                presetSerializedObject.CopyFromSerializedProperty(serializedObject.FindProperty("settings"));
                presetSerializedObject.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(preset, path);
                AssetDatabase.SaveAssets();

                Debug.Log("Settings preset exported to " + path);
            }
        }
        private void ImportSettingsPreset()
        {
            string path = EditorUtility.OpenFilePanel("Import Settings Preset", Application.dataPath, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    AlchemyNavigationSettingsPreset preset = null;
                    preset = AssetDatabase.LoadAssetAtPath<AlchemyNavigationSettingsPreset>(path);

                    if (preset != null)
                    {
                        var presetSerializedObject = new SerializedObject(preset);
                        serializedObject.CopyFromSerializedProperty(presetSerializedObject.FindProperty("settings"));
                        Debug.Log(string.Format("Settings preset was imported from {0}", path));
                    }
                    else
                    {
                        Debug.LogError(string.Format("Asset at {0} could not be loaded", path));
                    }
                }
                else
                {
                    Debug.LogError("Only settings presets in project assets folder can be loaded.");
                }
            }
        }
    } 
}

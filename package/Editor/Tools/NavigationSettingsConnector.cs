using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor
{
    public class NavigationSettingsConnector
    {
        private const int NumberOfOptions = 32;
        private readonly GUIContent[] layerOptions;
        private readonly GUIContent[] areaOptions;
        private readonly string[] areaMaskOptions;
        private readonly int[] indexValues;

        public bool Connected { get; private set; }

        public static AlchemyNavigationSystem TryGetSystem()
        {
            var systems = GameObject.FindObjectsOfType<AlchemyNavigationSystem>();
            if (systems.Length == 1)
            {
                return systems[0];
            }
            return null;
        }

        public static NavigationSettingsConnector Create()
        {
            var result = new NavigationSettingsConnector();
            result.Connected = result.TryCreateOptionsBasedOnSystem();
            if (!result.Connected)
            {
                result.CreateDefaultOptions();
            }
            result.CreateIndexValues();
            result.CreateAreaMaskOptions();
            return result;
        }

        private NavigationSettingsConnector()
        {
            layerOptions = new GUIContent[NumberOfOptions];
            areaOptions = new GUIContent[NumberOfOptions];
            areaMaskOptions = new string[NumberOfOptions];
            indexValues = new int[NumberOfOptions];
        }

        private void CreateDefaultOptions()
        {
            for (int i = 0; i < NumberOfOptions; i++)
            {
                areaOptions[i] = new GUIContent("Area " + i);
            }
            for (int i = 0; i < NumberOfOptions; i++)
            {
                layerOptions[i] = new GUIContent("Layer " + i);
            }
        }

        private bool TryCreateOptionsBasedOnSystem()
        {
            bool result = false;
            var system = TryGetSystem();
            if(system != null)
            {
                var systemSerializedObject = new SerializedObject(system);
                var settingsProperty = systemSerializedObject.FindProperty("settings");
                RecalculateAreasOptions(settingsProperty);
                RecalculateLayersOptions(settingsProperty);
                result = true;
            }
            return result;
        }

        private void CreateIndexValues()
        {
            for (int i = 0; i < NumberOfOptions; i++)
            {
                indexValues[i] = i;
            }
        }
        private void CreateAreaMaskOptions()
        {
            for (int i = 0; i < NumberOfOptions; i++)
            {
                areaMaskOptions[i] = areaOptions[i].text;
            }
        }

        private void RecalculateLayersOptions(SerializedProperty settingsProperty)
        {
            var layersProperty = settingsProperty.FindPropertyRelative("layers");
            int layersCount = layersProperty.arraySize;
            for (int i = 0; i < NumberOfOptions; i++)
            {
                string content;
                if(i < layersCount)
                {
                    content = $"{i}: {layersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue}";
                }
                else
                {
                    content = $"{i}: NOT DEFINED";
                }
                layerOptions[i] = new GUIContent(content);
            }
        }
        private void RecalculateAreasOptions(SerializedProperty settingsProperty)
        {
            var areasProperty = settingsProperty.FindPropertyRelative("areas");
            for (int i = 0; i < NumberOfOptions; i++)
            {
                var content = $"{i}: {areasProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue}";
                areaOptions[i] = new GUIContent(content);
            }
        }
        

        public int DrawLayerIndexPopup(string label, int index)
        {
            return EditorGUILayout.Popup(new GUIContent(label), index, layerOptions);
        }
        public void DrawLayerIndexPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.IntPopup(position, property, layerOptions, indexValues, label);
        }

        public int DrawAreaIndexPopup(string label, int index)
        {
            return EditorGUILayout.Popup(new GUIContent(label), index, areaOptions);
        }
        public void DrawAreaIndexPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.IntPopup(position, property, areaOptions, indexValues, label);
        }

        public int DrawAreaMaskPopup(string label, int mask)
        {
            return EditorGUILayout.MaskField(label, mask, areaMaskOptions);
        }

        public void DrawAreaMaskPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var scope = new EditorGUI.PropertyScope(position, label, property))
            {
                label = scope.content;
                EditorGUI.BeginChangeCheck();
                int newValue = EditorGUI.MaskField(position, label, property.intValue, areaMaskOptions);

                if (EditorGUI.EndChangeCheck())
                    property.intValue = newValue;
            }
        }
    }
}

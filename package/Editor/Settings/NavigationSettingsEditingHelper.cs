using AlchemyBow.Navigation.Settings;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor
{
    public class NavigationSettingsEditingHelper
    {
        public enum SettingsEditingMode { Areas, Layers }
        private readonly SerializedProperty settingsProperty;
        private readonly bool playModeReadonly;

        private SettingsEditingMode settingsEditingMode;
        private int selectedLayer;

        public NavigationSettingsEditingHelper(SerializedProperty settingsProperty, bool playModeReadonly)
        {
            settingsEditingMode = SettingsEditingMode.Layers;
            this.settingsProperty = settingsProperty;
            this.playModeReadonly = playModeReadonly;
        }

        public void DrawSettingsEditor(bool applyModifiedProperties)
        {
            DrawSettingsEditingSwitch();
            EditorGUI.BeginDisabledGroup(playModeReadonly && Application.isPlaying);
            if (settingsEditingMode == SettingsEditingMode.Areas)
            {
                DrawAreasSettings();
            }
            else if (settingsEditingMode == SettingsEditingMode.Layers)
            {
                DrawLayersSettings();
            }
            EditorGUI.EndDisabledGroup();
            if (applyModifiedProperties)
            {
                settingsProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawSettingsEditingSwitch()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(settingsEditingMode == SettingsEditingMode.Layers, "Layers", "Button"))
            {
                settingsEditingMode = SettingsEditingMode.Layers;
            }
            if (GUILayout.Toggle(settingsEditingMode == SettingsEditingMode.Areas, "Areas", "Button"))
            {
                settingsEditingMode = SettingsEditingMode.Areas;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAreasSettings()
        {
            bool areNamesUnique = true;
            var uniqueNames = new HashSet<string>();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(60));
            EditorGUILayout.LabelField("Weight", GUILayout.MinWidth(40), GUILayout.MaxWidth(80));
            EditorGUILayout.LabelField("Name", GUILayout.MinWidth(70));
            EditorGUILayout.LabelField("Color", GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            var areasProperty = settingsProperty.FindPropertyRelative("areas");


            for (int i = 0; i < NavigationSettings.AreasCount; i++)
            {
                var areaProperty = areasProperty.GetArrayElementAtIndex(i);
                var nameProperty = areaProperty.FindPropertyRelative("name");
                var weightProperty = areaProperty.FindPropertyRelative("weight");
                var colorProperty = areaProperty.FindPropertyRelative("color");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Area " + i + ":", GUILayout.Width(60));

                weightProperty.floatValue = EditorGUILayout.FloatField(weightProperty.floatValue, GUILayout.MinWidth(40), GUILayout.MaxWidth(80));
                nameProperty.stringValue = EditorGUILayout.TextField(nameProperty.stringValue, GUILayout.MinWidth(70));
                colorProperty.colorValue = EditorGUILayout.ColorField(colorProperty.colorValue, GUILayout.Width(40));

                EditorGUILayout.EndHorizontal();

                areNamesUnique &= uniqueNames.Add(nameProperty.stringValue);
            }
            if (!areNamesUnique)
            {
                EditorGUILayout.HelpBox("Names of areas should be unique.", MessageType.Warning);
            }
        }
        private void DrawLayersSettings()
        {
            bool areNamesUnique = true;
            var uniqueNames = new HashSet<string>();
            var layersProperty = settingsProperty.FindPropertyRelative("layers");
            int layersCount = layersProperty.arraySize;
            var layerOptionsNames = new string[layersCount];
            for (int i = 0; i < layersCount; i++)
            {
                string name = layersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                layerOptionsNames[i] = string.Format("Layer {0}: {1}", i, name);
                areNamesUnique &= uniqueNames.Add(name);
            }
            EditorGUILayout.Space();
            selectedLayer = EditorGUILayout.Popup("Select Layer",selectedLayer, layerOptionsNames);
            EditorGUILayout.PropertyField(layersProperty.GetArrayElementAtIndex(selectedLayer), new GUIContent("Selected Layer"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Layer"))
            {
                layersCount++;
                layersProperty.arraySize = layersCount;
                var newLayer = layersProperty.GetArrayElementAtIndex(layersCount - 1);
                newLayer.FindPropertyRelative("name").stringValue = "new layer";
                newLayer.FindPropertyRelative("connectionRadius").floatValue = .1f;
                selectedLayer = layersCount - 1;
            }
            if (layersCount > 1 && GUILayout.Button("Remove Layer"))
            {
                layersProperty.DeleteArrayElementAtIndex(selectedLayer);
                selectedLayer = 0;
            }
            EditorGUILayout.EndHorizontal();

            if (!areNamesUnique)
            {
                EditorGUILayout.HelpBox("Names of layers should be unique.", MessageType.Warning);
            }
        }
    } 
}

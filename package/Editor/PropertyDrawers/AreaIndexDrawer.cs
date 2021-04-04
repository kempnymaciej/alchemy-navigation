using AlchemyBow.Navigation.PropertyAttributes;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AreaIndexAttribute))]
    public class AreaIndexDrawer : PropertyDrawer
    {
        private NavigationSettingsConnector settingsConnector;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (settingsConnector == null)
            {
                settingsConnector = NavigationSettingsConnector.Create();
            }
            settingsConnector.DrawAreaIndexPopup(position, property, label);
        }
    }
}
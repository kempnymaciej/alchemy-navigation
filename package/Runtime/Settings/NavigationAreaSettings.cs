using UnityEngine;

namespace AlchemyBow.Navigation.Settings
{
    /// <summary>
    /// Describes the settings for a specific area.
    /// </summary>
    [System.Serializable]
    public sealed class NavigationAreaSettings
    {
        private const float MinWeight = 1;

        [SerializeField]
        private string name;
        [SerializeField, Min(MinWeight)]
        private float weight;
        [SerializeField]
        private Color color;

        /// <summary>
        /// Creates a new instance of the NavigationAreaSettings class.
        /// </summary>
        /// <param name="name">A user-friendly name to appear in the Unity Editor.</param>
        /// <param name="weight">The greater the weight, the less frequently paths through the certain area are chosen. (1 is a standard value.)</param>
        /// <param name="color">The preview color.</param>
        public NavigationAreaSettings(string name, float weight, Color color)
        {
            this.name = name;
            this.weight = weight > MinWeight ? weight : MinWeight;
            this.color = color;
        }

        /// <summary>
        /// A user-friendly name to appear in the Unity Editor.
        /// </summary>
        /// <returns>The user-friendly name of the area.</returns>
        public string Name { get => name; }

        /// <summary>
        /// The greater the weight, the less frequently paths through the certain area are chosen. (1 is a standard value.)
        /// </summary>
        /// <returns>The weight of the area.</returns>
        public float Weight { get => weight; }

        /// <summary>
        /// The preview color.
        /// </summary>
        /// <returns>The color of the area.</returns>
        public Color Color { get => color; }

        /// <summary>
        /// Creates the default instance of the NavigationAreaSettings class.
        /// </summary>
        /// <param name="index">The index of the area.</param>
        /// <returns>A new instance of the NavigationAreaSettings class with default values.</returns>
        public static NavigationAreaSettings CreateDefault(int index)
        {
            return new NavigationAreaSettings("Area " + index, 1, GetAutoColor(index));
        }

        private static Color GetAutoColor(int index)
        {
            //TODO: Introduce a greater variety of colors.
            return Color.Lerp(Color.green, Color.cyan, (float)index / NavigationSettings.AreasCount);
        }
    } 
}

using UnityEngine;

namespace AlchemyBow.Navigation.Settings
{
    /// <summary>
    /// Describes the navigations settings.
    /// </summary>
    [System.Serializable]
    public class NavigationSettings
    {
        /// <summary>
        /// The number of areas.
        /// </summary>
        /// <returns>The number of areas.</returns>
        public const int AreasCount = 32;
        /// <summary>
        /// The maximum number of layers.
        /// </summary>
        /// <returns>The maximum number of layers.</returns>
        public const int MaxLayersCount = 32;

        [SerializeField]
        private NavigationLayerSettings[] layers = null;
        [SerializeField]
        private NavigationAreaSettings[] areas = null;

        /// <summary>
        /// The current number of the layers.
        /// </summary>
        /// <returns>The current number of the layers.</returns>
        public int LayersCount => layers.Length;

        /// <summary>
        /// Gets the settings of the specified layer.
        /// </summary>
        /// <param name="index">The index of the layer.</param>
        /// <returns>The settings of the specified layer.</returns>
        public NavigationLayerSettings GetLayerSettings(int index) => layers[index];

        /// <summary>
        /// Gets the settings of the specified area.
        /// </summary>
        /// <param name="index">The index of the area.</param>
        /// <returns>The settings of the specified area.</returns>
        public NavigationAreaSettings GetAreaSettings(int index) => areas[index];

        /// <summary>
        /// Creates the default instance of the NavigationSettings class.
        /// </summary>
        public NavigationSettings()
        {
            layers = new NavigationLayerSettings[1];
            layers[0] = NavigationLayerSettings.CreateDefault();
            areas = new NavigationAreaSettings[AreasCount];
            for (int i = 0; i < AreasCount; i++)
            {
                areas[i] = NavigationAreaSettings.CreateDefault(i);
            }
        }
    } 
}

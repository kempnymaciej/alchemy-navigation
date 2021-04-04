using UnityEngine;

namespace AlchemyBow.Navigation.Settings
{
    /// <summary>
    /// A .asset holder for `NavigationSettings` presets.
    /// </summary>
    public sealed class AlchemyNavigationSettingsPreset : ScriptableObject
    {
        [SerializeField]
        private NavigationSettings settings = null;

        /// <summary>
        /// The settings stored in the preset.
        /// </summary>
        /// <returns>The settings stored in the preset.</returns>
        public NavigationSettings Settings { get => settings; }
    } 
}

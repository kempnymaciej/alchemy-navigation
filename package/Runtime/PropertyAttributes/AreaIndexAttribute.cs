using UnityEngine;

namespace AlchemyBow.Navigation.PropertyAttributes
{
    /// <summary>
    /// An attribute that improves the `AreaIndex` fields in the editor.
    /// </summary>
    /// <remarks>
    /// If there is a `AlchemyNavigationSystem` in the scene, the names of the areas are fetched.
    /// </remarks>
    public sealed class AreaIndexAttribute : PropertyAttribute
    {
        public AreaIndexAttribute()
        {
        }
    }
}
using UnityEngine;

namespace AlchemyBow.Navigation.PropertyAttributes
{
    /// <summary>
    /// An attribute that improves the `AreaMask` fields in the editor.
    /// </summary>
    /// <remarks>
    /// If there is a `AlchemyNavigationSystem` in the scene, the names of the areas are fetched.
    /// </remarks>
    public sealed class AreaMaskAttribute : PropertyAttribute
    {
        public AreaMaskAttribute()
        {
        }
    }
}

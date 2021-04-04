using UnityEngine;

namespace AlchemyBow.Navigation.PropertyAttributes
{
    /// <summary>
    /// An attribute that improves the `LayerIndex` fields in the editor.
    /// </summary>
    /// <remarks>
    /// If there is a `AlchemyNavigationSystem` in the scene, the names of the layers are fetched.
    /// </remarks>
    public sealed class LayerIndexAttribute : PropertyAttribute
    {
        public LayerIndexAttribute()
        {
        }
    }
}

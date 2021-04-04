using UnityEngine;

namespace AlchemyBow.Navigation.Simple.Elements
{
    /// <summary>
    /// The interface of an object using the sterring behaviour.
    /// </summary>
    public interface ISteeredBehaviour
    {
        /// <summary>
        /// The velocity at which the object is moving.
        /// </summary>
        /// <returns>The velocity at which the object is moving.</returns>
        Vector3 Velocity { get; }
    } 
}

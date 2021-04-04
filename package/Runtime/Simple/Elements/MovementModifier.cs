using UnityEngine;

namespace AlchemyBow.Navigation.Simple.Elements
{
    /// <summary>
    /// An abstract component that can modify <c>SimpleAgent</c> motion by adding acceleration in a certain direction.
    /// </summary>
    public abstract class MovementModifier : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1)]
        private float strength = 1;

        /// <summary>
        /// Gets or sets the ratio between the acceleration added by the modifier and the maximum acceleration of the <c>SimpleAgent</c>.
        /// </summary>
        /// <returns>The ratio between the acceleration added by the modifier and the maximum acceleration of the <c>SimpleAgent</c>.</returns>
        public float Strength { get => strength; set => strength = Mathf.Clamp01(value); }

        /// <summary>
        /// Calculates the direction of the modifier acceleration.
        /// </summary>
        /// <returns>The direction of the modifier acceleration.</returns>
        public abstract Vector3 CalculateDirection();
    } 
}

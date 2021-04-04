using System;
using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// The wrapper class for a <c>Vector3</c> point.
    /// </summary>
    public class Vertex : UsageTrackingObject<Edge> , IComparable<Vertex>
    {
        private const int InitialUsersCapacity = 2;

        /// <summary>
        /// The vertex value.
        /// </summary>
        /// <returns>The vertex value.</returns>
        public readonly Vector3 value;

        /// <summary>
        /// The cached magnitude of the vector.
        /// </summary>
        /// <returns>The cached magnitude of the vector.</returns>
        public readonly float magnitude;

        /// <summary>
        /// Creates an instance of the Vertex class.
        /// </summary>
        /// <param name="value">A point in 3d space.</param>
        public Vertex(Vector3 value) : base(InitialUsersCapacity)
        {
            this.value = value;
            this.magnitude = value.magnitude;
        }

        private Vertex(float magnitude) : base(0)
        {
            this.magnitude = magnitude;
        }

        /// <summary>
        /// Compares the current instance with another instance of the <c>Vertex</c> class based on cached magnitude.
        /// </summary>
        /// <param name="other">The other instance to compare with this instance.</param>
        /// <returns>An integer that indicates whether the current instance precedes, follows, or occurs in the same position in the magnitude order.</returns>
        public int CompareTo(Vertex other)
        {
            float distance = (magnitude - other.magnitude);
            return distance > 0 ? 1 : distance < 0 ? -1 : 0;
        }


        /// <summary>
        /// Creates a fake instance of the Vertex class with specific magnitude.
        /// </summary>
        /// <param name="magnitude">The specific magnitude.</param>
        /// <returns>A fake instance of the Vertex class with specific magnitude.</returns>
        public static Vertex CreateMagnitudeStub(float magnitude)
        {
            return new Vertex(magnitude);
        }
    }
}

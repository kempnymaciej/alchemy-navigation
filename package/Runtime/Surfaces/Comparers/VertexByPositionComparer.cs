using AlchemyBow.Navigation.Collections;
using System;
using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Describes a comparer that allows you to find faces by position.
    /// </summary>
    public sealed class VertexByPositionComparer : OrderedSetRangeComparer<Vertex>
    {
        private readonly Vector3 position;
        private readonly float magnitude;
        private readonly float epsilonConnectionRadius;

        private Vertex result;

        /// <summary>
        /// Creates an instance of the VertexByPositionComparer class.
        /// </summary>
        /// <param name="position">The position to look for.</param>
        /// <param name="connectionRadius">The connection radius. (<c>NavigationSettings</c>)</param>
        public VertexByPositionComparer(Vector3 position, float connectionRadius)
        {
            this.position = position;
            magnitude = position.magnitude;
            epsilonConnectionRadius = connectionRadius + NavigationSurface.EpsilonThreshold;
        }

        /// <summary>
        /// The comparsion result.
        /// </summary>
        /// <returns>The comparsion result.</returns>
        public Vertex Result => result;

        /// <summary>
        /// Creates the minimum boundary.
        /// </summary>
        /// <returns>A fake vertex with fixed magnitude.</returns>
        protected override IComparable<Vertex> CreateMinBaundary()
        {
            return Vertex.CreateMagnitudeStub(magnitude - epsilonConnectionRadius);
        }

        /// <summary>
        /// Creates the maximum boundary.
        /// </summary>
        /// <returns>A fake vertex with fixed magnitude.</returns>
        protected override IComparable<Vertex> CreateMaxBaundary()
        {
            return Vertex.CreateMagnitudeStub(magnitude + epsilonConnectionRadius);
        }

        /// <summary>
        /// Called for each vertex in the range.
        /// </summary>
        /// <param name="value">The vertex.</param>
        public override void OnMove(Vertex value)
        {
            if(result == null && Vector3.Distance(position, value.value) < epsilonConnectionRadius)
            {
                result = value;
            }
        }
    }
}

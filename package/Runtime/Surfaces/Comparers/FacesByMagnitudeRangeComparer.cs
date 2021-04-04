using AlchemyBow.Navigation.Collections;
using System;
using System.Collections.Generic;

namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Describes a comparer that allows you to find faces within a specified range.
    /// </summary>
    public sealed class FacesByMagnitudeRangeComparer : OrderedSetRangeComparer<Vertex>
    {
        private readonly float min;
        private readonly float max;
        private readonly int areaMask;

        /// <summary>
        /// The comparsion result.
        /// </summary>
        /// <returns>The comparsion result.</returns>
        public readonly HashSet<Face> result;

        /// <summary>
        /// Creates an instance of the FacesByMagnitudeRangeComparer class.
        /// </summary>
        /// <param name="a">The magnitude baundary A.</param>
        /// <param name="b">The magnitude baundary B.</param>
        /// <param name="areaMask">An area mask that is used to selectively ignore faces.</param>
        public FacesByMagnitudeRangeComparer(float a, float b, int areaMask)
        {
            if(a < b)
            {
                min = a;
                max = b;
            }
            else
            {
                min = b;
                max = a;
            }
            this.areaMask = areaMask;
            result = new HashSet<Face>();
        }

        /// <summary>
        /// Creates the minimum boundary.
        /// </summary>
        /// <returns>A fake vertex with fixed magnitude.</returns>
        protected override IComparable<Vertex> CreateMinBaundary()
        {
            return Vertex.CreateMagnitudeStub(min);
        }

        /// <summary>
        /// Creates the maximum boundary.
        /// </summary>
        /// <returns>A fake vertex with fixed magnitude.</returns>
        protected override IComparable<Vertex> CreateMaxBaundary()
        {
            return Vertex.CreateMagnitudeStub(max);
        }

        /// <summary>
        /// Called for each vertex in the range.
        /// </summary>
        /// <param name="value">The vertex.</param>
        public override void OnMove(Vertex value)
        {
            int vertexUsersCount = value.UsersCount;
            for (int i = 0; i < vertexUsersCount; i++)
            {
                var edge = value.GetUser(i);
                int edgeUsersCount = edge.UsersCount;
                for (int j = 0; j < edgeUsersCount; j++)
                {
                    var face = edge.GetUser(j);
                    if((face.AreaMask & areaMask) > 0)
                    {
                        result.Add(face);
                    }
                }
            }
        }
    }
}

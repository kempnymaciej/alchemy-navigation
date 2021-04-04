using AlchemyBow.Navigation.Surfaces;
using UnityEngine;

namespace AlchemyBow.Navigation.DebugUnits
{
    /// <summary>
    /// A spectacular drawer component that uses Gizmos.
    /// </summary>
    public sealed class GizmosSurfaceDrawer : ISurfaceDrawer
    {
        private const float offsetLength = .06f;

        private readonly Color color;
        private readonly Edge[] edges;
        private readonly Vector3[] centers;

        private readonly int numberOfCenters;
        private readonly Vector3 offset;

        /// <summary>
        /// Creates a new instance of the GizmosSurfaceDrawer class.
        /// </summary>
        /// <param name="color">The color to be used.</param>
        /// <param name="edges">The edges to be drawn.</param>
        /// <param name="centers">The centers of faces to be drawn.</param>
        public GizmosSurfaceDrawer(Color color, Edge[] edges, Vector3[] centers)
        {
            this.color = color;
            this.edges = edges;
            this.centers = centers;
            this.numberOfCenters = centers.Length;
            this.offset = Vector3.up * offsetLength;
        }

        /// <summary>
        /// Draws surface with Gizmos.
        /// </summary>
        public void DrawSurface()
        {
            Gizmos.color = color;
            foreach (var edge in edges)
            {
                Gizmos.DrawLine(edge.a.value, edge.b.value);
            }
            for (int i = 0; i < numberOfCenters; i++)
            {
                Gizmos.DrawLine(centers[i], centers[i] + offset);
            }
        }
    }
}

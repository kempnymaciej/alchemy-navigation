using AlchemyBow.Navigation.Surfaces;
using UnityEngine;

namespace AlchemyBow.Navigation.DebugUnits
{
    /// <summary>
    /// An optimized drawer component that uses Gizmos.
    /// </summary>
    public sealed class MinimalGizmosSurfaceDrawer : ISurfaceDrawer
    {
        private readonly Edge[] edges;

        /// <summary>
        /// Creates a new instance of the MinimalGizmosSurfaceDrawer class.
        /// </summary>
        /// <param name="edges">The edges to be drawn.</param>
        public MinimalGizmosSurfaceDrawer(Edge[] edges)
        {
            this.edges = edges;
        }

        /// <summary>
        /// Draws surface with Gizmos.
        /// </summary>
        public void DrawSurface()
        {
            Gizmos.color = Color.gray;
            foreach (var edge in edges)
            {
                Gizmos.DrawLine(edge.a.value, edge.b.value);
            }
        }
    } 
}

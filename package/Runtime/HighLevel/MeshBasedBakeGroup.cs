using System.Collections.Generic;

namespace AlchemyBow.Navigation.HighLevel
{
    /// <summary>
    /// Describes a group of triangles during a baking process.
    /// </summary>
    public sealed class MeshBasedBakeGroup
    {
        /// <summary>
        /// The vertices used in the group.
        /// </summary>
        /// <returns>The vertices used in the group.</returns>
        public readonly HashSet<int> vertexIndices;

        /// <summary>
        /// The indices triangles used in the group.
        /// </summary>
        /// <returns>The indices triangles used in the group.</returns>
        /// <remarks>These are the indices of the triangles, not the triangles themselves.</remarks>
        public readonly HashSet<int> triangleIndices;

        /// <summary>
        /// Creates the instance of the MeshBasedBakeGroup class.
        /// </summary>
        public MeshBasedBakeGroup()
        {
            this.vertexIndices = new HashSet<int>();
            this.triangleIndices = new HashSet<int>();
        }
    } 
}

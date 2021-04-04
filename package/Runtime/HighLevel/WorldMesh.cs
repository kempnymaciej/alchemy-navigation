using UnityEngine;

namespace AlchemyBow.Navigation.HighLevel
{
    /// <summary>
    /// Describes a mesh with global coordinates.
    /// </summary>
    public sealed class WorldMesh
    {
        /// <summary>
        /// The vertices of the Mesh.
        /// </summary>
        /// <returns>The vertices of the Mesh.</returns>
        public readonly Vector3[] vertices;

        /// <summary>
        /// The triangles of the Mesh.
        /// </summary>
        /// <returns>The triangles of the Mesh.</returns>
        public readonly int[] triangles;

        /// <summary>
        /// The normals of the Mesh traingles.
        /// </summary>
        /// <returns>The normals of the Mesh traingles.</returns>
        public readonly Vector3[] triangleNormals;

        private WorldMesh(Vector3[] vertices, int[] triangles, Vector3[] triangleNormals)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.triangleNormals = triangleNormals;
        }

        /// <summary>
        /// Creates the instance of the WorldMesh class.
        /// </summary>
        /// <param name="meshFilter">A source of the mesh.</param>
        /// <returns>The instance of the NavigationSettings class</returns>
        public static WorldMesh Create(MeshFilter meshFilter)
        {
            var mesh = Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh;
            var transform = meshFilter.transform;
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var triangles = (int[])mesh.triangles.Clone();
            int numberOfTriangles = triangles.Length / 3;
            var triangleNormals = new Vector3[numberOfTriangles];

            for (int t = 0; t < numberOfTriangles; t++)
            {
                int index = 3 * t;
                Vector3 an = normals[triangles[index]];
                Vector3 bn = normals[triangles[index + 1]];
                Vector3 cn = normals[triangles[index + 2]];
                triangleNormals[t] = transform.TransformDirection((an + bn + cn) / 3);
            }

            int numberOfVertices = vertices.Length;
            for (int i = 0; i < numberOfVertices; i++)
            {
                vertices[i] = transform.TransformPoint(vertices[i]);
            }

            return new WorldMesh(vertices, triangles, triangleNormals);
        }
    } 
}

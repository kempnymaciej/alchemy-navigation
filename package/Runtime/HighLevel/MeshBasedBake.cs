using System.Collections.Generic;
using UnityEngine;

namespace AlchemyBow.Navigation.HighLevel
{
    /// <summary>
    /// Performs and holds a result of the baking process.
    /// </summary>
    public sealed class MeshBasedBake
    {
        private readonly MeshBasedBakeSettings settings;
        private readonly List<Vector3> vertices;
        private readonly List<int> triangles;

        /// <summary>
        /// Gets a duplicate of the vertices.
        /// </summary>
        /// <returns>A duplicate of the vertices.</returns>
        public Vector3[] Vertices => vertices.ToArray();

        /// <summary>
        /// Gets a duplicate of the triangles;
        /// </summary>
        /// <returns>A duplicate of the triangles.</returns>
        public int[] Triangles => triangles.ToArray();

        /// <summary>
        /// Gets the number of triangles that were omitted during the baking process. 
        /// </summary>
        /// <returns>The number of triangles that were omitted during the baking process.</returns>
        public int OmittedTriangles { get; private set; }

        private MeshBasedBake(MeshBasedBakeSettings settings)
        {
            this.settings = settings ?? new MeshBasedBakeSettings();
            vertices = new List<Vector3>();
            triangles = new List<int>();
        }

        /// <summary>
        /// Creates the instance of the MeshBasedBake class that holds the result of baking.
        /// </summary>
        /// <param name="sources">Mesh sources.</param>
        /// <param name="settings">A settings for the baking process.</param>
        /// <returns>The instance of the MeshBasedBake class that holds the result of baking.</returns>
        public static MeshBasedBake Bake(MeshFilter[] sources, MeshBasedBakeSettings settings)
        {
            var result = new MeshBasedBake(settings);

            int numberOfMeshes = sources.Length;
            for (int i = 0; i < numberOfMeshes; i++)
            {
                result.AddWorldMesh(WorldMesh.Create(sources[i]));
            }
            return result;
        }

        private void AddWorldMesh(WorldMesh mesh)
        {
            int numberOfTriangles = mesh.triangleNormals.Length;
            for (int t = 0; t < numberOfTriangles; t++)
            {
                if(Vector3.Angle(settings.upwards, mesh.triangleNormals[t]) > settings.maxSlope)
                {
                    OmittedTriangles++;
                    continue;
                }

                int index = 3 * t;
                int initialNumberOfVertices = vertices.Count;
                int a = FindOrAddVertex(mesh.vertices[mesh.triangles[index]]);
                int b = FindOrAddVertex(mesh.vertices[mesh.triangles[index+1]]);
                int c = FindOrAddVertex(mesh.vertices[mesh.triangles[index+2]]);

                if (IsValidTriangle(a, b, c) && !IsDuplicateTriangle(a,b,c))
                {
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);
                }
                else
                {
                    OmittedTriangles++;
                    if (c >= initialNumberOfVertices)
                        vertices.RemoveAt(c);
                    if (b >= initialNumberOfVertices)
                        vertices.RemoveAt(b);
                    if (a >= initialNumberOfVertices)
                        vertices.RemoveAt(a);
                }
            }
        }

        private bool IsValidTriangle(int a, int b, int c)
        {
            return a != b && b != c && c != a;
        }
        private bool IsDuplicateTriangle(int a, int b, int c)
        {
            int trianglesLength = triangles.Count;
            var vertices = new HashSet<int> { a, b, c };
            for (int i = 0; i < trianglesLength;)
            {
                int aa = i++;
                int bb = i++;
                int cc = i++;
                if (vertices.Contains(triangles[aa])
                    && vertices.Contains(triangles[bb])
                    && vertices.Contains(triangles[cc]))
                {
                    return true;
                }
            }
            return false;
        }

        private int FindOrAddVertex(Vector3 vertex)
        {
            int currentNumberOfVertices = vertices.Count;
            for (int i = 0; i < currentNumberOfVertices; i++)
            {
                if(Vector3.Distance(vertex, vertices[i]) < settings.connectionRadius)
                {
                    return i;
                }
            }
            vertices.Add(vertex);
            return currentNumberOfVertices;
        }

        /// <summary>
        /// Splits result triangles into groups by their connections.
        /// </summary>
        /// <returns>The result triangles splited into groups.</returns>
        public int[][] GroupTrianglesByConnection()
        {
            var groups = new HashSet<MeshBasedBakeGroup>();

            int numberOfTriangles = triangles.Count / 3;
            for (int t = 0; t < numberOfTriangles; t++)
            {
                var connectedGroups = FindConnectedGroups(t, groups);
                if (connectedGroups.Count == 0)
                {
                    var group = new MeshBasedBakeGroup();
                    AddTriangleToGroup(group, t);
                    groups.Add(group);
                }
                else if (connectedGroups.Count == 1)
                {
                    foreach (var connectedGroup in connectedGroups)
                    {
                        AddTriangleToGroup(connectedGroup, t);
                    }
                }
                else
                {
                    var group = new MeshBasedBakeGroup();
                    AddTriangleToGroup(group, t);
                    foreach (var connectedGroup in connectedGroups)
                    {
                        groups.Remove(connectedGroup);
                        group.vertexIndices.UnionWith(connectedGroup.vertexIndices);
                        group.triangleIndices.UnionWith(connectedGroup.triangleIndices);
                    }
                    groups.Add(group);
                }
            }

            return GrupsToArray(groups);
        }

        private void AddTriangleToGroup(MeshBasedBakeGroup group, int triangleIndex)
        {
            group.triangleIndices.Add(triangleIndex);
            triangleIndex *= 3;
            group.vertexIndices.Add(triangles[triangleIndex++]);
            group.vertexIndices.Add(triangles[triangleIndex++]);
            group.vertexIndices.Add(triangles[triangleIndex]);
        }

        private HashSet<MeshBasedBakeGroup> FindConnectedGroups(int triangleIndex,
            HashSet<MeshBasedBakeGroup> groups)
        {
            int index = 3 * triangleIndex;
            int a = index++;
            int b = index++;
            int c = index;
            var connectedGroups = new HashSet<MeshBasedBakeGroup>();
            foreach (var group in groups)
            {
                int connections = 0;
                if (group.vertexIndices.Contains(triangles[a]))
                    connections++;
                if (group.vertexIndices.Contains(triangles[b]))
                    connections++;
                if (group.vertexIndices.Contains(triangles[c]))
                    connections++;
                if (connections >= 2)
                {
                    connectedGroups.Add(group);
                }
            }
            return connectedGroups;
        }

        private int[][] GrupsToArray(HashSet<MeshBasedBakeGroup> groups)
        {
            int numberOfGroups = groups.Count;
            var result = new int[numberOfGroups][];
            int i = 0;
            foreach (var group in groups)
            {
                result[i] = RetriveTrianglesFromGroup(group);
                i++;
            }
            return result;
        }
        private int[] RetriveTrianglesFromGroup(MeshBasedBakeGroup group)
        {
            var triangles = this.triangles.ToArray();
            int numberOfTriangles = group.triangleIndices.Count;
            int resultSize = numberOfTriangles * 3;
            var result = new int[resultSize];
            int i = 0;
            foreach (int triangleIndex in group.triangleIndices)
            {
                System.Array.Copy(triangles, 3 * triangleIndex, result, i, 3);
                i+=3;
            }
            return result;
        }
    } 
}

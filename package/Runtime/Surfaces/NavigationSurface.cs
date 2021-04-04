using AlchemyBow.Navigation.Collections;
using AlchemyBow.Navigation.DebugUnits;
using AlchemyBow.Navigation.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Describes a navigation surface. (One of the system layers.)
    /// </summary>
    public sealed class NavigationSurface
    {
        /// <summary>
        /// The calculations offset.
        /// </summary>
        /// <returns>The calculations offset.</returns>
        public const float EpsilonThreshold = NavigationInfo.EpsilonOffset;

        private readonly float connectionRadius;
        private readonly float maxEdgeLength;

        private readonly OrderedSet<Vertex> vertices;
        private readonly Dictionary<NavigationFaceWrapper, Face> faces;

        private float EpsilonConnectionRadius => connectionRadius + EpsilonThreshold;

        /// <summary>
        /// Creates an instance of the NavigationSurface class.
        /// </summary>
        /// <param name="layerSettings">Settings of the surface.</param>
        public NavigationSurface(NavigationLayerSettings layerSettings)
        {
            this.vertices = new OrderedSet<Vertex>();
            this.faces = new Dictionary<NavigationFaceWrapper, Face>();
            this.connectionRadius = layerSettings.ConnectionRadius;
            this.maxEdgeLength = layerSettings.MaxEdgeLength;
        }

        #region Registration
        /// <summary>
        /// Registers a triangle as a face.
        /// </summary>
        /// <param name="a">Point a of the triangle.</param>
        /// <param name="b">Point b of the triangle.</param>
        /// <param name="c">Point c of the triangle.</param>
        /// <param name="handle">The face handle.</param>
        public void RegisterFace(Vector3 a, Vector3 b, Vector3 c, NavigationFaceWrapper handle)
        {
            var aVertex = FindOrCreateVertex(a);
            var bVertex = FindOrCreateVertex(b);
            var cVertex = FindOrCreateVertex(c);

            var aEdge = FindOrCreateEdge(aVertex, bVertex);
            var bEdge = FindOrCreateEdge(bVertex, cVertex);
            var cEdge = FindOrCreateEdge(cVertex, aVertex);

            var face = new Face(aEdge, bEdge, cEdge, handle);
            aEdge.AddUser(face);
            bEdge.AddUser(face);
            cEdge.AddUser(face);
            faces.Add(handle, face);
        }

        /// <summary>
        /// Unregisters the face with the handle.
        /// </summary>
        /// <param name="handle">The handle to the face.</param>
        public void UnregisterFace(NavigationFaceWrapper handle)
        {
            if (faces.TryGetValue(handle, out var face))
            {
                faces.Remove(handle);
                RemoveFaceUsageFromEdge(face.ab, face);
                RemoveFaceUsageFromEdge(face.bc, face);
                RemoveFaceUsageFromEdge(face.ca, face);
            }
            else
            {
                Debug.LogError("Unknown handle. Ignoring request.");
            }
        }

        #region Vertices
        private Vertex FindOrCreateVertex(Vector3 point)
        {
            var vertex = FindVertex(point);
            if (vertex == null)
            {
                vertex = new Vertex(point);
                vertices.Add(vertex);
            }

            return vertex;
        }

        private Vertex FindVertex(Vector3 point)
        {
            var comparer = new VertexByPositionComparer(point, connectionRadius);
            vertices.UseRangeComparer(comparer);
            return comparer.Result;
        }
        #endregion

        #region Edges
        private Edge FindOrCreateEdge(Vertex x, Vertex y)
        {
            var edge = FindEdge(x, y);
            if (edge == null)
            {
                edge = new Edge(x, y);
            }

            x.AddUser(edge);
            y.AddUser(edge);

            return edge;
        }

        private Edge FindEdge(Vertex x, Vertex y)
        {
            for (int i = 0; i < x.UsersCount; i++)
            {
                for (int j = 0; j < y.UsersCount; j++)
                {
                    if (x.GetUser(i) == y.GetUser(j))
                    {
                        return x.GetUser(i);
                    }
                }
            }
            return null;
        }
        #endregion

        #region UsageRemoval
        private void RemoveFaceUsageFromEdge(Edge edge, Face face)
        {
            edge.RemoveUser(face);
            RemoveEdgeUsageFromVertex(edge.a, edge);
            RemoveEdgeUsageFromVertex(edge.b, edge);
        }
        private void RemoveEdgeUsageFromVertex(Vertex vertex, Edge edge)
        {
            vertex.RemoveUser(edge);
            if (vertex.HasNoUsers)
            {
                vertices.Remove(vertex);
            }
        }
        #endregion 
        #endregion

        /// <summary>
        /// Determines whether the surface contains the face.
        /// </summary>
        /// <param name="face">The face to locate.</param>
        /// <returns><c>true</c> if the face is found in the surface; otherwise, <c>false</c>.</returns>
        public bool ContainsFace(Face face)
        {
            return faces.ContainsValue(face);
        }

        /// <summary>
        /// Raycasts all faces in the radius.
        /// </summary>
        /// <param name="ray">The starting point and direction of the ray.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="areaMask">An area mask that is used to selectively ignore faces when casting a ray.</param>
        /// <param name="hit">If true is returned, <c>hit</c> will contain more information about the hit.</param>
        /// <returns><c>true</c> if the ray intersects with any face; otherwise, <c>false</c>.</returns>
        public bool RaycastAllInRadius(Ray ray, float radius, int areaMask, out SurfaceRaycastHit hit)
        {
            //TODO: Optimization.
            hit = new SurfaceRaycastHit();

            ray.origin = ray.GetPoint(-EpsilonConnectionRadius);
            float originMagnitude = ray.origin.magnitude;
            float offset = radius + maxEdgeLength + 2 *EpsilonConnectionRadius;
            var comparer = new FacesByMagnitudeRangeComparer(originMagnitude - offset, originMagnitude + offset, areaMask);
            vertices.UseRangeComparer(comparer);

            foreach (var face in comparer.result)
            {
                if (face.Raycast(ray, out float distance) && distance < hit.distance)
                {
                    hit.face = face;
                    hit.distance = distance;
                }
            }

            if (hit.face != null)
            {
                hit.position = ray.GetPoint(hit.distance);
                hit.distance = hit.distance + EpsilonConnectionRadius;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the first Face under the position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="areaMask">An area mask that is used to selectively ignore faces when casting a ray.</param>
        /// <returns>The first Face under the position or <c>null</c>.</returns>
        public Face FindFirstFaceUnderPosition(Vector3 position, int areaMask)
        {
            //TODO: add support for different directions.
            var ray = new Ray(position, Vector3.down);
            if (RaycastAllInRadius(ray, 0, areaMask, out var hit))
            {
                return hit.face;
            }

            return null;
        }


        #region Debug

        /// <summary>
        /// Creates an optimized Gizmos drawer for the current state of the surface.
        /// </summary>
        /// <returns>An optimized Gizmos drawer for the current state of the surface.</returns>
        public ISurfaceDrawer CreateMinimalDrawer()
        {
            var edges = new HashSet<Edge>();
            foreach (var face in faces.Values)
            {
                edges.Add(face.ab);
                edges.Add(face.bc);
                edges.Add(face.ca);
            }
            return new MinimalGizmosSurfaceDrawer(edges.ToArray());
        }

        /// <summary>
        /// Creates a sophisticated Gizmos drawer for the current state of the surface.
        /// </summary>
        /// <param name="settings">The navigation settings of the system.</param>
        /// <returns>A sophisticated Gizmos drawer for the current state of the surface.</returns>
        public ISurfaceDrawer CreateAdvancedDrawer(NavigationSettings settings)
        {
            var centers = new List<Vector3>[NavigationSettings.AreasCount];
            var edges = new HashSet<Edge>[NavigationSettings.AreasCount];
            for (int i = 0; i < NavigationSettings.AreasCount; i++)
            {
                centers[i] = new List<Vector3>();
                edges[i] = new HashSet<Edge>();
            }

            foreach (var face in faces.Values)
            {
                int areaIndex = AreaMaskToIndex(face.AreaMask);
                edges[areaIndex].Add(face.ab);
                edges[areaIndex].Add(face.bc);
                edges[areaIndex].Add(face.ca);
                centers[areaIndex].Add(face.CalculateCenter());
            }

            var subDrawers = new List<GizmosSurfaceDrawer>();
            for (int i = 0; i < NavigationSettings.AreasCount; i++)
            {
                if(centers[i].Count > 0)
                {
                    var color = settings.GetAreaSettings(i).Color;
                    var subDrawer = new GizmosSurfaceDrawer(color,
                        edges[i].ToArray(), centers[i].ToArray());
                    subDrawers.Add(subDrawer);
                }
            }

            return new CompositeSurfaceDrawer(subDrawers.ToArray());
        }
        private static int AreaMaskToIndex(int mask)
        {
            for (int i = 0; i < NavigationSettings.AreasCount; i++)
            {
                if((mask & (1 << i)) != 0)
                {
                    return i;
                }
            }
            return 0;
        }
        #endregion
    } 
}

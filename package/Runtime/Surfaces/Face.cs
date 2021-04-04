using AlchemyBow.Navigation.Surfaces.SafeAccess;
using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces
{
    /// <summary>
    /// Describes three connected edges.
    /// </summary>
    public class Face : IImmutableFace
    {
        /// <summary>
        /// The first edge.
        /// </summary>
        /// <returns>The first edge.</returns>
        public readonly Edge ab;

        /// <summary>
        /// The second edge.
        /// </summary>
        /// <returns>The second edge.</returns>
        public readonly Edge bc;

        /// <summary>
        /// The third edge.
        /// </summary>
        /// <returns>The third edge.</returns>
        public readonly Edge ca;

        private readonly bool bcIsReversed;
        private readonly bool caIsReversed;

        /// <summary>
        /// The cached plane of the face.
        /// </summary>
        /// <returns>The cached plane of the face.</returns>
        public readonly Plane plane;

        /// <summary>
        /// The handle to the face.
        /// </summary>
        /// <returns>The handle to the face.</returns>
        public readonly NavigationFaceWrapper handle;

        /// <summary>
        /// Creates an instance of the Face class.
        /// </summary>
        /// <param name="e1">Any edge.</param>
        /// <param name="e2">Any edge.</param>
        /// <param name="e3">Any edge.</param>
        /// <param name="handle">The handle to the face.</param>
        public Face(Edge e1, Edge e2, Edge e3, NavigationFaceWrapper handle)
        {
            ab = e1;
            if (ab.b == e2.a || ab.b == e2.b)
            {
                bc = e2;
                ca = e3;
            }
            else
            {
                bc = e3;
                ca = e2;
            }

            if (ab.b == bc.b)
            {
                bcIsReversed = true;
                caIsReversed = bc.a == ca.b;
            }
            else
            {
                bcIsReversed = false;
                caIsReversed = bc.b == ca.b;
            }

            plane = new Plane(A, B, C);

            this.handle = handle;
        }

        #region IPhysicalFace
        /// <summary>
        /// Gets the first vertex of the face.
        /// </summary>
        /// <returns>The first vertex of the face.</returns>
        public Vector3 A => ab.a.value;

        /// <summary>
        /// Gets the second vertex of the face.
        /// </summary>
        /// <returns>The second vertex of the face.</returns>
        public Vector3 B => bcIsReversed ? bc.b.value : bc.a.value;

        /// <summary>
        /// Gets the third vertex of the face.
        /// </summary>
        /// <returns>The third vertex of the face.</returns>
        public Vector3 C => caIsReversed ? ca.b.value : ca.a.value;

        /// <summary>
        /// Gets the plane of the face.
        /// </summary>
        /// <returns>The plane of the face.</returns>
        public Plane Plane => plane;

        /// <summary>
        /// Gets the weight of the face.
        /// </summary>
        /// <returns>The weight of the face.</returns>
        public float Weight => handle.weight;

        /// <summary>
        /// Gets the index of the area to witch the face belongs, as a mask.
        /// </summary>
        /// <returns>The index of the area to witch the face belongs, as a mask.</returns>
        public int AreaMask => handle.areaMask;

        /// <summary>
        /// Raycasts the face.
        /// </summary>
        /// <param name="ray">The starting point and direction of the ray.</param>
        /// <param name="distance">If true is returned, <c>distance</c> will be the signed distance to the intersection point.</param>
        /// <returns><c>true</c> if the ray intersects with the face; otherwise, <c>false</c>.</returns>
        public bool Raycast(Ray ray, out float distance)
        {
            if (plane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);
                return IsPointInsideFace(point);
            }
            return false;
        }

        /// <summary>
        /// Determines whether an point is on the face.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="offset">Calculation offset. (optional) </param>
        /// <returns><c>true</c> if the is on the face; otherwise, <c>false</c>.</returns>
        public bool IsPointInsideFace(Vector3 point, float offset = 0)
        {
            float epsilonZero = -offset;
            Vector3 C; // vector perpendicular to triangle's plane 

            Vector3 edge0 = B - A;
            Vector3 vp0 = point - A;
            C = Vector3.Cross(edge0, vp0);
            if (Vector3.Dot(plane.normal, C) < epsilonZero) return false; // P is on the right side 

            Vector3 edge1 = this.C - B;
            Vector3 vp1 = point - B;
            C = Vector3.Cross(edge1, vp1);
            if (Vector3.Dot(plane.normal, C) < epsilonZero) return false; // P is on the right side 

            Vector3 edge2 = A - this.C;
            Vector3 vp2 = point - this.C;
            C = Vector3.Cross(edge2, vp2);
            if (Vector3.Dot(plane.normal, C) < epsilonZero) return false; // P is on the right side; 

            return true; 
        }
        #endregion

        /// <summary>
        /// The neighbour of the first edge or <c>null</c>.
        /// </summary>
        /// <returns>The neighbour of the first edge or <c>null</c></returns>
        public Face ABNeighbour => ab.GetOtherUser(this);

        /// <summary>
        /// The neighbour of the second edge or <c>null</c>.
        /// </summary>
        /// <returns>The neighbour of the second edge or <c>null</c>.</returns>
        public Face BCNeighbour => bc.GetOtherUser(this);

        /// <summary>
        /// The neighbour of the third edge or <c>null</c>.
        /// </summary>
        /// <returns>The neighbour of the third edge or <c>null</c>.</returns>
        public Face CANeighbour => ca.GetOtherUser(this);

        /// <summary>
        /// Gets the edge that is shared with the indicated face.
        /// </summary>
        /// <param name="other">The face to find a shared edge.</param>
        /// <returns>The shared edge or <c>null</c>.</returns>
        public Edge GetSharedEdge(Face other)
        {
            if (ab.HasUser(other)) return ab;
            if (bc.HasUser(other)) return bc;
            if (ca.HasUser(other)) return ca;
            return null;
        }

        /// <summary>
        /// Calculates the center of the face.
        /// </summary>
        /// <returns>The center of the face.</returns>
        public Vector3 CalculateCenter() => (A + B + C) / 3;
    } 
}

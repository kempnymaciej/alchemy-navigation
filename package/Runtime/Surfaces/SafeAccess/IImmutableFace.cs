using UnityEngine;

namespace AlchemyBow.Navigation.Surfaces.SafeAccess
{
    /// <summary>
    /// Threadsafe inteterface for the <c>Face</c> class.
    /// </summary>
    public interface IImmutableFace
    {
        /// <summary>
        /// Gets the first vertex of the face.
        /// </summary>
        /// <returns>The first vertex of the face.</returns>
        Vector3 A { get; }

        /// <summary>
        /// Gets the second vertex of the face.
        /// </summary>
        /// <returns>The second vertex of the face.</returns>
        Vector3 B { get; }

        /// <summary>
        /// Gets the third vertex of the face.
        /// </summary>
        /// <returns>The third vertex of the face.</returns>
        Vector3 C { get; }

        /// <summary>
        /// Gets the plane of the face.
        /// </summary>
        /// <returns>The plane of the face.</returns>
        Plane Plane { get; }

        /// <summary>
        /// Gets the weight of the face.
        /// </summary>
        /// <returns>The weight of the face.</returns>
        float Weight { get; }

        /// <summary>
        /// Gets the index of the area to witch the face belongs, as a mask.
        /// </summary>
        /// <returns>The index of the area to witch the face belongs, as a mask.</returns>
        int AreaMask { get; }

        /// <summary>
        /// Raycasts the face.
        /// </summary>
        /// <param name="ray">The starting point and direction of the ray.</param>
        /// <param name="distance">If true is returned, <c>distance</c> will be the signed distance to the intersection point.</param>
        /// <returns><c>true</c> if the ray intersects with the face; otherwise, <c>false</c>.</returns>
        bool Raycast(Ray ray, out float distance);

        /// <summary>
        /// Determines whether an point is on the face.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="offset">Calculation offset. (optional) </param>
        /// <returns><c>true</c> if the is on the face; otherwise, <c>false</c>.</returns>
        bool IsPointInsideFace(Vector3 point, float offset = 0);
    } 
}

using AlchemyBow.Navigation.Surfaces.SafeAccess;
using UnityEngine;


namespace AlchemyBow.Navigation.Simple.Elements
{
    /// <summary>
    /// A helper class to track the path progress of the <c>SimpleAgent</c>.
    /// </summary>
    public sealed class PathProgress
    {
        /// <summary>
        /// The number of waypoints.
        /// </summary>
        /// <returns>The number of waypoints.</returns>
        public readonly int pointsCount;

        /// <summary>
        /// The number of faces.
        /// </summary>
        /// <returns>The number of faces.</returns>
        public readonly int facesCount;

        /// <summary>
        /// The waypoints.
        /// </summary>
        /// <returns>The waypoints.</returns>
        public readonly Vector3[] pointPath;

        /// <summary>
        /// The faces.
        /// </summary>
        /// <returns>The faces.</returns>
        public readonly IImmutableFace[] facePath;

        /// <summary>
        /// The index of the current waypoint.
        /// </summary>
        /// <returns>The index of the current waypoint.</returns>
        public int currentPoint;

        /// <summary>
        /// The index of the current face.
        /// </summary>
        /// <returns>The index of the current face.</returns>
        public int currentFace;

        /// <summary>
        /// Creates an instance of the PathProgress class.
        /// </summary>
        /// <param name="currentPoint">The index of the current waypoint.</param>
        /// <param name="pointPath">The path of waypoints.</param>
        /// <param name="currentFace">The index of the current face.</param>
        /// <param name="facePath">The path of faces.</param>
        public PathProgress(int currentPoint, Vector3[] pointPath,
            int currentFace, IImmutableFace[] facePath)
        {
            this.pointsCount = pointPath.Length;
            this.currentPoint = currentPoint;
            this.pointPath = pointPath;
            this.facesCount = facePath.Length;
            this.currentFace = currentFace;
            this.facePath = facePath;
        }

        /// <summary>
        /// Determines whether the agent has reached the last waypoint.
        /// </summary>
        /// <returns><c>true</c> if the agent has reached the last waypoint; otherwise, <c>false</c>.</returns>
        public bool IsFinished => currentPoint >= pointsCount;

        /// <summary>
        /// Determines whether there is the next waypoint.
        /// </summary>
        /// <returns><c>true</c> if there is the next waypoint; otherwise, <c>false</c>.</returns>
        public bool HasNextPoint => currentPoint + 1 < pointsCount;

        /// <summary>
        /// Gets the current waypoint.
        /// </summary>
        /// <returns>The current waypoint.</returns>
        public Vector3 CurrentPoint => pointPath[currentPoint];

        /// <summary>
        /// Determines whether there is the next face.
        /// </summary>
        /// <returns><c>true</c> if there is the next face; otherwise, <c>false</c>.</returns>
        public bool HasNextFace => currentFace + 1 < facesCount;

        /// <summary>
        /// Gets the next face.
        /// </summary>
        /// <returns>The next face.</returns>
        public IImmutableFace NextFace => facePath[currentFace + 1];

        /// <summary>
        /// Gets the current face.
        /// </summary>
        /// <returns>The current face.</returns>
        public IImmutableFace CurrentFace => facePath[currentFace];

        /// <summary>
        /// Determines whether the current point is on the current face or on any of the following.
        /// </summary>
        /// <returns><c>true</c> if the current point is in the current face or in any of the following; otherwise, <c>false</c>.</returns>
        public bool IsCurrentPointOnCurrentOrNextFaces()
        {
            for (int i = currentFace; i < facesCount; i++)
            {
                if (facePath[i].IsPointInsideFace(pointPath[currentPoint]))
                    return true;
            }
            return false;
        }
    } 
}
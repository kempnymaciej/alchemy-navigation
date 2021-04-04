using AlchemyBow.Navigation.Surfaces.SafeAccess;
using UnityEngine;

namespace AlchemyBow.Navigation
{
    /// <summary>
    /// Describes a pathfinding request and holds its result.
    /// </summary>
    public sealed class PathfindingRequest
    {
        /// <summary>
        /// Describes the status of the pathfinding process calculation.
        /// </summary>
        public enum Status 
        {
            /// <summary>
            /// The request is waiting to be computed.
            /// </summary>
            Pending,
            /// <summary>
            /// The request was skipped and a different path has been computed for the requestor
            /// </summary>
            Skipped, //TODO: Implement or remove.
            /// <summary>
            /// The request is computed.
            /// </summary>
            Finished
        }

        /// <summary>
        /// Describes how to calculate a path.
        /// </summary>
        public enum PathType 
        {
            /// <summary>
            /// Both a face path and a waypoint path are computed. The waypoint path contains all important points.
            /// </summary>
            Exact,
            /// <summary>
            /// Both a face path and a waypoint path are computed. The waypoint path contains only corner points independent of the y axis.
            /// </summary>
            Optimized,
            /// <summary>
            /// Only the face path is computed.
            /// </summary>
            FaceOnly
        }

        /// <summary>
        /// The index of the layer on which to search for the path.
        /// </summary>
        /// <returns>The index of the layer on which to search for the path.</returns>
        public readonly int layer;

        /// <summary>
        /// The area mask that specyfies which faces should be considered during pathfinding.
        /// </summary>
        /// <returns>TThe area mask that specyfies which faces should be considered during pathfinding.</returns>
        public readonly int areaMask;

        /// <summary>
        /// Defines how close the agent center can get to edges of the navigation mesh.
        /// </summary>
        /// <returns>The navigation radius of the agent.</returns>
        public readonly float radius;

        /// <summary>
        /// The start point of a path.
        /// </summary>
        /// <returns>The start point of a path.</returns>
        public readonly Vector3 startPosition;

        /// <summary>
        /// The end point of a path.
        /// </summary>
        /// <returns>The end point of a path.</returns>
        public readonly Vector3 endPosition;

        /// <summary>
        /// How to calculate a path.
        /// </summary>
        /// <returns>The type of a path.</returns>
        public readonly PathType pathType;

        /// <summary>
        /// The start face of the path. (optional) 
        /// </summary>
        /// <returns>The start face of the path, or <c>null</c>.</returns>
        public readonly IImmutableFace startFace;

        /// <summary>
        /// The current status of the pathfinding process calculation.
        /// </summary>
        /// <returns>The current status of the pathfinding process calculation.</returns>
        public Status status;

        /// <summary>
        /// The result waypoint path.
        /// </summary>
        /// <returns>The result waypoint path, or <c>null</c>.</returns>
        public Vector3[] path;
        /// <summary>
        /// The result face path.
        /// </summary>
        /// <returns>The result face path, or <c>null</c>.</returns>
        public IImmutableFace[] facePath;

        /// <summary>
        /// Creates a new instance of the PathfindingRequest class.
        /// </summary>
        /// <param name="layer">Index of the layer on which to search for the path.</param>
        /// <param name="areaMask">The area mask that specyfies which faces should be considered during pathfinding.</param>
        /// <param name="radius">Defines how close the agent center can get to edges of the navigation mesh.</param>
        /// <param name="startPosition">The end point of a path.</param>
        /// <param name="endPosition">The end point of a path.</param>
        /// <param name="pathType">How to calculate a path.</param>
        /// <param name="startFace">The start face of the path. (optional)</param>
        public PathfindingRequest(int layer, int areaMask, float radius,
            Vector3 startPosition, Vector3 endPosition, PathType pathType, 
            IImmutableFace startFace = null)
        {
            this.layer = layer;
            this.areaMask = areaMask;
            this.radius = radius;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.pathType = pathType;
            this.startFace = startFace;

            status = Status.Pending;
        }
    }
}

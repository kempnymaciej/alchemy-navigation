using System;
using System.Collections.Generic;
using UnityEngine;
using AlchemyBow.Navigation.Utilities;
using AlchemyBow.Navigation.Surfaces;
using AlchemyBow.Navigation.Surfaces.SafeAccess;

namespace AlchemyBow.Navigation.BackgroundProcessing
{
    /// <summary>
    /// Describes a set of <c>PathfindingRequest</c> to be executed and a specified final task.
    /// </summary>
    public sealed class PathfindingProcess : IBackgroundCommand
    {
        private readonly Action onJoin;
        private readonly Dictionary<object, PathfindingRequest> requests;
        private readonly NavigationSurface[] layers;

        /// <summary>
        /// Creates a new instance of the PathfindingProcess class.
        /// </summary>
        /// <param name="requests">The requests to compute</param>
        /// <param name="layers">The layers available in the system.</param>
        /// <param name="onJoin">What to do when finished.</param>
        public PathfindingProcess(Dictionary<object, PathfindingRequest> requests
            , NavigationSurface[] layers, Action onJoin)
        {
            this.onJoin = onJoin;
            this.requests = requests;
            this.layers = layers;
        }

        /// <summary>
        /// Computes all requests.
        /// </summary>
        public void Execute()
        {
            //TODO: Make it paralleled.
            var funnel = new Funnel();
            foreach (var request in requests.Values)
            {
                var facePath = FaceAStar.FindFacePath(request, layers[request.layer]);
                if (facePath != null)
                {
                    request.facePath = facePath?.ToArray();
                    if (request.pathType != PathfindingRequest.PathType.FaceOnly)
                    {
                        Vector3 ensuredGoal = EnsurePoint(request.endPosition, facePath[facePath.Count - 1]);
                        if (facePath.Count == 1)
                        {
                            request.path = new Vector3[] { request.startPosition, ensuredGoal };
                        }
                        else
                        {
                            var channel = CellPathToChannel(facePath, request.startPosition, ensuredGoal, request.radius);
                            if(request.pathType == PathfindingRequest.PathType.Optimized)
                            {
                                request.path = funnel.Calculate3DPath(channel, false);
                            }
                            else if (request.pathType == PathfindingRequest.PathType.Exact)
                            {
                                request.path = funnel.Calculate3DPath(channel, true);
                            }
                            else
                            {
                                Debug.LogError("Not supported path type (?). " + request.pathType);
                            }
                        }
                    }
                }
                request.status = PathfindingRequest.Status.Finished;
            }
        }

        /// <summary>
        /// Invokes the final task.
        /// </summary>
        public void OnJoin()
        {
            onJoin.Invoke();
        }

        private Vector3 EnsurePoint(Vector3 point, IImmutableFace lastFace)
        {
            //TODO: Add support for different directions
            var ray = new Ray(point, Vector3.down);
            lastFace.Plane.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }

        private static Vector3[] CellPathToChannel(List<IImmutableFace> cellPath
            , Vector3 startPoint, Vector3 endPoint, float agentRadius)
        {
            int channelLength = 2 * cellPath.Count;
            int portalsCount = cellPath.Count - 1;
            var channel = new Vector3[channelLength];
            channel[0] = startPoint;
            channel[channelLength - 1] = endPoint;

            float agentDiameter = agentRadius + agentRadius;
            for (int i = 0; i < portalsCount; i++)
            {
                var sharedEdge = ((Face)cellPath[i]).GetSharedEdge((Face)cellPath[i + 1]);
                Vector3 a = sharedEdge.a.value;
                Vector3 b = sharedEdge.b.value;

                int portalToChanel = i + i + 1;
                if (Vector3.Distance(a, b) < agentDiameter)
                {
                    Vector3 edgeCenter = Vector3.Lerp(a, b, .5f);
                    channel[portalToChanel] = edgeCenter;
                    channel[portalToChanel + 1] = edgeCenter;
                }
                else
                {
                    channel[portalToChanel] = Vector3.MoveTowards(a, b, agentRadius);
                    channel[portalToChanel + 1] = Vector3.MoveTowards(b, a, agentRadius);
                }
            }
            return channel;
        }
    }
}

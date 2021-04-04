using AlchemyBow.Navigation.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlchemyBow.Navigation.Surfaces;
using AlchemyBow.Navigation.Surfaces.SafeAccess;

namespace AlchemyBow.Navigation.Utilities
{
    /// <summary>
    /// The face based path computation class. 
    /// </summary>
    public sealed class FaceAStar
    {
        private readonly int agentAreaMask;

        private readonly Face start;
        private readonly Face end;
        private readonly Vector3 endCenter;

        private readonly OrderedSet<FaceStarUnit> openSet;
        private readonly HashSet<Face> closeSet;

        private FaceStarUnit current;
        private Vector3 currentCenter;

        private List<IImmutableFace> result;

        /// <summary>
        /// Calculates the face path.
        /// </summary>
        /// <param name="request">The request to calculate.</param>
        /// <param name="surface">The surface to be used for calculations.</param>
        /// <returns>The face path in the form of a list.</returns>
        public static List<IImmutableFace> FindFacePath(PathfindingRequest request, NavigationSurface surface)
        {
            int agentAreaMask = request.areaMask;
            Face start;
            if(request.startFace != null)
            {
                start = (Face)request.startFace;
            }
            else
            {
                start = surface.FindFirstFaceUnderPosition(request.startPosition, agentAreaMask);
            }
            Face end = surface.FindFirstFaceUnderPosition(request.endPosition, agentAreaMask);

            if (start == null || end == null)
            {
                //Debug.Log((start == null) + " " +  (end == null) + " null null FaceAStar");
                return null;
            }
            else if (start == end)
            {
                //Debug.Log("start == end FaceAStar");
                return new List<IImmutableFace>() { start };
            }
            else
            {
                //Debug.Log("process FaceAStar");
                var process = new FaceAStar(start, end, agentAreaMask);
                process.FindFacePath();
                return process.result;
            }
        }

        private FaceAStar(Face start, Face end, int agentAreaMask)
        {
            this.agentAreaMask = agentAreaMask;
            this.start = start;
            this.end = end;
            this.endCenter = end.CalculateCenter();

            openSet = new OrderedSet<FaceStarUnit>();
            openSet.Add(new FaceStarUnit(start));
            closeSet = new HashSet<Face>();
        }

        private void FindFacePath()
        {
            //TODO: Optimization
            while (openSet.Any)
            {
                current = openSet.Min;
                currentCenter = current.face.CalculateCenter();

                openSet.Remove(current);
                closeSet.Add(current.face);

                if (current.face != end)
                {
                    HandleNeighbour(current.face.ABNeighbour);
                    HandleNeighbour(current.face.BCNeighbour);
                    HandleNeighbour(current.face.CANeighbour);
                }
                else
                {
                    RetraceCellPath(current);
                    return;
                }
            }
        }

        private void HandleNeighbour(Face neighbour)
        {
            if (neighbour == null || (neighbour.AreaMask & agentAreaMask) == 0 || closeSet.Contains(neighbour))
            {
                return;
            }
            var neighbourCenter = neighbour.CalculateCenter();

            float neighbourDistance = Vector3.Distance(currentCenter, neighbourCenter);
            float neighbourDistanceCostBonus = (neighbour.Weight * neighbourDistance - neighbourDistance) / 2;

            float possibleGCost = current.gCost + neighbourDistance + neighbourDistanceCostBonus;

            var neighbourSetElement = openSet.BruteFind((e) => e.face == neighbour);

            if (neighbourSetElement == null)
            {
                openSet.Add(new FaceStarUnit(neighbour, current, possibleGCost, Vector3.Distance(neighbourCenter, endCenter) + neighbourDistanceCostBonus));
            }
            else if (possibleGCost < neighbourSetElement.gCost)
            {
                openSet.Remove(neighbourSetElement);
                neighbourSetElement.Update(current, possibleGCost, Vector3.Distance(neighbourCenter, endCenter) + neighbourDistanceCostBonus);
                openSet.Add(neighbourSetElement);
            }
        }
        private void RetraceCellPath(FaceStarUnit end)
        {
            if(start == end.face)
            {
                result = new List<IImmutableFace>() { start };
            }
            else
            {
                result = new List<IImmutableFace>();
                var current = end;
                while (current.face != start)
                {
                    result.Add(current.face);
                    current = current.parent;
                }
                result.Add(current.face);
                result.Reverse();
            }
        }
    } 
}

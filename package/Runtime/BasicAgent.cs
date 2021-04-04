using AlchemyBow.Navigation.PropertyAttributes;
using AlchemyBow.Navigation.Surfaces.SafeAccess;
using System.Collections.Generic;
using UnityEngine;

namespace AlchemyBow.Navigation
{
    /// <summary>
    /// Describes an abstract unit to handle pathfinding requests and communice with <c>AlchemyNavigationSystem</c>.
    /// </summary>
    public abstract class BasicAgent : MonoBehaviour
    {
        [SerializeField, LayerIndex]
        private int layer = 0;
        [SerializeField, AreaMask]
        private int areaMask = -1;
        [SerializeField, Min(0)]
        private float radius = .1f;

        private PathfindingRequest delayedRequest;
        private List<PathfindingRequest> requests = new List<PathfindingRequest>();

        /// <summary>
        /// Defines how close the agent center can get to edges of the navigation mesh.
        /// </summary>
        /// <returns> The radius of the agent.</returns>
        public float Radius => radius; //TODO: Ass set property.

        /// <summary>
        /// Determines whether the agent has destination.
        /// </summary>
        /// <returns><c>true</c> if the agent has destination; otherwise, <c>false</c>.</returns>
        public bool HasDestination => HasPendingRequests || IsPathWalking;

        /// <summary>
        /// Determines whether the agent has pending requests.
        /// </summary>
        /// <returns><c>true</c> if the agent pending requests; otherwise, <c>false</c>.</returns>
        protected bool HasPendingRequests => requests.Count > 0 || delayedRequest != null;

        /// <summary>
        /// Determines whether the agent is walking along the path.
        /// </summary>
        /// <returns><c>true</c> if the agent is walking along the path.</returns>
        protected abstract bool IsPathWalking { get; }

        /// <summary>
        /// Gets or sets the target layer of the agent.
        /// </summary>
        /// <returns>The target layer of the agent.</returns>
        /// <remarks> If set value is changed, <c>OnLayerChanged()</c> is called.</remarks>
        public int Layer
        {
            get => layer;
            set
            {
                if (layer != value)
                {
                    layer = value;
                    OnLayerChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the area mask of the agent.
        /// </summary>
        /// <returns>The area mask of the agent.</returns>
        /// <remarks> If set value is changed, <c>OnAreaMaskChanged()</c> is called.</remarks>
        public int AreaMask
        {
            get => areaMask;
            set
            {
                if (areaMask != value)
                {
                    areaMask = value;
                    OnAreaMaskChanged();
                }
            }
        }

        private void OnEnable()
        {
            AlchemyNavigationSystem.OnPathfindingFinished += OnRequestsProcessed;
            AlchemyNavigationSystem.OnSurfaceAvailable += OnSurfaceAvailable;
            AlchemyNavigationSystem.OnSystemDeinitialized += OnConnectionWithSystemLost;
            OnAgentEnable();
            if (AlchemyNavigationSystem.IsSurfaceActive)
            {
                OnSurfaceAvailable();
            }
        }
        private void OnDisable()
        {
            AlchemyNavigationSystem.OnPathfindingFinished -= OnRequestsProcessed;
            AlchemyNavigationSystem.OnSurfaceAvailable -= OnSurfaceAvailable;
            AlchemyNavigationSystem.OnSystemDeinitialized -= OnConnectionWithSystemLost;

            CancelAllRequests();
            OnAgentDisable();
        }

        /// <summary>
        /// Schedules the path calculation.
        /// </summary>
        /// <param name="start">The start point of a path.</param>
        /// <param name="destination">The end point of a path.</param>
        /// <param name="pathType">The target type of a path.</param>
        /// <param name="startFace">(optional) The start face of the path.</param>
        protected void RequestPath(Vector3 start, Vector3 destination,
            PathfindingRequest.PathType pathType, IImmutableFace startFace = null)
        {
            var request = new PathfindingRequest(layer, areaMask, radius,
                start, destination, pathType, startFace);
            if (AlchemyNavigationSystem.IsSurfaceActive)
            {
                requests.Add(request);
                AlchemyNavigationSystem.Current.CreatePathfindingRequest(this, request);
            }
            else
            {
                delayedRequest = request;
            }
        }

        /// <summary>
        /// Cancels all requested path calculations.
        /// </summary>
        public void CancelAllRequests()
        {
            delayedRequest = null;
            requests.Clear();
            OnRequestsCanceled();
        }

        private void OnRequestsProcessed()
        {
            if (requests.Count == 0)
            {
                return;
            }

            int latestRequest = requests.Count - 1;
            for (int i = latestRequest; i >= 0; i--)
            {
                if (requests[i].status == PathfindingRequest.Status.Finished)
                {
                    var request = requests[i];
                    requests.RemoveRange(0, i + 1);
                    OnPath(request.path, request.facePath);
                    break;
                }
            }
        }

        private void OnSurfaceAvailable()
        {
            var delayedRequest = this.delayedRequest;
            this.delayedRequest = null;
            OnSurfaceAvailable(delayedRequest);
        }

        /// <summary>
        /// Called when the layer property is changed.
        /// </summary>
        protected abstract void OnLayerChanged();

        /// <summary>
        /// Called when the area mask property is changed.
        /// </summary>
        protected abstract void OnAreaMaskChanged();

        /// <summary>
        /// Called when the agent becomes enabled and active.
        /// </summary>
        protected abstract void OnAgentEnable();

        /// <summary>
        /// Called when the agent becomes disabled or inactive.
        /// </summary>
        protected abstract void OnAgentDisable();

        /// <summary>
        /// Called when one of the requested paths is calculated.
        /// </summary>
        /// <param name="path">The calculated path consisting of waypoints.</param>
        /// <param name="facePath">The calculated path consisting of faces.</param>
        protected abstract void OnPath(Vector3[] path, IImmutableFace[] facePath);

        /// <summary>
        /// Called when requests are canceled.
        /// </summary>
        protected abstract void OnRequestsCanceled();

        /// <summary>
        /// Called when the surface becomes available.
        /// </summary>
        /// <param name="delayedRequest">A request that was delayed until surface becomes available.</param>
        protected abstract void OnSurfaceAvailable(PathfindingRequest delayedRequest);

        /// <summary>
        /// Called when the system is deinitialized.
        /// </summary>
        /// <remarks>
        /// By default, sets the agent's game object inactive.
        /// </remarks>
        protected virtual void OnConnectionWithSystemLost()
        {
            gameObject.SetActive(false);
        }
    } 
}

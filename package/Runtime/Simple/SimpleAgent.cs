using UnityEngine;
using AlchemyBow.Navigation.Surfaces.SafeAccess;
using AlchemyBow.Navigation.Simple.Elements;

namespace AlchemyBow.Navigation.Simple
{
    /// <summary>
    /// A high level component that allows you to create characters that intelligently move along navigation meshes to their destinations.
    /// </summary>
    [SelectionBase]
    public sealed class SimpleAgent : BasicAgent, ISteeredBehaviour
    {
        private const PathfindingRequest.PathType PathType = PathfindingRequest.PathType.Optimized;
        private const float EpsilonOffset = NavigationInfo.EpsilonOffset;

        /// <summary>
        /// Raised when the agent reaches the target location.
        /// </summary>
        public event NavigationAct OnDestinationReached;

        [SerializeField, Min(EpsilonOffset)]
        private float stoppingDistance = .5f;

        [SerializeField]
        private float maxVelocity = 3;
        [SerializeField]
        private float maxAcceleration = 20;

        [SerializeField]
        private float rotationSpeed = 10;

        private Vector3 position;
        private Vector3 velocity;

        private MovementModifier[] movementModifiers;

        private void ApplyPosition() => transform.position = position;


        private PathProgress progress;

        private CachedFace currentFace;

        /// <summary>
        /// Determines whether an agent is walking along the path.
        /// </summary>
        /// <returns><c>true</c> if the agent is walking along the path; otherwise, <c>false</c>.</returns>
        protected override bool IsPathWalking => progress != null;

        /// <summary>
        /// Gets the velocity at which the agent is moving.
        /// </summary>
        /// <returns>The velocity at which the agent is moving.</returns>
        public Vector3 Velocity => velocity;

        /// <summary>
        /// Sets the target location.
        /// </summary>
        /// <param name="destination">The the target location.</param>
        /// <param name="stopCurrentMovement">Determines whether to stop the current motion or not.</param>
        public void SetDestination(Vector3 destination, bool stopCurrentMovement)
        {
            if (stopCurrentMovement)
            {
                CancelAllRequests();
            }
            if(currentFace != null)
            {
                RequestPath(position, destination, PathType, currentFace.source);
            }
            else
            {
                RequestPath(position, destination, PathType);
            }
        }

        #region BasicAgent overrides
        /// <summary>
        /// Called when the agent becomes enabled and active.
        /// </summary>
        protected override void OnAgentEnable()
        {
            currentFace = null;
            velocity = Vector3.zero;
            position = transform.position;
            ApplyPosition();
            ReloadMovementModifiers();
        }

        /// <summary>
        /// Called when the agent becomes disabled or inactive.
        /// </summary>
        protected override void OnAgentDisable()
        {
            
        }

        /// <summary>
        /// Called when the layer property is changed.
        /// </summary>
        protected override void OnLayerChanged()
        {
            //TODO: Implement OnLayerChanged behaviour.
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Called when the area mask property is changed.
        /// </summary>
        protected override void OnAreaMaskChanged()
        {
            //TODO: Implement OnAreaMaskChanged behaviour.
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Called when one of the requested paths is calculated.
        /// </summary>
        /// <param name="pointPath">The calculated path consisting of waypoints.</param>
        /// <param name="facePath">The calculated path consisting of faces.</param>
        protected override void OnPath(Vector3[] pointPath, IImmutableFace[] facePath)
        {
            if(currentFace == null)
            {
                Debug.LogError("The agent lost the track of the current cell.");
            }

            if(pointPath == null || facePath == null)
            {
                OnPathNotFound();
            }
            else
            {
                int facesCount = facePath.Length;
                for (int i = 0; i < facesCount; i++)
                {
                    if(facePath[i] == currentFace.source)
                    {
                        progress = new PathProgress(1, pointPath, i, facePath);
                        return;
                    }
                }

                progress = null;
                if (!HasPendingRequests)
                {
                    RequestPath(position, pointPath[pointPath.Length - 1], PathType, currentFace.source);
                }
            }
        }
        private void OnPathNotFound()
        {
            Debug.Log("No path found.");
            progress = null;
        }

        /// <summary>
        /// Called when requests are canceled.
        /// </summary>
        protected override void OnRequestsCanceled()
        {
            progress = null;
        }

        /// <summary>
        /// Called when the surface becomes available.
        /// </summary>
        /// <param name="delayedRequest">A request that was delayed until surface becomes available.</param>
        protected override void OnSurfaceAvailable(PathfindingRequest delayedRequest)
        {
            if(currentFace == null 
                || !AlchemyNavigationSystem.Current.ContainsFace(Layer, currentFace.source))
            {
                var ray = new Ray(position, Vector3.down);
                if(AlchemyNavigationSystem.Current.Raycast(ray, Layer, AreaMask, out var result))
                {
                    currentFace = new CachedFace(result.face);
                    position = result.position;
                    ApplyPosition();
                }
                else
                {
                    Debug.LogWarning("Agent couldn't be placed on navigation surface.");
                    gameObject.SetActive(false);
                    return;
                }
            }

            if(delayedRequest != null)
            {
                CancelAllRequests();
                RequestPath(position, delayedRequest.endPosition, PathType, currentFace.source);
            }
            else if(IsPathWalking)
            {
                Vector3 destination = progress.pointPath[progress.pointsCount - 1];
                CancelAllRequests();
                RequestPath(position, destination, PathType, currentFace.source);
            }
        }
        #endregion

        private void Update()
        {
            if(currentFace == null)
            {
                return;
            }

            Vector3 acceleration;

            if(progress == null)
            {
                if(velocity != Vector3.zero)
                    acceleration = Vector3.ClampMagnitude(-velocity, maxAcceleration);
                else
                    acceleration = Vector3.zero;
            }
            else
            {
                if (progress.IsFinished)
                {
                    progress = null;
                    if (!HasPendingRequests)
                    {
                        RaiseOnDestinationReached();
                    }
                    
                    Update();
                    return;
                }
                if (Vector3.Distance(position, progress.CurrentPoint) < stoppingDistance)
                {
                    progress.currentPoint++;
                    Update();
                    return;
                }

                acceleration = CalculateAccelerationToSeek(position, progress.CurrentPoint, !progress.HasNextPoint);
            }

            acceleration = ApplyModifiers(acceleration);
            velocity += acceleration * Time.deltaTime;
            Debug.DrawRay(transform.position, velocity);
            if(velocity.sqrMagnitude > .0001f)
            {
                velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
                Move(velocity * Time.deltaTime);
                ApplyPosition();
                transform.forward = Vector3.MoveTowards(transform.forward, new Vector3(velocity.x, 0, velocity.z), rotationSpeed * Time.deltaTime);
            }
            else
            {
                velocity = Vector3.zero;
            }
        }

        private void RaiseOnDestinationReached()
        {
            OnDestinationReached?.Invoke();
        }

        private Vector3 CalculateAccelerationToSeek(Vector3 current, Vector3 target, bool stopping)
        {
            Vector3 desireForce = (target - current);
            if (!stopping)
            {
                desireForce = desireForce.normalized * maxVelocity;
            }
            Vector3 steeringForce = desireForce - velocity;
            return Vector3.ClampMagnitude(steeringForce, maxAcceleration);
        }

        private Vector3 ApplyModifiers(Vector3 acceleration)
        {
            if(movementModifiers != null)
            {
                foreach (var modifier in movementModifiers)
                {
                    acceleration += maxAcceleration * modifier.Strength * modifier.CalculateDirection();
                }
                acceleration = Vector3.ClampMagnitude(acceleration, maxAcceleration);
            }
            return acceleration;
        }

        private void Move(Vector3 move)
        {
            Vector3 displacement = Vector3.ProjectOnPlane(move, currentFace.plane.normal);

            Vector3 potentialPosition = position + displacement;
            if (currentFace.source.IsPointInsideFace(potentialPosition, EpsilonOffset))
            {
                position = potentialPosition;
            }
            else
            {
                Vector3 clampedPosition = currentFace.SnapToFace(position, displacement);
                Vector3 clampedDisplacement = clampedPosition - position;
                position = clampedPosition;
                Vector3 cutDisplacement = displacement.normalized * (displacement.magnitude - clampedDisplacement.magnitude); // + 2 * EpsilonOffset
                if (progress != null && progress.HasNextFace && progress.NextFace.IsPointInsideFace(position, EpsilonOffset))
                {
                    progress.currentFace++;
                    currentFace = new CachedFace(progress.CurrentFace);
                    var ray = new Ray(position, Vector3.down);
                    currentFace.plane.Raycast(ray, out float enter);
                    position = ray.GetPoint(enter);
                    while(progress.HasNextPoint && !progress.IsCurrentPointOnCurrentOrNextFaces())
                    {
                        progress.currentPoint++;
                    }
                    Move(cutDisplacement);
                }
                else
                {
                    position += currentFace.CastDisplacementOnClosestEdge(position, cutDisplacement);
                }
            }
        }

        /// <summary>
        /// Reloads all movement modifiers.
        /// </summary>
        /// <remarks>You need to call this method when movement modifiers are added/removed at run-time.</remarks>
        public void ReloadMovementModifiers()
        {
            movementModifiers = GetComponents<MovementModifier>();
            if(movementModifiers.Length == 0)
            {
                movementModifiers = null;
            }
        }

        #region Gizmos
#if UNITY_EDITOR
        [SerializeField]
        private bool drawGizmos = true;
        private void OnDrawGizmosSelected()
        {
            if (drawGizmos && progress != null && !progress.IsFinished)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, progress.CurrentPoint);
                for (int i = progress.currentPoint; i < progress.pointsCount - 1; i++)
                {
                    Gizmos.DrawLine(progress.pointPath[i], progress.pointPath[i + 1]);
                }

                Gizmos.color = Color.yellow;
                Vector3 offset = new Vector3(0, .01f, 0);
                for (int i = 0; i < progress.facesCount; i++)
                {
                    Vector3 a = progress.facePath[i].A + offset;
                    Vector3 b = progress.facePath[i].B + offset;
                    Vector3 c = progress.facePath[i].C + offset;
                    Gizmos.DrawLine(a, b);
                    Gizmos.DrawLine(b, c);
                    Gizmos.DrawLine(c, a);
                }


                if (currentFace != null)
                {
                    Gizmos.color = Color.green;
                    offset = new Vector3(0, .02f, 0);
                    Gizmos.DrawLine(currentFace.a, currentFace.b);
                    Gizmos.DrawLine(currentFace.b, currentFace.c);
                    Gizmos.DrawLine(currentFace.c, currentFace.a);
                }
            }
        }
#endif
        #endregion
    }
}

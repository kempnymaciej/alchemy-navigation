using AlchemyBow.Navigation.BackgroundProcessing;
using AlchemyBow.Navigation.Surfaces;
using AlchemyBow.Navigation.Surfaces.SafeAccess;
using AlchemyBow.Navigation.Settings;
using AlchemyBow.Navigation.Utilities;
using System.Collections.Generic;
using UnityEngine;
using AlchemyBow.Navigation.DebugUnits;

namespace AlchemyBow.Navigation
{
    /// <summary>
    /// This class contains main logic of <c>AlchemyNavigation</c> system.
    /// </summary>
    /// <remarks>
    /// This is strongly recommended to use <c>AlchemyNavigation.Current</c>.
    /// </remarks>
    public sealed class AlchemyNavigationSystem : MonoBehaviour
    {
        private static AlchemyNavigationSystem current;

        /// <summary>
        /// Raised whenever the <c>AlchemyNavigationSystem</c> instance is initialized.
        /// </summary>
        public static event NavigationAct OnSystemInitialized;

        /// <summary>
        /// Raised whenever the surface of the <c>AlchemyNavigationSystem</c> instance becomes available.
        /// </summary>
        public static event NavigationAct OnSurfaceAvailable;

        /// <summary>
        /// Raised whenever the surface of the <c>AlchemyNavigationSystem</c> instance becomes unavailable.
        /// </summary>
        public static event NavigationAct OnSurfaceUnavailable;

        /// <summary>
        /// Raised whenever the <c>AlchemyNavigationSystem</c> instance is deinitialized.
        /// </summary>
        public static event NavigationAct OnSystemDeinitialized;

        /// <summary>
        /// Raised whenever the <c>AlchemyNavigationSystem</c> instance finishes pathfinding calculations.
        /// </summary>
        public static event NavigationAct OnPathfindingFinished;

        /// <summary>
        /// Gets the current initialized instance of <c>AlchemyNavigationSystem</c> or <c>null</c>.
        /// </summary>
        /// <returns>
        /// The current initialized instance of <c>AlchemyNavigationSystem</c> or <c>null</c>.
        /// </returns>
        public static AlchemyNavigationSystem Current => current;

        /// <summary>
        /// Determines whether there is the initialized instance of <c>AlchemyNavigationSystem</c>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there is the initialized instance of <c>AlchemyNavigationSystem</c>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSystemInitialized => current != null;

        /// <summary>
        /// Determines whether there is the initialized instance of <c>AlchemyNavigationSystem</c> and its surface is available.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there is initialized instance of <c>AlchemyNavigationSystem</c> and its surface is available; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSurfaceActive => IsSystemInitialized && current.IsSurfaceAvailable;

        [SerializeField]
        private NavigationSettings settings = new NavigationSettings();
        [SerializeField]
        private AlchemyNavigationSystemDebugUnit debugUnit = new AlchemyNavigationSystemDebugUnit();

        private NavigationSurface[] layers;

        private Queue<ICommand> buildingQueue;
        private Dictionary<object, PathfindingRequest> pendingPathfindingRequests;
    

        private BackgroundProcessor backgroundProcessor;

        private bool isSurfaceAvailable;

        private bool IsSurfaceAvailable 
        {
            get => isSurfaceAvailable;
            set
            {
                debugUnit.OnIsSurfaceAvailableChange(value);
                
                isSurfaceAvailable = value;
                if (isSurfaceAvailable)
                {
                    OnSurfaceAvailable?.Invoke();
                }
                else
                {
                    OnSurfaceUnavailable?.Invoke();
                }
            } 
        }

        /// <summary>
        /// Gets the settings used by the instance of <c>AlchemyNavigationSystem</c>.
        /// </summary>
        /// <returns>
        /// The settings used by the instance of <c>AlchemyNavigationSystem</c>.
        /// </returns>
        public NavigationSettings Settings => settings;

        private void Awake()
        {
            if (!IsSystemInitialized)
            {
                Initialize();
            }
            else
            {
                gameObject.name += "_BrokenAlchemyNavigationSystem";
                throw new System.Exception("There is more than one active AlchemyNavigationSystem. Destroy previous system before adding another!");
            }
        }
        private void OnDestroy()
        {
            if(current == this)
            {
                Deinitialize();
            }
            else
            {
                Debug.LogError("AlchemyNavigationSystem whitch you are trying to destroy is not current. Make sure you don't have two AlchemyNavigationSystems.");
            }
        }

        private void Initialize()
        {
            backgroundProcessor = new BackgroundProcessor();
            buildingQueue = new Queue<ICommand>();
            pendingPathfindingRequests = new Dictionary<object, PathfindingRequest>();
            layers = new NavigationSurface[settings.LayersCount];
            for (int i = 0; i < settings.LayersCount; i++)
            {
                layers[i] = new NavigationSurface(settings.GetLayerSettings(i));
            }

            current = this;
            debugUnit.OnInitialize();
            OnSystemInitialized?.Invoke();
        }
        private void Deinitialize()
        {
            backgroundProcessor.AbortProcess();
            backgroundProcessor = null;

            layers = null;
            current = null;

            debugUnit.OnDeinitialize();
            OnSystemDeinitialized?.Invoke();
        }

        private void Update()
        {
            if (backgroundProcessor.GetState() == BackgroundProcessor.States.WaitingForJoin)
            {
                backgroundProcessor.JoinProcess();
            }

            if (backgroundProcessor.GetState() == BackgroundProcessor.States.ReadyToStart)
            {
                if(buildingQueue.Count > 0)
                {
                    StartBuildingProcess();
                }
                else if(pendingPathfindingRequests.Count > 0)
                {
                    StartPathfindingProcess();
                }
            }
        }

        private void StartBuildingProcess()
        {
            debugUnit.OnStartBuildingProcess(buildingQueue.Count);

            IsSurfaceAvailable = false;
            var process = new BuildingProcess(buildingQueue, OnBuildingProcesJoin);
            backgroundProcessor.StartProcess(process);
            
            buildingQueue = new Queue<ICommand>();
        }
        private void OnBuildingProcesJoin()
        {
            debugUnit.OnBuildingProcesJoin(layers, settings);
            IsSurfaceAvailable = true;
        }

        private void StartPathfindingProcess()
        {
            debugUnit.OnStartPathfindingProcess(pendingPathfindingRequests.Count);

            var process = new PathfindingProcess(pendingPathfindingRequests, layers, OnPathfindingProcessJoin);
            backgroundProcessor.StartProcess(process);
            pendingPathfindingRequests = new Dictionary<object, PathfindingRequest>();
        }
        private void OnPathfindingProcessJoin()
        {
            OnPathfindingFinished?.Invoke();
            debugUnit.OnPathfindingProcessJoin();
        }


        /// <summary>
        /// Registers a face in the system.
        /// </summary>
        /// <param name="a">Point a of the triangle.</param>
        /// <param name="b">Point b of the triangle.</param>
        /// <param name="c">Point c of the triangle.</param>
        /// <param name="areaIndex">Index of the area to which the triangle belongs.</param>
        /// <param name="layerIndex">Index of the layer to which the triangle belongs.</param>
        /// <returns>
        /// The handle that can by used to unregister the face later.
        /// </returns>
        /// <remarks>
        /// <para>Does not provide validation. The programmer's job is to ensure the correctness of the added elements.</para>
        /// <para>The face is not added, immediately.</para>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// Thrown when <c>areaIndex</c> or <c>layerIndex</c> are out of bounds.
        /// </exception>
        public NavigationFaceWrapper RegisterFace(Vector3 a, Vector3 b, Vector3 c, int areaIndex, int layerIndex)
        {
            if (areaIndex< 0 || areaIndex > 31)
            {
                throw new System.Exception("The area index is out of bounds.");
            }
            if (layerIndex < 0 || layerIndex >= layers.Length)
            {
                throw new System.Exception("The layer index is out of bounds.");
            }

            var handle = new NavigationFaceWrapper(layerIndex, settings.GetAreaSettings(areaIndex).Weight, 1 << areaIndex);
            buildingQueue.Enqueue(new RegisterFaceCommand(a, b, c, layers[layerIndex], handle));
            return handle;
        }

        /// <summary>
        /// Unregisters the face from the system.
        /// </summary>
        /// <param name="handle">The unique face handle that was received during registration.</param>
        /// <remarks>The face is not removed, immediately.</remarks>
        public void UnregisterFace(NavigationFaceWrapper handle)
        {
            buildingQueue.Enqueue(new UnregisterFaceCommand(handle, layers[handle.layerIndex]));
        }

        /// <summary>
        /// Schedules a <c>PathfindingRequest</c>.
        /// </summary>
        /// <param name="requestorKey">A unique identifier to distinguish requestors.</param>
        /// <param name="request">A request to schedule.</param>
        /// <remarks>
        /// If there are multiple requests from one requestor during one calculation circle, only the last request is calculated.
        /// </remarks>
        public void CreatePathfindingRequest(object requestorKey, 
            PathfindingRequest request)
        {
            pendingPathfindingRequests[requestorKey] = request;
        }

        /// <summary>
        /// Determines whether the system contains the specified face.
        /// </summary>
        /// <param name="layer">The index of the layer to check.</param>
        /// <param name="face">The face to locate.</param>
        /// <returns><c>true</c> if the surface is available and contains the specified face; otherwise, <c>false</c>.</returns>
        /// <remarks>Use only when the surface is available.</remarks>
        public bool ContainsFace(int layer, IImmutableFace face)
        {
            if (!isSurfaceAvailable)
            {
                Debug.LogWarning("This method cannot used while surface is not available.");
                return false;
            }
            var actualFace = face as Face;
            return actualFace != null && layers[layer].ContainsFace(actualFace);
        }

        /// <summary>
        /// Casts a ray against faces in the system and returns information on what was hit.
        /// </summary>
        /// <param name="ray">The starting point and direction of the ray.</param>
        /// <param name="layer">The index of layer to check.</param>
        /// <param name="areaMask">An area mask that is used to selectively ignore faces when casting a ray.</param>
        /// <param name="result">If <c>true</c> is returned, <c>result</c> will contain information about the hit.</param>
        /// <returns>
        /// <c>true</c> if the surface is available and the ray intersects any face; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>Use only when the surface is available.</remarks>
        public bool Raycast(Ray ray, int layer, int areaMask, 
            out SurfaceImmutableRaycastHit result)
        {
            result = null;
            if (!isSurfaceAvailable)
            {
                Debug.LogWarning("This method cannot used while surface is not available.");
                return false;
            }

            float raycastLength = settings.GetLayerSettings(layer).RaycastLength;
            if (layers[layer].RaycastAllInRadius(ray, raycastLength, areaMask, out var hit))
            {
                if (hit.distance <= raycastLength)
                {
                    result = hit.ToImmutable();
                    return true;
                }
            }
            return false;
        }

        [ContextMenu("ForceGizmosDrawerUpdate")]
        private void ForceGizmosDrawerUpdate()
        {
            if (IsSurfaceActive)
            {
                debugUnit.UpdateGizmosDrawer(layers, settings);
            }
        }

        private void OnDrawGizmos()
        {
            if (IsSurfaceActive)
            {
                debugUnit.OnSurfaceAvailableGizmos();
            }
        }
    }
}

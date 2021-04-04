using AlchemyBow.Navigation.Settings;
using AlchemyBow.Navigation.Surfaces;
using UnityEngine;

namespace AlchemyBow.Navigation.DebugUnits
{
    /// <summary>
    /// Handles `Alchemy Navigation System` debugging.
    /// </summary>
    [System.Serializable]
    public sealed class AlchemyNavigationSystemDebugUnit
    {
        [SerializeField, Tooltip("If true, logs some events to the console.")]
        private bool logEvents = false;
        [SerializeField, Tooltip("If true, logs in-depth information about the building process to the console.")]
        private bool logBuildingProcess = false;
        [SerializeField, Tooltip("If true, logs in-depth information about the pathfinding process to the console.")]
        private bool logPathfindingProcess = false;
        [SerializeField, Tooltip("If true, the surface is drawn by gizmos.")]
        private bool drawSurface = false;
        [SerializeField, Range(0, 31), Tooltip("The index of the layer to be drawn by gizmos.")]
        private int drawSurfaceLayerIndex = 0;
        [SerializeField, Tooltip("If true, the surface is drawn in a minimized form. (optimalization)")]
        private bool drawSurfaceMinimized = false;


        private float buildingProcessStartTime;
        private float pathfindingProcessStartTime;
        private ISurfaceDrawer surfaceDrawer;

        /// <summary>
        /// Called by the system when the surface becomes available or unavailable.
        /// </summary>
        /// <param name="value"><c>true</c> if the surface becomes available; otherwise, <c>false</c>.</param>
        public void OnIsSurfaceAvailableChange(bool value)
        {
            if (logEvents)
            {
                Debug.Log(value ? "Surface available." : "Surface unavailable.");
            }
        }

        /// <summary>
        /// Called by the system during initialization.
        /// </summary>
        public void OnInitialize()
        {
            if (logEvents)
            {
                Debug.Log("System initialized.");
            }
            surfaceDrawer = null;
        }

        /// <summary>
        /// Called by the system during deinitialization.
        /// </summary>
        public void OnDeinitialize()
        {
            if (logEvents)
            {
                Debug.Log("System deinitialized.");
            }
            surfaceDrawer = null;
        }

        /// <summary>
        /// Called by the system when a building process is started.
        /// </summary>
        /// <param name="numberOfOperations">The number of operations to be performed.</param>
        public void OnStartBuildingProcess(int numberOfOperations)
        {
            if (logBuildingProcess)
            {
                Debug.Log($"BuildingProcess started. Number of operations: {numberOfOperations}.");
            }
            surfaceDrawer = null;
            buildingProcessStartTime = Time.timeSinceLevelLoad;
        }

        /// <summary>
        /// Called by the system when a building process is joined.
        /// </summary>
        /// <param name="layers">The layer available in the system.</param>
        /// <param name="settings">The settings of the system.</param>
        public void OnBuildingProcesJoin(NavigationSurface[] layers, NavigationSettings settings)
        {
            if (logBuildingProcess)
            {
                Debug.Log("BuildingProcess finished after ~" +
                    (Time.timeSinceLevelLoad - buildingProcessStartTime));
            }

            UpdateGizmosDrawer(layers, settings);
        }

        /// <summary>
        /// Updates the drawing component.
        /// </summary>
        /// <param name="layers">The layer available in the system.</param>
        /// <param name="settings">The settings of the system.</param>
        public void UpdateGizmosDrawer(NavigationSurface[] layers, NavigationSettings settings)
        {
            if (drawSurface && drawSurfaceLayerIndex >= 0 && drawSurfaceLayerIndex < layers.Length)
            {
                if (drawSurfaceMinimized)
                {
                    surfaceDrawer = layers[drawSurfaceLayerIndex].CreateMinimalDrawer();
                }
                else
                {
                    surfaceDrawer = layers[drawSurfaceLayerIndex].CreateAdvancedDrawer(settings);
                }
            }
            else
            {
                surfaceDrawer = null;
            }
        }

        /// <summary>
        /// Called by the system when a pathfinding process is started.
        /// </summary>
        /// <param name="numberOfOperations">The number of operations to be performed.</param>
        public void OnStartPathfindingProcess(int numberOfOperations)
        {
            if (logPathfindingProcess)
            {
                Debug.Log($"PathfindingProcess started. Number of operations: {numberOfOperations}.");
            }
            pathfindingProcessStartTime = Time.timeSinceLevelLoad;
        }

        /// <summary>
        /// Called by the system when a pathfinding process is joined.
        /// </summary>
        public void OnPathfindingProcessJoin()
        {
            if (logPathfindingProcess)
            {
                Debug.Log("PathfindingProcess finished after ~" +
                    (Time.timeSinceLevelLoad - pathfindingProcessStartTime));
            }
        }

        /// <summary>
        /// Draws Gizmos.
        /// </summary>
        public void OnSurfaceAvailableGizmos()
        {
            if (drawSurface)
            {
                surfaceDrawer?.DrawSurface();
            }
        }
    } 
}

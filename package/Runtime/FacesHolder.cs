using UnityEngine;
using System;

namespace AlchemyBow.Navigation
{
    /// <summary>
    ///  A high level component that allows you to create predefined elements of navigation mesh, edit them and move them around the scene in both edit and play mode.
    /// </summary>
    /// <remarks> This class is not fully finished and will be optimized.</remarks>
    [ExecuteInEditMode, DisallowMultipleComponent]
    public sealed class FacesHolder : MonoBehaviour //TODO: Recreate with better optimization.
    {
        [SerializeField, Min(0)]
        private int areaIndex = 0; //runtime inspector change not allowed
        [SerializeField, Min(0)]
        private int layerIndex = 0; //runtime inspector change not allowed

        [SerializeField]
        private Vector3[] localNodes = null;

        private NavigationFaceWrapper[] wrappers;

        /// <summary>
        /// Gets or sets an area index of the faces.
        /// </summary>
        /// <remarks> If set value is different <c>ReregisterInSystem()</c> is called.</remarks>
        public int AreaIndex
        {
            get => areaIndex;
            set
            {
                if (areaIndex != value)
                {
                    areaIndex = value;
                    ReregisterInSystem();
                }
            }
        }

        /// <summary>
        /// Gets or sets a layer index of the faces.
        /// </summary>
        /// <remarks> If set value is different <c>ReregisterInSystem()</c> is called.</remarks>
        public int LayerIndex
        {
            get => layerIndex;
            set
            {
                if (layerIndex != value)
                {
                    layerIndex = value;
                    ReregisterInSystem();
                }
            }
        }

        public int NodesCount => localNodes.Length;
        public int FacesCount => localNodes.Length / NavigationInfo.NodesCount;

        #region NodesAndFacesGetters
        public Vector3 GetLocalNode(int index) => localNodes[index];
        public Vector3 GetWorldNode(int index) => transform.TransformPoint(localNodes[index]);
       
        public Vector3[] GetLocalFace(int index)
        {
            var nodes = new Vector3[NavigationInfo.NodesCount];
            GetLocalFaceNoAlloc(index, nodes);
            return nodes;
        }
        public void GetLocalFaceNoAlloc(int index, Vector3[] nodes)
        {
            int indexOffSet = index * NavigationInfo.NodesCount;
            for (int i = 0; i < NavigationInfo.NodesCount; i++)
            {
                nodes[i] = localNodes[indexOffSet + i];
            }
        }

        public Vector3[] GetWorldFace(int index)
        {
            var nodes = new Vector3[NavigationInfo.NodesCount];
            GetWorldFaceNoAlloc(index, nodes);
            return nodes;
        }
        public void GetWorldFaceNoAlloc(int index, Vector3[] nodes)
        {
            int indexOffSet = index * NavigationInfo.NodesCount;
            for (int i = 0; i < NavigationInfo.NodesCount; i++)
            {
                nodes[i] = transform.TransformPoint(localNodes[indexOffSet + i]);
            }
        }

        public Vector3[] GetAllLocalNodes()
        {
            var nodes = new Vector3[localNodes.Length];
            GetAllLocalNodesNoAlloc(nodes);
            return nodes;
        }
        public void GetAllLocalNodesNoAlloc(Vector3[] nodes)
        {
            int nodesCount = NodesCount;
            for (int i = 0; i < nodesCount; i++)
            {
                nodes[i] = localNodes[i];
            }
        }
        public Vector3[] GetAllWorldNodes()
        {
            var nodes = new Vector3[localNodes.Length];
            GetAllWorldNodesNoAlloc(nodes);
            return nodes;
        }
        public void GetAllWorldNodesNoAlloc(Vector3[] nodes)
        {
            int nodesCount = NodesCount;
            for (int i = 0; i < nodesCount; i++)
            {
                nodes[i] = transform.TransformPoint(localNodes[i]);
            }
        }
        #endregion

        #region NodesSetters
        public void SetNode(int index, Vector3 worldPosition)
        {
            SetNodeNoReregister(index, worldPosition);
            ReregisterInSystem();
        }
        public void SetNodeNoReregister(int index, Vector3 worldPosition)
        {
            localNodes[index] = transform.InverseTransformPoint(worldPosition);
        } 
        #endregion

        public void AddFace(Vector3 worldPositionA, Vector3 worldPositionB, Vector3 worldPositionC)
        {
            int newNodesCount = localNodes.Length + NavigationInfo.NodesCount;
            Array.Resize(ref localNodes, newNodesCount);
            SetNodeNoReregister(newNodesCount - 1, worldPositionC);
            SetNodeNoReregister(newNodesCount - 2, worldPositionB);
            SetNodeNoReregister(newNodesCount - 3, worldPositionA);
            ReregisterInSystem();
        }
        public void RemoveFace(int faceIndex)
        {
            if(FacesCount <= 1)
            {
                Debug.LogError("The holder must always have at least 1 face. This removal request will be ignored.");
            }
            else if (faceIndex >= FacesCount)
            {
                Debug.LogError("The target face is out of the holder bounds. This removal request will be ignored.");
            }
            else
            {
                int newNodesCount = localNodes.Length - NavigationInfo.NodesCount;
                if(faceIndex < FacesCount - 1)
                {
                    for (int i = faceIndex * NavigationInfo.NodesCount; i < newNodesCount; i++)
                    {
                        localNodes[i] = localNodes[i + NavigationInfo.NodesCount];
                    }
                }
                Array.Resize(ref localNodes, newNodesCount);
                ReregisterInSystem();
            }
        }

        #region CommonMonoBehaviourElements
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                transform.hasChanged = false;
                AlchemyNavigationSystem.OnSystemInitialized += OnSystemInitialized;
                if (AlchemyNavigationSystem.IsSystemInitialized)
                {
                    OnSystemInitialized();
                }
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                AlchemyNavigationSystem.OnSystemInitialized -= OnSystemInitialized;
                if (AlchemyNavigationSystem.IsSystemInitialized)
                {
                    if (wrappers != null)
                    {
                        foreach (var wrapper in wrappers)
                        {
                            AlchemyNavigationSystem.Current.UnregisterFace(wrapper);
                        } 
                    }
                }
                wrappers = null;
            }
        }

        private void Update()
        {
            if (Application.isPlaying && transform.hasChanged)
            {
                ReregisterInSystem();
            }
            transform.hasChanged = false;
        }

        private void Reset()
        {
            localNodes = new Vector3[]{
                new Vector3(1,  0,  1),
                new Vector3(1,  0, -1),
                new Vector3(-1, 0, -1),
                new Vector3(1,  0,  1),
                new Vector3(-1, 0, 1),
                new Vector3(-1, 0, -1)
            };

            if (Application.isPlaying)
            {
                ReregisterInSystem();
            }
        }
        #endregion

        private void OnSystemInitialized()
        {
            int facesCount = FacesCount;
            wrappers = new NavigationFaceWrapper[facesCount];
            int nodeIndex = 0;
            for (int i = 0; i < facesCount; i++)
            {
                wrappers[i] = AlchemyNavigationSystem.Current.RegisterFace(
                    transform.TransformPoint(localNodes[nodeIndex]),
                    transform.TransformPoint(localNodes[nodeIndex + 1]),
                    transform.TransformPoint(localNodes[nodeIndex + 2]),
                    AreaIndex, LayerIndex);
                nodeIndex += NavigationInfo.NodesCount;
            }
        }

        public void ReregisterInSystem()
        {
            if (Application.isPlaying && this.enabled && AlchemyNavigationSystem.IsSystemInitialized)
            {
                foreach (var wrapper in wrappers)
                {
                    AlchemyNavigationSystem.Current.UnregisterFace(wrapper);
                }
                int facesCount = FacesCount;
                int nodeIndex = 0;
                for (int i = 0; i < facesCount; i++)
                {
                    wrappers[i] = AlchemyNavigationSystem.Current.RegisterFace(
                        transform.TransformPoint(localNodes[nodeIndex]),
                        transform.TransformPoint(localNodes[nodeIndex + 1]),
                        transform.TransformPoint(localNodes[nodeIndex + 2]),
                        AreaIndex, LayerIndex);
                    nodeIndex += NavigationInfo.NodesCount;
                }
            }
        }
    } 
}

using UnityEngine;
using UnityEditor;
using AlchemyBow.Navigation.Editor.CellsHolderEditorElements;

namespace AlchemyBow.Navigation.Editor
{
    [CustomEditor(typeof(FacesHolder)), CanEditMultipleObjects]
    public class CellsHolderEditor : UnityEditor.Editor //TODO: Recreate with better optimization.
    {
        public enum EditingMode { None, Vertex, Edge, Extrude, Select}

        #region Constants
        private const float MinDisplayOtherCellsRadius = 10f;
        private const float MaxDisplayOtherCellsRadius = 100f;
        private const string EnableEditingPrefs = "ANCHE_EnableEditing";
        private const string DisplayOtherCellsInLayerPrefs = "ANCHE_DisplayOtherCellsInLayer";
        private const string IncludeInactiveCellsPrefs = "ANCHE_IncludeInactiveCells";
        private const string DisplayOtherCellsRadiusPrefs = "ANCHE_DisplayOtherCellsRadius";
        #endregion

        #region Colors
        public readonly Color selectedCellColor = Color.blue;
        public readonly Color notSelectedCellColor = new Color(.36f, .45f, .56f);
        public readonly Color otherCellColor = Color.gray;
        #endregion

        private NavigationSettingsConnector settingsConnector;

        private SerializedProperty areaIndexProperty;
        private SerializedProperty layerIndexProperty;


        private int selectedCellIndex;
        private Vector3[] nodes;
        private Vector3[] otherCellsAsNodes;

        private BasicCellsHolderEditorMode currentMode;

        public FacesHolder Holder => (FacesHolder)target;

        public int SelectedCellIndex
        {
            get => selectedCellIndex;
            set
            {
                var holder = (FacesHolder)target;
                if (value < holder.FacesCount)
                {
                    selectedCellIndex = value;
                }
                else
                {
                    Debug.LogError("Cell index out of bounds. Selecting 0 cell.");
                    selectedCellIndex = 0;
                }
            }
        }

        protected bool IsMultiEditing { get; private set; }

        private bool enableEditing;
        public bool EnableEditing 
        { 
            get => enableEditing; 
            private set
            {
                enableEditing = value;
                ChangeMode(value ? new CellsHolderEditorSelectMode(this) : null, false);
            }
        }


        public bool DisplayOtherCellsInLayer { get; private set; }
        public bool IncludeInactiveCells { get; private set; }
        public float DisplayOtherCellsRadius { get; private set; }

        private void PullEditorPrefs()
        {
            EnableEditing = EditorPrefs.GetBool(EnableEditingPrefs, false);
            DisplayOtherCellsInLayer = EditorPrefs.GetBool(DisplayOtherCellsInLayerPrefs, false);
            IncludeInactiveCells = EditorPrefs.GetBool(IncludeInactiveCellsPrefs, false);
            DisplayOtherCellsRadius = EditorPrefs.GetFloat(DisplayOtherCellsRadiusPrefs, MaxDisplayOtherCellsRadius);
            
        }
        private void PushEditorPrefs()
        {
            EditorPrefs.SetBool(EnableEditingPrefs, EnableEditing);
            EditorPrefs.SetBool(DisplayOtherCellsInLayerPrefs, DisplayOtherCellsInLayer);
            EditorPrefs.SetBool(IncludeInactiveCellsPrefs, IncludeInactiveCells);
            EditorPrefs.SetFloat(DisplayOtherCellsRadiusPrefs, DisplayOtherCellsRadius);
        }
        private void UpdateOtherCells()
        {
            if (DisplayOtherCellsInLayer)
            {
                var cellHolder = (FacesHolder)target;
                var allNodes = cellHolder.GetAllWorldNodes();
                int nodesCount = cellHolder.NodesCount;
                Vector3 center = Vector3.zero;
                for (int i = 0; i < nodesCount; i++)
                {
                    center += allNodes[i];
                }
                center /= nodesCount;
                float radiusExtend = 0;
                for (int i = 0; i < nodesCount; i++)
                {
                    float distance = Vector3.Distance(center, allNodes[i]);
                    if (radiusExtend < distance)
                    {
                        radiusExtend = distance;
                    }
                }

                otherCellsAsNodes = NavigationEditorTools.RetrieveCellsInRadiusAsNodes(
                        center, DisplayOtherCellsRadius + radiusExtend,
                        layerIndexProperty.intValue, IncludeInactiveCells, cellHolder);
            }
        }

        public void ChangeMode(BasicCellsHolderEditorMode mode, bool forceRepaintSceneGUI)
        {
            currentMode?.Deinit();
            currentMode = mode;
            currentMode?.Init();
            if (forceRepaintSceneGUI)
            {
                ForceRepaintSceneGUI();
            }
        }

        protected virtual void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndo;
            settingsConnector = NavigationSettingsConnector.Create();
            IsMultiEditing = serializedObject.isEditingMultipleObjects;
            areaIndexProperty = serializedObject.FindProperty("areaIndex");
            layerIndexProperty = serializedObject.FindProperty("layerIndex");

            nodes = new Vector3[4 * NavigationInfo.NodesCount];

            PullEditorPrefs();
            UpdateOtherCells();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        private void OnUndo()
        {
            var holder = (FacesHolder)target;
            if (selectedCellIndex >= holder.NodesCount / NavigationInfo.NodesCount)
            {
                selectedCellIndex = 0;
            }
            currentMode?.OnUndo();
        }

        #region Inspector
        public override void OnInspectorGUI()
        {
            DrawAreaAndLayerProperties();
            EditorGUILayout.Space();

            if (!IsMultiEditing)
            {
                DrawEditingSettingsInspectorGUI();
                EditorGUILayout.Space();

                DrawDisplayOtherCellsSettingsInspectorGUI();

                currentMode?.OnInspectorGUI();
            }
        }

        private void DrawAreaAndLayerProperties()
        {
            serializedObject.Update();
            EditorGUI.showMixedValue = areaIndexProperty.hasMultipleDifferentValues;
            int areaIndex = settingsConnector.DrawAreaIndexPopup("Area Index", areaIndexProperty.intValue);
            if (areaIndexProperty.intValue != areaIndex)
            {
                areaIndexProperty.intValue = areaIndex;
                OnAreaChange(areaIndex);
            }
            EditorGUI.showMixedValue = layerIndexProperty.hasMultipleDifferentValues;
            int layerIndex = settingsConnector.DrawLayerIndexPopup("Layer Index", layerIndexProperty.intValue);
            if (layerIndexProperty.intValue != layerIndex)
            {
                layerIndexProperty.intValue = layerIndex;
                OnLayerChange(layerIndex);
            }
            EditorGUI.showMixedValue = false;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEditingSettingsInspectorGUI()
        {
            bool dirty = false;
            if (GUILayout.Toggle(EnableEditing, "Enable Scene Editing", "Button") != EnableEditing)
            {
                EnableEditing = !EnableEditing;
                dirty = true;
            }
            if (EnableEditing)
            {
                DrawEditingModesInspectorGUI();
            }

            if (dirty)
            {
                PushEditorPrefs();
                ForceRepaintSceneGUI();
            }
        }

        private void DrawEditingModesInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var editingMode = EditingMode.None;
            if (currentMode != null)
            {
                editingMode = currentMode.editingMode;
            }

            if (GUILayout.Toggle(editingMode == EditingMode.Select, "Select", "Button")
                && editingMode != EditingMode.Select)
            {
                ChangeMode(new CellsHolderEditorSelectMode(this), true);
            }
            else if (GUILayout.Toggle(editingMode == EditingMode.Vertex, "Vertex", "Button")
                && editingMode != EditingMode.Vertex)
            {
                ChangeMode(new CellsHolderEditorVertexMode(this), true);
            }
            else if (GUILayout.Toggle(editingMode == EditingMode.Edge, "Edge", "Button")
                && editingMode != EditingMode.Edge)
            {
                ChangeMode(new CellsHolderEditorEdgeMode(this), true);
            }
            else if (GUILayout.Toggle(editingMode == EditingMode.Extrude, "Extrude", "Button")
                && editingMode != EditingMode.Extrude)
            {
                ChangeMode(new CellsHolderEditorExtrudeMode(this), true);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDisplayOtherCellsSettingsInspectorGUI()
        {
            bool dirty = false;
            if (GUILayout.Toggle(DisplayOtherCellsInLayer, "Display Other Cells In Layer") != DisplayOtherCellsInLayer)
            {
                DisplayOtherCellsInLayer = !DisplayOtherCellsInLayer;
                UpdateOtherCells();
                dirty = true;
            }
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            if (DisplayOtherCellsInLayer)
            {
                if (GUILayout.Toggle(IncludeInactiveCells, "Include Inactive Cells") != IncludeInactiveCells)
                {
                    IncludeInactiveCells = !IncludeInactiveCells;
                    UpdateOtherCells();
                    dirty = true;
                }

                float nextDisplayOtherCellsRadius = EditorGUILayout.Slider("Display Other Cells Radius", DisplayOtherCellsRadius, MinDisplayOtherCellsRadius, MaxDisplayOtherCellsRadius, GUILayout.MinHeight(16));
                if (nextDisplayOtherCellsRadius != DisplayOtherCellsRadius)
                {
                    DisplayOtherCellsRadius = nextDisplayOtherCellsRadius;
                    UpdateOtherCells();
                    dirty = true;
                }

                if (GUILayout.Button("Force Other Cells Refresh"))
                {
                    UpdateOtherCells();
                }


            }
            EditorGUI.EndDisabledGroup();
            if (dirty)
            {
                PushEditorPrefs();
                ForceRepaintSceneGUI();
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Advanced editing is not available during play mode.", MessageType.Warning);
            }
        } 
        #endregion

        protected virtual void OnLayerChange(int layerIndex)
        {
            UpdateOtherCells();
        }
        protected virtual void OnAreaChange(int areaIndex) { }

        protected virtual void OnSceneGUI()
        {
            var holder = (FacesHolder)target;
            int nodesCount = holder.NodesCount;
            if (nodes.Length < nodesCount)
            {
                System.Array.Resize(ref nodes, Mathf.NextPowerOfTwo(nodesCount));
            }
            holder.GetAllWorldNodesNoAlloc(nodes);

            if (IsMultiEditing)
            {
                Handles.color = notSelectedCellColor;
                NavigationEditorTools.DrawCellsWithHandles(nodes, nodesCount);
            }
            else
            {
                if (DisplayOtherCellsInLayer && !Application.isPlaying)
                {
                    Handles.color = otherCellColor;
                    NavigationEditorTools.DrawCellsWithHandles(otherCellsAsNodes, otherCellsAsNodes.Length);
                }

                if (EnableEditing)
                {
                    DrawCellsWithSelectionRespect(nodes, nodesCount);
                }
                else
                {
                    Handles.color = notSelectedCellColor;
                    NavigationEditorTools.DrawCellsWithHandles(nodes, nodesCount);
                }
                
                currentMode?.OnSceneGUI(nodes, nodesCount);
            }
        }

        public void ForceRepaintSceneGUI()
        {
            SceneView.RepaintAll();
            HandleUtility.Repaint();
        }

        private void DrawCellsWithSelectionRespect(Vector3[] nodes, int nodesCount)
        {
            int selectedCellStartNode = selectedCellIndex * NavigationInfo.NodesCount;

            Handles.color = notSelectedCellColor;
            NavigationEditorTools.DrawCellsAndCentersWithHandles(nodes, 
                0, selectedCellStartNode);
            NavigationEditorTools.DrawCellsAndCentersWithHandles(nodes, 
                selectedCellStartNode + NavigationInfo.NodesCount, nodesCount);

            Handles.color = selectedCellColor;
            Handles.DrawLine(nodes[selectedCellStartNode], nodes[selectedCellStartNode + 1]);
            Handles.DrawLine(nodes[selectedCellStartNode + 1], nodes[selectedCellStartNode + 2]);
            Handles.DrawLine(nodes[selectedCellStartNode + 2], nodes[selectedCellStartNode]);
        }

        #region Adding&Removing Cells
        public void RemoveSelectedCell()
        {
            var holder = (FacesHolder)target;
            Undo.RecordObject(holder, "AN cell remove.");
            holder.RemoveFace(SelectedCellIndex);
            SelectedCellIndex = SelectedCellIndex < holder.FacesCount ? SelectedCellIndex : holder.FacesCount - 1;
            ForceRepaintSceneGUI();
        }

        public void AddAndSelectCell(Vector3 a, Vector3 b, Vector3 c)
        {
            var holder = (FacesHolder)target;
            Undo.RecordObject(holder, "AN cell add.");
            holder.AddFace(a, b, c);
            SelectedCellIndex = holder.FacesCount - 1;
            ForceRepaintSceneGUI();
        } 
        #endregion

        #region ChangingNodes
        public void MagnetNodeChangeWithUndoRecord(Vector3[] nodes, int nodesCount, Vector3 current, Vector3 to)
        {
            var holder = (FacesHolder)target;
            Undo.RecordObject(holder, "Change NN Position");
            MagnetNodeChange(nodes, nodesCount, current, to);
        }

        public void MagnetEdgeChangeWithUndoRecord(Vector3[] nodes, int nodesCount
            , Vector3 currentA, Vector3 toA, Vector3 currentB, Vector3 toB)
        {
            var holder = (FacesHolder)target;
            Undo.RecordObject(holder, "Change NN Position");
            MagnetNodeChange(nodes, nodesCount, currentA, toA);
            MagnetNodeChange(nodes, nodesCount, currentB, toB);
        }

        private void MagnetNodeChange(Vector3[] nodes, int nodesCount, Vector3 current, Vector3 to)
        {
            var holder = (FacesHolder)target;
            for (int i = 0; i < nodesCount; i++)
            {
                if (nodes[i] == current)
                {
                    holder.SetNodeNoReregister(i, to);
                }
            }
            holder.ReregisterInSystem();
        }
        #endregion

        #region Snapping
        public bool SnapToNodesOrOtherCells(ref Vector3 position, Vector3[] nodes, int nodesCount, float snappingRadius, Vector3[] ignorePositions)
        {
            bool result = true;
            int nodesSnapIndex = SnapToNodes(position, nodes, nodesCount, snappingRadius, ignorePositions);
            int otherSnapIndex = DisplayOtherCellsInLayer ? SnapToNodes(position, otherCellsAsNodes, otherCellsAsNodes.Length, snappingRadius, ignorePositions) : -1;
            if (nodesSnapIndex == -1 && otherSnapIndex == -1)
            {
                result = false;
            }
            else if(nodesSnapIndex >= 0 && otherSnapIndex >= 0)
            {
                position = Vector3.Distance(nodes[nodesSnapIndex], position) > Vector3.Distance(otherCellsAsNodes[otherSnapIndex], position) 
                    ? nodes[nodesSnapIndex] : otherCellsAsNodes[otherSnapIndex];
            }
            else if (nodesSnapIndex >= 0)
            {
                position = nodes[nodesSnapIndex];
            }
            else if (otherSnapIndex >= 0)
            {
                position = otherCellsAsNodes[otherSnapIndex];
            }
            return result;
        }

        public int SnapToNodes(Vector3 position, Vector3[] nodes, int nodesCount, float snappingRadius, Vector3[] ignorePositions)
        {
            int resultIndex = -1;
            float potencialResultDistance = float.MaxValue;
            for (int i = 0; i < nodesCount; i++)
            {
                if (ShouldIgnoreSnapping(nodes[i], ignorePositions))
                {
                    continue;
                }

                float distance = Vector3.Distance(position, nodes[i]);
                if (distance < snappingRadius && distance < potencialResultDistance)
                {
                    potencialResultDistance = distance;
                    resultIndex = i;
                }
            }
            return resultIndex;
        }

        private bool ShouldIgnoreSnapping(Vector3 position, Vector3[] ignorePositions)
        {
            int ignoredPositionsLength = ignorePositions.Length;
            for (int i = 0; i < ignoredPositionsLength; i++)
            {
                if (position == ignorePositions[i])
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}

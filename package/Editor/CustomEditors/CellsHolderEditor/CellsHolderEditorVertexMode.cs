using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.CellsHolderEditorElements
{
    public class CellsHolderEditorVertexMode : BasicCellsHolderEditorMode
    {
        private const float MinSnappingRadius = 0f;
        private const float MaxSnappingRadius = 5f;
        private const string SnappingRadiusPrefs = "CHEVM_SnappingRadius";
        private const string UseLocalRotationPrefs = "CHEVM_UseLocalRotation";
        
        private Vector3[] ignoredDuringSnapping;
        private bool useLocalRotation;
        private float snappingRadius;

        public CellsHolderEditorVertexMode(CellsHolderEditor controller) 
            : base(controller, CellsHolderEditor.EditingMode.Vertex) { }

        public override void Init()
        {
            ignoredDuringSnapping = new Vector3[NavigationInfo.NodesCount];
            useLocalRotation = EditorPrefs.GetBool(UseLocalRotationPrefs, false);
            snappingRadius = EditorPrefs.GetFloat(SnappingRadiusPrefs, MinSnappingRadius);
        }
        public override void Deinit()
        {
            
        }
        public override void OnUndo()
        {
            //controller.ChangeMode(new AlchemyNavigationMultiCellHolderEditorVertexMode(controller));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Vertex Mode Options", EditorStyles.boldLabel);

            float nextSnappingRadius = EditorGUILayout.Slider("Snapping Radius", snappingRadius, MinSnappingRadius, MaxSnappingRadius, GUILayout.MinHeight(16));
            if (nextSnappingRadius != snappingRadius)
            {
                snappingRadius = nextSnappingRadius;
                EditorPrefs.SetFloat(SnappingRadiusPrefs, snappingRadius);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(!useLocalRotation, "Global", "Button") == useLocalRotation
                || GUILayout.Toggle(useLocalRotation, "Local", "Button") != useLocalRotation)
            {
                useLocalRotation = !useLocalRotation;
                EditorPrefs.SetBool(UseLocalRotationPrefs, useLocalRotation);
                controller.ForceRepaintSceneGUI();
            }
            EditorGUILayout.EndHorizontal();
        }

        public override void OnSceneGUI(Vector3[] nodes, int nodesCount)
        {
            bool snapping = snappingRadius > 0;
            if (snapping)
            {
                UpdateIgnoredDuringSnapping(nodes);
            }

            Quaternion handlesRotation = (useLocalRotation) ? controller.Holder.transform.rotation : Quaternion.identity;

            Vector3 nextPosition = Vector3.zero;
            int changedIndex = -1;
            EditorGUI.BeginChangeCheck();
            int selectedNodeStartIndex = controller.SelectedCellIndex * NavigationInfo.NodesCount;
            int selectedNodeEndIndex = selectedNodeStartIndex + NavigationInfo.NodesCount;
            for (int i = selectedNodeStartIndex; i < selectedNodeEndIndex; i++)
            {
                nextPosition = Handles.PositionHandle(nodes[i], handlesRotation);
                if (nextPosition != nodes[i])
                {
                    changedIndex = i;
                    if (snapping && controller.SnapToNodesOrOtherCells(ref nextPosition, nodes, nodesCount, snappingRadius, ignoredDuringSnapping))
                    {
                        GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                        Event.current.Use();
                    }
                    break;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (changedIndex != -1)
                {
                    controller.MagnetNodeChangeWithUndoRecord(nodes, nodesCount, nodes[changedIndex], nextPosition);
                }
            }
        }

        
        private void UpdateIgnoredDuringSnapping(Vector3[] nodes)
        {
            int firstNodeOfSelectedCell = controller.SelectedCellIndex * NavigationInfo.NodesCount;
            for (int i = 0; i < NavigationInfo.NodesCount; i++)
            {
                ignoredDuringSnapping[i] = nodes[firstNodeOfSelectedCell + i];
            }
        }

    }
}

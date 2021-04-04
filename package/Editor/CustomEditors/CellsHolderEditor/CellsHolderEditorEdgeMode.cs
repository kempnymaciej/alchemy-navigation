using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.CellsHolderEditorElements
{
    public class CellsHolderEditorEdgeMode : BasicCellsHolderEditorMode
    {
        private const RotationMode DefaultRotationMode = RotationMode.Global;
        private enum RotationMode { Global, Local, Normal}
        private const string RotationModePrefs = "CHEVM_RotationMode";

        private RotationMode rotationMode;

        public CellsHolderEditorEdgeMode(CellsHolderEditor controller)
            : base(controller, CellsHolderEditor.EditingMode.Edge) { }

        public override void Init()
        {
            rotationMode = (RotationMode)EditorPrefs.GetInt(RotationModePrefs, (int)DefaultRotationMode);
        }
        public override void Deinit()
        {
            
        }
        public override void OnUndo()
        {
            //controller.ChangeMode(new AlchemyNavigationMultiCellHolderEditorEdgeMode(controller));
        }

        public override void OnInspectorGUI()
        {
            bool dirty = false;
            EditorGUILayout.LabelField("Edge Mode Options", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(rotationMode == RotationMode.Global, "Global", "Button")
                && rotationMode != RotationMode.Global)
            {
                dirty = true;
                rotationMode = RotationMode.Global;
            }
            else if (GUILayout.Toggle(rotationMode == RotationMode.Local, "Local", "Button")
                && rotationMode != RotationMode.Local)
            {
                dirty = true;
                rotationMode = RotationMode.Local;
            }
            else if (GUILayout.Toggle(rotationMode == RotationMode.Normal, "Normal", "Button")
                && rotationMode != RotationMode.Normal)
            {
                dirty = true;
                rotationMode = RotationMode.Normal;
            }
            EditorGUILayout.EndHorizontal();

            if (dirty)
            {
                EditorPrefs.SetInt(RotationModePrefs, (int)rotationMode);
                controller.ForceRepaintSceneGUI();
            }
        }

        public override void OnSceneGUI(Vector3[] nodes, int nodesCount)
        {
            int selectedCellStartNode = controller.SelectedCellIndex * NavigationInfo.NodesCount;

            int changedIndex = -1;
            Vector3 nextA = Vector3.zero;
            Vector3 nextB = Vector3.zero;

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < NavigationInfo.NodesCount; i++)
            {
                Vector3 a = nodes[selectedCellStartNode + i];
                Vector3 b = nodes[selectedCellStartNode + ((i + 1) % NavigationInfo.NodesCount)];
               
                Vector3 position = (a + b) / 2;
                
                Quaternion rotation;
                switch (rotationMode)
                {
                    case RotationMode.Global:
                        rotation = Quaternion.identity;
                        break;
                    case RotationMode.Local:
                        rotation = controller.Holder.transform.rotation;
                        break;
                    case RotationMode.Normal:
                        Vector3 c = nodes[selectedCellStartNode + ((i + 2) % NavigationInfo.NodesCount)];
                        var plane = new Plane(a, b, c);
                        rotation = Quaternion.LookRotation(a - b, plane.normal);
                        break;
                    default:
                        rotation = Quaternion.identity;
                        break;
                }

                Vector3 displacement = Handles.PositionHandle(position, rotation) - position;
                if (displacement != Vector3.zero)
                {
                    nextA = a + displacement;
                    nextB = b + displacement;
                    changedIndex = i;
                    break;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (changedIndex != -1)
                {
                    int a = selectedCellStartNode + changedIndex;
                    int b = selectedCellStartNode + ((changedIndex + 1) % NavigationInfo.NodesCount);
                    controller.MagnetEdgeChangeWithUndoRecord(nodes, nodesCount, nodes[a], nextA, nodes[b], nextB);
                }
            }
        }


    }
}

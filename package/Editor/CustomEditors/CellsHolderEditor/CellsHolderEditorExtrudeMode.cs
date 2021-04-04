using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.CellsHolderEditorElements
{
    public class CellsHolderEditorExtrudeMode : BasicCellsHolderEditorMode
    {
        public CellsHolderEditorExtrudeMode(CellsHolderEditor controller)
            : base(controller, CellsHolderEditor.EditingMode.Extrude) { }

        public override void Init()
        {
            
        }
        public override void Deinit()
        {
            
        }
        public override void OnUndo()
        {
            //controller.ChangeMode(new AlchemyNavigationMultiCellHolderEditorExtrudeMode(controller));
        }

        public override void OnInspectorGUI()
        {
            
        }

        public override void OnSceneGUI(Vector3[] nodes, int nodesCount)
        {
            Ray ray = default;
            bool hasRay = false;

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                ray = NavigationEditorTools.GUIEventToRay(currentEvent);
                hasRay = true;

                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                Event.current.Use();
            }

            Vector3 selectedCellCenter = NavigationEditorTools.CalculateCellCenter(controller.SelectedCellIndex, nodes);

            int startNode = controller.SelectedCellIndex * NavigationInfo.NodesCount;
            for (int i = 0; i < NavigationInfo.NodesCount; i++)
            {
                Vector3 a = nodes[startNode + i];
                Vector3 b = nodes[startNode + ((i + 1) % NavigationInfo.NodesCount)];
                if (!EdgeContactAnyCellWithSelectedIgnored(a, b, nodes, nodesCount))
                {
                    Vector3 c = nodes[startNode + ((i + 2) % NavigationInfo.NodesCount)];
                    Vector3 move = ((a + b) / 2) - c;
                    c += 2 * move;

                    var handlesColorCopy = Handles.color;
                    Handles.color = Color.yellow;
                    Handles.DrawLine(a, c);
                    Handles.DrawLine(b, c);

                    if (hasRay && NavigationEditorTools.RayCheckTriangle(ray, a, b, c))
                    {
                        controller.AddAndSelectCell(a, b, c);
                    }

                    Handles.color = handlesColorCopy;
                }
            }
        }

        private bool EdgeContactAnyCellWithSelectedIgnored(Vector3 a, Vector3 b , Vector3[] nodes, int nodesCount)
        {
            int selectedStartNode = controller.SelectedCellIndex * NavigationInfo.NodesCount;
            for (int i = 0; i < nodesCount; i++)
            {
                if(a == nodes[i])
                {
                    int startNode = i - (i % NavigationInfo.NodesCount);
                    if(startNode != selectedStartNode)
                    {
                        for (int j = 0; j < NavigationInfo.NodesCount; j++)
                        {
                            if (b == nodes[startNode + j])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}

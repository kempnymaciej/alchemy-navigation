using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.CellsHolderEditorElements
{
    public class CellsHolderEditorSelectMode : BasicCellsHolderEditorMode
    {
        public CellsHolderEditorSelectMode(CellsHolderEditor controller) 
            : base(controller, CellsHolderEditor.EditingMode.Select) { }

        public override void Init()
        {

        }
        public override void Deinit()
        {
            
        }
        public override void OnUndo()
        {
            controller.ChangeMode(new CellsHolderEditorSelectMode(controller), false);
        }

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Select Mode Options", EditorStyles.boldLabel);
                if (GUILayout.Button("Remove Selected"))
                {
                    controller.RemoveSelectedCell();
                }
            }
        }

        public override void OnSceneGUI(Vector3[] nodes, int nodesCount)
        {
            var currentEvent = Event.current;
            
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                var ray = NavigationEditorTools.GUIEventToRay(currentEvent);
                if (NavigationEditorTools.RayCastCells(ray, out int cellIndex, nodes, nodesCount))
                {
                    controller.SelectedCellIndex = cellIndex;
                }

                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                Event.current.Use();
            }
        }
    }
}

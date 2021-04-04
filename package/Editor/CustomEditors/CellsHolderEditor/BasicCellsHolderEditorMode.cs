using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.CellsHolderEditorElements
{
    public abstract class BasicCellsHolderEditorMode
    {
        public readonly CellsHolderEditor.EditingMode editingMode;
        protected readonly CellsHolderEditor controller;

        protected BasicCellsHolderEditorMode(CellsHolderEditor controller
            , CellsHolderEditor.EditingMode editingMode)
        {
            this.controller = controller;
            this.editingMode = editingMode;
        }

        public abstract void Init();
        public abstract void Deinit();

        public abstract void OnSceneGUI(Vector3[] nodes, int nodesCount);
        public abstract void OnInspectorGUI();

        public abstract void OnUndo();
    } 
}

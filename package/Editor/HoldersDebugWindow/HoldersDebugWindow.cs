using AlchemyBow.Navigation.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.Windows
{
    public sealed class HoldersDebugWindow : EditorWindow
    {
        private const string RefreshInfo = "This is a static inspection tool. Click refresh after making changes to the holders.";

        private NavigationSettingsConnector settingsConnector;

        private int areaMask = -1;
        private int layerIndex = 0;

        private bool draw = false;
        private bool includeInactive = false;
        private bool refresh = false;

        private Vector3[] vertices;
        private int[] centers;
        private IndexEdge[][] areas;
        private Color[] areaColors;

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneViewDuringSceneGui;
            settingsConnector = NavigationSettingsConnector.Create();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneViewDuringSceneGui;
        }

        private void OnGUI()
        {
            layerIndex = settingsConnector.DrawLayerIndexPopup("Layer Index", layerIndex);
            areaMask = settingsConnector.DrawAreaMaskPopup("Area Mask", areaMask);
            
            if(GUILayout.Toggle(draw, "Draw", "Button") != draw)
            {
                draw = !draw;
                refresh = true;
            }
            if (draw)
            {
                if(GUILayout.Toggle(includeInactive, "Include Inactive", "Button") != includeInactive)
                {
                    includeInactive = !includeInactive;
                    refresh = true;
                }
                EditorGUILayout.HelpBox(RefreshInfo, MessageType.Info);
                if (GUILayout.Button("Refresh"))
                {
                    refresh = true;
                }
            }

            if (refresh)
            {
                SceneView.RepaintAll();
                HandleUtility.Repaint();
            }
        }

        private void OnSceneViewDuringSceneGui(SceneView obj)
        {
            if (refresh)
            {
                refresh = false;
                Refresh();
            }

            if (draw)
            {
                Draw();
            }
        }

        private void Refresh()
        {
            var areas = new HashSet<IndexEdge>[NavigationSettings.AreasCount];
            for (int i = 0; i < NavigationSettings.AreasCount; i++)
            {
                areas[i] = new HashSet<IndexEdge>();
            }
            var vertices = new List<Vector3>();
            var centers = new List<int>();
            var holders = NavigationEditorTools.FindAllComponentsInActiveScene<FacesHolder>(includeInactive);
            foreach (var holder in holders)
            {
                int areaIndex = holder.AreaIndex;
                if(holder.LayerIndex == layerIndex && ((1 << areaIndex) & areaMask) != 0)
                {
                    var triangles = holder.GetAllWorldNodes();
                    int trianglesLength = triangles.Length;
                    for (int i = 0; i < trianglesLength;)
                    {
                        int a = FindVertexOrAdd(triangles[i++], vertices);
                        int b = FindVertexOrAdd(triangles[i++], vertices);
                        int c = FindVertexOrAdd(triangles[i++], vertices);
                        int center = FindVertexOrAdd((vertices[a] + vertices[b] + vertices[c]) / 3, vertices);

                        areas[areaIndex].Add(new IndexEdge(a, b));
                        areas[areaIndex].Add(new IndexEdge(b, c));
                        areas[areaIndex].Add(new IndexEdge(c, a));
                        centers.Add(center);
                    }
                }
            }

            this.vertices = vertices.ToArray();
            this.centers = centers.ToArray();
            this.areas = new IndexEdge[NavigationSettings.AreasCount][];
            for (int i = 0; i < NavigationSettings.AreasCount; i++)
            {
                this.areas[i] = areas[i].ToArray();
            }
            RefreshAreaColors();
        }

        private void RefreshAreaColors()
        {
            areaColors = new Color[NavigationSettings.AreasCount];
            var settings = NavigationSettingsConnector.TryGetSystem()?.Settings;
            if (settings != null)
            {
                for (int i = 0; i < NavigationSettings.AreasCount; i++)
                {
                    areaColors[i] = settings.GetAreaSettings(i).Color;
                }
            }
            else
            {
                for (int i = 0; i < NavigationSettings.AreasCount; i++)
                {
                    areaColors[i] = Color.gray;
                }
            }
        }

        private static int FindVertexOrAdd(Vector3 vertex, List<Vector3> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (list[i] == vertex)
                {
                    return i;
                }
            }
            list.Add(vertex);
            return count;
        }

        private void Draw()
        {
            for (int i = 0; i < NavigationSettings.AreasCount; i++)
            {
                Handles.color = areaColors[i];
                foreach (var edge in areas[i])
                {
                    Handles.DrawLine(vertices[edge.a], vertices[edge.b]);
                }
            }

            Handles.color = Color.white;
            var offset = Vector3.up * .06f;
            foreach (int center in centers)
            {
                Handles.DrawLine(vertices[center], vertices[center] + offset);
            }
        }

        [MenuItem("Window/AlchemyNavigation/HoldersDebug")]
        private static void Init()
        {
            var window = EditorWindow.GetWindow<HoldersDebugWindow>("HoldersDebug");
            window.Show();
        }
    } 
}

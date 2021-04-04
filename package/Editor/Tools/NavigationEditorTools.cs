using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor
{
	public static class NavigationEditorTools
	{
        public static List<T> FindAllComponentsInActiveScene<T>(bool includeInactive) where T : MonoBehaviour
        {
            List<T> results = new List<T>();
            var rootGameObjects = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                .GetRootGameObjects().ToList();

            foreach (var go in rootGameObjects)
            {
                if (includeInactive || go.activeInHierarchy)
                {
                    results.AddRange(go.GetComponentsInChildren<T>(includeInactive));
                }
            }
            return results;
        }

        public static Vector3[] RetrieveCellsInRadiusAsNodes(Vector3 center, float radius
            , int layerIndex, bool includeInactive, params FacesHolder[] ignoreHolders)
        {
            var holders = FindAllComponentsInActiveScene<FacesHolder>(includeInactive);
            foreach(var holder in ignoreHolders)
            {
                holders.Remove(holder);
            }
            var result = new List<Vector3>();
            var tempCell = new Vector3[NavigationInfo.NodesCount];
            foreach (var holder in holders)
            {
                if (holder.LayerIndex == layerIndex)
                {
                    int cellsCount = holder.FacesCount;
                    for (int c = 0; c < cellsCount; c++)
                    {
                        holder.GetWorldFaceNoAlloc(c, tempCell);
                        for (int n = 0; n < NavigationInfo.NodesCount; n++)
                        {
                            if (Vector3.Distance(center, tempCell[n]) <= radius)
                            {
                                result.AddRange(tempCell);
                                break;
                            }
                        }
                    }
                }
            }
            return result.ToArray();
        }

        public static void DrawCellsWithHandles(Vector3[] nodes, int nodesCount)
        {
            for (int n = 0; n < nodesCount; n += NavigationInfo.NodesCount)
            {
                int p1 = n, p2 = n + 1, p3 = n + 2;
                Handles.DrawLine(nodes[p1], nodes[p2]);
                Handles.DrawLine(nodes[p2], nodes[p3]);
                Handles.DrawLine(nodes[p3], nodes[p1]);
            }
        }

        public static void DrawCellsAndCentersWithHandles(Vector3[] nodes, int start, int end)
        {
            Vector3 centerOffset = .06f * Vector3.up;
            for (int n = start; n < end; n += NavigationInfo.NodesCount)
            {
                int p1 = n, p2 = n + 1, p3 = n + 2;
                Handles.DrawLine(nodes[p1], nodes[p2]);
                Handles.DrawLine(nodes[p2], nodes[p3]);
                Handles.DrawLine(nodes[p3], nodes[p1]);
                Vector3 center = (nodes[p2] + nodes[p1] + nodes[p3]) / 3;
                Handles.DrawLine(center, center + centerOffset);
            }
        }

        public static Vector3 CalculateCellCenter(int cellIndex, Vector3[] nodes)
        {
            Vector3 center = Vector3.zero;
            int startNode = cellIndex * NavigationInfo.NodesCount;
            for (int i = 0; i < NavigationInfo.NodesCount; i++)
            {
                center += nodes[startNode + i];
            }
            return center / NavigationInfo.NodesCount;
        }

        #region Editor Raycasting
        public static Ray GUIEventToRay(Event sourceEvent)
        {
            Vector3 mousePosition = sourceEvent.mousePosition;
            var sceneViewCamera = SceneView.lastActiveSceneView.camera;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePosition.y = sceneViewCamera.pixelHeight - mousePosition.y * ppp;
            mousePosition.x *= ppp;

            return sceneViewCamera.ScreenPointToRay(mousePosition);
        }
        

        public static bool RayCastCells(Ray ray, out int cellIndex, Vector3[] nodes, int nodesCount)
        {
            cellIndex = 0;
            int cellsCount = nodesCount / NavigationInfo.NodesCount;
            for (int cell = 0; cell < cellsCount; cell++)
            {
                int a = cell * NavigationInfo.NodesCount;
                int b = a + 1;
                int c = a + 2;
                if (RayCheckTriangle(ray, nodes[a], nodes[b], nodes[c]))
                {
                    cellIndex = cell;
                    return true;
                }
            }

            return false;
        }
        public static bool RayCheckTriangle(Ray ray, Vector3 a, Vector3 b, Vector3 c)
        {
            bool result = false;
            var plane = new Plane(a, b, c);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                if (IsPointInTriangle_XZ(hitPoint, a, b, c))
                {
                    result = true;
                }
            }
            return result;
        }

        public static bool IsPointInTriangle_XZ(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            float s1 = c.z - a.z + ((a.z == c.z) ? .001f : 0);
            float s2 = c.x - a.x;
            float s3 = b.z - a.z;
            float s4 = point.z - a.z;

            float w1 = (a.x * s1 + s4 * s2 - point.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }
        #endregion

        public static void DrawWireCylinder(Vector3 position, Quaternion rotation, float radius, float height, Color color)
        {
            Handles.color = color;

            Matrix4x4 angleMatrix = Matrix4x4.TRS(position, rotation, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                Handles.DrawLine(new Vector3(0, 0, -radius), new Vector3(0, height, -radius));
                Handles.DrawLine(new Vector3(0, 0, radius), new Vector3(0, height, radius));
                Handles.DrawLine(new Vector3(-radius, 0, 0), new Vector3(-radius, height, 0));
                Handles.DrawLine(new Vector3(radius, 0, 0), new Vector3(radius, height, 0));

                Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius);
                Handles.DrawWireDisc(Vector3.up * height, Vector3.up, radius);
            }
        }
    }
}

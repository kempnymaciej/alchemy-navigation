using AlchemyBow.Navigation.HighLevel;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Navigation.Editor.Windows
{
    public sealed class MeshBasedBakeWindow : EditorWindow
    {
        public enum ResultPosition { CenterOfMass, LocalZero }

        [SerializeField]
        private MeshFilter[] meshFilters = null;
        [SerializeField]
        private Transform resultHolder = null;
        [SerializeField]
        private ResultPosition resultPosition = ResultPosition.CenterOfMass;
        [SerializeField]
        private MeshBasedBakeSettings settings = null;

        private SerializedObject serializedObject;

        private void OnEnable()
        {
            meshFilters = new MeshFilter[0];
            resultHolder = null;
            resultPosition = ResultPosition.CenterOfMass;
            settings = new MeshBasedBakeSettings();
            serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resultHolder"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resultPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("meshFilters"));
            
            if (GUILayout.Button("Bake"))
            {
                serializedObject.ApplyModifiedProperties();
                Bake();
            }
        }

        private void Bake()
        {
            var errors = Validate();
            if (errors.Length > 0)
            {
                PrintErrors(errors);
                Debug.Log("Baking failed.");
            }
            else
            {
                int createdFaceHolders = 0;
                var bake = MeshBasedBake.Bake(meshFilters, settings);
                var vertices = bake.Vertices;
                var groups = bake.GroupTrianglesByConnection();
                foreach(var group in groups)
                {
                    if(group.Length > 0)
                    {
                        CreateFaceHolder(vertices, group);
                        createdFaceHolders++;
                    }
                }
                Debug.Log($"Baking succeded. \n {bake.OmittedTriangles} triangles were omitted. {createdFaceHolders} FaceHolders were created.");
            }
        }

        private void CreateFaceHolder(Vector3[] vertices, int[] triangles)
        {
            var go = new GameObject();
            go.name = "BakedGroup " + System.DateTime.Now.ToString("dd-MM-yyyy HH-mm");
            var transform = go.transform;
            transform.parent = resultHolder;
            switch (resultPosition)
            {
                case ResultPosition.CenterOfMass:
                    transform.position = CalculateGroupCenter(vertices, triangles);
                    break;
                case ResultPosition.LocalZero:
                    transform.localPosition = Vector3.zero;
                    break;
            }
            
            
            var faceHolder = go.AddComponent<FacesHolder>();
            var so = new SerializedObject(faceHolder);
            var localNodes = so.FindProperty("localNodes");

            int numberOfNodes = triangles.Length;
            localNodes.arraySize = numberOfNodes;
            for (int i = 0; i < numberOfNodes; i++)
            {
                var element = localNodes.GetArrayElementAtIndex(i);
                element.vector3Value = transform.InverseTransformPoint(vertices[triangles[i]]);
            }
            so.ApplyModifiedProperties();
        }

        private Vector3 CalculateGroupCenter(Vector3[] vertices, int[] triangles)
        {
            Vector3 center = Vector3.zero;
            var uniqueVertices = new HashSet<int>();
            foreach (int vertexIndex in triangles)
            {
                uniqueVertices.Add(vertexIndex);
            }
            foreach (int uniqueVertex in uniqueVertices)
            {
                center += vertices[uniqueVertex];
            }
            return center / uniqueVertices.Count;
        }

        private string[] Validate()
        {
            var errors = new List<string>();
            if(meshFilters.Length == 0)
            {
                errors.Add("You must asign at least one MeshFilter befor baking.");
            }
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if(meshFilters[i] == null)
                {
                    errors.Add($"MeshFilter at index {i} is set to null. Delete it or asign its value before baking.");
                }
            }
            if(settings.upwards.magnitude == 0)
            {
                errors.Add("Upwards magnitude is equal 0. It must be greater than 0 befor baking.");
            }
            return errors.ToArray();
        }  
        private static void PrintErrors(string[] errors)
        {
            foreach (var error in errors)
            {
                Debug.LogError(error);
            }
        }

        [MenuItem("Window/AlchemyNavigation/MeshBasedBake")]
        private static void Init()
        {
            var window = EditorWindow.GetWindow<MeshBasedBakeWindow>("MeshBasedBake");
            window.Show();
        }
    } 
}

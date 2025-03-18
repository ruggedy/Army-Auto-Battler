using System;
using UnityEditor;
using UnityEngine;

namespace Graphics
{
    public class RectToDiamond : MonoBehaviour
    {
        public Sprite sprite;
        
        private void Start()
        {
            if (sprite == null)
            {
                return;
            }
            
            var mesh = new Mesh();
            
            var vertices = ArrayToVector3(sprite.vertices);
            var uv = sprite.uv; 
            var triangles = sprite.triangles;
            
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = Array.ConvertAll(triangles, i => (int)i);
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            GetComponent<MeshFilter>().mesh = mesh;
            
            var material = new Material(Shader.Find("Sprites/Default"));
            material.mainTexture = sprite.texture;
            
            GetComponent<MeshRenderer>().material = material;
        
            ChangeToDiamond();

            var assetPath = $"Assets/Tiles/iso_floor/floor.mesh";
            var meshFilter = GetComponent<MeshFilter>();
            var meshToSave = meshFilter.sharedMesh;

          
            
            AssetDatabase.CreateAsset(meshToSave, assetPath);
            AssetDatabase.SaveAssets();

        }

        void ChangeToDiamond()
        {
            var meshFilter = GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;

            var vertices = new Vector3[]
            {
                new Vector3(0, 1, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, -1, 0),
                new Vector3(1, 0, 0),
            };
            
            // current value of uv
            // (0.02, 0.96), (0.09, 0.84), (0.09, 0.96), (0.02, 0.84)



            var uv = new Vector2[]
            {
                new Vector2(0.02f, 0.96f),
                new Vector2(0.088f, 0.96f),
                new Vector2(0.088f, 0.84f),
                new Vector2(0.02f, 0.84f),
            };

            var triangles = new[]
            {
                0, 1, 2, // First triangle
                2, 3, 0  // Second triangle
            };

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
        }

        Vector3[] ArrayToVector3(Vector2[] array)
        {
            var vector3Array = new Vector3[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                vector3Array[i] = new Vector3(array[i].x, array[i].y, 0);
            }
            
            return vector3Array;
        }
    }
}
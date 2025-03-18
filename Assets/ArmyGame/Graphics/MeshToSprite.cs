using System;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

namespace Graphics
{
    public class MeshToSprite : MonoBehaviour
    {
        public int textureWidth = 512; 
        public int textureHeight = 512;
   
        public void ConvertMeshToSprite()
        {
            
            RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24);

            // Step 2: Create Camera
            GameObject cameraObject = new GameObject("MeshRenderCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;
            camera.orthographic = true;
            camera.targetTexture = renderTexture;
            
            var meshRenderer = GetComponent<MeshRenderer>();
            
            var bounds = meshRenderer.bounds;
            
            camera.cullingMask = LayerMask.GetMask("MeshToSprite");
            camera.transform.position = bounds.center + new Vector3(0, 0 , -10);
            camera.transform.LookAt(bounds.center);
            
            camera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) /2;
            
            camera.Render();
            
            RenderTexture.active = renderTexture;
            
            // Step 5: Convert to Texture2D
            Texture2D image = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
            image.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
            image.Apply();
            
            RenderTexture.active = null;
            
            var bytes = image.EncodeToPNG();
            var path = "Assets/Tiles/iso_floor/floor.png";
            System.IO.File.WriteAllBytes(path, bytes);
            
            AssetDatabase.Refresh();
            
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        private void Start()
        {
            ConvertMeshToSprite();
        }
    }
}
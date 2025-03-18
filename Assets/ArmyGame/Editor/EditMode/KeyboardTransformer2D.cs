using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArmyGame.Editor.EditMode
{
    [InitializeOnLoad]
    public static class KeyboardTransformer2D
    {
        private static float moveIncrement = 0.001f; 
        private static float rotationIncrement = 0.01f;
        private static Dictionary<KeyCode, bool> keyStates = new Dictionary<KeyCode, bool>();


        static KeyboardTransformer2D()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += UpdateTransform;
        }
        
       private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Check for key down events and record the key as held
        if (e.type == EventType.KeyDown)
        {
            // Only register if the key isn't already held (or update its state)
            keyStates[e.keyCode] = true;
            // Consume the event so Unity doesn't process it elsewhere
            e.Use();
            Debug.Log("Key down registered");
        }
        // Check for key up events and mark the key as released
        else if (e.type == EventType.KeyUp)
        {
            keyStates[e.keyCode] = false;
            e.Use();
            
            Debug.Log("Key up registered");
        }
    }

    // This update method is called continuously by the Editor
    private static void UpdateTransform()
    {
        // Ensure an object is selected in the Scene
        if (Selection.activeTransform == null)
            return;

        Transform t = Selection.activeTransform;
        
        bool transformChanged = false;

        // Check each held key and apply the appropriate transform change
        foreach (var key in keyStates)
        {
            if (key.Value) // If the key is currently held down
            {
                switch (key.Key)
                {
                    case KeyCode.LeftArrow:
                        t.position += Vector3.left * moveIncrement;
                        transformChanged = true;
                        break;
                    case KeyCode.RightArrow:
                        t.position += Vector3.right * moveIncrement;
                        transformChanged = true;
                        break;
                    case KeyCode.UpArrow:
                        t.position += Vector3.up * moveIncrement;
                        transformChanged = true;
                        break;
                    case KeyCode.DownArrow:
                        t.position += Vector3.down * moveIncrement;
                        transformChanged = true;
                        break;
                    case KeyCode.Q:
                        t.Rotate(Vector3.forward, rotationIncrement);
                        transformChanged = true;
                        break;
                    case KeyCode.E:
                        t.Rotate(Vector3.forward, -rotationIncrement);
                        transformChanged = true;
                        break;
                }
            }
        }

        // If we changed the transform, repaint the Scene view to reflect the updates immediately.
        if (transformChanged)
        {
            SceneView.RepaintAll();
        }
    }
    }
}
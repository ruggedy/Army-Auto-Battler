using System;
using ArmyGame.Editor.ScriptableObjects.Maps;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;


namespace ArmyGame.Editor.ScriptableObjects.MapZoneSelectionEditor
{
    public class EditorScript : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Tools/Map Zone Editor")]
        public static void ShowExample()
        {
            EditorScript wnd = GetWindow<EditorScript>();
            wnd.titleContent = new GUIContent("Save Grid Selection");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var zoneNameField = new TextField { label = "Map Zone Name", name = "ZONE_ELEMENT" };
            root.Add(zoneNameField);

            var folderLocation = new TextField { label = "Select Folder", name = "FOLDER_PATH" };
            folderLocation.isReadOnly = true;
            root.Add(folderLocation);

            var selectFolderButton = new Button(() =>
            {
                var path = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");

                if (!string.IsNullOrEmpty(path))
                {
                    folderLocation.value = path.Replace(Application.dataPath, "Assets");
                }
            }) { text = "Select Folder" };

            root.Add(selectFolderButton);

            var saveAssetButton = new Button { text = "Save Map Zone" };
            saveAssetButton.clicked += HandleSaveSelection;
            root.Add(saveAssetButton);
        }

        private void HandleSaveSelection()
        {
            if (!GridSelection.active || GridSelection.position.size == Vector3.zero)
            {
                Debug.LogWarning("No grid zone selected");
                return;
            }
            
            var zoneNameElement = rootVisualElement.Q<TextField>("ZONE_ELEMENT");
            var zonePathElement = rootVisualElement.Q<TextField>("FOLDER_PATH");

            if (zoneNameElement.value == null || zonePathElement.value == null)
            {
                Debug.LogWarning("No zone name of path selected");
                return;
            }

            var zone = MapZoneSO.Create(GridSelection.position);

            AssetDatabase.CreateAsset(zone, $"{zonePathElement.value}/{zoneNameElement.value}.asset");
            AssetDatabase.SaveAssets();
            
            ClearFields();
        }

        private void ClearFields()
        {
            var zoneNameElement = rootVisualElement.Q<TextField>("ZONE_ELEMENT");
            var zonePathElement = rootVisualElement.Q<TextField>("FOLDER_PATH");
            
            if (zoneNameElement.value == null || zonePathElement.value == null) return;
            
            zoneNameElement.value = string.Empty;
            zonePathElement.value = string.Empty;
        }
    }
}
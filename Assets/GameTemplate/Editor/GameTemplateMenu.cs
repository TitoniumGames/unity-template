using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameTemplate.Editor
{
    public static class GameTemplateMenu
    {
        private const string MENU_PATH = "Tools/Game Template/";
        private const string DELETE_EXAMPLE_MENU = MENU_PATH + "Delete Example Folder";
        private const string EXAMPLE_FOLDER_PATH = "Assets/GameTemplate/Example";

        [MenuItem(DELETE_EXAMPLE_MENU)]
        public static void DeleteExampleFolder()
        {
            if (!Directory.Exists(EXAMPLE_FOLDER_PATH))
            {
                EditorUtility.DisplayDialog("Game Template", "Example folder does not exist!", "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Example Folder", 
                "Are you sure you want to delete the Example folder?\n\nThis action cannot be undone.", 
                "Delete", 
                "Cancel"
            );

            if (confirmed)
            {
                try
                {
                    // Delete the folder and all its contents
                    AssetDatabase.DeleteAsset(EXAMPLE_FOLDER_PATH);
                    
                    // Refresh the asset database
                    AssetDatabase.Refresh();
                    
                    Debug.Log("Game Template: Example folder deleted successfully!");
                    
                    EditorUtility.DisplayDialog("Game Template", "Example folder deleted successfully!", "OK");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Game Template: Failed to delete Example folder: {e.Message}");
                    EditorUtility.DisplayDialog("Game Template", $"Failed to delete Example folder: {e.Message}", "OK");
                }
            }
        }

        [MenuItem(DELETE_EXAMPLE_MENU, true)]
        public static bool ValidateDeleteExampleFolder()
        {
            return Directory.Exists(EXAMPLE_FOLDER_PATH);
        }

        [MenuItem(MENU_PATH + "Documentation/Core README")]
        public static void OpenCoreREADME()
        {
            string readmePath = "Assets/GameTemplate/Core/README.md";
            if (File.Exists(readmePath))
            {
                Object readme = AssetDatabase.LoadAssetAtPath<Object>(readmePath);
                if (readme != null)
                {
                    Selection.activeObject = readme;
                    EditorGUIUtility.PingObject(readme);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Game Template", "Core README not found!", "OK");
            }
        }

        [MenuItem(MENU_PATH + "Documentation/EventSystem README")]
        public static void OpenEventSystemReadme()
        {
            string readmePath = "Assets/GameTemplate/EventSystem/README.md";
            if (File.Exists(readmePath))
            {
                Object readme = AssetDatabase.LoadAssetAtPath<Object>(readmePath);
                if (readme != null)
                {
                    Selection.activeObject = readme;
                    EditorGUIUtility.PingObject(readme);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Game Template", "EventSystem README not found!", "OK");
            }
        }

        [MenuItem(MENU_PATH + "About Game Template")]
        public static void ShowAboutDialog()
        {
            EditorUtility.DisplayDialog(
                "Game Template", 
                "Game Template v1.0.0\n\nA comprehensive Unity game development template with core systems and event management.\n", 
                "OK"
            );
        }
    }
} 
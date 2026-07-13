using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Installer.Editor.Validator
{
    public class FrameworkValidatorWindow : EditorWindow
    {
        private Vector2 _scroll;
        private int _selectedTab = 0; // 0 = Framework, 1 = Services

        public static void ShowWindow()
        {
            var window = GetWindow<FrameworkValidatorWindow>();

            window.titleContent = new GUIContent("Framework Validator");
            window.minSize = new Vector2(700, 520);

            window.Show();
        }
        
        private void DrawFrameworkProgress()
        {
            int installed = FrameworkValidator.InstalledCount();
            int total = FrameworkValidator.TotalCount();

            float progress = total == 0 ? 0 : (float)installed / total;

            Rect rect = GUILayoutUtility.GetRect(1, 22);

            EditorGUI.ProgressBar(
                rect,
                progress,
                $"Framework Ready ({installed}/{total})");
        }
        
        private void DrawServiceProgress()
        {
            int installed = 0;
            int total = 2;

            if (HasTitoServices())
                installed++;

            if (HasUnityIAP())
                installed++;

            float progress = (float)installed / total;

            Rect rect = GUILayoutUtility.GetRect(1, 22);

            EditorGUI.ProgressBar(
                rect,
                progress,
                $"Services Ready ({installed}/{total})");
        }

        private void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(8);
            DrawTabs();

            GUILayout.Space(15);

            if (_selectedTab == 0)
                DrawFrameworkProgress();
            else
                DrawServiceProgress();

            GUILayout.Space(15);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (_selectedTab == 0)
            {
                foreach (var item in FrameworkValidator.Items)
                {
                    DrawDependency(item);
                    GUILayout.Space(8);
                }
            }
            else
            {
                DrawService();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            DrawFooter();
        }
        
        private static bool HasUnityIAP()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(a => a.GetName().Name == "Unity.Purchasing");
        }

        #region Header

        private void DrawHeader()
        {
            GUILayout.Space(10);

            GUILayout.Label("Tito Framework Validator", EditorStyles.boldLabel);

            GUILayout.Label(
                "Validate all required dependencies before starting development.",
                EditorStyles.wordWrappedMiniLabel);
        }

        #endregion

        #region Tabs

        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();

            string[] tabs = { "Framework", "Services" };
            _selectedTab = GUILayout.Toolbar(_selectedTab, tabs, GUILayout.Height(26));

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Services
        
        private static bool HasTitoServices()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(a => a.GetName().Name == "Tito.Services");
        }
        
        [MenuItem("Tools/Debug/Print Assemblies")]
        private static void PrintAssemblies()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name))
            {
                Debug.Log(asm.GetName().Name);
            }
        }
        
        private void DrawServiceDependency(
            string title,
            string description,
            bool installed,
            Action install)
        {
            Color accent = installed
                ? new Color(0.20f, 0.80f, 0.30f)
                : new Color(0.90f, 0.25f, 0.25f);

            EditorGUILayout.BeginHorizontal();

            // Left Accent
            Rect accentRect = GUILayoutUtility.GetRect(4, 70, GUILayout.Width(4));
            EditorGUI.DrawRect(accentRect, accent);

            GUILayout.Space(4);

            EditorGUILayout.BeginVertical("HelpBox", GUILayout.Height(70));

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                installed ? "✔" : "✖",
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter
                },
                GUILayout.Width(30));

            GUILayout.BeginVertical();

            GUILayout.Label(
                title,
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12
                });

            GUILayout.Space(2);

            GUILayout.Label(
                description,
                EditorStyles.wordWrappedMiniLabel);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUI.enabled = !installed;

            if (GUILayout.Button(
                    installed ? "Installed" : "Install",
                    GUILayout.Width(120),
                    GUILayout.Height(30)))
            {
                install?.Invoke();
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawService()
        {
            DrawServiceDependency(
                "Unity In-App Purchasing",
                "Install the Unity Purchasing package.",
                HasUnityIAP(),
                () =>
                {
                    UnityEditor.PackageManager.Client.Add(
                        "com.unity.purchasing");
                });
            
            GUILayout.Space(8);
            
            DrawServiceDependency(
                "Tito Services",
                "Install the Tito Services package.",
                HasTitoServices(),
                () =>
                {
                    UnityEditor.PackageManager.Client.Add(
                        "https://github.com/TitoniumGames/unity-template.git?path=/Assets/Services");
                });
        }

        #endregion

        #region Progress

        private void DrawProgress()
        {
            int installed = FrameworkValidator.InstalledCount();
            int total = FrameworkValidator.TotalCount();

            float progress = total == 0 ? 0 : (float)installed / total;

            Rect rect = GUILayoutUtility.GetRect(1, 22);

            EditorGUI.ProgressBar(
                rect,
                progress,
                $"Framework Ready ({installed}/{total})");
        }

        #endregion

        #region Dependency

        private void DrawDependency(FrameworkValidator.Item item)
        {
            bool installed = item.Validator();

            Color accent = installed
                ? new Color(0.20f, 0.80f, 0.30f)
                : new Color(0.90f, 0.25f, 0.25f);

            EditorGUILayout.BeginHorizontal();

            // Left Accent
            Rect accentRect = GUILayoutUtility.GetRect(4, 70, GUILayout.Width(4));
            EditorGUI.DrawRect(accentRect, accent);

            GUILayout.Space(4);

            EditorGUILayout.BeginVertical("HelpBox", GUILayout.Height(70));

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                installed ? "✔" : "✖",
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter
                },
                GUILayout.Width(30));

            GUILayout.BeginVertical();

            GUILayout.Label(
                item.Name,
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12
                });

            GUILayout.Space(2);

            GUILayout.Label(
                item.Description,
                EditorStyles.wordWrappedMiniLabel);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUI.enabled = !installed;

            if (GUILayout.Button(
                    installed ? "Installed" : "Install",
                    GUILayout.Width(100),
                    GUILayout.Height(30)))
            {
                item.Install?.Invoke();
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Footer

        private void DrawFooter()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh", GUILayout.Height(34)))
            {
                Repaint();
            }

            if (GUILayout.Button("Close", GUILayout.Height(34)))
            {
                Close();
            }
            
            if (GUILayout.Button("README", GUILayout.Height(34)))
            {
                Application.OpenURL(FrameworkLinks.Readme);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (_selectedTab == 0)
            {
                if (GUILayout.Button("Install Unity Template", GUILayout.Height(34)))
                {
                    UnityEditor.PackageManager.Client.Add("https://github.com/TitoniumGames/unity-template.git?path=/Assets/GameTemplate");
                }
            }
            else
            {
                if (GUILayout.Button("Install Tito Services", GUILayout.Height(34)))
                {
                    UnityEditor.PackageManager.Client.Add("https://github.com/TitoniumGames/unity-template.git?path=/Assets/Services");
                }
            }
            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
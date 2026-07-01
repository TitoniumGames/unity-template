using UnityEditor;
using UnityEngine;

namespace Installer.Editor.Validator
{
    public class FrameworkValidatorWindow : EditorWindow
    {
        private Vector2 _scroll;

        public static void ShowWindow()
        {
            var window = GetWindow<FrameworkValidatorWindow>();

            window.titleContent = new GUIContent("Framework Validator");
            window.minSize = new Vector2(700, 520);

            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(15);

            DrawProgress();

            GUILayout.Space(15);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var item in FrameworkValidator.Items)
            {
                DrawDependency(item);

                GUILayout.Space(8);
            }

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            DrawFooter();
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
        }

        #endregion
    }
}
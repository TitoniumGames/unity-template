using Installer.Editor.Validator;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class PackageInstaller
{
    private static AddRequest _request;

    public static bool IsInstalling => _request != null;

    public static void Install(string packageUrl)
    {
        if (_request != null)
            return;

        EditorUtility.DisplayProgressBar(
            "Installing Package",
            "Installing package...",
            0f);

        _request = Client.Add(packageUrl);

        EditorApplication.update += Progress;
    }

    private static void Progress()
    {
        if (_request == null)
            return;

        if (!_request.IsCompleted)
        {
            float progress = Mathf.PingPong((float)EditorApplication.timeSinceStartup, 1f);
            EditorUtility.DisplayProgressBar(
                "Installing Package",
                "Please wait...",
                progress);

            return;
        }

        EditorApplication.update -= Progress;

        EditorUtility.ClearProgressBar();

        if (_request.Status == StatusCode.Success)
        {
            Debug.Log($"Installed {_request.Result.packageId}");
        }
        else
        {
            Debug.LogError(_request.Error.message);
        }

        _request = null;
    }
}
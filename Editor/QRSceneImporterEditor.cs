using UnityEditor;
using UnityEngine;
using System.IO;

public class QRSceneImporter
{
    [InitializeOnLoadMethod]
    private static void CopySceneToProject()
    {
        const string targetScenePath = "Assets/Scenes/QRScanScene.unity";

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScenePath) != null)
            return;

        string sourceScenePath = FindSceneInPackage();
        if (string.IsNullOrEmpty(sourceScenePath))
        {
            Debug.LogError("[QRSceneImporter] QRScanScene not found in the package.");
            return;
        }

        CreateDirectoryIfNeeded("Assets/Scenes");

        string error = AssetDatabase.CopyAsset(sourceScenePath, targetScenePath);
        if (!string.IsNullOrEmpty(error))
            Debug.LogError($"[QRSceneImporter] Failed to copy scene: {error}");
        else
            Debug.Log("[QRSceneImporter] QRScanScene copied to project successfully.");

        AssetDatabase.Refresh();
    }

    private static string FindSceneInPackage()
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string path in allAssetPaths)
        {
            if (path.EndsWith("QRScanScene.unity") && path.Contains("Packages/"))
                return path;
        }
        return null;
    }

    private static void CreateDirectoryIfNeeded(string path)
    {
        string currentPath = "";
        foreach (string folder in path.Split('/'))
        {
            currentPath += $"{folder}/";
            if (!AssetDatabase.IsValidFolder(currentPath.TrimEnd('/')))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(currentPath), folder);
        }
    }
}
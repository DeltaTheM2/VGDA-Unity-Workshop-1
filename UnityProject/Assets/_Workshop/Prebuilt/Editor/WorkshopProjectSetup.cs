using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// On first open: opens the Arena and sets grid snapping (Unity keeps both in Library, which isn't shared).
[InitializeOnLoad]
public static class WorkshopProjectSetup
{
    private const string ArenaScenePath = "Assets/_Workshop/Scenes/Arena.unity";
    private const string DoneMarkerPath = "Library/WorkshopSetupDone.txt";

    static WorkshopProjectSetup()
    {
        if (Application.isBatchMode || File.Exists(DoneMarkerPath))
        {
            return;
        }
        EditorApplication.delayCall += RunOnce;
    }

    private static void RunOnce()
    {
        EditorSnapSettings.gridSize = Vector3.one;
        EditorSnapSettings.move = Vector3.one;
        EditorSnapSettings.rotate = 90f;
        EditorSnapSettings.scale = 1f;
        EditorSnapSettings.gridSnapEnabled = true;
        Tools.pivotRotation = PivotRotation.Global; // grid snapping only works with Global handles

        var activeScene = EditorSceneManager.GetActiveScene();
        bool nothingOpen = string.IsNullOrEmpty(activeScene.path) && !activeScene.isDirty;
        if (nothingOpen && File.Exists(ArenaScenePath))
        {
            EditorSceneManager.OpenScene(ArenaScenePath);
        }

        File.WriteAllText(DoneMarkerPath, "Workshop first-open setup ran on this computer.");
    }
}

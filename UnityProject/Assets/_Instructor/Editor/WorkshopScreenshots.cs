using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Workshop.Instructor
{
    // Renders the images used in Docs/ (run it on the final checkpoint).
    //   Unity -batchmode -projectPath UnityProject -executeMethod Workshop.Instructor.WorkshopScreenshots.RenderDocImages -quit
    public static class WorkshopScreenshots
    {
        const int Width = 1280;
        const int Height = 720;
        static string ImagesDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/images"));

        [MenuItem("Workshop/Render Doc Screenshots", priority = 40)]
        public static void RenderDocImages()
        {
            Directory.CreateDirectory(ImagesDir);
            ShaderUtil.allowAsyncCompilation = false;

            EditorSceneManager.OpenScene(WorkshopBuilder.ArenaScenePath);
            var overviewFrom = new Vector3(-28f, 22f, -8f);
            var overviewAt = new Vector3(0f, 1f, 24f);
            Shot(null, overviewFrom, overviewAt, 45f); // warm-up: the first render can show placeholder materials
            Shot("demo-course-overview.png", overviewFrom, overviewAt, 45f);
            Shot("demo-course-ramp.png", new Vector3(8f, 6f, 2f), new Vector3(0f, 1f, 14f), 50f);
            Shot("demo-course-corridor.png", new Vector3(0f, 9f, 26f), new Vector3(0f, 2f, 42f), 50f);

            EditorSceneManager.OpenScene(WorkshopBuilder.WinScenePath);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; // overlay UI doesn't render into a camera; not saved
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f;
            Render(Camera.main, "win-scene.png");

            EditorSceneManager.OpenScene(WorkshopBuilder.ArenaScenePath);
            Debug.Log($"[Workshop] Screenshots written to {ImagesDir}");
        }

        static void Shot(string fileName, Vector3 from, Vector3 lookAt, float fieldOfView)
        {
            var cameraObject = new GameObject("ScreenshotCamera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            camera.fieldOfView = fieldOfView;
            camera.farClipPlane = 500f;
            cameraObject.transform.position = from;
            cameraObject.transform.LookAt(lookAt);
            Render(camera, fileName);
            Object.DestroyImmediate(cameraObject);
        }

        static void Render(Camera camera, string fileName)
        {
            var target = new RenderTexture(Width, Height, 24);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            if (fileName != null)
                File.WriteAllBytes(Path.Combine(ImagesDir, fileName), image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(image);
            target.Release();
        }
    }
}

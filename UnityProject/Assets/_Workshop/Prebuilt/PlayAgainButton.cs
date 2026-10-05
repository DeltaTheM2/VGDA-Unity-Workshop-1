using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgainButton : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("The scene to load if we don't know which level you came from.")]
    [SerializeField] private string fallbackScene = "Arena";

    // The last level unloaded before the Win scene, so Play Again returns to it (the Arena or a checkpoint scene).
    private static string lastLevel;

    [RuntimeInitializeOnLoadMethod]
    private static void TrackLastLevel()
    {
        SceneManager.sceneUnloaded += RememberLevel;
    }

    private static void RememberLevel(Scene scene)
    {
        if (scene.name != "Win")
        {
            lastLevel = scene.name;
        }
    }

    public void PlayAgain()
    {
        if (string.IsNullOrEmpty(lastLevel))
        {
            SceneManager.LoadScene(fallbackScene);
        }
        else
        {
            SceneManager.LoadScene(lastLevel);
        }
    }
}

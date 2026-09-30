using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgainButton : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("The scene to go back to.")]
    [SerializeField] private string sceneToLoad = "Arena";

    public void PlayAgain()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}

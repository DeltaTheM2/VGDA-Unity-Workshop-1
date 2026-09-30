using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalTrigger : MonoBehaviour
{
    [Header("Winning")]
    [Tooltip("The scene to load when the player reaches the goal. It must be listed in File > Build Profiles.")]
    [SerializeField] private string winSceneName = "Win";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(winSceneName);
        }
    }
}

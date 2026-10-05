// Checkpoint copy written by Tools/extract-lesson-scripts.sh from Docs/INSTRUCTOR_GUIDE.md. Edit the guide, not this file.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorkshopCheckpoints
{
    [AddComponentMenu("")]
    public class KillOnTouch : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentSceneName);
            }
        }
    }
}

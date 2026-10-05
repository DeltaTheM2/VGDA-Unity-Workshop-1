// Checkpoint copy written by Tools/extract-lesson-scripts.sh from Docs/INSTRUCTOR_GUIDE.md. Edit the guide, not this file.
using UnityEngine;

namespace WorkshopCheckpoints
{
    [AddComponentMenu("")]
    public class Coin : MonoBehaviour
    {
        [Header("Spinning")]
        [Tooltip("How fast the coin spins, in degrees per second.")]
        [SerializeField] private float spinSpeed = 90f;

        private void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.AddCoin();
                Destroy(gameObject);
            }
        }
    }
}

// Checkpoint copy written by Tools/extract-lesson-scripts.sh from Docs/INSTRUCTOR_GUIDE.md. Edit the guide, not this file.
using UnityEngine;

namespace WorkshopCheckpoints
{
    [AddComponentMenu("")]
    public class BouncePad : MonoBehaviour
    {
        [Header("Bounce")]
        [Tooltip("How hard to launch the player upward.")]
        [SerializeField] private float bounceForce = 16f;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Rigidbody playerBody = collision.rigidbody;
                playerBody.linearVelocity = new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z);
                playerBody.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            }
        }
    }
}

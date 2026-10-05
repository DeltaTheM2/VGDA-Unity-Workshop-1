// Checkpoint copy written by Tools/extract-lesson-scripts.sh from Docs/INSTRUCTOR_GUIDE.md. Edit the guide, not this file.
using UnityEngine;

namespace WorkshopCheckpoints
{
    [AddComponentMenu("")]
    public class Spinner : MonoBehaviour
    {
        [Header("Spinning")]
        [Tooltip("The axis to spin around, in the object's own space. (0, 1, 0) spins like a record; (0, 0, 1) spins like a wheel facing you.")]
        [SerializeField] private Vector3 axis = Vector3.up;
        [Tooltip("Degrees per second. Use a negative number to spin the other way.")]
        [SerializeField] private float degreesPerSecond = 180f;

        private void Update()
        {
            // Time.deltaTime turns "degrees per second" into "degrees this frame", so it spins at the same speed on any computer.
            transform.Rotate(axis, degreesPerSecond * Time.deltaTime);
        }
    }
}

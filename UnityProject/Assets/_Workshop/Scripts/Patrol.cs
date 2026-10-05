using UnityEngine;

public class Patrol : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("The points to visit, in order. After the last one, we head back to the first. Don't make them children of this object, or they'll move along with it!")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [Tooltip("Speed in meters per second.")]
    [SerializeField] private float speed = 3f;

    private int targetIndex = 0;

    private void Update()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        Vector3 targetPosition = waypoints[targetIndex].position;

        // Time.deltaTime IS needed here: we move the Transform ourselves, not through a velocity.
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (transform.position == targetPosition)
        {
            targetIndex = targetIndex + 1;
            if (targetIndex >= waypoints.Length)
            {
                targetIndex = 0;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform current = waypoints[i];
            Transform next = waypoints[(i + 1) % waypoints.Length];

            if (current == null || next == null)
            {
                continue;
            }

            Gizmos.DrawSphere(current.position, 0.3f);
            Gizmos.DrawLine(current.position, next.position);
        }
    }
}

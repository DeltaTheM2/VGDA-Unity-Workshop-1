using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 respawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        respawnPosition = transform.position;
    }

    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }

    public void Respawn()
    {
        transform.SetParent(null);
        rb.linearVelocity = Vector3.zero;
        transform.position = respawnPosition;
    }
}

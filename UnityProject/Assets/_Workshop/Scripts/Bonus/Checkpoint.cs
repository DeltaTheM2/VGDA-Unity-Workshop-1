using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Respawn")]
    [Tooltip("Where the player reappears, measured from this checkpoint's position. (0, 1, 0) = standing right here.")]
    [SerializeField] private Vector3 respawnOffset = new Vector3(0f, 1f, 0f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
            playerRespawn.SetRespawnPosition(transform.position + respawnOffset);
        }
    }
}

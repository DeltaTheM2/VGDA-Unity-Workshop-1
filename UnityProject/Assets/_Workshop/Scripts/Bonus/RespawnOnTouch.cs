using UnityEngine;

// Like KillOnTouch, but sends the player back to their last checkpoint instead of reloading the level.
public class RespawnOnTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
            playerRespawn.Respawn();
        }
    }
}

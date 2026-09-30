using UnityEngine;

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

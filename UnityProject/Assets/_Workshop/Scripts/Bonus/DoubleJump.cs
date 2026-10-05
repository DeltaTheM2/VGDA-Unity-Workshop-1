using UnityEngine;
using UnityEngine.InputSystem;

// Add next to PlayerController. PlayerController still jumps from the ground; this only adds mid-air jumps.
public class DoubleJump : MonoBehaviour
{
    [Header("Air Jumps")]
    [Tooltip("Extra jumps you get before touching the ground again. 1 = double jump, 2 = triple jump...")]
    [SerializeField] private int airJumps = 1;
    [Tooltip("Upward kick for each air jump. Try matching PlayerController's Jump Force.")]
    [SerializeField] private float airJumpForce = 9f;

    [Header("Ground Check (use the same values as PlayerController)")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private InputAction jumpAction;
    private int airJumpsLeft = 0;
    private bool airJumpQueued = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Update()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            airJumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            airJumpsLeft = airJumps;
        }
        else if (airJumpQueued && airJumpsLeft > 0)
        {
            airJumpsLeft = airJumpsLeft - 1;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * airJumpForce, ForceMode.Impulse);
        }

        airJumpQueued = false;
    }
}

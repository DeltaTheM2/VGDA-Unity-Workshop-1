using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Running speed, in meters per second.")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jumping")]
    [Tooltip("How hard we kick the player upward. Bigger = higher jump.")]
    [SerializeField] private float jumpForce = 9f;

    [Header("Ground Check")]
    [Tooltip("How far down from the player's center we look for ground. The capsule is 2 m tall, so its feet are 1 m down, plus 0.1 m of wiggle room.")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [Tooltip("Which layers count as ground you can jump off. Set this to Ground.")]
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Read input in Update: a tap lasts one frame, and FixedUpdate might not run on that frame.
    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }
    }

    // Change the Rigidbody in FixedUpdate, in step with the physics engine.
    private void FixedUpdate()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, rayColor);

        // No Time.deltaTime: velocity is already in meters per second.
        rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.y * moveSpeed);

        if (jumpQueued && isGrounded)
        {
            // Cancel any vertical speed so every jump is the same height.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpQueued = false;
    }
}

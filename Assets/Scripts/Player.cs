using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player movement (jump, slide) and detects collisions with obstacles.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Player Resource")]
    private int life; 

    [Header("Movement")]
    [Tooltip("The initial vertical velocity applied on jump.")]
    public float jumpVelocity = 15f; // Renamed from jumpForce for clarity
    public float slideDuration = 1.0f;

    [Header("Variable Jump")]
    [Tooltip("Gravity multiplier when falling (to feel 'weightier').")]
    public float fallMultiplier = 2.5f;
    [Tooltip("Gravity multiplier when jump button is released early (for short jumps).")]
    public float lowJumpMultiplier = 2f;


    // Player State
    [HideInInspector]
    public bool isJumping = false;
    [HideInInspector]
    public bool isSliding = false;
    private bool isGrounded = true;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Time.timeScale = 1; // Ensure game is running
    }

    void Update()
    {
        // 1. Update jumping state
        // The player is "jumping" if they are not grounded
        isJumping = !isGrounded;

        // 2. Handle Jump Input
        // This now just *starts* the jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // 3. Handle Slide Input
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isSliding)
        {
            isSliding = true;
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Get current velocity
        float yVelocity = rb.linearVelocity.y;

        // --- Variable Jump Gravity Logic ---
        if (yVelocity < 0) // We are falling
        {
            // Apply extra gravity when falling
            // (Physics.gravity.y is -9.81, so we add to it)
            // (fallMultiplier - 1) because 1x gravity is already applied
            yVelocity += Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (yVelocity > 0 && !Input.GetKey(KeyCode.Space)) // We are rising, but jump is released
        {
            // Apply extra gravity to cut the jump short
            yVelocity += Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // --- Apply Final Velocity ---
        // We apply the (potentially modified) yVelocity,
        // and lock X and Z axes to 0 for the endless runner.
        rb.linearVelocity = new Vector3(0, yVelocity, 0);
    }

    private void Jump()
    {
        // Set the vertical velocity directly.
        // This gives us the full height jump *potential*.
        // FixedUpdate will cut it short if the button is released.
        rb.linearVelocity = new Vector3(0, jumpVelocity, 0);
    }

    private IEnumerator SlideCoroutine()
    {
        isSliding = true;

        // --- Optional: Modify collider for sliding ---
        // e.g., transform.localScale = new Vector3(1, 0.5f, 1);

        yield return new WaitForSeconds(slideDuration);

        // --- Optional: Reset collider ---
        // e.g., transform.localScale = Vector3.one;

        isSliding = false;
    }

    /// <summary>
    /// This is the entry point for collision.
    /// It finds the Obstacle component and calls its polymorphic method.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we collided with is an Obstacle
        Obstacle obstacle = other.GetComponent<Obstacle>();

        if (obstacle != null)
        {
            // Use polymorphism:
            // We don't know or care what *kind* of obstacle it is.
            // We just tell it to handle the collision, and it will
            // run its own specific version of HandleCollision.
            obstacle.HandleCollision(this);
        }
    }

    // --- ADDED: Collision-based Ground Check ---

    /// <summary>
    /// This method is called by the physics engine when the player's
    /// collider *starts* touching another non-trigger collider.
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        // Check if the object we landed on has the "Ground" tag
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    /// <summary>
    /// This method is called by the physics engine when the player's
    /// collider *stops* touching another non-trigger collider.
    /// </summary>
    /// 

    private void OnCollisionExit(Collision collision)
    {
        // Check if the object we just left was tagged as "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public void SetLife(int targetLife)
    {
        life = targetLife;
    }

    public void ReduceLife()
    {
        Debug.Log("Life Reduced");

        if (life <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Public method that obstacles can call to end the game.
    /// </summary>
    private void Die()
    {
        Debug.Log("Game Over!");
        // Stop all game movement
        Time.timeScale = 0;

        // Here you would add logic for a game over screen, respawning, etc.
    }
}
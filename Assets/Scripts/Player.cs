using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    //Rigidbody
    private Rigidbody rb;

    //Stationary Position 
    private Vector3 stationaryPosition;
    private float velocityX = 0.0f;
    public float smoothTime = 0.25f;

    //Actions
    public Action<GameObject> OnSteppedPlatform;

    //Debug
    MeshRenderer meshRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = rb.GetComponent<MeshRenderer>();
        stationaryPosition = transform.position;
        Time.timeScale = 1; // Ensure game is running
    }

    void Update()
    {
        //if (IsPlayerOffScreen())
        //{
        //    Debug.Log("Player get damaged");
        //    ResetPos();
        //    ReduceLife();
        //    return;
        //}

        GoToStationaryPos();
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
            StartCoroutine(SlideCoroutine());
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

    private void GoToStationaryPos()
    {
        if (isGrounded) 
        {
            return;
        }

        float newX = Mathf.SmoothDamp(
            transform.position.x, // The current position
            stationaryPosition.x,              // The target position
            ref velocityX,        // The current velocity (modified by the function)
            smoothTime            // The time to reach the target
        );

        transform.position = new Vector3(
            newX,
            transform.position.y,
            transform.position.z
        );
    }

    private IEnumerator SlideCoroutine()
    {
        isSliding = true;

        // --- Optional: Modify collider for sliding ---
        // e.g., transform.localScale = new Vector3(1, 0.5f, 1);

        meshRenderer.material.color = Color.yellow;
        yield return new WaitForSeconds(slideDuration);
        meshRenderer.material.color = Color.white;

        // --- Optional: Reset collider ---
        // e.g., transform.localScale = Vector3.one;

        isSliding = false;
    }

    private void ResetPos()
    {
        transform.position = new Vector3(
             stationaryPosition.x,
             5f,
             transform.position.z
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we collided with is an Obstacle
        try
        {
            Obstacle obstacle = other.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.HandleCollision(this);
            }
        }
        catch (Exception e) { Debug.LogWarning(e); }

        try
        {
            Platform pitPlatform = other.GetComponent<Platform>();
            if (pitPlatform != null)
            {
                pitPlatform.HandleCollision(this);
            }
        }
        catch (Exception e) { Debug.LogWarning(e); }
    }

    // --- ADDED: Collision-based Ground Check ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            OnSteppedPlatform(collision.gameObject);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Check if the object we landed on has the "Ground" tag
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

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

        life--;
        if (life <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Game Over!");
        // Stop all game movement
        Time.timeScale = 0;
    }
}
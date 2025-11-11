using UnityEngine;

/// <summary>
/// ABSTRACT base class for all obstacles.
/// It defines the *contract* that all obstacles must follow.
/// In this case, every obstacle MUST have a HandleCollision method.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class Obstacle : MonoBehaviour
{
    protected virtual void Start()
    {
        // Ensure the collider is set to be a trigger
        // so OnTriggerEnter in the Player script fires.
        GetComponent<Collider>().isTrigger = true;
    }

    /// <summary>
    /// Abstract method. Each concrete obstacle (Pit, Height, Low)
    /// MUST provide its own implementation for this method.
    /// </summary>
    /// <param name="player">The player object that collided with this obstacle.</param>
    public abstract void HandleCollision(Player player);
}
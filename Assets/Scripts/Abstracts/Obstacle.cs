using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Obstacle : MonoBehaviour
{
    protected virtual void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public abstract void HandleCollision(Player player);
}
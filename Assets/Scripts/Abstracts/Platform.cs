using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Platform : MonoBehaviour
{
    protected virtual void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public abstract void HandleCollision(Player player);
}

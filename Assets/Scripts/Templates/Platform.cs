using UnityEngine;

//Made this platform template, In case I want to add plataform with effects
//in the future. (e.g slow platform, dash platform, anti gravity platform. etc)
[RequireComponent(typeof(Collider))]
public abstract class Platform : MonoBehaviour
{
    protected virtual void Start()
    {
        //Make sure this is collider not trigger
        GetComponent<Collider>().isTrigger = false;
    }

    public abstract void HandleCollision(Player player);
}

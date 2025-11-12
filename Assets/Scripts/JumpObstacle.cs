using UnityEngine;

public class JumpObstacle : Obstacle
{
    public override void HandleCollision(Player player)
    {
        player.ReduceLife();
        Debug.Log("Collided with jump obstacle, Reducing health");
    }
}

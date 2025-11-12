using UnityEngine;

public class SlideObstacle : Obstacle
{
    public override void HandleCollision(Player player)
    {
        if(player.isSliding)
        {
            Debug.Log("Player is Sliding, Damage is ignored");
            return;
        }

        player.ReduceLife();
    }
}

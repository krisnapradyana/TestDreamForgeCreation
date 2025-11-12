using UnityEngine;

public class PitPlatform : Platform
{
    CoreGameplay coreGameplay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        coreGameplay = FindFirstObjectByType<CoreGameplay>();
    }

    public float GetDistanceToPlayer()
    {
        return Vector3.Distance(this.transform.position, coreGameplay.CurrentPlayer.transform.position);
    }

    public override void HandleCollision(Player player)
    {
        player.ReduceLife();
        coreGameplay.RefreshPlayer();   
    }
    
}

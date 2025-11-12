using UnityEngine;

public class NormalPlatform : Platform
{
    [SerializeField] Transform refreshPoint;
    [SerializeField] CoreGameplay coreGameplay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        //override start from base 
        //I did not need to set this collider into trigger
        
    }

    public void SetCoreGameplay(CoreGameplay gameplayManager)
    {
        coreGameplay = gameplayManager;
    }

    public float DistanceToPlayer()
    {
        return Vector3.Distance(refreshPoint.transform.position, coreGameplay.CurrentPlayer.transform.position);
    }

    public Vector3 GetRefreshPosition()
    {
        return refreshPoint.position; 
    }

    // Update is called once per frame
    public override void HandleCollision(Player player)
    {
        Debug.Log("Stepped on normal platform");
    }
}

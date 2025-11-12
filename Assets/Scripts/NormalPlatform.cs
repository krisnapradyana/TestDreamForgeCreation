using System.Collections.Generic;
using UnityEngine;

public class NormalPlatform : Platform
{
    [SerializeField] Transform refreshPoint;
    [SerializeField] List<GameObject> obstacles;
    [SerializeField] List<Transform> obstaclePoint;
    [SerializeField] float obstacleSpawnRate = 0.05f;
    CoreGameplay coreGameplay;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        //Spawn on obstacle per platform
        float randomChance = Random.value;

        if (randomChance > obstacleSpawnRate)
        {            
            return;
        }

        var spawnedObstacles = Instantiate(obstacles[Random.Range(0, obstacles.Count)], this.transform);
        spawnedObstacles.transform.position = obstaclePoint[Random.Range(0, obstaclePoint.Count)].transform.position;
    }

    public void SetObstacleRate(float spawnRate)
    {
        obstacleSpawnRate = spawnRate;
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

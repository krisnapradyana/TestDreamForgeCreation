using System.Collections.Generic;
using UnityEngine;

public class NormalPlatform : MonoBehaviour
{
    public GameObject ground;
    [SerializeField] Transform refreshPoint;
    [SerializeField] List<GameObject> obstacles;
    [SerializeField] List<Transform> obstaclePoint;
    [SerializeField] float obstacleSpawnRate = 0.05f;
    [SerializeField] GameObject currentObstacle = null;    
    CoreGameplay coreGameplay;

    private void Awake()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ShowPlatform()
    {
        gameObject.SetActive(true);
        //Spawn on obstacle per platform
        float randomChance = Random.value;

        if (randomChance > obstacleSpawnRate)
        {
            return;
        }

        currentObstacle = Instantiate(obstacles[Random.Range(0, obstacles.Count)], this.transform);
        currentObstacle.transform.position = obstaclePoint[Random.Range(0, obstaclePoint.Count)].transform.position;
    }

    public void HidePlatform()
    {
        gameObject.SetActive(false);
        Destroy(currentObstacle);
        currentObstacle = null;
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
    //public override void HandleCollision(Player player)
    //{
    //    Debug.Log("Stepped on normal platform");
    //}
}

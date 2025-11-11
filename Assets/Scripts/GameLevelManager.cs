using System.Collections.Generic;
using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    [Header("Platform Pooling")]
    [SerializeField] List<GameObject> platformPrefabs;
    [SerializeField] int poolSize;
    private List<GameObject> platformPool;

    [Header("Platform spawning settings")]
    [SerializeField] float spawnOffsetX = 0;
    [SerializeField] float despawnX = -20f;
    [SerializeField] float triggerSpawnX;
    [SerializeField] float platformYMin;
    [SerializeField] float heigthOffset;
    [SerializeField] float platformYMax;
    [SerializeField] float platformSpeed;
    float edgeScreenRight = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializePlatformPool();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlatform();
    }

    void InitializePlatformPool()
    {
        platformPool = new List<GameObject>();
        if (platformPrefabs.Count <= 0)
        {
            Debug.Log("No platform prefab provided");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            {
                var randomNum = Random.Range(0, platformPrefabs.Count);
                GameObject temp = Instantiate(platformPrefabs[randomNum], transform);
                temp.SetActive(false);
                platformPool.Add(temp);
            }
        }
    }

    void SpawnPlatform()
    {
        GameObject platform = GetPooledPlatform();
        if (platform == null)
        {
            edgeScreenRight = triggerSpawnX + 1;
            return;
        }

        //get platform width
        float platformWidth = GetPlatformWidth(platform);
        Debug.Log("Platform width : " + platformWidth);
        //random height
        float randomY = Random.Range(platformYMin - heigthOffset, platformYMax - heigthOffset);
        //spawn x position
        float spawnX = (edgeScreenRight + (platformWidth / 2));
        platform.transform.position = new Vector3(spawnX - spawnOffsetX, randomY, 0);
        platform.SetActive(true);

        edgeScreenRight = spawnX + (platformWidth / 2);
        
    }

    GameObject GetPooledPlatform()
    {
        foreach (var platform in platformPool)
        {
            if (!platform.activeInHierarchy)
            {
                return platform;
            }
        }
        Debug.LogWarning("Platform pool exhausted! Consider increasing pool size.");
        return null;
    }

    float GetPlatformWidth(GameObject platform)
    {
        return platform.transform.localScale.x;
    }

    void MovePlatform()
    {
        if (platformPool == null) Debug.LogError("No platform provided");
        float moveDistamce = platformSpeed * Time.deltaTime;

        foreach (var platform in platformPool)
        {
            if (platform.activeInHierarchy)
            {
                platform.transform.Translate(-moveDistamce, 0, 0);
                if(platform.transform.position.x < despawnX)
                {
                    platform.SetActive(false);
                }
            }
        }

        edgeScreenRight -= moveDistamce;

        while (edgeScreenRight < triggerSpawnX)
        {
            SpawnPlatform();
        }
    }
}

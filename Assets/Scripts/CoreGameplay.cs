using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class CoreGameplay : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("These settings will decide when player off screen, and the outcome is player get damage")]
    [SerializeField] float offScreenMargin = 1.0f;
    [SerializeField] Player player;
    [SerializeField] int targetLife;

    [Header("Level Settings")]
    [SerializeField] private float travelledDistance = 0f;
    [SerializeField] private float baseDistance = 1000f;

    [Header("Platform Pooling")]
    [SerializeField] List<NormalPlatform> platformPrefabs;
    [SerializeField] int poolSize;
    private List<NormalPlatform> platformPool;

    [Header("Queued Grounds")]
    [SerializeField] Queue<NormalPlatform> platformQueue;

    [Header("Platform spawning settings")]
    [SerializeField] float spawnOffsetX = 0;
    [SerializeField] float despawnX = -20f;
    [SerializeField] float triggerSpawnX;
    [SerializeField] float platformYMin;
    [SerializeField] float heigthOffset;
    [SerializeField] float platformYMax;
    [SerializeField] float platformSpeed;
    [SerializeField] int preparePlatformCount;

    [Header("Pit Generation")]
    [SerializeField] float pitSpawnChance = 0.25f;    
    [SerializeField] float pitGap = 5.0f;             
    [SerializeField] int minPlatformsBetweenPits = 2;
    private int platformsSpawnedSincePit = 0; // Counter

    [Header("Obstacle Settings")]
    [SerializeField] float obstacleRate = 0.3f;

    float edgeScreenRight = 0;

    [Header("Level State")]
    [SerializeField] bool isMoving = true;

    private int spawnedCount = 0;
    private bool doneStartPrepare = false;
    private bool levelHasCompleted = false;
    public Player CurrentPlayer { get { return player; }}


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player.SetLife(targetLife);
        platformQueue = new Queue<NormalPlatform>();
        InitializePlatformPool();
    }

    // Update is called once per frame
    void Update()
    {
        if (levelHasCompleted) return;

        if (travelledDistance > baseDistance)
        {
            Debug.Log("Game Ended");
            return;
        }

        if (IsPlayerOffScreen())
        {
            Debug.Log("Player get damaged");
            player.ReduceLife();
            RefreshPlayer();
            return;
        }

        MovePlatform();
        TravelDistance();
    }

    void InitializePlatformPool()
    {
        platformPool = new List<NormalPlatform>();
        if (platformPrefabs.Count <= 0)
        {
            Debug.Log("No platform prefab provided");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            {
                var randomNum = Random.Range(0, platformPrefabs.Count);
                GameObject temp = Instantiate(platformPrefabs[randomNum].gameObject, transform);
                var currentPlatform = temp.GetComponent<NormalPlatform>();
                currentPlatform.SetCoreGameplay(this);
                currentPlatform.SetObstacleRate(obstacleRate);
                temp.SetActive(false);
                platformPool.Add(currentPlatform);
            }
        }
    } 

    void TravelDistance()
    {
        if(!isMoving)
        {
            return;
        }

        travelledDistance += 1 * Time.deltaTime;
    }

    void SpawnPlatform()
    {
        if (doneStartPrepare && platformsSpawnedSincePit >= minPlatformsBetweenPits)
        {
            // Check the random chance
            if (Random.Range(0f, 1f) < pitSpawnChance)
            {
                // --- CREATE A PIT ---
                edgeScreenRight += pitGap;
                // Reset the counter
                platformsSpawnedSincePit = 0;
                return;
            }
        }

        NormalPlatform platform = null;

        //separate behaviour post preparation
        if (doneStartPrepare)
        {
            platform = GetPooledPlatform();
        }
        else
        {
            platform = GetNormalPlatform();
        }

        if (platform == null)
        {
            edgeScreenRight = triggerSpawnX + 1;
            return;
        }

        //get platform width
        float platformWidth = GetPlatformWidth(platform);
        Debug.Log("Platform width : " + platformWidth);
        float spawnX = (edgeScreenRight + (platformWidth / 2));

        //Separate behaviour post preparation
        if (doneStartPrepare)
        {
            float randomY = Random.Range(platformYMin - heigthOffset, platformYMax - heigthOffset);
            platform.transform.position = new Vector3(spawnX - spawnOffsetX, randomY, 0);

        }
        else
        {
            float posY = -10;
            platform.transform.position = new Vector3(spawnX - spawnOffsetX, posY, 0);
        }
        platform.ShowPlatform();
        platformQueue.Enqueue(platform);

        spawnedCount++;
        if (spawnedCount > preparePlatformCount)
        {
            doneStartPrepare = true;
        }
        edgeScreenRight = spawnX + (platformWidth / 2);

        if (doneStartPrepare)
        {
            platformsSpawnedSincePit++;
        }
    }

    NormalPlatform GetPooledPlatform()
    {
        int poolSize = platformPool.Count;

        // 1. Pick a random starting point
        int startIndex = Random.Range(0, poolSize);

        // 2. Loop through the entire pool (exactly 'poolSize' times)
        for (int i = 0; i < poolSize; i++)
        {
            int indexToCheck = (startIndex + i) % poolSize;
            if (!platformPool[indexToCheck].gameObject.activeInHierarchy)
            {
                // Found one! Return it.
                return platformPool[indexToCheck];
            }
        }
        Debug.LogWarning("Platform pool exhausted! Consider increasing pool size.");
        return null;

    }
    NormalPlatform GetNormalPlatform()
    {
        int poolSize = platformPool.Count;
        if (poolSize == 0) return null; // Safety check

        int startIndex = Random.Range(0, poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            int indexToCheck = (startIndex + i) % poolSize;
            NormalPlatform platformObject = platformPool[indexToCheck];

            if (!platformObject.gameObject.activeInHierarchy && platformObject.TryGetComponent<NormalPlatform>(out NormalPlatform platform))
            {
                return platformObject;
            }
        }
        return null;
    }

    float GetPlatformWidth(NormalPlatform platform)
    {
        return platform.ground.transform.localScale.x;
    }

    void MovePlatform()
    {
        if (!isMoving)
        {
            return;
        }

        if (platformPool == null) Debug.LogError("No platform provided");
        float moveDistamce = platformSpeed * Time.deltaTime;

        foreach (var platform in platformPool)
        {
            if (platform.gameObject.activeInHierarchy)
            {
                platform.transform.Translate(-moveDistamce, 0, 0);
                if(platform.transform.position.x < despawnX)
                {
                    platform.GetComponent<NormalPlatform>().HidePlatform();
                    platformQueue.Dequeue();
                }
            }
        }

        edgeScreenRight -= moveDistamce;

        while (edgeScreenRight < triggerSpawnX)
        {
            SpawnPlatform();
        }
    }

    public void RefreshPlayer()
    {
        var activeList = platformPool
            .Where(obj => obj != null && obj.gameObject.activeInHierarchy)
            .ToList();

        NormalPlatform closestObject = activeList
            .Select(obj => new { GO = obj, Comp = obj.GetComponent<NormalPlatform>() })
            .Where(x => x.Comp != null)                                           
            .OrderBy(x => x.Comp.DistanceToPlayer())                                       
            .Select(x => x.GO)
            .FirstOrDefault();

        player.transform.position = closestObject.GetComponent<NormalPlatform>().GetRefreshPosition();

        //player.transform.position = new Vector3(targetX, yPlatformPos, player.transform.position.z);
        StartCoroutine(RefreshPlayerCoroutine());
    }

    IEnumerator RefreshPlayerCoroutine()
    {
        isMoving = false;
        yield return new WaitForSeconds(1f);
        isMoving = true;
    }

    bool IsPlayerOffScreen()
    {
        // Convert the player's world position to viewport position
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(player.transform.position);

        // Now check if it's outside the 0-1 range on X or Y,
        // using the margin for a buffer.
        bool isOffScreen = (
            viewportPosition.x < 0 - offScreenMargin ||
            viewportPosition.x > 1 + offScreenMargin ||
            viewportPosition.y < 0 - offScreenMargin ||
            viewportPosition.y > 1 + offScreenMargin
        );

        return isOffScreen;
    }
}

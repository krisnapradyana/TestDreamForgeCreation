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
    [SerializeField] List<Platform> platformPrefabs;
    [SerializeField] int poolSize;
    private List<GameObject> platformPool;

    [Header("Queued Grounds")]
    private GameObject playerCurrentPlatform;
    [SerializeField] Queue<GameObject> platformQueue;

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
    public float pitSpawnChance = 0.25f; // 25% chance to spawn a pit
    public float pitGap = 5.0f;           // How wide the pit/gap should be
    public int minPlatformsBetweenPits = 2; // Must spawn at least 2 platforms before a new pit

    private int platformsSpawnedSincePit = 0; // Counter

    float edgeScreenRight = 0;

    [Header("Time based settings")]
    [SerializeField] bool isMoving = true;

    private int spawnedCount = 0;
    private bool doneStartPrepare = false;
    public Player CurrentPlayer { get { return player; }}

    private void OnDestroy()
    {
        player.OnSteppedPlatform -= SetCurrentPlatform; 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player.SetLife(targetLife);
        player.OnSteppedPlatform += SetCurrentPlatform;
        platformQueue = new Queue<GameObject>();
        InitializePlatformPool();
    }

    // Update is called once per frame
    void Update()
    {
        if (travelledDistance > baseDistance)
        {
            Debug.Log("Game Ended");
            return;
        }

        if (IsPlayerOffScreen())
        {
            Debug.Log("Player get damaged");
            //player.ResetPos();
            player.ReduceLife();
            RefreshPlayer();
            return;
        }

        MovePlatform();
        TravelDistance();
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
                GameObject temp = Instantiate(platformPrefabs[randomNum].gameObject, transform);
                temp.GetComponent<NormalPlatform>().SetCoreGameplay(this);
                temp.SetActive(false);
                platformPool.Add(temp);
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

    void SetCurrentPlatform (GameObject curentPlatform)
    {
        playerCurrentPlatform = curentPlatform;
    }

    void SpawnPlatform()
    {
        // --- PIT CREATION LOGIC ---
        // We only try to spawn a pit AFTER the preparation phase
        // AND after the minimum number of platforms have spawned
        if (doneStartPrepare && platformsSpawnedSincePit >= minPlatformsBetweenPits)
        {
            // Check the random chance
            if (Random.Range(0f, 1f) < pitSpawnChance)
            {
                // --- CREATE A PIT ---
                // Instead of spawning a platform, just advance the "edge"
                edgeScreenRight += pitGap;

                // Reset the counter
                platformsSpawnedSincePit = 0;

                // Stop here; we are done for this spawn call
                return;
            }
        }

        // --- PLATFORM CREATION LOGIC (if no pit was created) ---

        GameObject platform = null;

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
        platform.SetActive(true);
        platformQueue.Enqueue(platform);

        spawnedCount++;
        if (spawnedCount > preparePlatformCount)
        {
            doneStartPrepare = true;
        }
        edgeScreenRight = spawnX + (platformWidth / 2);

        // --- UPDATE PIT COUNTER ---
        // If we spawned a platform *after* preparation, count it.
        if (doneStartPrepare)
        {
            platformsSpawnedSincePit++;
        }
    }

    GameObject GetPooledPlatform()
    {
        int poolSize = platformPool.Count;

        // 1. Pick a random starting point
        int startIndex = Random.Range(0, poolSize);

        // 2. Loop through the entire pool (exactly 'poolSize' times)
        for (int i = 0; i < poolSize; i++)
        {
            int indexToCheck = (startIndex + i) % poolSize;
            if (!platformPool[indexToCheck].activeInHierarchy)
            {
                // Found one! Return it.
                return platformPool[indexToCheck];
            }
        }
        Debug.LogWarning("Platform pool exhausted! Consider increasing pool size.");
        return null;

    }
    GameObject GetNormalPlatform()
    {
        int poolSize = platformPool.Count;
        if (poolSize == 0) return null; // Safety check

        // 1. Pick a random starting point to find a different object each time
        int startIndex = Random.Range(0, poolSize);

        // 2. Loop through the entire pool (exactly 'poolSize' times)
        for (int i = 0; i < poolSize; i++)
        {
            int indexToCheck = (startIndex + i) % poolSize;
            GameObject platformObject = platformPool[indexToCheck];

            // 3. Check BOTH conditions:
            //    a) Is the object inactive (i.e., available to be used)?
            //    b) Does it have the 'NormalPlatform' component?
            if (!platformObject.activeInHierarchy &&
                 platformObject.TryGetComponent<NormalPlatform>(out NormalPlatform platform))
            {
                // Found a perfect match! Return it.
                // DO NOT call SetActive(true) here. 
                // The code that *calls* this function should activate it.
                return platformObject;
            }
        }

        // We looped through everything and couldn't find an available platform
        // of this type. Return null.
        return null;
    }

    float GetPlatformWidth(GameObject platform)
    {
        return platform.transform.localScale.x;
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
            if (platform.activeInHierarchy)
            {
                platform.transform.Translate(-moveDistamce, 0, 0);
                if(platform.transform.position.x < despawnX)
                {
                    platform.SetActive(false);
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
            .Where(obj => obj != null && obj.activeInHierarchy)
            .ToList();

        GameObject closestObject = activeList
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

    GameObject GetNextPlatform()
    {
        List<GameObject> myList = platformQueue.ToList();

        int index = myList.IndexOf(playerCurrentPlatform);

        if (index != -1 && index < myList.Count - 1)
        {
            GameObject nextItem = myList[index + 1];   
            return nextItem;
        }

        Debug.Log("Condition is not fullfilled check the logic here");
        return null;
    }


    bool IsPlayerOffScreen()
    {
        // Convert the player's world position to viewport position
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(player.transform.position);

        // Check if the Z coordinate is negative.
        // This means the object is *behind* the camera, which is
        // technically "off-screen" even if X/Y are 0-1.
        if (viewportPosition.z < 0)
        {
            return true;
        }

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

using UnityEngine;
using System.Collections.Generic; // Added this line

/// <summary>
/// This script manages game level difficulty and controls platform movement.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Game Level Settings")]
    [Tooltip("The current level. Difficulty scales based on this.")]
    [SerializeField] int level = 1;
    [SerializeField] int playerLife; // Note: This isn't used in the requested methods, but is here.

    [Tooltip("Modifier for obstacle spawning. Scales with level.")]
    [SerializeField] float obsacleRandomModifier;

    [Tooltip("Base speed for platform movement (both horizontal and vertical). Scales with level.")]
    [SerializeField] float platformSpeed = 1.0f;

    [Tooltip("The highest Y position the platform will move to.")]
    [SerializeField] float platformYMax = 1.0f;

    [Tooltip("The lowest Y position the platform will move to.")]
    [SerializeField] float platformYMin = -1.0f;

    [Tooltip("Base spacing between obstacles. Scales (reduces) with level.")]
    [SerializeField] int obstacleSpace = 10;

    [Tooltip("Base heigth of the platform, the higher value more lower the platform will be")]
    [SerializeField] float platformHeigthOffset = 10f;

    [Tooltip("The base distance/length for level 1.")]
    [SerializeField] float baseLevelDistance = 1000f;


    [Header("Platform Pooling")]
    [Tooltip("The prefab for the platform to be spawned.")]
    [SerializeField] GameObject platformPrefab;
    [Tooltip("The total number of platforms to keep in the pool.")]
    [SerializeField] int poolSize = 30;
    [Tooltip("The X position (left) at which platforms are disabled.")]
    [SerializeField] float despawnX = -20f;
    [Tooltip("The X position (right) that triggers a new platform to spawn.")]
    [SerializeField] float spawnTriggerX = 20f;


    private float basePlatformSpeed;
    private int baseObstacleSpace;
    private float baseObsacleRandomModifier;
    private float baseDistance; // To store the base level distance

    private List<GameObject> platformPool;
    private float rightmostPlatformEdge = 0f;
    private float currentLevelDistance; // The calculated total distance for this level

    private int verticalMoveDirection = 1; // 1 for up, -1 for down

    /// <summary>
    /// Awake is called before Start. Use it to initialize
    /// variables and store base values.
    /// </summary>
    void Awake()
    {
        basePlatformSpeed = platformSpeed;
        baseObstacleSpace = obstacleSpace;
        baseObsacleRandomModifier = obsacleRandomModifier;
        baseDistance = baseLevelDistance;
    }

    /// <summary>
    /// Start is called once on the first frame.
    /// It's a great place to set the initial difficulty.
    /// </summary>
    void Start()
    {
        SetLevelDifficulties();
        InitializePlatformPool();
        rightmostPlatformEdge = despawnX;
    }

    /// <summary>
    /// Update is called once per frame.
    /// We use it to call our platform movement logic.
    /// </summary>
    void Update()
    {
        MovePlatforms();
    }


    /// <summary>
    /// Calculates and applies difficulty settings based on the current 'level'.
    /// </summary>
    private void SetLevelDifficulties()
    {
        platformSpeed = basePlatformSpeed * Mathf.Pow(1.1f, (float)level - 1f);

        float levelModifier = 1f + ((float)level - 1f) * 0.2f; // Increases by 20% per level
        obstacleSpace = (int)Mathf.Max(1, (float)baseObstacleSpace / levelModifier);

        obsacleRandomModifier = baseObsacleRandomModifier * (1f + ((float)level - 1f) * 0.1f);

        currentLevelDistance = baseDistance * Mathf.Pow(1.15f, (float)level - 1f);


        Debug.Log($"Level {level} Settings: Speed={platformSpeed}, Space={obstacleSpace}, Mod={obsacleRandomModifier}, Distance={currentLevelDistance}");
    }

    /// <summary>
    /// Initializes the object pool for platforms.
    /// </summary>
    private void InitializePlatformPool()
    {
        platformPool = new List<GameObject>();
        if (platformPrefab == null)
        {
            Debug.LogError("Platform Prefab is not assigned in GameLevelManager!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject plat = Instantiate(platformPrefab, transform);
            plat.SetActive(false);
            platformPool.Add(plat);
        }
    }

    /// <summary>
    /// Gets an inactive platform from the pool.
    /// </summary>
    private GameObject GetPooledPlatform()
    {
        foreach (GameObject plat in platformPool)
        {
            if (!plat.activeInHierarchy)
            {
                return plat;
            }
        }
        Debug.LogWarning("Platform pool exhausted! Consider increasing pool size.");
        return null; // No available platforms
    }

    /// <summary>
    /// Helper method to get the width of a platform.
    /// </summary>
    private float GetPlatformWidth(GameObject platform)
    {
        if (platform.TryGetComponent<SpriteRenderer>(out var sr))
        {
            return sr.bounds.size.x;
        }
        if (platform.TryGetComponent<BoxCollider2D>(out var bc))
        {
            return bc.bounds.size.x;
        }

        Debug.LogWarning($"Platform '{platform.name}' has no SpriteRenderer or BoxCollider2D to get width from. Defaulting to 1f.", platform);
        return 1f;
    }

    /// <summary>
    /// Spawns a single platform from the pool at the correct new position.
    /// </summary>
    private void SpawnPlatform()
    {
        GameObject platform = GetPooledPlatform();
        if (platform == null)
        {
            rightmostPlatformEdge = spawnTriggerX + 1f; // Stop trying to spawn
            return;
        }

        float platformWidth = GetPlatformWidth(platform);

        float randomY = Random.Range(platformYMin - platformHeigthOffset, platformYMax - platformHeigthOffset);

        float spawnX = rightmostPlatformEdge + obstacleSpace + (platformWidth / 2f);

        platform.transform.position = new Vector3(spawnX, randomY, 0);
        platform.SetActive(true);

        rightmostPlatformEdge = spawnX + (platformWidth / 2f);
    }


    /// <summary>
    /// Moves all active platforms and spawns new ones as needed.
    /// </summary>
    private void MovePlatforms()
    {
        if (platformPool == null) return;

        float moveDistance = platformSpeed * Time.deltaTime;

        foreach (GameObject platform in platformPool)
        {
            if (platform.activeInHierarchy)
            {
                platform.transform.Translate(-moveDistance, 0, 0);

                if (platform.transform.position.x < despawnX)
                {
                    platform.SetActive(false);
                }
            }
        }

        rightmostPlatformEdge -= moveDistance;

        while (rightmostPlatformEdge < spawnTriggerX && rightmostPlatformEdge < currentLevelDistance)
        {
            SpawnPlatform();
        }
    }
}
using UnityEngine;
using UnityEngine.Pool;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform followTarget;  
    [SerializeField] private Transform spawnOrigin;   

    [Header("Prefabs")]
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private GameObject coinPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Khoảng cách theo trục Y giữa các obstacle. Nên để bằng jumpHeight của Ball.")]
    [SerializeField] private float verticalStep = 3f;

    [Tooltip("Spawner sẽ luôn đảm bảo spawn tới khoảng này phía TRÊN followTarget.")]
    [SerializeField] private float spawnAheadDistance = 30f;

    [Tooltip("Obstacle nào thấp hơn followTarget quá khoảng này sẽ bị huỷ.")]
    [SerializeField] private float despawnBelowDistance = 10f;

    [Tooltip("Số obstacle spawn sẵn khi game bắt đầu.")]
    [SerializeField] private int prewarmCount = 5;

    [Header("Probabilities (sum ≈ 1.0)")]
    [Range(0f, 1f)] public float blockWeight = 0.4f;
    [Range(0f, 1f)] public float brickWeight = 0.4f;
    [Range(0f, 1f)] public float coinWeight  = 0.2f;

    private float _highestY;
    private float _initialHighestY;
    private float _obstacleX;
    private bool  _running = false;

    // Pools
    private ObjectPool<GameObject> _blockPool;
    private ObjectPool<GameObject> _brickPool;
    private ObjectPool<GameObject> _coinPool;

    private void OnEnable()
    {
        MainMenuController.OnGameStart += HandleGameStart;
    }

    private void OnDisable()
    {
        MainMenuController.OnGameStart -= HandleGameStart;
    }

    private void Awake()
    {
        if (!spawnOrigin)
            spawnOrigin = transform;

        _obstacleX       = spawnOrigin.position.x;
        _highestY        = spawnOrigin.position.y;
        _initialHighestY = _highestY;

        if (blockPrefab) _blockPool = CreatePool(blockPrefab);
        if (brickPrefab) _brickPool = CreatePool(brickPrefab);
        if (coinPrefab)  _coinPool  = CreatePool(coinPrefab);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var obj = Instantiate(prefab);
                obj.SetActive(false);
                return obj;
            },
            actionOnGet: obj =>
            {
                obj.SetActive(true);
            },
            actionOnRelease: obj =>
            {
                obj.SetActive(false);
                obj.transform.SetParent(null); 

                var obs = obj.GetComponent<Obstacle>();
                if (obs != null)
                    obs.ResetForReuse();
            },
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: 16,
            maxSize: 128
        );
    }

    private void Start()
    {
        if (!followTarget)
        {
            Debug.LogWarning("[ObstacleSpawner] followTarget chưa gán.");
        }
    }

    private void HandleGameStart()
    {
        _running = true;
        Prewarm();
    }

    private void Update()
    {
        if (!_running || !followTarget) return;

        while (_highestY < followTarget.position.y + spawnAheadDistance)
        {
            _highestY += verticalStep;
            SpawnOneAtY(_highestY);
        }

        DespawnLowObstacles();
    }

    private void Prewarm()
    {
        float y = spawnOrigin.position.y;

        for (int i = 0; i < prewarmCount; i++)
        {
            SpawnOneAtY(y);
            y += verticalStep;
        }

        _highestY = y - verticalStep;
    }

    private void SpawnOneAtY(float y)
    {
        ObstacleType type = GetRandomType();
        GameObject obj    = GetFromPool(type);
        if (!obj) return;

        Vector3 pos = new Vector3(_obstacleX, y, spawnOrigin.position.z);

        obj.transform.SetParent(transform, true);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.identity;
        obj.name = $"Obstacle_{type}_{y:0.0}";

        var obs = obj.GetComponent<Obstacle>();
        if (obs != null)
        {
            obs.type = type;
            obs.RequestDespawn = HandleObstacleDespawn;
            obs.ResetForReuse(); 
        }
    }

    private GameObject GetFromPool(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Block: return _blockPool?.Get();
            case ObstacleType.Brick: return _brickPool?.Get();
            case ObstacleType.Coin:  return _coinPool?.Get();
        }
        return null;
    }

    private void HandleObstacleDespawn(Obstacle obs)
    {
        if (obs == null) return;

        obs.transform.SetParent(null);

        switch (obs.type)
        {
            case ObstacleType.Block:
                if (_blockPool != null) _blockPool.Release(obs.gameObject);
                else Destroy(obs.gameObject);
                break;

            case ObstacleType.Brick:
                if (_brickPool != null) _brickPool.Release(obs.gameObject);
                else Destroy(obs.gameObject);
                break;

            case ObstacleType.Coin:
                if (_coinPool != null) _coinPool.Release(obs.gameObject);
                else Destroy(obs.gameObject);
                break;
        }
    }

    private ObstacleType GetRandomType()
    {
        float total = blockWeight + brickWeight + coinWeight;
        if (total <= 0f)
            return ObstacleType.Block;

        float r = UnityEngine.Random.value * total;

        if (r < blockWeight) return ObstacleType.Block;
        if (r < blockWeight + brickWeight) return ObstacleType.Brick;
        return ObstacleType.Coin;
    }

    private void DespawnLowObstacles()
    {
        if (!followTarget) return;

        float cutoff = followTarget.position.y - despawnBelowDistance;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.position.y < cutoff)
            {
                var obs = child.GetComponent<Obstacle>();
                if (obs != null)
                    obs.SafeDespawn();    
            }
        }
    }

    public void ResetToMenu()
    {
        _running = false;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var obs = transform.GetChild(i).GetComponent<Obstacle>();
            if (obs != null)
                obs.SafeDespawn();
        }

        _highestY = _initialHighestY;
    }
}

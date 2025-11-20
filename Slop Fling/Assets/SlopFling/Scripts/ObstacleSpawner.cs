using System;
using UnityEngine;

public enum ObstacleType
{
    Block,
    Brick,
    Coin
}

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform followTarget;   // Ball (hoặc camera) – dùng để biết đã đi cao tới đâu
    [SerializeField] private Transform spawnOrigin;    // Đặt empty này ở VỊ TRÍ OBSTACLE BÊN PHẢI, ngay hàng đầu tiên

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

    private float _highestY;     // Y cao nhất đã spawn
    private bool _running = false;
    private float _obstacleX;    // X cố định bên phải (theo spawnOrigin)

    private void OnEnable()
    {
        MainMenuController.OnGameStart += HandleGameStart;
    }

    private void OnDisable()
    {
        MainMenuController.OnGameStart -= HandleGameStart;
    }

    private void Start()
    {
        if (!followTarget)
        {
            Debug.LogWarning("[ObstacleSpawner] followTarget chưa gán.");
        }

        if (!spawnOrigin)
            spawnOrigin = transform;

        // X bên phải được lấy theo spawnOrigin
        _obstacleX = spawnOrigin.position.x;

        // Y hiện tại = vị trí spawnOrigin (hàng đầu tiên)
        _highestY = spawnOrigin.position.y;
    }

    private void HandleGameStart()
    {
        _running = true;
        Prewarm();
    }

    private void Update()
    {
        if (!_running || !followTarget) return;

        // Spawn thêm khi chưa đủ "ahead distance"
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
        GameObject prefab = GetPrefab(type);
        if (!prefab) return;

        // Obstacles luôn ở X cố định bên phải, chỉ đổi Y
        Vector3 pos = new Vector3(_obstacleX, y, spawnOrigin.position.z);

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, transform);
        obj.name = $"Obstacle_{type}_{y:0.0}";
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

    private GameObject GetPrefab(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Block: return blockPrefab;
            case ObstacleType.Brick: return brickPrefab;
            case ObstacleType.Coin:  return coinPrefab;
        }
        return null;
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
                Destroy(child.gameObject); // sau này đổi thành trả về pool
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject brickHitPrefab;
    [SerializeField] private GameObject coinHitPrefab;
    [SerializeField] private GameObject stickShootPrefab;

    private ObjectPool<GameObject> _brickPool;
    private ObjectPool<GameObject> _coinPool;
    private ObjectPool<GameObject> _shootPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (brickHitPrefab) _brickPool = CreatePool(brickHitPrefab);
        if (coinHitPrefab)  _coinPool  = CreatePool(coinHitPrefab);
        if (stickShootPrefab) _shootPool = CreatePool(stickShootPrefab);
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
            },
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: 8,
            maxSize: 64
        );
    }

    private IEnumerator ReturnAfter(float delay, GameObject obj, ObjectPool<GameObject> pool)
    {
        yield return new WaitForSeconds(delay);
        if (pool != null && obj != null)
            pool.Release(obj);
    }

    // ========= API =========

    public void PlayBrickHit(Vector3 pos, Vector3 normal)
    {
        if (_brickPool == null) return;
        var obj = _brickPool.Get();
        SetupAndPlay(obj, pos, normal, _brickPool);
    }

    public void PlayCoinHit(Vector3 pos, Vector3 normal)
    {
        if (_coinPool == null) return;
        var obj = _coinPool.Get();
        SetupAndPlay(obj, pos, normal, _coinPool);
    }

    public void PlayStickShoot(Vector3 pos, Vector3 dir)
    {
        if (_shootPool == null) return;
        var obj = _shootPool.Get();
        // dir để xoay, có thể bỏ nếu VFX không cần
        SetupAndPlay(obj, pos, dir.normalized, _shootPool);
    }

    private void SetupAndPlay(GameObject obj, Vector3 pos, Vector3 normal, ObjectPool<GameObject> pool)
    {
        Vector3 spawnPos = pos + (normal.sqrMagnitude > 0.001f ? normal.normalized * 0.05f : Vector3.zero);
        obj.transform.position = spawnPos;

        if (normal.sqrMagnitude > 0.001f)
            obj.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
        else
            obj.transform.rotation = Quaternion.identity;

        var systems = obj.GetComponentsInChildren<ParticleSystem>();
        float maxDuration = 0.1f;

        foreach (var ps in systems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
            maxDuration = Mathf.Max(maxDuration, ps.main.duration);
        }

        StartCoroutine(ReturnAfter(maxDuration, obj, pool));
    }
}

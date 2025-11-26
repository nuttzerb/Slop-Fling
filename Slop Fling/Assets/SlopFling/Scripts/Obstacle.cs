using System;
using UnityEngine;

public enum ObstacleType
{
    Block,
    Brick,
    Coin
}

public class Obstacle : MonoBehaviour
{
    public ObstacleType type = ObstacleType.Block;

    [Header("Brick Visual (optional)")]
    [SerializeField] private Renderer brickRenderer;
    [SerializeField] private Color brickHitColor = Color.green;

    public Action<Obstacle> RequestDespawn;

    private bool _wasHit;
    private bool _isInPool;
    private Color _brickOriginalColor;

    private void Awake()
    {
        if (brickRenderer != null)
            _brickOriginalColor = brickRenderer.material.color;
    }

    public void OnHitVisual()
    {
        if (_wasHit) return;
        _wasHit = true;

        if (type == ObstacleType.Brick && brickRenderer != null)
        {
            brickRenderer.material.color = brickHitColor;

            Vector3 pos = brickRenderer.bounds.center;
            VfxManager.Instance?.PlayBrickHit(pos, Vector3.left);
        }

        if (type == ObstacleType.Coin)
        {
            Vector3 pos = transform.position;
            VfxManager.Instance?.PlayCoinHit(pos, Vector3.left);

            if (RequestDespawn != null)
                RequestDespawn(this);
            else
                Destroy(gameObject);
        }
    }


    public void SafeDespawn()
    {
        if (_isInPool) return;
        _isInPool = true;

        if (RequestDespawn != null)
            RequestDespawn(this);
        else
            Destroy(gameObject);
    }

    public void ResetForReuse()
    {
        _wasHit = false;
        _isInPool = false;

        if (brickRenderer != null)
            brickRenderer.material.color = _brickOriginalColor;
    }
}

using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleType type = ObstacleType.Block;

    [Header("Brick Visual (optional)")]
    [SerializeField] private Renderer brickRenderer;
    [SerializeField] private Color brickHitColor = Color.green;

    private bool _wasHit;

    /// <summary>
    /// Gọi khi ball fling trúng (chỉ 1 lần).
    /// Visual nằm trong này, logic score/mạng để GameSession xử lý.
    /// </summary>
    public void OnHitVisual()
    {
        if (_wasHit) return;
        _wasHit = true;

        if (type == ObstacleType.Brick && brickRenderer != null)
        {
            brickRenderer.material.color = brickHitColor;
        }

        // Coin có thể Destroy sau khi ăn, Block/Brick thì tuỳ bạn
        if (type == ObstacleType.Coin)
        {
            Destroy(gameObject);
        }
    }
}

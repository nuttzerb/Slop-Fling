using UnityEngine;

public class StickVisual : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private float maxBend = 0.6f;    // độ cong tối đa khi búng
    [SerializeField] private float releaseDuration = 0.15f;
    [SerializeField] private int segments = 12;       // số điểm trên line (>= 3)

    private Transform _start; // Ball
    private Vector3 _end;     // Điểm tường
    private bool _releasing;
    private float _releaseTimer;

    public void Attach(Transform start, Vector3 endWorld)
    {
        _start = start;
        _end = endWorld;

        if (!line) line = GetComponent<LineRenderer>();
        if (segments < 3) segments = 3;
        line.positionCount = segments;

        // mới attach: gậy thẳng
        UpdateLine(0f);
    }

    /// <summary>Gọi khi chuẩn bị nảy ball lên (hết holdDuration).</summary>
    public void Release()
    {
        _releasing = true;
        _releaseTimer = 0f;
    }

    private void Update()
    {
        if (_start == null || line == null)
        {
            Destroy(gameObject);
            return;
        }

        if (_releasing)
        {
            _releaseTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_releaseTimer / releaseDuration);

            // 0 → maxBend → 0 (búng lên một cái)
            float bend = Mathf.Sin(t * Mathf.PI) * maxBend;
            UpdateLine(bend);

            // fade dần
            Color c = line.startColor;
            float alpha = 1f - t;
            c.a = alpha;
            line.startColor = c;
            line.endColor = c;

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // đang ghim: gậy thẳng
            UpdateLine(0f);
        }
    }

    private void UpdateLine(float bendAmount)
    {
        Vector3 startPos = _start.position;
        Vector3 endPos   = _end;

        // hướng từ ball -> tường
        Vector3 dir = (endPos - startPos);
        float length = dir.magnitude;
        if (length < 0.0001f)
            length = 0.0001f;

        dir /= length;

        // vector cong vuông góc
        Vector3 bendDir = Vector3.up;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1); // 0..1 dọc theo gậy

            // điểm thẳng giữa start-end
            Vector3 p = Vector3.Lerp(startPos, endPos, t);

            // offset cong: sin(pi * t) tạo đỉnh ở giữa, 2 đầu = 0
            float bend = bendAmount * Mathf.Sin(Mathf.PI * t);
            p += bendDir * bend;

            line.SetPosition(i, p);
        }
    }
}

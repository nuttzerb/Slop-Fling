using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float jumpSpeed = 8f;
    [SerializeField] private float holdDuration = 1.2f;

    [Header("Attach / Raycast")]
    [SerializeField] private float attachRayDistance = 10f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Transform camTransform;
    [SerializeField] private float deathBelowCam = 5f;

    private Rigidbody rb;
    private bool canControl = false;
    private bool isHolding = false;      // trước là isAttached
    private Coroutine holdRoutine;
    private Vector3 _startPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _startPos = transform.position;
        rb.useGravity = true;

        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotation;
    }
    private void Start()
    {
        if (!camTransform && Camera.main != null)
            camTransform = Camera.main.transform;
    }

    public void SetIdleState()
    {
        canControl = false;
        isHolding = false;

        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }
        transform.position = _startPos;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
    }

    public void BeginGameplay()
    {
        rb.isKinematic = false;
        canControl = true;
        isHolding = false;

        FlingUp();   // first jump
    }

    private void Update()
    {
        // Nếu game over thì không cho điều khiển nữa
        if (GameSession.Instance != null && GameSession.Instance.IsGameOver)
            return;

        if (!canControl || isHolding)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryAttachToWall();
        }
        if (camTransform != null && GameSession.Instance != null && !GameSession.Instance.IsGameOver)
        {
            float camBottomY = camTransform.position.y - deathBelowCam;
            if (transform.position.y < camBottomY)
            {
                Debug.Log("Ball fell out of camera → Game Over");
                canControl = false;
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;

                GameSession.Instance.GameOverFromFall();
            }
        }
    }


    private void FlingUp()
    {
        var v = rb.linearVelocity;
        v.y = jumpSpeed;
        rb.linearVelocity = v;
    }

    private void TryAttachToWall()
    {
        Ray ray = new Ray(transform.position, Vector3.right);
        Debug.DrawRay(ray.origin, ray.direction * attachRayDistance, Color.red, 0.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, attachRayDistance, obstacleMask))
        {
            // 👉 Lấy obstacle & báo cho GameSession
            Obstacle obstacle = hit.collider.GetComponentInParent<Obstacle>();
            if (obstacle != null)
            {
                GameSession.Instance?.HandleObstacleHit(obstacle);
                Debug.Log("TryAttachToWall: " + obstacle.name);
            }

            // 👉 Fling thành công → hold tại chỗ rồi bật lên lại
            if (holdRoutine != null)
                StopCoroutine(holdRoutine);

            holdRoutine = StartCoroutine(HoldAndFling());
        }
        else
        {
            // Miss → không gì, ball rơi tiếp
        }
    }


    private IEnumerator HoldAndFling()
    {
        isHolding = true;
        canControl = false;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;

        // GIỮ NGUYÊN VỊ TRÍ HIỆN TẠI
        Vector3 holdPos = transform.position;
        transform.position = holdPos;

        // Hold 1.2s
        yield return new WaitForSeconds(holdDuration);

        // Bật vật lý lại và nảy lên
        rb.isKinematic = false;
        isHolding = false;
        canControl = true;

        FlingUp();
    }

}

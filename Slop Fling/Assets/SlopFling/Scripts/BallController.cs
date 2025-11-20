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

    private Rigidbody rb;
    private bool canControl = false;
    private bool isHolding = false;      // trước là isAttached
    private Coroutine holdRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotation;
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
        if (!canControl || isHolding)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryAttachToWall();
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
            // Fling thành công → chỉ HOLD tại chỗ
            if (holdRoutine != null)
                StopCoroutine(holdRoutine);

            holdRoutine = StartCoroutine(HoldAndFling());
        }
        else
        {
            // Miss: không làm gì, ball tiếp tục rơi tự do
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

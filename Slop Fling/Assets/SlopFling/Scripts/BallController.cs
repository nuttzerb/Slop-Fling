using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody rb;

    private bool canPlay = false;

    public void SetIdleState()
    {
        canPlay = false;

        if (rb)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void BeginGameplay()
    {
        canPlay = true;

        if (rb)
            rb.isKinematic = false;
    }

    private void Update()
    {
        if (!canPlay) return;

        bool tapped = false;

        if (Touchscreen.current != null)
        {
            // Mobile: tap màn hình
            tapped = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        }
        else if (Mouse.current != null)
        {
            // Editor: click chuột trái
            tapped = Mouse.current.leftButton.wasPressedThisFrame;
        }

        if (tapped)
        {
            Debug.Log("Tap detected (New Input System)");
        }
    }

}

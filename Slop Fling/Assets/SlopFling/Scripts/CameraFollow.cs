using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smooth = 5f;
    private Vector3 offset;
    private Vector3 _startPos;
    private Quaternion _startRot;
    public void ResetToStart()
    {
        transform.position = _startPos;
        transform.rotation = _startRot;
    }
    private void Awake()
    {
        _startPos = transform.position;
        _startRot = transform.rotation;
    }
    private void Start()
    {
        if (target)
            offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;

        // Endless mode → camera chỉ đi lên, không đi xuống
        if (desired.y < transform.position.y)
            desired.y = transform.position.y;

        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smooth);
    }
}

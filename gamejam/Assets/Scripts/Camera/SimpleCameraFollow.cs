using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Settings")]
    public float smoothTime = 0.2f;
    public bool useDeadzone = true;
    public Vector2 deadzoneSize = new Vector2(4f, 2f); // Area where camera doesn't move

    [Header("Map Bounds (Optional)")]
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-25, -25);
    public Vector2 maxBounds = new Vector2(25, 25);

    private Vector3 _currentVelocity;
    private Camera _cam;

    void Start()
    {
        _cam = GetComponent<Camera>();
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = transform.position;

        if (useDeadzone)
        {
            // Logic: Only move desiredPos if target is outside deadzone relative to camera center
            Vector3 targetLocalPos = transform.InverseTransformPoint(target.position);
            
            float dx = 0;
            float dy = 0;

            if (targetLocalPos.x > deadzoneSize.x / 2) dx = targetLocalPos.x - deadzoneSize.x / 2;
            if (targetLocalPos.x < -deadzoneSize.x / 2) dx = targetLocalPos.x + deadzoneSize.x / 2;
            
            if (targetLocalPos.y > deadzoneSize.y / 2) dy = targetLocalPos.y - deadzoneSize.y / 2;
            if (targetLocalPos.y < -deadzoneSize.y / 2) dy = targetLocalPos.y + deadzoneSize.y / 2;

            if (dx != 0 || dy != 0)
            {
                // We need to move the camera by (dx, dy) locally
                desiredPos = transform.position + transform.TransformVector(new Vector3(dx, dy, 0));
            }
        }
        else
        {
            desiredPos = target.position + offset;
        }

        // Clamp to Bounds (keeping Z same)
        if (useBounds && _cam != null)
        {
            float vertExtent = _cam.orthographicSize;
            float horzExtent = vertExtent * Screen.width / Screen.height;

            // Simple clamp assuming bounds are centered at 0 or defined in world space
            // Ensure camera doesn't show outside bounds
            float minX = minBounds.x + horzExtent;
            float maxX = maxBounds.x - horzExtent;
            float minY = minBounds.y + vertExtent;
            float maxY = maxBounds.y - vertExtent;

            // If bounds are smaller than view, center it
            if (maxX < minX) 
            {
                desiredPos.x = (minBounds.x + maxBounds.x) / 2;
            }
            else
            {
                desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
            }

            if (maxY < minY)
            {
                desiredPos.y = (minBounds.y + maxBounds.y) / 2;
            }
            else
            {
                desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
            }
        }

        desiredPos.z = offset.z;

        // Apply Smooth
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _currentVelocity, smoothTime);
    }

    void OnDrawGizmosSelected()
    {
        if (useDeadzone)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(deadzoneSize.x, deadzoneSize.y, 1));
        }

        if (useBounds)
        {
            Gizmos.color = Color.green;
            // Draw bounds rect
            Vector3 center = new Vector3((minBounds.x + maxBounds.x)/2, (minBounds.y + maxBounds.y)/2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1);
            Gizmos.DrawWireCube(center, size);
        }
    }
}

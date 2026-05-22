using UnityEngine;

/// <summary>
/// Kamera top-down yang mengikuti pemain dengan smooth damp.
/// Sesuai GDD: Top-Down View / Bird View.
/// Bisa dikonfigurasikan batas (bounds) agar tidak keluar area map.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float     smoothTime = 0.15f;
    [SerializeField] private Vector3   offset     = new Vector3(0f, 0f, -10f);

    [Header("Map Bounds (aktifkan jika perlu)")]
    [SerializeField] private bool  useBounds = false;
    [SerializeField] private float minX = -20f, maxX = 20f;
    [SerializeField] private float minY = -20f, maxY = 20f;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        if (useBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
            desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        }

        desiredPos.z = offset.z; // Pertahankan Z kamera

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
    }

    /// <summary>Set target secara runtime (misalnya saat scene load).</summary>
    public void SetTarget(Transform newTarget) => target = newTarget;
}

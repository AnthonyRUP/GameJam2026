using UnityEngine;

/// <summary>
/// Simple smoothed camera follow. Moves this transform's x/y toward the target's x/y
/// while preserving whatever z the camera already had (PixelPerfectCamera handles
/// pixel-snapping on top of this regardless of how the transform is driven).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 10f;

    public void SetTarget(Transform newTarget) => target = newTarget;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 current = transform.position;
        Vector3 desired = new Vector3(target.position.x, target.position.y, current.z);
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(current, desired, t);
    }
}

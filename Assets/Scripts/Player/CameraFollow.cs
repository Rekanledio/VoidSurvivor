using UnityEngine;

namespace VoidSurvivor.Player
{
    /// <summary>
    /// Smooth orthographic follow for the main camera. No Cinemachine dependency.
    /// Follows the target's X/Y only; Z stays at cameraOffset to keep the
    /// orthographic camera parallel to the 2D plane. Bounds are optional and
    /// reserved for the real arena later.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Tooltip("Higher = snappier. Exponential smoothing, frame-rate independent.")]
        private float followSpeed = 8f;
        [SerializeField] private Vector3 cameraOffset = new(0f, 0f, -10f);
        [SerializeField, Tooltip("Optional symmetric follow bounds around the origin. Leave disabled until the real arena exists.")]
        private bool clampToBounds;
        [SerializeField] private Vector2 boundsHalfExtents = new(20f, 20f);

        private void LateUpdate()
        {
            if (target == null) return;

            Vector2 desired = (Vector2)(target.position + cameraOffset);

            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            Vector2 smoothed = Vector2.Lerp((Vector2)transform.position, desired, t);

            if (clampToBounds)
            {
                smoothed = PlayerController.ClampToBounds(smoothed, boundsHalfExtents);
            }

            transform.position = new Vector3(smoothed.x, smoothed.y, cameraOffset.z);
        }
    }
}

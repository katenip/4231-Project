using UnityEngine;

public class FollowParentSoft : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Setup")]
    public bool autoUseCurrentParent = true;
    public bool unparentOnAwake = true;
    public bool captureStartingLocalPosition = true;

    [Header("Slot")]
    public Vector3 targetLocalPosition;

    [Header("Follow")]
    [Min(0f)] public float followSpeed = 25f;
    public bool snapIfTooFar = true;
    [Min(0f)] public float snapDistance = 1.5f;

    private bool initialized;

    void Awake()
    {
        Transform originalParent = transform.parent;

        if (autoUseCurrentParent && target == null && originalParent != null)
            target = originalParent;

        if (captureStartingLocalPosition && originalParent != null)
            targetLocalPosition = transform.localPosition;

        if (unparentOnAwake && originalParent != null)
            transform.SetParent(null, true);

        initialized = true;
    }

    void LateUpdate()
    {
        if (!initialized || target == null) return;

        Vector3 desiredWorldPosition = target.TransformPoint(targetLocalPosition);
        Vector3 toTarget = desiredWorldPosition - transform.position;

        if (snapIfTooFar && toTarget.sqrMagnitude > snapDistance * snapDistance)
        {
            transform.position = desiredWorldPosition;
            return;
        }

        float maxStep = followSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, desiredWorldPosition, maxStep);
    }
}
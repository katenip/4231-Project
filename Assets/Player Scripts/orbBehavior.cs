using UnityEngine;
using UnityEngine.InputSystem;

public class orbBehavior : MonoBehaviour
{
    public GameObject orb;
    public GameObject arm;
    public Transform lineStart;
    public Color lineColor = Color.cyan;
    public float lineDuration = 2f;
    public AudioSource audioSource;
    public bool hasOrb = false;

    public bool isVisible;

    void Awake()
    {
        //isVisible = (orb != null && orb.activeInHierarchy) || (arm != null && arm.activeInHierarchy);
        isVisible = false;
    }

    void Update()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame && hasOrb)
        {
            isVisible = !isVisible;

            if (isVisible)
            {
                audioSource.Play();
            }
            if (orb != null) orb.SetActive(isVisible);
            if (arm != null) arm.SetActive(isVisible);
        }
    }

    public void OnLeftClickAction(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isVisible && orb != null)
        {
            Vector3 startPos = lineStart != null ? lineStart.position : transform.position;
            Debug.DrawLine(startPos, orb.transform.position, lineColor, lineDuration);
        }
    }
    public void activate()
    {
        isVisible = true;
        if (orb != null) orb.SetActive(isVisible);
        if (arm != null) arm.SetActive(isVisible);
    }
}

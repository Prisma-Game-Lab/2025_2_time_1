using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Camera playerCamera;
    public Image crosshair;
    public float maxDistance = 100f;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;

    void Update()
    {
        if (playerCamera == null || crosshair == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            HoldableObject holdable = hit.collider.GetComponentInParent<HoldableObject>();
            crosshair.color = holdable != null ? hitColor : normalColor;
        }
        else
        {
            crosshair.color = normalColor;
        }
    }
}

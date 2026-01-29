using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Camera playerCamera;
    public Sprite idlecrosshair;
    public Sprite holdcrosshair;
    public float maxDistance = 100f;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;

    private Image crosshair;

    private void Start()
    {
        crosshair = gameObject.GetComponent<Image>();
    }
    void Update()
    {
        if (playerCamera == null || idlecrosshair == null || holdcrosshair == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            HoldableObject holdable = hit.collider.GetComponentInParent<HoldableObject>();
            crosshair.sprite = holdable != null ? holdcrosshair : idlecrosshair;
        }
        else
        {
            crosshair.sprite = idlecrosshair;
        }
    }
}

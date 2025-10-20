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
            // muda de cor se mira num objeto com componente ResettableObject
            if (hit.collider.GetComponent<ResettableObject>() != null)
                crosshair.color = hitColor;
            else
                crosshair.color = normalColor;
        }
        else
        {
            crosshair.color = normalColor;
        }
    }
}

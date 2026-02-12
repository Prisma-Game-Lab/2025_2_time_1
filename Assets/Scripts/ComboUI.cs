using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider chargeSlider;
    public Image fillImage;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color fullChargeColor = Color.red;

    private void Start()
    {
        chargeSlider.maxValue = 3;
        chargeSlider.value = 0;

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnChargeProgressChanged += UpdateChargeBar;
        }

        UpdateChargeBar(0);
    }

    private void OnDestroy()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnChargeProgressChanged -= UpdateChargeBar;
        }
    }

    private void UpdateChargeBar(int value)
    {
        chargeSlider.value = value;

        if (value >= 3)
            fillImage.color = fullChargeColor;
        else
            fillImage.color = normalColor;
    }
}

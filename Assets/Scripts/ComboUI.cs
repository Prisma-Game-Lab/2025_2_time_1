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

    private PlayerMovement player;

    private void Awake()
    {
        chargeSlider.maxValue = 3;
        chargeSlider.value = 0;

        // Escuta quando o Player nascer
        PlayerMovement.OnPlayerSpawned += ConnectToPlayer;

        // Caso o player já exista
        if (PlayerMovement.Instance != null)
        {
            ConnectToPlayer(PlayerMovement.Instance);
        }
    }

    private void ConnectToPlayer(PlayerMovement pm)
    {
        player = pm;
        player.OnChargeProgressChanged += UpdateChargeBar;
        UpdateChargeBar(0);
    }

    private void OnDestroy()
    {
        PlayerMovement.OnPlayerSpawned -= ConnectToPlayer;

        if (player != null)
            player.OnChargeProgressChanged -= UpdateChargeBar;
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

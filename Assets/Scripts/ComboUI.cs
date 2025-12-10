using UnityEngine;
using TMPro;

public class ComboUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI chargeText;

    private void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnComboChanged += UpdateCombo;
            PlayerMovement.Instance.OnChargesChanged += UpdateCharges;

            UpdateCombo(PlayerMovement.Instance.GetComboCount());
            UpdateCharges(PlayerMovement.Instance.GetCharges());
        }
    }

    private void OnDestroy()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnComboChanged -= UpdateCombo;
            PlayerMovement.Instance.OnChargesChanged -= UpdateCharges;
        }
    }

    private void UpdateCombo(int value)
    {
        comboText.text = "Combo: " + value;
    }

    private void UpdateCharges(int value)
    {
        chargeText.text = "Charges: " + value;
    }
}

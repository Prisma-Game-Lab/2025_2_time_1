using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ComboUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider chargeSlider;
    public Image fillImage;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color fullChargeColor = Color.red;

    private PlayerMovement player;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (player != null)
            player.OnChargeProgressChanged -= UpdateChargeBar;
    }

    private void Start()
    {
        chargeSlider.maxValue = 3;
        chargeSlider.value = 0;

        ConnectToPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConnectToPlayer();
    }

    void ConnectToPlayer()
    {
        player = FindObjectOfType<PlayerMovement>();

        if (player != null)
        {
            player.OnChargeProgressChanged -= UpdateChargeBar;
            player.OnChargeProgressChanged += UpdateChargeBar;

            Debug.Log("ComboUI conectado ao Player.");
        }
        else
        {
            Debug.LogWarning("PlayerMovement NÃO encontrado na cena.");
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

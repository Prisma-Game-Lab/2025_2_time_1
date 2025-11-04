using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // 1. ADICIONE ESTA LINHA para usar TextMeshPro

public class MenuInicial : MonoBehaviour
{
    [SerializeField] private string cenaParaCarregar;
    [SerializeField] private GameObject painelInicial;
    [SerializeField] private GameObject painelOpcoes;

    [Header("Configurações de UI")]
    [SerializeField] private Slider sliderSensi;
    [SerializeField] private TMP_InputField sensitivityInputField; // 2. ADICIONE ESTA LINHA
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_InputField volumeInputField; // 3. ADICIONE ESTA LINHA

    private void Start()
    {
        // --- Configuração de Sensibilidade ---
        if (sliderSensi != null && sensitivityInputField != null)
        {
            // 1. Pega o valor do GameManager
            float currentSens = GameManager.Instance.MouseSensitivity;

            // 2. Define o valor do Slider e o texto do InputField
            sliderSensi.value = currentSens;
            sensitivityInputField.text = currentSens.ToString("F2"); // Ex: "100.00"

            // 3. Adiciona os Listeners
            sliderSensi.onValueChanged.AddListener(OnSensitivitySliderChanged);
            sensitivityInputField.onEndEdit.AddListener(OnSensitivityInputChanged);
        }

        // --- Configuração de Volume ---
        if (volumeSlider != null && volumeInputField != null)
        {
            // 1. Pega o valor salvo
            float currentVol = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);

            // 2. Define o Slider (0-1) e o texto (0-100)
            volumeSlider.value = currentVol;
            volumeInputField.text = (currentVol * 100f).ToString("F0"); // Ex: "100"

            // 3. Adiciona os Listeners
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
            volumeInputField.onEndEdit.AddListener(OnVolumeInputChanged);
        }
    }

    // --- FUNÇÕES DE SENSIBILIDADE ---
    // (Renomeei a sua OnSensitivityChanged para ficar mais claro)
    public void OnSensitivitySliderChanged(float newValue)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSensitivity(newValue);
        }
        // Atualiza o texto
        sensitivityInputField.text = newValue.ToString("F2");
    }

    public void OnSensitivityInputChanged(string newText)
    {
        if (float.TryParse(newText, out float newValue))
        {
            // Valida o valor
            newValue = Mathf.Clamp(newValue, sliderSensi.minValue, sliderSensi.maxValue);
            // Atualiza o slider (que vai chamar a função OnSensitivitySliderChanged)
            sliderSensi.value = newValue;
        }
        else
        {
            // Reverte o texto se for inválido
            sensitivityInputField.text = sliderSensi.value.ToString("F2");
        }
    }

    // --- FUNÇÕES DE VOLUME ---
    // (Renomeei a sua OnVolumeChanged)
    public void OnVolumeSliderChanged(float newValue) // valor 0.0 a 1.0
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(newValue);
        }
        // Atualiza o texto (mostrando 0-100)
        volumeInputField.text = (newValue * 100f).ToString("F0");
    }

    public void OnVolumeInputChanged(string newText) // valor 0 a 100
    {
        if (float.TryParse(newText, out float newValue))
        {
            // Valida o valor
            newValue = Mathf.Clamp(newValue, 0f, 100f);
            // Converte (para 0-1) e atualiza o slider
            volumeSlider.value = newValue / 100f;
        }
        else
        {
            // Reverte o texto se for inválido
            volumeInputField.text = (volumeSlider.value * 100f).ToString("F0");
        }
    }

    // --- FUNÇÕES DE NAVEGAÇÃO ---

    public void CarregarCena()
    {
        // IMPORTANTE: Mude o estado do jogo para "Playing" ANTES de carregar
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        }
        SceneManager.LoadScene(cenaParaCarregar);
    }

    public void Sair()
    {
        Application.Quit();
    }

    public void AbriOpcoes()
    {
        if (sliderSensi != null && sensitivityInputField != null)
        {
            float currentSens = GameManager.Instance.MouseSensitivity;
            sliderSensi.value = currentSens;
            sensitivityInputField.text = currentSens.ToString("F2");
        }
        if (volumeSlider != null && volumeInputField != null)
        {
            float currentVol = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);
            volumeSlider.value = currentVol;
            volumeInputField.text = (currentVol * 100f).ToString("F0");
        }

        painelInicial.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        painelInicial.SetActive(true);
        painelOpcoes.SetActive(false);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string cenaMenu;
    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject fundo;
    [SerializeField] private GameObject temCerteza;

    [Header("Configura��es de UI")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInputField;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_InputField volumeInputField;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_InputField fovInputField;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        }

        despausaJogo();

        // --- Configura��o de Sensibilidade ---
        if (sensitivitySlider != null && sensitivityInputField != null)
        {
            // 1. Carrega o valor do GameManager
            float currentSens = GameManager.Instance.MouseSensitivity;

            // 2. Define o Slider e o InputField
            sensitivitySlider.value = currentSens;
            sensitivityInputField.text = currentSens.ToString("F2");

            // 3. Adiciona Listeners
            sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
            sensitivityInputField.onEndEdit.AddListener(OnSensitivityInputChanged);
        }

        // --- Configura��o de Volume ---
        if (volumeSlider != null && volumeInputField != null)
        {
            // 1. Carrega o valor do PlayerPrefs
            float currentVol = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);

            // 2. Define o Slider (0-1) e o InputField (0-100)
            volumeSlider.value = currentVol;
            volumeInputField.text = (currentVol * 100f).ToString("F0"); // Ex: "100"

            // 3. Adiciona Listeners
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
            volumeInputField.onEndEdit.AddListener(OnVolumeInputChanged);
        }
        // --- Configura��o de FOV ---
        if (fovSlider != null && fovInputField != null)
        {
            // 1. Carrega o valor do PlayerPrefs
            float currentFOV = PlayerPrefs.GetFloat(GameManager.FOV_KEY, 40f);

            // 2. Define o Slider (0-1) e o InputField (0-100)
            fovSlider.value = currentFOV;
            fovInputField.text = (currentFOV * 100f).ToString("F0"); // Ex: "100"

            // 3. Adiciona Listeners
            fovSlider.onValueChanged.AddListener(OnFOVSliderChanged);
            fovInputField.onEndEdit.AddListener(OnFOVInputChanged);
        }
    }

    // --- FUN��ES DO SLIDER DE SENSIBILIDADE ---
    // (Renomeei de OnSensitivityChanged para ser mais claro)
    public void OnSensitivitySliderChanged(float newValue)
    {
        // 1. Atualiza o GameManager (que salva no PlayerPrefs)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSensitivity(newValue);
        }

        // 2. Atualiza o texto do InputField
        sensitivityInputField.text = newValue.ToString("F2");
    }

    public void OnSensitivityInputChanged(string newText)
    {
        if (float.TryParse(newText, out float newValue))
        {
            // 1. Valida (Clamp) o valor para o range do slider
            newValue = Mathf.Clamp(newValue, sensitivitySlider.minValue, sensitivitySlider.maxValue);

            // 2. Define o valor do slider
            // Isso vai disparar o OnSensitivitySliderChanged automaticamente,
            // que far� o resto (salvar no GameManager e formatar o texto)
            sensitivitySlider.value = newValue;
        }
        else
        {
            // Se o texto for inv�lido (ex: "abc"), reverte para o valor atual
            sensitivityInputField.text = sensitivitySlider.value.ToString("F2");
        }
    }

    // --- FUN��ES DO SLIDER DE VOLUME ---
    // (Renomeei de OnVolumeChanged para ser mais claro)
    public void OnVolumeSliderChanged(float newValue) // newValue � de 0.0 a 1.0
    {
        // 1. Atualiza o AudioManager (que salva no PlayerPrefs)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(newValue);
        }

        // 2. Atualiza o texto do InputField (mostrando 0-100)
        volumeInputField.text = (newValue * 100f).ToString("F0");
    }

    public void OnVolumeInputChanged(string newText)
    {
        if (float.TryParse(newText, out float newValue)) // newValue � de 0 a 100
        {
            // 1. Valida (Clamp) o valor para 0-100
            newValue = Mathf.Clamp(newValue, 0f, 100f);

            // 2. Converte (para 0-1) e define o valor do slider
            // Isso vai disparar o OnVolumeSliderChanged automaticamente
            volumeSlider.value = newValue / 100f;
        }
        else
        {
            // Se o texto for inv�lido, reverte
            volumeInputField.text = (volumeSlider.value * 100f).ToString("F0");
        }
    }

    public void OnFOVSliderChanged(float newValue) // newValue � de 0.0 a 1.0
    {
        // 1. Atualiza o GameManager (que salva no PlayerPrefs)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFieldOfView(newValue);
        }

        // 2. Atualiza o texto do InputField (mostrando 0-100)
        fovInputField.text = (fovSlider.value * 100f).ToString("F0");
    }

    public void OnFOVInputChanged(string newText)
    {
        if (float.TryParse(newText, out float newValue)) // newValue � de 0 a 100
        {
            // 1. Valida (Clamp) o valor para 0-100
            newValue = Mathf.Clamp(newValue, 0f, 100f);

            // 2. Converte (para 0-1) e define o valor do slider
            // Isso vai disparar o OnFOVSliderChanged automaticamente
            fovSlider.value = newValue / 100f;
        }
        else
        {
            // Se o texto for inv�lido, reverte
            fovInputField.text = (fovSlider.value * 100f).ToString("F0");
        }
    }

    // --- O RESTO DO SEU SCRIPT ---

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
                if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
                {
                    pausaJogo();
                }
                else
                {
                    despausaJogo();
                }
            }
        }
    }

    void despausaJogo()
    {
        crosshair.SetActive(true);
        painelPause.SetActive(false);
        painelOpcoes.SetActive(false);
        fundo.SetActive(false);
        temCerteza.SetActive(false);
    }

    void pausaJogo()
    {
        crosshair.SetActive(false);
        painelPause.SetActive(true);
        painelOpcoes.SetActive(false);
        fundo.SetActive(true);
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
            despausaJogo();
        }
    }

    public void Sair()
    {
        Application.Quit();
    }

    public void AbriOpcoes()
    {
        // ATUALIZADO: Garante que os InputFields tamb�m sejam atualizados
        if (sensitivitySlider != null && sensitivityInputField != null)
        {
            float currentSens = GameManager.Instance.MouseSensitivity;
            sensitivitySlider.value = currentSens;
            sensitivityInputField.text = currentSens.ToString("F2");
        }
        if (volumeSlider != null && volumeInputField != null)
        {
            float currentVol = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);
            volumeSlider.value = currentVol;
            volumeInputField.text = (currentVol * 100f).ToString("F0");
        }

        painelPause.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        painelPause.SetActive(true);
        painelOpcoes.SetActive(false);
    }

    public void CarregarMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Menu);
        }
        SceneManager.LoadScene(cenaMenu);
    }

    public void IrMenuPrincripal()
    {
        painelPause.SetActive(false);
        temCerteza.SetActive(true);
    }

    public void NaoVoltarMenuPrincipal()
    {
        painelPause.SetActive(true);
        temCerteza.SetActive(false);
    }
}
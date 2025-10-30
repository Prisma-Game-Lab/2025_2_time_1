using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string cenaMenu;
    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject fundo;

    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        }

        despausaJogo();

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = GameManager.Instance.MouseSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    public void OnSensitivityChanged(float newValue)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSensitivity(newValue);
        }

    }

    public void OnVolumeChanged(float newValue)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(newValue);
        }
    }

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
        Debug.Log("Desativando o painel de pause");
        painelPause.SetActive(false);
        Debug.Log("Desativando o painel de opções");
        painelOpcoes.SetActive(false);
        Debug.Log("Desativando o fundo");
        fundo.SetActive(false);
        Debug.Log("Despausando o jogo");
        crosshair.SetActive(true);
        Debug.Log("Desativando o painel de pause");

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

    public void CarregarMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Menu);
        }
        SceneManager.LoadScene(cenaMenu);
    }

    public void AbriOpcoes()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = GameManager.Instance.MouseSensitivity;
        }
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f);
        }

        painelPause.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        painelPause.SetActive(true);
        painelOpcoes.SetActive(false);
    }
}
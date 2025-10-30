using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInicial : MonoBehaviour
{
    [SerializeField] private string cenaParaCarregar;
    [SerializeField] private GameObject painelInicial;
    [SerializeField] private GameObject painelOpcoes;

    [SerializeField] private Slider sliderSensi;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        if (sliderSensi != null)
        {
            sliderSensi.value = GameManager.Instance.MouseSensitivity;
            sliderSensi.onValueChanged.AddListener(OnSensitivityChanged);
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
        // O AudioManager (que é um DontDestroyOnLoad)
        // deve estar presente na cena do menu principal.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(newValue);
        }
    }

    public void CarregarCena()
    {
        SceneManager.LoadScene(cenaParaCarregar);
    }

    public void Sair()
    {
        Application.Quit();
    }

    public void AbriOpcoes()
    {
        if (sliderSensi != null)
        {
            sliderSensi.value = GameManager.Instance.MouseSensitivity;
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

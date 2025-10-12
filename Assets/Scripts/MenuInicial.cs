using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    [SerializeField] private string cenaParaCarregar;
    [SerializeField] private GameObject painelInicial;
    [SerializeField] private GameObject painelOpcoes;

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
        painelInicial.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        painelInicial.SetActive(true);
        painelOpcoes.SetActive(false);
    }

}

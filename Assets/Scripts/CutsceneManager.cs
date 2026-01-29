using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nomeDaFase;

    void Start()
    {
        // Inscreve a função no evento de término do vídeo
        videoPlayer.loopPointReached += AoTerminarVideo;
    }

    void Update()
    {
        // Pular cutscene ao apertar uma tecla
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CarregarFase();
        }
    }

    void AoTerminarVideo(VideoPlayer vp)
    {
        CarregarFase();
    }

    void CarregarFase()
    {
        SceneManager.LoadScene(nomeDaFase);
    }
}
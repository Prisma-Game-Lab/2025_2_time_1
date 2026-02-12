using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Referências")]
    public VideoPlayer videoPlayer;

    [Header("Vídeos")]
    public VideoClip introClip;
    public VideoClip endingClip;

    [Header("Cenas")]
    public string nextSceneAfterIntro = "Fase1";
    public string nextSceneAfterEnding = "Menu";

    private bool isEnding = false;

    void Start()
    {
        videoPlayer.loopPointReached += AoTerminarVideo;

        // Verifica se deve tocar final
        if (PlayerPrefs.GetInt("PlayEnding", 0) == 1)
        {
            isEnding = true;
            videoPlayer.clip = endingClip;
            PlayerPrefs.SetInt("PlayEnding", 0);
        }
        else
        {
            isEnding = false;
            videoPlayer.clip = introClip;
        }

        videoPlayer.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CarregarProximaCena();
        }
    }

    void AoTerminarVideo(VideoPlayer vp)
    {
        CarregarProximaCena();
    }

    void CarregarProximaCena()
    {
        if (isEnding)
            SceneManager.LoadScene(nextSceneAfterEnding);
        else
            SceneManager.LoadScene(nextSceneAfterIntro);
    }
}

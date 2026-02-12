using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class RoomController : MonoBehaviour
{
    [Header("PORTA")]
    public DoorController door;
    public int killsToOpenDoor = 3;

    [Header("CUTSCENE FINAL")]
    public bool playEndingCutscene = false;
    public VideoPlayer endingVideo;
    public int killsToPlayCutscene = 10;
    public string sceneAfterCutscene = "Menu";

    private int currentKills = 0;

    private bool doorOpened = false;
    private bool cutscenePlayed = false;

    private void OnEnable()
    {
        EnemyAI.OnEnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        EnemyAI.OnEnemyDied -= OnEnemyDied;

        if (endingVideo != null)
            endingVideo.loopPointReached -= OnVideoFinished;
    }

    private void OnEnemyDied(EnemyAI enemy)
    {
        currentKills++;

        Debug.Log($"Inimigos mortos: {currentKills}");

        // ===== ABRIR PORTA =====
        if (!doorOpened && door != null && currentKills >= killsToOpenDoor)
        {
            doorOpened = true;
            door.OpenDoor();
        }

        // ===== TOCAR CUTSCENE =====
        if (!cutscenePlayed && playEndingCutscene && endingVideo != null && currentKills >= killsToPlayCutscene)
        {
            cutscenePlayed = true;
            PlayCutscene();
        }
    }

    private void PlayCutscene()
    {
        Debug.Log("Indo para cutscene final");

        PlayerPrefs.SetInt("PlayEnding", 1);
        SceneManager.LoadScene("Cutscenes");
    }


    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Cutscene finalizada");

        // Remove evento pra evitar duplicação
        endingVideo.loopPointReached -= OnVideoFinished;

        // Volta o tempo ao normal
        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneAfterCutscene);
    }
}

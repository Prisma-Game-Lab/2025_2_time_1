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
    }

    private void OnEnemyDied(EnemyAI enemy)
    {
        currentKills++;

        Debug.Log($"Inimigos mortos: {currentKills}");


        if (!doorOpened && door != null && currentKills >= killsToOpenDoor)
        {
            doorOpened = true;
            door.OpenDoor();
        }

        
        if (!cutscenePlayed && playEndingCutscene && endingVideo != null && currentKills >= killsToPlayCutscene)
        {
            cutscenePlayed = true;
            PlayCutscene();
        }
    }

    private void PlayCutscene()
    {
        Debug.Log("Tocando cutscene final");

        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);

        endingVideo.Play();
        endingVideo.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Cutscene finalizada");

        SceneManager.LoadScene(sceneAfterCutscene);
    }
}

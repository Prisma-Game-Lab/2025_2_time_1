using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    public GameState CurrentState { get; private set; } = GameState.Menu;

    //Variaveis para Sensibilidade do Mouse
    private const string SENSITIVITY_KEY = "MouseSensitivity";
    [SerializeField] private float defaultSensitivity = 100f;
    public float MouseSensitivity { get; private set; }

    [SerializeField] private GameState myGameState = GameState.Menu;

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        MouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, defaultSensitivity);
    }

    private void Start()
    {
        ChangeState(myGameState);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Game State mudado para: {newState}");

        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            ChangeState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }

    public void SetSensitivity(float newSensitivity)
    {
        MouseSensitivity = newSensitivity;
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, newSensitivity);
        PlayerPrefs.Save();

        Debug.Log($"Sensibilidade alterada para: {newSensitivity}");
    }
}

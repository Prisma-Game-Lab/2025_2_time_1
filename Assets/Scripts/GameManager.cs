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

    //Variaveis para Campo de Visao (FOV)
    Camera myCamera;
    public const string FOV_KEY = "FieldOfView";
    [SerializeField] private float defaultFOV = 40f;
    public float FieldOfView { get; private set; }

    // Variaveis do AudioManager
    private AudioManager audioManager;
    [SerializeField] private string musicToPlay = "FunnyMusic";

    private void Awake()
    {
        myCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        if (audioManager != null)
        {   
            Debug.Log("Tocando música de fundo...");
            audioManager.Play(musicToPlay);
        }
        else
        {
            Debug.Log("AudioManager não encontrado na cena!");
        }
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        MouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, defaultSensitivity);
        FieldOfView = PlayerPrefs.GetFloat(FOV_KEY, defaultFOV);
    }

    private void Start()
    {
        ChangeState(GameState.Menu);
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

    public void SetFieldOfView(float newFOV)
    {
        FieldOfView = newFOV;
        myCamera.fieldOfView = newFOV;
        PlayerPrefs.SetFloat(FOV_KEY, newFOV);
        PlayerPrefs.Save();

        Debug.Log($"Campo de Visão alterado para: {newFOV}");
    }
}

using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]

    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
        [HideInInspector] public AudioSource source;
    }

    public List<Sound> sounds;

    public const string MasterVolumeKey = "MasterVolume";
    private float masterVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume * masterVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        Sound s = sounds.Find(x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Som '{name}' n�o encontrado!");
            return;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = sounds.Find(x => x.name == name);
        if (s != null)
            s.source.Stop();
    }

    public void StopAll()
    {
        foreach (var s in sounds)
            s.source.Stop();
    }

    public void SetMasterVolume(float newVolume)
    {
        masterVolume = Mathf.Clamp01(newVolume);
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.Save();

        foreach (var s in sounds)
        {
            if (s.source != null)
            {
                s.source.volume = s.volume * masterVolume;
            }
        }
    }
}

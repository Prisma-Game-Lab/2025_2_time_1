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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        Sound s = sounds.Find(x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Som '{name}' não encontrado!");
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
}

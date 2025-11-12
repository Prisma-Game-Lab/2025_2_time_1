using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField]public AudioClip walkSound;
    private AudioSource walkingSource;

    void Start()
    {
        walkingSource = GetComponent<AudioSource>();
    }

    public void PlayWalkSound()
    {
        if (!walkingSource.isPlaying)
        {
            walkingSource.clip = walkSound;
            walkingSource.volume = 0.5f;
            walkingSource.pitch = 1f;
            walkingSource.Play();
        }
    }
    public void StopWalkSound()
    {
        if (walkingSource.isPlaying)
        {
            walkingSource.Stop();
        }
    }

}

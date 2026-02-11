using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField]public AudioClip walkSound;

    [Header("Combat")]
    [SerializeField] public AudioClip lightAttackSwing; // Som do "vush" do soco rápido
    [SerializeField] public AudioClip heavyAttackSwing; // Som do "vush" do soco forte
    [SerializeField] public AudioClip punchHit; // Som de impacto

    private AudioSource walkingSource;

    private AudioSource sfxSource;

    void Start()
    {
        walkingSource = GetComponent<AudioSource>();

        sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.spatialBlend = walkingSource.spatialBlend; // Mantém 3D ou 2D igual
        sfxSource.volume = walkingSource.volume;
        sfxSource.playOnAwake = false;
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

    public void PlayAttackSwing(bool isHeavy)
    {
        AudioClip clip = isHeavy ? heavyAttackSwing : lightAttackSwing;
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f); 
            sfxSource.PlayOneShot(clip, 0.7f);
        }
    }

    public void PlayHitSound()
    {
        if (punchHit != null)
        {
            sfxSource.pitch = Random.Range(0.8f, 1.2f);
            sfxSource.PlayOneShot(punchHit, 0.45f);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAudioManager : MonoBehaviour {

    public float lowestPitch = 0.95f;
    public float highestPitch = 1.05f;

    [SerializeField]
    private AudioClip shootingClip;
    [SerializeField]
    private AudioClip walkingClip;
    [SerializeField]
    private AudioClip runningClip;
    [SerializeField]
    private AudioClip jumpingClip;
    [SerializeField]
    private AudioClip landingClip;

    private AudioSource source;

    private float walkSoundTimer;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleWalkSoundTimer();
    }

    private void HandleWalkSoundTimer()
    {
        walkSoundTimer -= Time.deltaTime;
    }

    private void PlayClipOneShot(AudioClip clip)
    {
        float randomPitch = Random.Range(lowestPitch, highestPitch);
        source.pitch = randomPitch;
        source.PlayOneShot(clip);
    }

    public void PlayWalkingSound()
    {
        if (walkSoundTimer <= 0)
        {
            PlayClipOneShot(walkingClip);
            walkSoundTimer = 0.2f;
        }
    }

    public void PlayRunningSound()
    {
        if (walkSoundTimer <= 0)
        {
            PlayClipOneShot(runningClip);
            walkSoundTimer = 0.2f;
        }
    }

    public void PlayShootingSound()
    {
        PlayClipOneShot(shootingClip);
    }

    public void PlayJumpSound()
    {
        PlayClipOneShot(jumpingClip);
    }

    public void PlayLandingSound()
    {
        PlayClipOneShot(landingClip);
    }
}

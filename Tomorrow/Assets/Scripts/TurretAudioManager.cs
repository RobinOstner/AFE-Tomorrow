using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAudioManager : MonoBehaviour
{

    public float lowestPitch = 0.95f;
    public float highestPitch = 1.05f;

    [SerializeField]
    private AudioClip shootingClip;

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

    public void PlayShootingSound()
    {
        PlayClipOneShot(shootingClip);
    }
}

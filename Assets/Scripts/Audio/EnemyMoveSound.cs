using UnityEngine;
using UnityEngine.Audio;

public class EnemyMoveSound : MonoBehaviour
{
    public AudioClip movementSound; 
    public AudioMixerGroup audioMixerGroup; 
    public float movementThreshold = 0.01f; 
    public float soundCooldown = 0.5f; 

    private AudioSource audioSource;
    private Vector3 lastPosition; 
    private float lastSoundTime; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        
        if (audioMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
        }

        
        lastPosition = transform.position;
        lastSoundTime = -soundCooldown; 
    }

    void Update()
    {
        
        if (IsMoving() && Time.time >= lastSoundTime + soundCooldown)
        {
            PlayMovementSound();
            lastSoundTime = Time.time; 
        }

        
        lastPosition = transform.position;
    }

    private void PlayMovementSound()
    {
        if (movementSound != null)
        {
            audioSource.PlayOneShot(movementSound);
        }
        else
        {
            Debug.LogWarning("Rörelseljudet är inte tilldelat.");
        }
    }

    private bool IsMoving()
    {
        
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        return distanceMoved > movementThreshold;
    }
}
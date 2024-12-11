using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class EnemyMoveSound : MonoBehaviour
{
    public AudioClip movementSound; // Ljud för rörelse
    public AudioMixerGroup audioMixerGroup; // Kopplad Audio Mixer Group
    public float movementThreshold = 0.01f; // Minsta rörelse som räknas som rörelse
    public float soundCooldown = 0.5f; // Cooldown-tid mellan ljudspelningar

    private AudioSource audioSource;
    private Vector3 lastPosition; // Senaste position för att spåra rörelse
    private float lastSoundTime; // Tidpunkten då senaste ljudet spelades

    void Start()
    {
        // Hämta eller lägg till en AudioSource-komponent
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Koppla Audio Mixer Group om tilldelad
        if (audioMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
        }

        // Spara den initiala positionen och starttid
        lastPosition = transform.position;
        lastSoundTime = -soundCooldown; // Så att ljud kan spelas direkt vid start
    }

    void Update()
    {
        // Kontrollera om fienden har rört sig tillräckligt
        if (IsMoving() && Time.time >= lastSoundTime + soundCooldown)
        {
            PlayMovementSound();
            lastSoundTime = Time.time; // Uppdatera tiden för senaste ljudspelning
        }

        // Uppdatera senaste position
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
        // Kontrollera om fiendens position har förändrats tillräckligt
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        return distanceMoved > movementThreshold;
    }
}
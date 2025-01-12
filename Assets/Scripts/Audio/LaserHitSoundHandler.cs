using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHitSoundHandler : MonoBehaviour
{
    public AudioClip playerHitSound;  
    private AudioSource audioSource;

    private void Start()
    {
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on this object! Please add an AudioSource component.");
        }
    }

    public void PlayPlayerHitSound()
    {
        
        if (audioSource != null && playerHitSound != null)
        {
            audioSource.PlayOneShot(playerHitSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or playerHitSound not assigned.");
        }
    }

    
}


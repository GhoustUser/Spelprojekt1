using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class EnemyAttackSound : MonoBehaviour
{
    public AudioClip attackSound; 
    public AudioMixerGroup audioMixerGroup; 
    private AudioSource audioSource;

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
    }

    public void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        else
        {
            Debug.LogWarning("Attack-ljudet är inte tilldelat.");
        }
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Kolla om det är spelaren
        {
            PlayAttackSound();
        }
    }

    // Alternativt om du använder en Trigger istället för Collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Kolla om det är spelaren
        {
            PlayAttackSound();
        }
    }
}

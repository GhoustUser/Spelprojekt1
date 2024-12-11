using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class EnemyAttackSound : MonoBehaviour
{
    public AudioClip attackSound; // Ljudet som spelas vid attack
    public AudioMixerGroup audioMixerGroup; // Kopplad Audio Mixer Group
    public string playerTag = "Player"; // Taggen för spelaren
    public float attackCooldown = 1.0f; // Cooldown-tid mellan attacker

    private AudioSource audioSource;
    private float lastAttackTime; // Tidpunkt för senaste attack

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

        // Initiera senaste attacktid
        lastAttackTime = -attackCooldown; // Tillåter ljud direkt vid start
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kontrollera om spelaren träffas
        if (other.CompareTag(playerTag))
        {
            PlayAttackSound();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Alternativ metod: Kontrollera om spelaren träffas via kollision
        if (collision.collider.CompareTag(playerTag))
        {
            PlayAttackSound();
        }
    }

    public void TriggerAttackSound()
    {
        // Om en attackanimation eller AI-skript anropar ljudet direkt
        PlayAttackSound();
    }

    private void PlayAttackSound()
    {
        // Kontrollera om vi är inom cooldown-period
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
                lastAttackTime = Time.time; // Uppdatera senaste attacktid
            }
            else
            {
                Debug.LogWarning("Attackljudet är inte tilldelat.");
            }
        }
    }
}
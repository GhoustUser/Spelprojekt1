using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class EnemyAttackSound : MonoBehaviour
{
    public AudioClip attackSound; 
    public AudioMixerGroup audioMixerGroup; 
    public string playerTag = "Player"; 
    public float attackCooldown = 1.0f; 

    private AudioSource audioSource;
    private float lastAttackTime; 

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

        
        lastAttackTime = -attackCooldown; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag(playerTag))
        {
            PlayAttackSound();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.collider.CompareTag(playerTag))
        {
            PlayAttackSound();
        }
    }

    public void TriggerAttackSound()
    {
        
        PlayAttackSound();
    }

    private void PlayAttackSound()
    {
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
                lastAttackTime = Time.time; 
            }
            else
            {
                Debug.LogWarning("Attackljudet är inte tilldelat.");
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyAudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip attackSound;  
    [SerializeField] private AudioClip hitSound;     
    [SerializeField] private AudioClip missSound;    
    [SerializeField] private AudioClip redSound;      
    [SerializeField] private AudioClip laserShootSound; 

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup attackSoundGroup;   
    [SerializeField] private AudioMixerGroup hitSoundGroup;      
    [SerializeField] private AudioMixerGroup missSoundGroup;     
    [SerializeField] private AudioMixerGroup redSoundGroup;     
    [SerializeField] private AudioMixerGroup laserShootSoundGroup;  

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();  
    }

    
    public void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.outputAudioMixerGroup = attackSoundGroup;  
            audioSource.PlayOneShot(attackSound);
        }
    }

    
    public void PlayHitSound()
    {
        if (hitSound != null)
        {
            audioSource.outputAudioMixerGroup = hitSoundGroup;  
            audioSource.PlayOneShot(hitSound);
        }
    }

    
    public void PlayMissSound()
    {
        if (missSound != null)
        {
            audioSource.outputAudioMixerGroup = missSoundGroup;  
            audioSource.PlayOneShot(missSound);
        }
    }

    
    public void PlayRedSound()
    {
        if (redSound != null)
        {
            audioSource.outputAudioMixerGroup = redSoundGroup;  
            audioSource.PlayOneShot(redSound);
        }
    }

    
    public void PlayLaserShootSound()
    {
        if (laserShootSound != null)
        {
            audioSource.outputAudioMixerGroup = laserShootSoundGroup;  
            audioSource.PlayOneShot(laserShootSound);
        }
    }

    
    public void SetSoundVolume(string parameterName, float volume)
    {
        audioSource.outputAudioMixerGroup.audioMixer.SetFloat(parameterName, volume);
    }
}

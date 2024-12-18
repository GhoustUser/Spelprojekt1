using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class MeleeSoundManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixerGroup mixerGroup; 
    [SerializeField] private AudioClip playerDamageSound; 

    
    public void PlayPlayerDamageSound()
    {
        PlaySound(playerDamageSound);
    }

    
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();

        
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroup;
        audioSource.Play();

        
        Destroy(tempAudio, clip.length);
    }
}
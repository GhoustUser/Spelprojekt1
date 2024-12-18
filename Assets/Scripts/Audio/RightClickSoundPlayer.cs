using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
    
public class RightClickSoundPlayer : MonoBehaviour
{
    public AudioClip clickSound; 
    public AudioMixerGroup mixerGroup;
    public float cooldownTime = 1f; 
    private float nextPlayTime = 0f; 

    void Update()
    {
        
        if (Input.GetMouseButtonDown(1) && Time.time >= nextPlayTime)
        {
            PlaySound();
        }
    }

    void PlaySound()
    {
        if (clickSound != null)
        {
            
            GameObject tempAudioSourceObject = new GameObject("TempAudioSource");
            AudioSource tempAudioSource = tempAudioSourceObject.AddComponent<AudioSource>();

            
            tempAudioSource.clip = clickSound;
            tempAudioSource.outputAudioMixerGroup = mixerGroup; 
            tempAudioSource.Play();

            
            Destroy(tempAudioSourceObject, clickSound.length);
            nextPlayTime = Time.time + cooldownTime; 
        }
        else
        {
            Debug.LogWarning("No audiofile in Inspector!");
        }
    }
}
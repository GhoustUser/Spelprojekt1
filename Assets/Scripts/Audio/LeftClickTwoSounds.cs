using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftClickTwoSounds : MonoBehaviour
{
    public AudioClip sound1; // Första ljudet
    public AudioClip sound2; // Andra ljudet
    private AudioSource audioSource;

    void Start()
    {
        // Hämta eller lägg till en AudioSource-komponent
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Kolla om vänster musknapp klickas
        if (Input.GetMouseButtonDown(0)) // 0 = vänster musknapp
        {
            PlayRandomSound();
        }
    }

    void PlayRandomSound()
    {
        if (audioSource == null) return;

        // Slumpa mellan två ljud
        AudioClip selectedSound = Random.value < 0.5f ? sound1 : sound2;

        if (selectedSound != null)
        {
            audioSource.PlayOneShot(selectedSound);
        }
        else
        {
            Debug.LogWarning("Ett eller båda ljudklippen är inte tilldelade.");
        }
    }
}

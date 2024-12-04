using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSoundController : MonoBehaviour
{
    public AudioClip sound1; // Det första ljudet
    public AudioClip sound2; // Det andra ljudet
    private AudioSource audioSource;

    void Start()
    {
        // Hämta AudioSource-komponenten
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("Ingen AudioSource hittades på objektet.");
        }
    }

    // Funktion som anropas av Animation Event
    public void PlayRandomSound()
    {
        if (audioSource == null) return;

        // Slumpa mellan ljud 1 och ljud 2
        AudioClip selectedSound = Random.value < 0.5f ? sound1 : sound2;

        // Spela upp det valda ljudet
        audioSource.PlayOneShot(selectedSound);
    }
}
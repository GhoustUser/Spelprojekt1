using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLooper : MonoBehaviour
{
    public AudioClip sound1;  // Det första ljudet
    public AudioClip sound2;  // Det andra ljudet
    private AudioSource audioSource;
    private bool playFirstSound = true;  // Spåra vilket ljud som ska spelas

    void Start()
    {
        // Hämta AudioSource-komponenten
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("Ingen AudioSource hittades på objektet.");
            return;
        }

        // Börja spela det första ljudet
        PlayNextSound();
    }

    void Update()
    {
        // Kontrollera om ljudet har spelats klart
        if (!audioSource.isPlaying)
        {
            PlayNextSound();
        }
    }

    void PlayNextSound()
    {
        // Växla mellan de två ljudklippen
        if (playFirstSound)
        {
            audioSource.clip = sound1;
        }
        else
        {
            audioSource.clip = sound2;
        }

        playFirstSound = !playFirstSound;  // Växla till nästa ljud för nästa gång
        audioSource.Play();
    }
}

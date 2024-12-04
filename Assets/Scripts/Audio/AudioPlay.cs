using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//använd raden nedan för att komma åt t.ex. AudioMixer och AudioMixerSnapshots!
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioPlay : MonoBehaviour
{
    private AudioSource source;
    private bool firstPlay = false;
    private bool playingAudioFunction = false;
    private int tempClip = -1;

    [Header("Start Settings")]
    public bool playOnStart = false;
    public bool oneShot = false;
    public float startDelay = 0f;

    [Header("Fade In Settings")]
    public bool playWithFade = false;
    [Range(0.0f, 1.0f)]
    public float targetVolume = 1f;
    [Range(0.0f, 3.0f)]
    public float fadeSpeed = 1f;

    [Header("Audio Clips")]
    public AudioClip[] clipsToPlay;

    [Header("Random Spawn Time")]
    [Range(0.0f, 100.0f)]
    public float minTime = 0f;
    [Range(0.0f, 100.0f)]
    public float maxTime = 0f;

    [Header("Random Volume")]
    [Range(0.0f, 1.0f)]
    public float minVolume = 1f;
    [Range(0.0f, 1.0f)]
    public float maxVolume = 1f;

    [Header("Random Pitch")]
    [Range(0.0f, 3.0f)]
    public float minPitch = 1f;
    [Range(0.0f, 3.0f)]
    public float maxPitch = 1f;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        if (playOnStart)
            PlayAudio();
    }

    public void PlayAudio()
    {
        if (clipsToPlay.Length == 0)
        {
            Debug.Log("Missing AudioClips");
        }
        else
        {
            if (playingAudioFunction == false || oneShot == true)
            {
                
                Debug.Log("Started AudioFunction");
                StartCoroutine("RandomClip");
            }
            else
                Debug.Log("Audio Function already playing");
        }
    }

    public void StopAudio(bool fadeOut)
    {
        StopAllCoroutines();
        if (fadeOut == true)
            StartCoroutine("FadeOut");
        else
        {
            playingAudioFunction = false;
            source.Stop();
            Debug.Log("Stopped AudioFunction");
        }
        firstPlay = false;
    }

    private IEnumerator FadeOut()
    {
        while (source.volume > 0f)
        {
            source.volume = Mathf.MoveTowards(source.volume, 0f, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        source.Stop();
        Debug.Log("Stopped AudioFunction");
        playingAudioFunction = false;
    }

    private IEnumerator AudioFunction()
    {
        if (firstPlay == false)
        {
            yield return new WaitForSeconds(startDelay);
            if (playWithFade == true)
                source.volume = 0f;
            else
                source.volume = Random.Range(minVolume, maxVolume);
            source.pitch = Random.Range(minPitch, maxPitch);
            source.Play();
            float timeTaken = 0f;
            if (playWithFade)
            {
                while (source.volume < targetVolume)
                {
                    source.volume = Mathf.MoveTowards(source.volume, targetVolume, fadeSpeed * Time.deltaTime);
                    timeTaken += Time.deltaTime;
                    yield return null;
                }
                //Här varnar vi för om ni har en fade som är längre än det första AudioClipet som körs som fadeIn.
                //Det påverkar "RandomSpawnTime".
                if (source.clip.length < timeTaken)
                    Debug.Log("Warning: You have a fade time that is longer than your AudioClip");
            }
            firstPlay = true;
            float waitingTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds((source.clip.length - timeTaken) + waitingTime);
            playingAudioFunction = false;
            if (oneShot == false)
            {
                StartCoroutine("RandomClip");
            }
            else
            {
                if(playWithFade)
                    source.volume = 0f;
                firstPlay = false;
                Debug.Log("Stopped AudioFunction");
            }
        }
        else
        {
            source.volume = Random.Range(minVolume, maxVolume);
            source.pitch = Random.Range(minPitch, maxPitch);
            float waitTime = Random.Range(minTime, maxTime);
            source.Play();
            yield return new WaitForSeconds(source.clip.length + waitTime);
            playingAudioFunction = false;
            if (oneShot == false)
            {
                StartCoroutine("RandomClip");
            }
            else
            {
                firstPlay = false;
                Debug.Log("Stopped AudioFunction else");
            }
        }
    }

    private IEnumerator RandomClip()
    {
        playingAudioFunction = true;
        if (clipsToPlay.Length == 1)
        {
            source.clip = clipsToPlay[0];
            StartCoroutine("AudioFunction");
        }
        else
        {
            int clip = Random.Range(0, clipsToPlay.Length);
            if (clip == tempClip)
            {
                Debug.Log("Tried to play the clip - Restarting");
                StartCoroutine("RandomClip");
            }
            else
            {
                Debug.Log("Picked a new clip - Success");
                source.clip = clipsToPlay[clip];
                tempClip = clip;
                StartCoroutine("AudioFunction");
            }
            yield return null;
        }
    }
}
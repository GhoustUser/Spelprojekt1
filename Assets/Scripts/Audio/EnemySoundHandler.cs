using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class EnemySoundHandler : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip knockbackSound;

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup damageMixerGroup;
    [SerializeField] private AudioMixerGroup knockbackMixerGroup;

   
    public void PlayDamageSound(AudioSource source)
    {
        if (damageSound != null && source != null)
        {
            source.outputAudioMixerGroup = damageMixerGroup;
            source.PlayOneShot(damageSound);
        }
    }

   
    public void PlayKnockbackSound(AudioSource source)
    {
        if (knockbackSound != null && source != null)
        {
            source.outputAudioMixerGroup = knockbackMixerGroup;
            source.PlayOneShot(knockbackSound);
        }
    }
}
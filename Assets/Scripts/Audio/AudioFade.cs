using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioFade : MonoBehaviour
{
    private AudioSource source;
    public float fadeInTargetVolume;
    public float fadeOutTargetVolume;
    public float fadeSpeed;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void StartFadeIn()
    {
        StopAllCoroutines();
        if (gameObject.activeInHierarchy) StartCoroutine("DoFadeIn");
    }

    public void StartFadeOut()
    {
        StopAllCoroutines();
        if (gameObject.activeInHierarchy) StartCoroutine("DoFadeOut");
    }

    private IEnumerator DoFadeIn()
    {
        while(source.volume != fadeInTargetVolume)
        {
            if (source == null) yield break;
            source.volume = Mathf.MoveTowards(source.volume, fadeInTargetVolume, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        StopAllCoroutines();
    }

    private IEnumerator DoFadeOut()
    {
        while (source.volume != fadeOutTargetVolume)
        {
            if (source == null) yield break;
            source.volume = Mathf.MoveTowards(source.volume, fadeOutTargetVolume, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        StopAllCoroutines();
    }
}

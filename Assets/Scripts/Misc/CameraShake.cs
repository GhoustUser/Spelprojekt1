using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private static CinemachineVirtualCamera virtualCamera;
    private static CinemachineBasicMultiChannelPerlin perlin;

    private static float shakeDuration;
    private static float shakeMagnitude;
    private static float shakeFrequency;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0)
            {
                perlin.m_AmplitudeGain = 0f;
            }
        }
    }

    public static void ShakeCamera(float duration, float magnitude, float frequency)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeFrequency = frequency;

        perlin.m_AmplitudeGain = shakeMagnitude;
        perlin.m_FrequencyGain = shakeFrequency;
    }
}

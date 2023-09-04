using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;
    CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannel;

    private void Awake()
    {
        virtualCamera= GetComponent<CinemachineVirtualCamera>();
        cinemachineBasicMultiChannel = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float intensity, float time)
    {
        shakeTimerTotal=time;
        startingIntensity = intensity;
        cinemachineBasicMultiChannel.m_AmplitudeGain= intensity;
        shakeTimer = time;
    }

    private void Update()
    {
        if(shakeTimer>0)
        {
            shakeTimer-=Time.deltaTime;
            cinemachineBasicMultiChannel.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
        }
    }
}

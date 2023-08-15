using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RythmController : MonoBehaviour
{
    [SerializeField] private int BPM = 60;
    private float secPerBeat;

    [Tooltip("Porcentaje del tiempo entre beats en el que el input es válido")]
    [Range(0f, 1f)]
    [SerializeField] private float inputThreshold;

    [SerializeField] private AudioClip tickSFX;
    private AudioSource aSource;

    private bool gameStarted;
    private float lastBeatTime;
    private float percentage;

    private TestUI testUI;

    void Start()
    {
        aSource = GetComponent<AudioSource>();
        secPerBeat= 60f / BPM;
        testUI = FindObjectOfType<TestUI>();
    }

    private void Update()
    {
        if(gameStarted)
        {
            float currentTime = Time.time - lastBeatTime;            
            testUI.UpdateDotPosition(currentTime / secPerBeat);
            if (currentTime >= secPerBeat) 
            {
                aSource.PlayOneShot(tickSFX);
                lastBeatTime = Time.time;
            }
            if(currentTime < inputThreshold*secPerBeat || currentTime > secPerBeat * (1-inputThreshold)) 
            {
                testUI.SetIndicatorValid();
            }
            else
            {
                testUI.SetIndicatorInvalid();
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.P)) 
            {
                gameStarted= true;
                aSource.PlayOneShot(tickSFX);
                lastBeatTime= Time.time;
            }
        }
    }


}

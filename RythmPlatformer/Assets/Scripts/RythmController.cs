using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RythmController : MonoBehaviour
{
    [SerializeField] private int BPM = 60;
    private float secPerBeat;

    [Tooltip("Porcentaje del tiempo entre beats en el que el input es válido (porcentaje antes y porcentaje despues)")]
    [Range(0f, 5f)]
    [SerializeField] private float inputThreshold;

    [SerializeField] private AudioClip tickSFX;
    private AudioSource aSource;

    private bool gameStarted;
    private float lastBeatTime;
    private float percentage;

    public bool validInput { get; private set; }

    private TestUI testUI;

    #region Singleton
    private static RythmController _instance;
    public static RythmController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<RythmController>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    #endregion

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
                validInput= true;
            }
            else
            {
                testUI.SetIndicatorInvalid();
                validInput= false;
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

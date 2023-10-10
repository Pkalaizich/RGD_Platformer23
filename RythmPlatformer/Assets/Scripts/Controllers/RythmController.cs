using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RythmController : MonoBehaviour
{
    [SerializeField] private int BPM = 60;
    public float secPerBeat { get; private set; }

    [Tooltip("Porcentaje del tiempo entre beats en el que el input es válido (porcentaje antes y porcentaje despues)")]
    [Range(0f, 5f)]
    [SerializeField] private float inputThreshold;
    public float THRESHOLD => inputThreshold; 

    [SerializeField] private AudioClip tickSFX;
    private AudioSource aSource;

    private bool gameStarted;
    private float lastBeatTime;    

    public bool validInput { get; private set; }

    public bool recivedInputInBeat = false;

    private TestUI testUI;
    
    private CharacterMovementRb charMovRb;
    private bool inThresHold;

    #region Singleton
    private static RythmController _instance;
    public static RythmController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<RythmController>();
                //DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    #endregion

    void Awake()
    {        
        charMovRb = FindObjectOfType<CharacterMovementRb>();
        aSource = GetComponent<AudioSource>();
        secPerBeat= 60f / BPM;
        testUI = FindObjectOfType<TestUI>();
        inThresHold = true;
    }
    private void Start()
    {
        GameplayEvents.OnGameStarted.AddListener(StartRythm);
    }

    private void Update()
    {
        if(GameManager.Instance.gameIsActive)
        {
            float currentTime = Time.time - lastBeatTime;            
            //if (currentTime >= secPerBeat) 
            //{
            //    //aSource.PlayOneShot(tickSFX);
            //    lastBeatTime = Time.time;
            //    StartCoroutine(CheckInput());
            //}
            if(currentTime < inputThreshold*secPerBeat || currentTime > secPerBeat * (1-inputThreshold)) 
            {
                if(!inThresHold)
                {
                    inThresHold= true;
                    GameplayEvents.OnThresholdEnter?.Invoke();
                }
                testUI.SetIndicatorValid();
                validInput= true;
            }
            else
            {
                if(inThresHold)
                {
                    inThresHold = false;
                    GameplayEvents.OnBeatEnded?.Invoke();
                }                
                testUI.SetIndicatorInvalid();
                validInput= false;
            }
        }        
    }

    private void StartRythm()
    {
        //aSource.PlayOneShot(tickSFX);
        //lastBeatTime = Time.time;
    }
    private IEnumerator CheckInput()
    {
        yield return new WaitForSeconds(inputThreshold*secPerBeat);
        InputController.Instance.ProcessInput();        
    }

    /// <summary>
    /// Returns the duration of the input window in seconds
    /// </summary>
    /// <returns></returns>
    public float ThresholdDuration()
    {
        float duration = inputThreshold * secPerBeat * 2;
        return duration;
    }

    public void EnterBeat()
    {
        lastBeatTime = Time.time;
        StartCoroutine(CheckInput());
    }

    public void CountDownBeat()
    {
        lastBeatTime = Time.time;
    }
}

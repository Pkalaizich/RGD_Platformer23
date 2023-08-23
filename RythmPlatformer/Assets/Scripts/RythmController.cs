using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RythmController : MonoBehaviour
{
    [SerializeField] private int BPM = 60;
    public float secPerBeat { get; private set; }

    [Tooltip("Porcentaje del tiempo entre beats en el que el input es v�lido (porcentaje antes y porcentaje despues)")]
    [Range(0f, 5f)]
    [SerializeField] private float inputThreshold;

    [SerializeField] private AudioClip tickSFX;
    private AudioSource aSource;

    private bool gameStarted;
    private float lastBeatTime;    

    public bool validInput { get; private set; }

    public bool recivedInputInBeat = false;

    private TestUI testUI;
    private CharacterMovement charMov;
    private CharacterMovementRb charMovRb;

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

    void Awake()
    {
        //charMov= FindObjectOfType<CharacterMovement>();
        charMovRb = FindObjectOfType<CharacterMovementRb>();
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
                StartCoroutine(CheckInput());
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

    private IEnumerator CheckInput()
    {
        yield return new WaitForSeconds(inputThreshold*secPerBeat);
        if(!recivedInputInBeat)
        {
            //charMov.InputNotSent();
            charMovRb.WrongInput();
            TestUI.Instance.SetMessage("MAL!");
        }
        else
        {
            recivedInputInBeat= false;
        }
    }

}

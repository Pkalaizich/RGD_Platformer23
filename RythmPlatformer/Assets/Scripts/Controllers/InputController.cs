using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private float inputTimeWindow;

    private float lastInputTime;
    private bool inTimeWindow;

    private bool validTime;

    public List<InputActions> thisBeatActions { get; private set; }


    #region Singleton
    private static InputController _instance;
    public static InputController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<InputController>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    #endregion

    private void Start()
    {
        thisBeatActions = new List<InputActions>();
        inTimeWindow = false;
        validTime = false;
    }

    private void Update()
    {
        validTime = RythmController.Instance.validInput;

        #region Inputs
        if (!thisBeatActions.Contains(InputActions.Offbeat))
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if(validTime)
                {                                   
                    if(!thisBeatActions.Contains(InputActions.Left))
                        AddInputToList(InputActions.Right);
                }
                else
                {
                    AddInputToList(InputActions.Offbeat);
                }
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (validTime)
                {
                    if (!thisBeatActions.Contains(InputActions.Right))
                        AddInputToList(InputActions.Left);
                }
                else
                {
                    AddInputToList(InputActions.Offbeat);
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (validTime)
                {                    
                    AddInputToList(InputActions.Jump);
                }
                else
                {
                    AddInputToList(InputActions.Offbeat);
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (validTime)
                {
                    AddInputToList(InputActions.Dash);
                }
                else
                {
                    AddInputToList(InputActions.Offbeat);
                }
            }
            #endregion
        }
    }

    private void AddInputToList(InputActions input)
    {
        //RythmController.Instance.recivedInputInBeat = true;
        if (!inTimeWindow)
        {
            lastInputTime = Time.time;
            inTimeWindow = true;
        }
        if (thisBeatActions.Contains(input))
        {
            return;
        }
        if(Time.time - lastInputTime <= inputTimeWindow)
            thisBeatActions.Add(input);
    }

    public void ProcessInput()
    {
        GameplayEvents.OnProcessInputs?.Invoke();
        
    }
    
    public void ResetInputs()
    {
        thisBeatActions.Clear();
        inTimeWindow = false;
    }

    public enum InputActions
    {
        Offbeat,
        Right,
        Left,
        Up,
        Down,
        Jump,
        Attack,
        Dash,
    }

}
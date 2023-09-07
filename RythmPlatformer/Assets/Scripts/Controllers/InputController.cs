using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private float inputTimeWindow;

    private float lastInputTime;
    private bool inTimeWindow;

    private bool validTime;
    private bool validInput = false;
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
                //DontDestroyOnLoad(_instance.gameObject);
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
        GameplayEvents.OnCountdownEnded.AddListener(ActivateInput);
    }

    private void ActivateInput()
    {
        validInput= true;
    }

    private void Update()
    {
        if(GameManager.Instance.gameIsActive && validInput)
        {
            validTime = RythmController.Instance.validInput;

            #region Inputs
            if (!thisBeatActions.Contains(InputActions.Offbeat))
            {
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (validTime)
                    {
                        if (!thisBeatActions.Contains(InputActions.Left) && !thisBeatActions.Contains(InputActions.Down))
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
                        if (!thisBeatActions.Contains(InputActions.Right) && !thisBeatActions.Contains(InputActions.Down))
                            AddInputToList(InputActions.Left);
                    }
                    else
                    {
                        AddInputToList(InputActions.Offbeat);
                    }
                }
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (validTime)
                    {
                        if (!thisBeatActions.Contains(InputActions.Right) && !thisBeatActions.Contains(InputActions.Left))
                            AddInputToList(InputActions.Down);
                    }
                    else
                    {
                        AddInputToList(InputActions.Offbeat);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
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
                if (Input.GetButtonDown("Fire1"))
                {
                    if (validTime)
                    {
                        AddInputToList(InputActions.Attack);
                    }
                    else
                    {
                        AddInputToList(InputActions.Offbeat);
                    }
                }
                #endregion
            }
        }
        
    }

    public void AddInputToList(InputActions input)
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

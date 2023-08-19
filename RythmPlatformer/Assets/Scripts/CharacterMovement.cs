using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private CharacterController charController;

    [SerializeField] private float speedIncrement;
    [SerializeField] private int speedLevelsAmount;

    [SerializeField] private float jumpSpeed;
    private float gravity;

    [SerializeField] private float jumpBeatDuration;
    public int currentSpeedLevel;

    public PlayerState CurrentState;
    //public float currentSpeed;

    private Vector3 playerVelocity = Vector3.zero;

    private void Awake()
    {
        charController= GetComponent<CharacterController>();
        CurrentState = PlayerState.Stopped;        
    }

    private void Start()
    {
        float jumpTime = RythmController.Instance.secPerBeat * jumpBeatDuration;        
        gravity =  - (2 * jumpSpeed) / jumpTime; 
    }

    private void FixedUpdate()
    {
        charController.Move(playerVelocity * Time.deltaTime);
    }

    private void Update()
    {
        if (charController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }        
        if (charController.isGrounded)
        {            
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                RythmController.Instance.recivedInputInBeat =true;
                if(RythmController.Instance.validInput)
                {
                    currentSpeedLevel = 2;
                    TestUI.Instance.SetMessage("BIEN!");
                }
                else
                {
                    currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;
                    TestUI.Instance.SetMessage("MAL!");
                }
                playerVelocity.x = currentSpeedLevel * speedIncrement;                
            }        
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                RythmController.Instance.recivedInputInBeat = true;
                if (RythmController.Instance.validInput)
                {
                    currentSpeedLevel = 2;
                    TestUI.Instance.SetMessage("BIEN!");
                }
                else
                {
                    currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;
                    TestUI.Instance.SetMessage("MAL!");
                }
                playerVelocity.x = -1f *currentSpeedLevel * speedIncrement;                
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                RythmController.Instance.recivedInputInBeat = true;
                if (RythmController.Instance.validInput)
                {
                    playerVelocity.y += jumpSpeed;
                    CurrentState = PlayerState.Jumping;
                    TestUI.Instance.SetMessage("BIEN!");
                }
                else
                {
                    currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;
                    playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * speedIncrement;
                    TestUI.Instance.SetMessage("MAL!");
                }
                
            }
        }
        playerVelocity.y += gravity * Time.deltaTime;
        
    }



    public void InputNotSent()
    {
        if(CurrentState != PlayerState.Jumping)
        {
            currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;
            TestUI.Instance.SetMessage("MAL!");
            playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * speedIncrement;
        }        
    }
    

    public enum PlayerState
    {
        Stopped,
        Running,
        RuningHalfSpeed,
        Jumping,
        WallGrab
    }
}

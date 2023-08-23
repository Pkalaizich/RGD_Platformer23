using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterMovement;

public class CharacterMovementRb : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider capsCollider;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;
    [HideInInspector] [SerializeField] private float speedIncrement;

    [Tooltip("Cuantas unidades se quiere que avance el jugador por beat")]
    [SerializeField] private float unitsPerBeatSpeed;
    private float unitMeasuredSpeedIncrement;

    

    [SerializeField] private int dashModificator;
    private float dashSpeed;

    [SerializeField] private float jumpSpeed;
    

    

    [Range(1f, 2f)]
    [SerializeField] private float walljumpModificator;

    private float currentGravity;
    private float normalGravity;
    private float grabbedGravity;

    [Tooltip("Multiplicador que afecta a la velocidad vertical inicial del salto cuando se hace un walljump")]
    [Range(0f, 1f)]
    [SerializeField] private float grabbedGravityModificator;

    [SerializeField] private float jumpBeatDuration;
    public int currentSpeedLevel;

    public PlayerStateRb CurrentState;
    //public float currentSpeed;

    private Vector3 playerVelocity = Vector3.zero;

    private bool wallAtRight;

    public bool grounded;
    public bool wallGrab;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsCollider = GetComponent<CapsuleCollider>();
        CurrentState = PlayerStateRb.Stopped;
    }

    private void Start()
    {        
        float jumpTime = RythmController.Instance.secPerBeat * jumpBeatDuration;        
        normalGravity = -(2 * jumpSpeed) / jumpTime;
        grabbedGravity = normalGravity * grabbedGravityModificator;
        currentGravity= grabbedGravity;

        unitMeasuredSpeedIncrement = unitsPerBeatSpeed / RythmController.Instance.secPerBeat;
        dashSpeed = unitMeasuredSpeedIncrement * dashModificator;
    }
    

    private void Update()
    {        
        grounded = IsGrounded();        
        if (grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        if(grounded)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                RythmController.Instance.recivedInputInBeat = true;
                if (RythmController.Instance.validInput)
                {
                    currentSpeedLevel = 1;
                    TestUI.Instance.SetMessage("BIEN!");
                }
                else
                {
                    WrongInput();
                }
                //playerVelocity.x = currentSpeedLevel * speedIncrement;
                playerVelocity.x = currentSpeedLevel * unitMeasuredSpeedIncrement;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                RythmController.Instance.recivedInputInBeat = true;
                if (RythmController.Instance.validInput)
                {
                    currentSpeedLevel = 1;
                    TestUI.Instance.SetMessage("BIEN!");
                }
                else
                {
                    WrongInput();
                }
                //playerVelocity.x = -1f * currentSpeedLevel * speedIncrement;
                playerVelocity.x = -1f * currentSpeedLevel * unitMeasuredSpeedIncrement;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RythmController.Instance.recivedInputInBeat = true;
                if (RythmController.Instance.validInput)
                {
                    playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * unitMeasuredSpeedIncrement;
                    playerVelocity.y += jumpSpeed;
                    CurrentState = PlayerStateRb.Jumping;
                    TestUI.Instance.SetMessage("BIEN!");
                }
                else
                {
                    WrongInput();
                }
            }
        }
        else
        {
            IsGrabingWall();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RythmController.Instance.recivedInputInBeat = true;
                if (RythmController.Instance.validInput && CurrentState==PlayerStateRb.WallGrab)
                {
                    playerVelocity.y += walljumpModificator * jumpSpeed;
                    currentSpeedLevel = 1;
                    int modificator = wallAtRight ? -1 : 1;
                    //playerVelocity.x = modificator * currentSpeedLevel * speedIncrement;
                    playerVelocity.x = modificator * currentSpeedLevel * unitMeasuredSpeedIncrement;
                    currentGravity = normalGravity;
                    CurrentState = PlayerStateRb.Jumping;
                    wallGrab = false;
                    TestUI.Instance.SetMessage("BIEN!");
                }
                else
                {
                    WrongInput();
                }
            }
            
        }
        if(Input.GetKeyDown(KeyCode.LeftShift) && CurrentState != PlayerStateRb.WallGrab && playerVelocity.x!=0)
        {
            RythmController.Instance.recivedInputInBeat = true;
            if(RythmController.Instance.validInput)
            {
                playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * dashSpeed;
            }
            else
            {
                WrongInput();
            }            
        }
        playerVelocity.y += currentGravity * Time.deltaTime;
        rb.velocity = playerVelocity;
    }

    public void WrongInput()
    {
        if (CurrentState != PlayerStateRb.Jumping)
        {
            currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;
            TestUI.Instance.SetMessage("MAL!");
            if(CurrentState == PlayerStateRb.WallGrab)
            {
                currentGravity= normalGravity;
                CurrentState= PlayerStateRb.Jumping;
            }
            if (currentSpeedLevel != 0)
            {
                //playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * speedIncrement;
                //playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * unitMeasuredSpeedIncrement;
                //playerVelocity.x = 0f;
            }
            else
            {
                playerVelocity.x = 0f;
            }
        }
        
    }


    private bool IsGrounded()
    {
        bool ground = Physics.Raycast(capsCollider.bounds.center, Vector3.down, capsCollider.height / 2 ,groundMask);
        if(ground && playerVelocity.y<=0.5f)
        {
            currentGravity = normalGravity;
            if(currentSpeedLevel == 0) CurrentState= PlayerStateRb.Stopped;
            if (currentSpeedLevel == 1) CurrentState = PlayerStateRb.RunningHalfSpeed; 
            if(currentSpeedLevel ==2) CurrentState= PlayerStateRb.Running;
        }
        return ground;
        //return Physics.BoxCast(capsCollider.bounds.center, capsCollider.bounds.size, Vector3.down, Quaternion.identity, 0f, groundMask);
    }

    private bool IsGrabingWall()
    {
        if (playerVelocity.x == 0 && CurrentState!= PlayerStateRb.WallGrab)
            return false;

        bool grabing = Physics.Raycast(capsCollider.bounds.center, Vector3.right * (Mathf.Abs(playerVelocity.x) / playerVelocity.x), capsCollider.radius+0.05f, wallMask);
        wallGrab = grabing;
        if(grabing)
        {
            wallAtRight = playerVelocity.x > 0 ? true : false;
            currentGravity = grabbedGravity;
            playerVelocity.x = 0f;
            playerVelocity.y = 0f;
            currentSpeedLevel = 0;
            CurrentState = PlayerStateRb.WallGrab;
        }
        

        return grabing;
    }

    public enum PlayerStateRb
    {
        Stopped,
        Running,
        RunningHalfSpeed,
        Jumping,
        WallGrab
    }
}

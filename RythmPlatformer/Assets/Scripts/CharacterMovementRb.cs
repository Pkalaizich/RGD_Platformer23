using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementRb : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider capsCollider;

    [Header("Mascaras de capas a detectar")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask enemyMask;
    [Space(10)]
    //[HideInInspector] [SerializeField] private float speedIncrement;


    [Header("Parámetros")]
    [Tooltip("Cuantas unidades se quiere que avance el jugador por beat")]
    [SerializeField] private float unitsPerBeatSpeed;
    private float unitMeasuredSpeedIncrement;
    [SerializeField] private int dashModificator;
    private float dashSpeed;
    [SerializeField] private float jumpSpeed;   
    [Range(1f, 2f)]
    [SerializeField] private float walljumpModificator;
    [Tooltip("Multiplicador que afecta a la velocidad vertical inicial del salto cuando se hace un walljump")]
    [Range(0f, 1f)]
    [SerializeField] private float grabbedGravityModificator;
    [SerializeField] private float jumpBeatDuration;

    [SerializeField] private GameObject armature;

    #region Private variables
    private float currentGravity;
    private float normalGravity;
    private float grabbedGravity;
    private Vector3 playerVelocity = Vector3.zero;
    private bool wallAtRight;
    public bool grounded;
    private bool wallGrab;
    private int currentSpeedLevel;
    public PlayerStateRb CurrentState;
    private Animator animator;
    #endregion
    


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
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

        GameplayEvents.OnProcessInputs.AddListener(SetMovement);
    }
    

    private void Update()
    {
        IsGrabingWall();
        grounded = IsGrounded();        
        if (grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        playerVelocity.y += currentGravity * Time.deltaTime;
        rb.velocity = playerVelocity;
    }

    public void WrongInput()
    {
        if (CurrentState != PlayerStateRb.Jumping)
        {
            currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;            
            if(CurrentState == PlayerStateRb.WallGrab)
            {
                currentGravity= normalGravity;
                CurrentState= PlayerStateRb.Jumping;
            }
            if (currentSpeedLevel != 0)
            {                
                playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * unitMeasuredSpeedIncrement;
            }
            else
            {
                playerVelocity.x = 0f;
            }
        }
        
    }

    public void SetMovement()
    {
        List<InputController.InputActions> inputs = InputController.Instance.thisBeatActions;
        if (inputs.Count ==0 || inputs.Contains(InputController.InputActions.Offbeat))
        {
            WrongInput();
        }
        else
        {
            if (grounded)
            {
                if (inputs.Contains(InputController.InputActions.Right))
                {
                    currentSpeedLevel = 1;
                    playerVelocity.x = currentSpeedLevel * unitMeasuredSpeedIncrement;
                    animator.SetTrigger("Run");
                    armature.transform.localScale = new Vector3(1, 1, 1);
                }
                if (inputs.Contains(InputController.InputActions.Left))
                {
                    currentSpeedLevel = 1;
                    playerVelocity.x = -1f* currentSpeedLevel * unitMeasuredSpeedIncrement;
                    animator.SetTrigger("Run");
                    armature.transform.localScale = new Vector3(1, 1, -1);
                }
                if(inputs.Contains(InputController.InputActions.Jump))
                {
                    if(Mathf.Abs(playerVelocity.x) >=0.1f)
                    {
                        playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * unitMeasuredSpeedIncrement;
                    }                    
                    playerVelocity.y += jumpSpeed;
                    CurrentState = PlayerStateRb.Jumping;
                    animator.SetTrigger("ToIdle");
                }
                if(inputs.Contains(InputController.InputActions.Attack))
                {
                    Debug.Log("Atacando");
                    CheckEnemy();
                    animator.SetTrigger("ToIdle");
                }
            }
            else
            {
                if (inputs.Contains(InputController.InputActions.Jump))
                {
                    if(CurrentState == PlayerStateRb.WallGrab)
                    {
                        playerVelocity.y += walljumpModificator * jumpSpeed;
                        currentSpeedLevel = 1;
                        int modificator = wallAtRight ? -1 : 1;                        
                        playerVelocity.x = modificator * currentSpeedLevel * unitMeasuredSpeedIncrement;
                        currentGravity = normalGravity;
                        CurrentState = PlayerStateRb.Jumping;
                        wallGrab = false;
                        animator.SetTrigger("ToIdle");
                    }
                }
            }
        }

        InputController.Instance.ResetInputs();
    }

    #region Chequeos
    private bool IsGrounded()
    {
        bool ground = Physics.Raycast(capsCollider.bounds.center, Vector3.down, capsCollider.height / 2 +0.05f ,groundMask);
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

    private bool CheckEnemy()
    {
        RaycastHit nearHit;
        float modifier = playerVelocity.x != 0 ? (Mathf.Abs(playerVelocity.x) / playerVelocity.x) : 1;
        bool nearEnemy = Physics.Raycast(capsCollider.bounds.center, Vector3.right * modifier, out nearHit, capsCollider.radius + unitsPerBeatSpeed*(1f/3f), enemyMask);               
        //bool nearEnemy = Physics.SphereCast(capsCollider.bounds.center + new Vector3((capsCollider.radius + unitsPerBeatSpeed * 1f / 6f) * modifier, 0, 0), unitsPerBeatSpeed * (1f / 6f), Vector3.right * modifier, out nearHit,Mathf.Infinity, enemyMask);        
        if (nearEnemy)
        {
            nearHit.transform.GetComponent<Enemy>().DeactivateEnemy(RythmController.Instance.secPerBeat* (1f/ 12f));
            Debug.Log("Ataque cercano");
            return true;
        }
        RaycastHit farHit;
        bool farEnemy = Physics.Raycast(capsCollider.bounds.center, Vector3.right * modifier, out farHit, capsCollider.radius + unitsPerBeatSpeed, enemyMask);
        //bool farEnemy = Physics.SphereCast(capsCollider.bounds.center + new Vector3((capsCollider.radius + unitsPerBeatSpeed * 4f / 6f) * modifier, 0, 0), unitsPerBeatSpeed * (2f / 6f), Vector3.right * modifier, out farHit, Mathf.Infinity, enemyMask);        
        if (farEnemy)
        {
            farHit.transform.GetComponent<Enemy>().DeactivateEnemy(RythmController.Instance.secPerBeat *(5f/ 12f));
            Debug.Log("Ataque lejano");
            return true;
        }
        return false;
    }
    #endregion


    public enum PlayerStateRb
    {
        Stopped,
        Running,
        RunningHalfSpeed,
        Jumping,
        WallGrab
    }
}

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

    [SerializeField] private float maxEnergy;
    private float currentEnergy;
    [SerializeField] private float energyLossPerError;
    [SerializeField] private float maxEnergyLoss;
    [SerializeField] private float energyRecoveredPerBeat;
    private int totalErrors;

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
    private TestUI testUI;
    #endregion
    


    private void Awake()
    {
        testUI = FindObjectOfType<TestUI>();
        rb = GetComponent<Rigidbody>();
        capsCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        CurrentState = PlayerStateRb.Stopped;
        totalErrors= 0;
        currentEnergy = maxEnergy;
    }

    private void Start()
    {        
        float jumpTime = RythmController.Instance.secPerBeat * jumpBeatDuration;        
        normalGravity = -(2 * jumpSpeed) / jumpTime;
        grabbedGravity = normalGravity * grabbedGravityModificator;
        currentGravity= normalGravity;

        unitMeasuredSpeedIncrement = unitsPerBeatSpeed / RythmController.Instance.secPerBeat;
        dashSpeed = unitMeasuredSpeedIncrement * dashModificator;

        GameplayEvents.OnProcessInputs.AddListener(SetMovement);
    }
    

    private void FixedUpdate()
    {
        IsGrabingWall();
        grounded = IsGrounded();        
        if (grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        playerVelocity.y += currentGravity * Time.deltaTime; 
        if(playerVelocity.y < (-1f * jumpSpeed))
        {
            playerVelocity.y = -1f * jumpSpeed;
        }
        rb.velocity = playerVelocity;        
    }

    public void WrongInput()
    {
        if (CurrentState != PlayerStateRb.Jumping)
        {
            if(CurrentState != PlayerStateRb.Stopped)
            {                
                totalErrors+= 1;
                GameplayEvents.OnBadAction?.Invoke();
                float energyLoss = Mathf.Clamp(totalErrors * energyLossPerError,energyLossPerError,maxEnergyLoss);
                currentEnergy = Mathf.Clamp(currentEnergy - energyLoss, 0, maxEnergy);
                testUI.UpdateEnergyBar(currentEnergy / maxEnergy);
                if(currentEnergy<=0)
                {
                    Debug.Log("GAME OVER!");
                }
            }
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
                animator.SetTrigger("ToIdle");
            }
        }
        
    }

    public void SetMovement()
    {
        bool right = armature.transform.localScale.z ==1? true:false;
        List<InputController.InputActions> inputs = InputController.Instance.thisBeatActions;
        if (inputs.Count ==0 || inputs.Contains(InputController.InputActions.Offbeat))
        {
            if (currentSpeedLevel != 0)
            {
                right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) >0? true : false;
            }
            WrongInput();
        }
        else
        {
            currentEnergy = Mathf.Clamp(currentEnergy+energyRecoveredPerBeat,0,maxEnergy);
            testUI.UpdateEnergyBar(currentEnergy/maxEnergy);

            if (grounded)
            {
                if (inputs.Contains(InputController.InputActions.Down))
                {
                    if (currentSpeedLevel != 0)
                    {
                        right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) > 0 ? true : false;
                    }
                    currentSpeedLevel = 0;
                    playerVelocity.x = currentSpeedLevel * unitMeasuredSpeedIncrement;
                    animator.SetTrigger("ToIdle");                    
                }
                if (inputs.Contains(InputController.InputActions.Right))
                {
                    right = true;
                    currentSpeedLevel = 1;
                    playerVelocity.x = currentSpeedLevel * unitMeasuredSpeedIncrement;
                    animator.SetTrigger("Run");                    
                }
                if (inputs.Contains(InputController.InputActions.Left))
                {
                    right = false;
                    currentSpeedLevel = 1;
                    playerVelocity.x = -1f* currentSpeedLevel * unitMeasuredSpeedIncrement;
                    animator.SetTrigger("Run");                    
                }
                if(inputs.Contains(InputController.InputActions.Jump))
                {
                    if(Mathf.Abs(playerVelocity.x) >=0.1f)
                    {
                        playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * unitMeasuredSpeedIncrement;
                        if (currentSpeedLevel != 0)
                        {
                            right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) > 0 ? true : false;
                        }
                    }                    
                    playerVelocity.y += jumpSpeed;
                    CurrentState = PlayerStateRb.Jumping;
                    //animator.SetTrigger("ToIdle");
                }
                if(inputs.Contains(InputController.InputActions.Attack) && !inputs.Contains(InputController.InputActions.Jump))
                {
                    if (currentSpeedLevel != 0)
                    {
                        right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) > 0 ? true : false;
                    }
                    Debug.Log("Atacando");
                    CheckEnemy();
                    //animator.SetTrigger("ToIdle");
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
                        right = modificator ==1? true: false;
                        playerVelocity.x = modificator * currentSpeedLevel * unitMeasuredSpeedIncrement;
                        currentGravity = normalGravity;
                        CurrentState = PlayerStateRb.Jumping;
                        wallGrab = false;
                        //animator.SetTrigger("ToIdle");
                    }
                }
            }
        }
        if (right)
        {
            armature.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            armature.transform.localScale = new Vector3(1, 1, -1);
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

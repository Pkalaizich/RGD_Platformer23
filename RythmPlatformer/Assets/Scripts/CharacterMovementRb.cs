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
    [SerializeField] private float enemyCollisionEnergyLoss;
    //[SerializeField] private float energyLossPerFailedAttack;
    [SerializeField] private float energyRecoveredPerBeat;
    [SerializeField] private float timeToResetErrors;
    private float lastErrorTime;
    private int totalErrors;
    private bool damageFromEnemy =false;
    private bool missedAttack =false;
    private bool attacking =false;

    [SerializeField] private GameObject armature;
    [SerializeField] private GameObject exclamation;
    [SerializeField] private float groundDistanceFallAnimation;

    [Header("Screen Shake Parameters")]
    [SerializeField] private float wrongInputShakeTime;
    [SerializeField] private float wrongInputShakeIntensity;
    private CameraShake camShak;

    [Space(25)]
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
    private WaitForSeconds wait;
    private static readonly int hGlobalParam = Animator.StringToHash("GlobalParameter");
    private float kickDuration;
    private float kickTime;
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
        camShak = FindObjectOfType<CameraShake>();
    }

    private void Start()
    {        
        wait = new WaitForSeconds(RythmController.Instance.secPerBeat/2);
        float jumpTime = RythmController.Instance.secPerBeat * jumpBeatDuration;        
        normalGravity = -(2 * jumpSpeed) / jumpTime;
        grabbedGravity = normalGravity * grabbedGravityModificator;
        currentGravity= normalGravity;
        kickDuration = RythmController.Instance.secPerBeat / 2;
        unitMeasuredSpeedIncrement = unitsPerBeatSpeed / RythmController.Instance.secPerBeat;
        dashSpeed = unitMeasuredSpeedIncrement * dashModificator;

        GameplayEvents.OnThresholdEnter.AddListener(CheckEnemyInNextBeat);
        GameplayEvents.OnProcessInputs.AddListener(SetMovement);
        GameplayEvents.OnGameWon.AddListener(()=> {
            SetAnimationByIndex(7);
            rb.velocity = new Vector3(0, playerVelocity.y, 0);
        });
    }
    

    private void FixedUpdate()
    {
        if(GameManager.Instance.gameIsActive)
        {
            IsGrabingWall();
            grounded = IsGrounded();
            if (grounded && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }
            if ((!grounded && CurrentState != PlayerStateRb.WallGrab) && playerVelocity.y < 0)
            {
                SetAnimationByIndex(3);
                if (CurrentState == PlayerStateRb.Falling)
                {
                    if (OverGround())
                    {
                        //animator.StartPlayback();
                        animator.speed = 1;
                    }
                    else
                    {
                        //animator.StopPlayback();
                        animator.speed = 0;
                    }
                }
                CurrentState = PlayerStateRb.Falling;
            }
            playerVelocity.y += currentGravity * Time.deltaTime;
            if (playerVelocity.y < (-1f * jumpSpeed))
            {
                playerVelocity.y = -1f * jumpSpeed;
            }
            rb.velocity = playerVelocity;
            if(attacking)
            {
                if(Time.time - kickTime > kickDuration)
                {
                    attacking=false;
                    playerVelocity.x = 0;
                    currentSpeedLevel = 0;
                    SetAnimationByIndex(0); //IDLE
                    KickResult();
                }
                
            }
        }              
    }

    public void WrongInput()
    {
        
        if (CurrentState != PlayerStateRb.Jumping && CurrentState != PlayerStateRb.Falling)
        {
            if(CurrentState != PlayerStateRb.Stopped)
            {
                if(damageFromEnemy)
                {
                    damageFromEnemy = false;
                    currentSpeedLevel = 0;
                }
                else
                {
                    totalErrors += 1;
                    lastErrorTime = Time.time;
                    float energyLoss = Mathf.Clamp(totalErrors * energyLossPerError, energyLossPerError, maxEnergyLoss);
                    EnergyLoss(energyLoss);
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
                playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel/2 * unitMeasuredSpeedIncrement;
            }
            else
            {
                playerVelocity.x = 0f;
                SetAnimationByIndex(0);                
            }
        }        
        
    }

    public void SetMovement()
    {
        exclamation.SetActive(false);
        //StartCoroutine(WaitToCheck());
        attacking = false;
        if(Time.time - lastErrorTime>= timeToResetErrors)
        {
            totalErrors = 0;
        }
        bool right = armature.transform.localScale.y >=50? true:false;
        List<InputController.InputActions> inputs = InputController.Instance.thisBeatActions;
        if (inputs.Contains(InputController.InputActions.Offbeat) || (inputs.Count == 0 && (CurrentState == PlayerStateRb.WallGrab)))
        {
            if (currentSpeedLevel != 0)
            {
                right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) >0? true : false;
            }
            else
            {
                right = armature.transform.localScale.y >= 50 ? true : false;
            }
            WrongInput();
        }
        else
        {
            if(inputs.Count == 0 && (CurrentState!=PlayerStateRb.Falling&& CurrentState != PlayerStateRb.Jumping))
            {
                if (currentSpeedLevel != 0)
                {
                    right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) > 0 ? true : false;
                }
                else
                {
                    right = armature.transform.localScale.y >= 50 ? true : false;
                }
                currentSpeedLevel = 0;
                playerVelocity.x = currentSpeedLevel * unitMeasuredSpeedIncrement;
                SetAnimationByIndex(0);  //IDLE            
            }
            else
            {
                currentEnergy = Mathf.Clamp(currentEnergy + energyRecoveredPerBeat, 0, maxEnergy);
                testUI.UpdateEnergyBar(currentEnergy / maxEnergy);

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
                        SetAnimationByIndex(0);  //IDLE                                    
                    }
                    if (inputs.Contains(InputController.InputActions.Right))
                    {
                        right = true;
                        currentSpeedLevel = 2;
                        playerVelocity.x = currentSpeedLevel * unitMeasuredSpeedIncrement / 2;
                        SetAnimationByIndex(1);   //RUN                                     
                    }
                    if (inputs.Contains(InputController.InputActions.Left))
                    {
                        right = false;
                        currentSpeedLevel = 2;
                        playerVelocity.x = -1f * currentSpeedLevel * unitMeasuredSpeedIncrement / 2;
                        SetAnimationByIndex(1);   //RUN                                     
                    }
                    if (inputs.Contains(InputController.InputActions.Jump))
                    {
                        if (Mathf.Abs(playerVelocity.x) >= 0.1f)
                        {
                            if (currentSpeedLevel != 0)
                            {
                                right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) > 0 ? true : false;
                                currentSpeedLevel = 2;
                            }
                            playerVelocity.x = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) * currentSpeedLevel * unitMeasuredSpeedIncrement / 2;
                        }
                        playerVelocity.y += jumpSpeed;
                        CurrentState = PlayerStateRb.Jumping;
                        SetAnimationByIndex(2);   //JUMP
                        MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.Jump);
                    }
                    if (inputs.Contains(InputController.InputActions.Attack) && !inputs.Contains(InputController.InputActions.Jump))
                    {
                        attacking = true;
                        SetAnimationByIndex(6);   //KICK
                        MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.KickSwing);
                        if (currentSpeedLevel != 0)
                        {
                            right = (Mathf.Abs(playerVelocity.x) / playerVelocity.x) > 0 ? true : false;
                        }
                        currentSpeedLevel = 4;
                        int modif = right ? 1 : -1;
                        playerVelocity.x = modif * currentSpeedLevel * unitMeasuredSpeedIncrement / 2;
                        Debug.Log("Atacando");
                        missedAttack = !CheckEnemy();
                        kickTime = Time.time;
                    }
                }
                else
                {
                    if (inputs.Contains(InputController.InputActions.Jump))
                    {
                        if (CurrentState == PlayerStateRb.WallGrab)
                        {
                            playerVelocity.y += walljumpModificator * jumpSpeed;
                            currentSpeedLevel = 2;
                            int modificator = wallAtRight ? -1 : 1;
                            right = modificator == 1 ? true : false;
                            playerVelocity.x = modificator * currentSpeedLevel * unitMeasuredSpeedIncrement / 2;
                            currentGravity = normalGravity;
                            CurrentState = PlayerStateRb.Jumping;
                            wallGrab = false;
                            SetAnimationByIndex(2); //CAMBIAR POR ANIMACION DE WALLJUMP!!
                            MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.Jump);
                        }
                    }
                }
            }
            
        }
        if (right)
        {
            armature.transform.localScale = new Vector3(100, 100, 100);
        }
        else
        {
            armature.transform.localScale = new Vector3(100, -100, 100);
        }

        InputController.Instance.ResetInputs();
        
    }

    public void EnemyCollision()
    {
        InputController.Instance.AddInputToList(InputController.InputActions.Offbeat);
        totalErrors += 1;
        lastErrorTime = Time.time;
        EnergyLoss(enemyCollisionEnergyLoss);
        damageFromEnemy = true;
    }

    private void KickResult()
    {
        if(missedAttack)
        {
            totalErrors += 1;
            lastErrorTime = Time.time;
            EnergyLoss(totalErrors*energyLossPerError);
        }
        else
        {
            MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.KickHit);
        }
        missedAttack= false;
    }

    public void EnergyLoss(float loss)
    {
        currentEnergy = Mathf.Clamp(currentEnergy - loss, 0, maxEnergy);
        testUI.UpdateEnergyBar(currentEnergy / maxEnergy);        
        GameplayEvents.OnBadAction?.Invoke();
        if (camShak != null)
        {
            camShak.ShakeCamera(wrongInputShakeIntensity, wrongInputShakeTime);
        }
        if (currentEnergy <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        GameplayEvents.OnGameEnded?.Invoke();
        this.gameObject.SetActive(false);
    }

    #region Chequeos
    private bool IsGrounded()
    {        
        bool ground = Physics.Raycast(capsCollider.bounds.center, Vector3.down, capsCollider.height / 2 +0.05f ,groundMask);
        if(ground && playerVelocity.y<=0.5f)
        {
            if(CurrentState == PlayerStateRb.Falling)
            {
                MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.JumpLand);
                if (currentSpeedLevel == 0)
                {
                    CurrentState = PlayerStateRb.Stopped;
                    SetAnimationByIndex(0);
                }
                if (currentSpeedLevel == 1)
                {
                    //animator.StartPlayback();
                    SetAnimationByIndex(1);
                    animator.speed = 0.5f;
                    CurrentState = PlayerStateRb.RunningHalfSpeed;
                }
                if (currentSpeedLevel == 2)
                {
                    //animator.StartPlayback();
                    SetAnimationByIndex(1);
                    CurrentState = PlayerStateRb.Running;
                }
            }
            currentGravity = normalGravity;
            animator.speed = 1;            
        }
        return ground;
        
    }

    private bool IsGrabingWall()
    {
        if (playerVelocity.x == 0 && CurrentState!= PlayerStateRb.WallGrab)
            return false;

        bool grabing = Physics.Raycast(capsCollider.bounds.center, Vector3.right * (Mathf.Abs(playerVelocity.x) / playerVelocity.x), capsCollider.radius+0.05f, wallMask);
        wallGrab = grabing;
        if(grabing)
        {
            if(CurrentState!=PlayerStateRb.WallGrab)
            {
                MusicManager.Instance.PlaySound((int)MusicManager.AvailableSFX.WallGrab);
            }
            animator.speed = 1;
            wallAtRight = playerVelocity.x > 0 ? true : false;
            currentGravity = grabbedGravity;
            playerVelocity.x = 0f;
            playerVelocity.y = 0f;
            currentSpeedLevel = 0;
            CurrentState = PlayerStateRb.WallGrab;
            SetAnimationByIndex(4); //CAMBIAR POR ANIMACION DE WALLGRAB!!!
        }
        

        return grabing;
    }

    private bool OverGround()
    {
        bool ground = Physics.Raycast(capsCollider.bounds.center, Vector3.down, capsCollider.height / 2 + groundDistanceFallAnimation, groundMask);
        return ground;
    }

    private bool CheckEnemy()
    {
        RaycastHit nearHit;
        float modifier = playerVelocity.x != 0 ? (Mathf.Abs(playerVelocity.x) / playerVelocity.x) : 1;
        bool nearEnemy = Physics.Raycast(capsCollider.bounds.center, Vector3.right * modifier, out nearHit, capsCollider.radius + unitsPerBeatSpeed*(1f/3f), enemyMask);               
        //bool nearEnemy = Physics.SphereCast(capsCollider.bounds.center + new Vector3((capsCollider.radius + unitsPerBeatSpeed * 1f / 6f) * modifier, 0, 0), unitsPerBeatSpeed * (1f / 6f), Vector3.right * modifier, out nearHit,Mathf.Infinity, enemyMask);        
        if (nearEnemy)
        {
            nearHit.transform.GetComponent<Enemy>().DeactivateEnemy(kickDuration);
            Debug.Log("Ataque cercano");
            return true;
        }
        RaycastHit farHit;
        bool farEnemy = Physics.Raycast(capsCollider.bounds.center, Vector3.right * modifier, out farHit, capsCollider.radius + unitsPerBeatSpeed, enemyMask);
        //bool farEnemy = Physics.SphereCast(capsCollider.bounds.center + new Vector3((capsCollider.radius + unitsPerBeatSpeed * 4f / 6f) * modifier, 0, 0), unitsPerBeatSpeed * (2f / 6f), Vector3.right * modifier, out farHit, Mathf.Infinity, enemyMask);        
        if (farEnemy)
        {
            farHit.transform.GetComponent<Enemy>().DeactivateEnemy(kickDuration);
            Debug.Log("Ataque lejano");
            return true;
        }
        return false;
    }

    private void CheckEnemyInNextBeat()
    {
        RaycastHit Hit;
        float modifier = playerVelocity.x != 0 ? (Mathf.Abs(playerVelocity.x) / playerVelocity.x) : 1;
        float thresholdDistance = currentSpeedLevel * unitsPerBeatSpeed / 2 * RythmController.Instance.ThresholdDuration();
        bool nearEnemy = Physics.Raycast(capsCollider.bounds.center, Vector3.right * modifier, out Hit, capsCollider.radius + unitsPerBeatSpeed + thresholdDistance-0.7f, enemyMask);
        if(nearEnemy)
        {
            exclamation.SetActive(true);
        }
    }
    #endregion

    //private IEnumerator WaitToCheck()
    //{
    //    yield return wait;
    //    CheckEnemyInNextBeat();
    //}

    /// <summary>
    /// 0:IDLE
    /// 1:RUN
    /// 2:JUMP
    /// 3:FALL
    /// 4:WALLGRAB
    /// 5:WALLJUMP
    /// 6:KICK
    /// </summary>
    /// <param name="animation"></param>
    private void SetAnimationByIndex(int animation)
    {
        animator.SetInteger(hGlobalParam, animation);
    }

    public enum PlayerStateRb
    {
        Stopped,
        Running,
        RunningHalfSpeed,
        Jumping,
        Falling,
        Attacking,
        WallGrab
    }
}

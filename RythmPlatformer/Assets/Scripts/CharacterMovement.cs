using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private CharacterController charController;

    [SerializeField] private float speedIncrement;
    [SerializeField] private int speedLevelsAmount;

    public int currentSpeedLevel;
    //public float currentSpeed;

    private Vector3 movement = Vector3.zero;

    private void Awake()
    {
        charController= GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        charController.Move(movement * Time.deltaTime);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            IncreaseSpeedByOneLevel();
        }
    }

    public void IncreaseSpeedByOneLevel()
    {
        currentSpeedLevel = Mathf.Clamp(currentSpeedLevel + 1, 0, speedLevelsAmount);
        movement.x = currentSpeedLevel * speedIncrement;
    }
}

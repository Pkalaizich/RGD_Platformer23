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
        //float horiz = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(RythmController.Instance.validInput)
            {
                currentSpeedLevel = 2;
                
            }
            else
            {
                currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;
            }
            movement.x = currentSpeedLevel * speedIncrement;
        }        
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (RythmController.Instance.validInput)
            {
                currentSpeedLevel = 2;

            }
            else
            {
                currentSpeedLevel = currentSpeedLevel == 2 ? 1 : 0;
            }
            movement.x = -1 *currentSpeedLevel * speedIncrement;
        }
    }

}

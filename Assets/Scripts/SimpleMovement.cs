using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
   
    public float _speed = 10;
    public float _rotationSpeed = 180;
    public float jumpSpeed = 15.0f;
    public float gravity = 20.0f;
  
    
    CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
   
    public void Update()
    {
        //Update the rotation
        rotate();
        Vector3 move = Vector3.zero;
        //Retrieve data from the input
        move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //check if the player is grounded
        if (characterController.isGrounded)
        {
            //if the jump button is pressed
            if (Input.GetButton("Jump"))
            {
                //modify the y value
                move.y = jumpSpeed;
            }    
        }
        //traansform the vector so that forward is forward for the player
        move = transform.TransformDirection(move);
        //update the y value based on gravity
        move.y -= gravity * Time.deltaTime;
        //move the player
        characterController.Move(move * _speed * Time.deltaTime);
    }


    void rotate()
    {
        float x;
        float y;
        Vector3 rotateValue;

        //Get movement from the mouse
        y = Input.GetAxis("Mouse X");
        x = Input.GetAxis("Mouse Y");
        //create a vector3 with the movement values
        rotateValue = new Vector3(x, y * -1, 0);
        //update the rotation of the player
        transform.eulerAngles = transform.eulerAngles - rotateValue;
    }      
}



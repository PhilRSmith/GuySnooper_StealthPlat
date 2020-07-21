using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*** START: Player Movement/Gravity Variables ***/
    [SerializeField]
    private float _playerSpeed=5.0f;
    [SerializeField]
    private float _playerGravity=0.5f;
    [SerializeField]
    private float _jumpHeight = 15.0f;
    [SerializeField]
    private float _dashImpulse = 100.0f;
    private float _yVelocity=0.0f;
    private float _horizontalInput;
    private CharacterController _playerController;
    private int _currentDirectionFaced = 1;
    /***   END: Player Movement/Gravity Variables ***/

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<CharacterController>();
        if (_playerController == null)
        {
            Debug.LogError("Player:PlayerController DOES NOT EXIST");
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        Vector3 playerDirection = new Vector3(_horizontalInput, 0.0f , 0.0f);
        Vector3 playerVelocity = playerDirection * _playerSpeed;
        
        /*Check to see which direction the player is facing */
        DirectionCheck(_horizontalInput);

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerVelocity = Dash(playerVelocity);
        }

        /* Jumping behavior */
        _yVelocity = Jump(_yVelocity);
        playerVelocity.y = _yVelocity;
        
        if(playerDirection.x!=0)
        {
            transform.rotation = Quaternion.LookRotation(playerDirection);
        }
        _playerController.Move(playerVelocity * Time.deltaTime);
    }

    float Jump(float y)
    {
         /* Jumping behavior START */
        if(_playerController.isGrounded == true)
        {
            y = -.5f;
            if(Input.GetKeyDown(KeyCode.Space))
            {
                y+=_jumpHeight;
            } 
        }
        else
        {
            y -= _playerGravity;
        }
        /* Jumping behavior END */
        return y;
    }

    Vector3 Dash(Vector3 playerVelocity)
    {
        if(_horizontalInput==0.0f)
        {
            playerVelocity.x = _currentDirectionFaced;
        }
        playerVelocity.x = playerVelocity.x + (_dashImpulse * _currentDirectionFaced);
        return playerVelocity;
    }
    void DirectionCheck(float xInput)
    {
         if(xInput>0.1f)
        {
            _currentDirectionFaced = 1;
        }
        else if(xInput <-0.1f)
        {
            _currentDirectionFaced = -1;
        }
    }

    

    void FaceMovementDirection()
    {
        
    }
}

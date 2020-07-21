using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigid : MonoBehaviour
{
    /*** START: Player Component Handles          ***/
    private Rigidbody _playerRigidBody;
    private CapsuleCollider _playerCollider;
    /***   END: Player Component Handles          ***/

    /*** START: Player Movement/Gravity Variables ***/
    [SerializeField]
    private float _playerSpeed=5.0f;
    [SerializeField]
    private float _playerGravityScale=0.5f;
    [SerializeField]
    private float _jumpVelocity = 5.0f;
    [SerializeField]
    private float _wallJumpImpulse = 10.0f;
    [SerializeField]
    private float _dashImpulse = 100.0f;
    [SerializeField]
    private bool isGrounded;
    private bool onWall;
    private float _yVelocity=0.0f;
    private float _horizontalInput;
    private int _currentDirectionFaced = 1;
    /***   END: Player Movement/Gravity Variables ***/

    // Start is called before the first frame update
    void Start()
    {
        _playerRigidBody = GetComponent<Rigidbody>();
        _playerCollider = GetComponent<CapsuleCollider>();
        if (_playerRigidBody == null)
        {
            Debug.LogError("Player:RigidBody DOES NOT EXIST");
        }
        if (_playerCollider == null)
        {
            Debug.LogError("Player:Collider DOES NOT EXIST");
        }
        _playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;  //Don't allow Player character to roll over
        
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        Vector3 playerVelocity = new Vector3((_horizontalInput * _playerSpeed), _playerRigidBody.velocity.y , 0.0f);
        
        
        /*Check to see which direction the player is facing */
        DirectionCheck(_horizontalInput);

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerVelocity = Dash(playerVelocity);
        }

        _playerRigidBody.velocity = playerVelocity;
        /* Jumping behavior */
        Jump();
        
        if(_horizontalInput!=0)
        {
            Vector3 lookDirection = new Vector3(0,0,1);
            lookDirection = lookDirection* _horizontalInput;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        
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
    private void CheckIfGrounded()
    {
        float heightBuffer = .02f;
        
        //if a collider was hit, we are grounded
        if (Physics.Raycast(_playerCollider.bounds.center, Vector3.down, _playerCollider.bounds.extents.y + heightBuffer)) 
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

    }

    private void CheckIfOnWall()
    {
        float distBuffer = .01f;
        if ((Physics.Raycast(_playerCollider.bounds.center, Vector3.left,_playerCollider.bounds.extents.x + distBuffer))
        || (Physics.Raycast(_playerCollider.bounds.center, Vector3.right,_playerCollider.bounds.extents.x + distBuffer)))
        {
            onWall=true;
        }
        else
        {
            onWall=false;
        }
    }

    void Jump()
    {
        
         /* Jumping behavior START */
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CheckIfGrounded();
            CheckIfOnWall();
            if(isGrounded)
            {
                _playerRigidBody.velocity += Vector3.up * _jumpVelocity;
            } 
            else if(onWall)
            {   
                Vector3 wallJump = new Vector3(_wallJumpImpulse, _jumpVelocity,0);
                //_playerRigidBody.velocity += Vector3.up * _jumpVelocity;
                if(_currentDirectionFaced==1)
                {
                    wallJump.x *=-1;
                    _playerRigidBody.AddForce(wallJump, ForceMode.Impulse);
                    
                }
                else if(_currentDirectionFaced==-1)
                {
                    _playerRigidBody.AddForce(wallJump, ForceMode.Impulse);
                }
         } 
        }
        else
        {
            
        }
        /* Jumping behavior END */
    }

    /*private void OnCollisionEnter(Collision other) 
    {
        foreach (ContactPoint contact in other.contacts)
        {
            if(!isGrounded && contact.normal.y <0.1f)
            {
                _playerRigidBody.AddForce(contact.normal * _wallJumpImpulse, ForceMode.Impulse);
            }
            Debug.DrawRay(contact.point, contact.normal,Color.green, 1.25f);
        }
    }*/

    Vector3 Dash(Vector3 playerVelocity)
    {
        if(_horizontalInput==0.0f)
        {
            playerVelocity.x = _currentDirectionFaced;
        }
        playerVelocity.x = playerVelocity.x + (_dashImpulse * _currentDirectionFaced);
        return playerVelocity;
    }
}

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
    private float _jumpVelocity = 8.0f;
    private float _wallJumpImpulse = 15.0f;
    [SerializeField]
    private float _dashImpulse = 100.0f;
    private bool _isGrounded;
    private bool _onWall;
    private float _yVelocity=0.0f;
    private float _horizontalInput;
    private int _currentDirectionFaced = 1;
    /***   END: Player Movement/Gravity Variables ***/

    /*** START: Collision/Environment Interactions **/
    private float _wallJumpDelay = .33f;
    private float _wallJumpLength;
    private bool _wallJumpInProgress = false;
    private float checkTime;
    /***   END: Collision/Environment Interactions **/

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
        checkTime = Time.time;
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
            if(_wallJumpInProgress==false)
            {   
                Vector3 lookDirection = new Vector3(0,0,1);
                lookDirection = lookDirection* _horizontalInput;
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
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
            _isGrounded = true;
            Debug.Log("Grounded");
        }
        else
        {
            _isGrounded = false;
        }

    }

    private void CheckIfOnWall()
    {
        float distBuffer = .01f;
        if ((Physics.Raycast(_playerCollider.bounds.center, Vector3.left,_playerCollider.bounds.extents.x + distBuffer))
        || (Physics.Raycast(_playerCollider.bounds.center, Vector3.right,_playerCollider.bounds.extents.x + distBuffer)))
        {
            _onWall=true;
            Debug.Log("On Wall");
        }
        else
        {
            _onWall=false;
        }
    }

    void Jump()
    {
        
         /* Jumping behavior START */
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CheckIfGrounded();
            if(_isGrounded)
            {
                _playerRigidBody.velocity += Vector3.up * _jumpVelocity;
            } 
            else
            {   
                CheckIfOnWall();
                if(_wallJumpInProgress==false)
                {
                    if(_onWall)
                    {
                        _wallJumpLength = Time.time + _wallJumpDelay;
                        if(_currentDirectionFaced==1)
                        {
                            
                            _currentDirectionFaced = -1;
                        } 
                        else if(_currentDirectionFaced==-1)
                        {
                            _currentDirectionFaced = 1;
                        }
                        _wallJumpInProgress=true;
                        StartCoroutine(WallJumpMovement());
                    }
                }  
            } 
        }
        else
        {
            
        }
        /* Jumping behavior END */
    }

    /* Coroutine that governs the exact movement performed during the initial part of a walljump*/
    IEnumerator WallJumpMovement()
    {
        Vector3 wallJump = new Vector3((_wallJumpImpulse * _currentDirectionFaced), 5.0f, 0);
        Vector3 lookDirection = new Vector3(0,0,1);
    
        lookDirection = lookDirection * _currentDirectionFaced;
        transform.rotation = Quaternion.LookRotation(lookDirection);


        while(Time.time<_wallJumpLength && (_wallJumpInProgress==true) )
        {
            wallJump.y -= .35f;
            if(Time.time<(_wallJumpLength - ((.5f)*_wallJumpDelay)))
            {
                _playerRigidBody.velocity = wallJump;
            }
            else
            {
                _playerRigidBody.velocity =((1.0f/((100*_wallJumpDelay)/2)) * wallJump);
            }
            
            yield return new WaitForSeconds(0.01f);
        }
        _wallJumpInProgress=false;
        
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
}

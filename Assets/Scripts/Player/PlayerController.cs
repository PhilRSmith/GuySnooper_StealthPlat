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
    private float _jumpHeight = 10.0f;
    [SerializeField]
    private float _dashImpulse = 100.0f;
    private float _yVelocity=0.0f;
    private float _horizontalInput;
    private CharacterController _playerController;
    private Vector3 playerDirection = new Vector3(0.0f, 0.0f , 0.0f);
    Vector3 previousMove = new Vector3(0.0f, 0.0f , 0.0f);
    Vector3 playerVelocity = new Vector3(0.0f, 0.0f , 0.0f);
    private int _currentDirectionFaced = 1;

    // START:Terrain Checks
    private bool _isGrounded;
    private bool _onWall = false;
    //   END:Terrain Checks

    /***   END: Player Movement/Gravity Variables ***/

    /*** START: Collision/Environment Interactions **/
    private float _wallJumpDelay = .50f;
    private float _wallJumpLength;
    private bool _wallJumpInProgress = false;
    private float checkTime;
    /***   END: Collision/Environment Interactions **/

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

    void LateUpdate() 
    {
        transform.position= new Vector3(transform.position.x, transform.position.y , 0.0f);
    }
    void Movement()
    {
        
        _horizontalInput = Input.GetAxis("Horizontal");
        if(_horizontalInput>0.1f)
        {
            _horizontalInput=1.0f;
        }
        else if(_horizontalInput<-0.1f)
        {
            _horizontalInput=-1.0f;
        }
        else
        {
            _horizontalInput=0.0f;
        }
        playerDirection.x = _horizontalInput;
        if(_wallJumpInProgress==false)
        {
            playerVelocity = playerDirection * _playerSpeed;
        }
        else
        {
            playerVelocity = previousMove;
        }   
        
        /*Check to see which direction the player is facing */
        

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerVelocity = Dash(playerVelocity);
        }

        /* Jumping behavior */
        _yVelocity = Jump(_yVelocity);
        playerVelocity.y = _yVelocity;
        
        DirectionCheck(playerVelocity.x);
        if(playerDirection.x!=0)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(_currentDirectionFaced, 0 , 0));
        }
        _playerController.Move(playerVelocity * Time.deltaTime);
        previousMove = playerVelocity;
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
            if(_wallJumpInProgress)
            {
                playerVelocity = previousMove;
            }
            else
            {
                if( _horizontalInput<0.1 && _horizontalInput > -0.1)
                {
                    playerVelocity = previousMove;
                }
                else
                {
                    
                }
            }
            
            
        }
        /* Jumping behavior END */
        return y;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {
        if(!_playerController.isGrounded)
        {
            Debug.Log(hit.normal);
        }
        if(!_playerController.isGrounded && hit.normal.y <0.1f)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(_wallJumpInProgress==false)
                {   
                    if(((hit.normal.x<0) && _horizontalInput>0) || ((hit.normal.x>0) && _horizontalInput<0))
                    {   
                        Debug.DrawRay(hit.point, hit.normal, Color.green, 2.00f);
                        _yVelocity = _jumpHeight;
                        Debug.Log(hit.normal);
                        StartCoroutine(WallJumpOccurring());
                        playerVelocity= hit.normal * (_playerSpeed +1);
                        transform.rotation = Quaternion.LookRotation(hit.normal);
                    }
                }
            }
        }
    }

    IEnumerator WallJumpOccurring()
    {
        _wallJumpInProgress=true;
        _wallJumpLength = Time.time + _wallJumpDelay;
        //Debug.Log("WallJump Activated");
        while(_wallJumpInProgress==true)
        {   
            yield return new WaitForSeconds(.02f);
            CheckIfOnWall();
            if(_onWall)
            {
                _wallJumpInProgress=false;
                //Debug.Log("WallJump Deactivated - hit another wall");
            }
            else if (Time.time>= _wallJumpLength)
            {
                _wallJumpInProgress=false;
                //Debug.Log("WallJump Deactivated - Timed out");
            }
            
        }
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

    private void CheckIfGrounded()
    {
        float heightBuffer = .02f;
        
        //if a collider was hit, we are grounded
        if (Physics.Raycast(_playerController.bounds.center, Vector3.down, _playerController.bounds.extents.y + heightBuffer)) 
        {
            _isGrounded = true;
            //Debug.Log("Grounded");
        }
        else
        {
            _isGrounded = false;
        }

    }

    private void CheckIfOnWall()
    {
        float distBuffer = .09f;
        if ((Physics.Raycast(_playerController.bounds.center, Vector3.left,_playerController.bounds.extents.x + distBuffer))
        || (Physics.Raycast(_playerController.bounds.center, Vector3.right,_playerController.bounds.extents.x + distBuffer)))
        {
            _onWall=true;
            //Debug.Log("On Wall");
        }
        else
        {
            _onWall=false;
        }
    }

    

    void FaceMovementDirection()
    {
        
    }
}

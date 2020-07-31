using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /*** START: Player Movement/Gravity Variables ***/
    [SerializeField]
    private float _playerCurrentSpeed=6.0f;
    [SerializeField]
    private float _playerBaseSpeed=6.0f;
    private float _playerCrouchedSpeed=3.6f;
    
    private float playerBaseGravity=-0.5f;
    private float playerSlopeForce = 3.0f;
    private float _playerGravity=-0.5f;
    
    private float _maxDownwardVelocity = -20.0f;
    [SerializeField]
    private float _jumpHeight = 10.0f;
    private float _yVelocity=0.0f;
    private float _horizontalInput;
    private CharacterController _playerController;
    private Vector3 playerDirection = new Vector3(0.0f, 0.0f , 0.0f);
    Vector3 previousMove = new Vector3(0.0f, 0.0f , 0.0f);
    Vector3 playerVelocity = new Vector3(0.0f, 0.0f , 0.0f);

    private int _currentDirectionFaced = 1;
    /***   END: Player Movement/Gravity Variables ***/

    // START:Terrain Checks
    private float slopeCheckLength = 0.2f;
    private bool _onWall = false;
    private bool _isCrouching = false;
    private bool _isJumping = false;
    //   END:Terrain Checks

    
    /*** START: Ability Variables **/
    private bool _wallJumpInProgress = false;
    private float _wallJumpDelay = .50f;
    private float _wallJumpLength;
    //
    private bool _dashInProgress = false;
    private float _dashDelay =.25f;
    private float _dashCooldown = 3.0f;
    private bool _isDashOnCooldown = false;
    private float _dashLength;
    [SerializeField]
    private float _dashImpulse = 25.0f;
    /***   END: Abailty Variables **/

    /*** START: Other Object Handles ***/
    private UIManager _uiManager;
    /***   END: Other Object Handles ***/

    /* BIG NOTE TO SELF: Careful, the code in here is starting to become a bit confusing for someone to read if uninitiated. try to be more careful with the booleans
    and spaghetti code!
    
    Improvements that could be made:
    - Make the section within jump() inside the !_isGrounded part of aerial movement
    - Consolidate related boolean checks into their own functions, e.g. stuff that makes crouch checks or aerial checks etc
    - Can't think of any more right now, so tired but this needs to be cleaned up if I'm going to actually make a full game
    */

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<CharacterController>();
        if (_playerController == null)
        {
            Debug.LogError("Player:CharacterController DOES NOT EXIST");
        }
        
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (_uiManager == null)
        {
            Debug.LogError("Player:UIManager DOES NOT EXIST");
        }   
    }
    // Update is called once per frame
    void Update()
    {  
        CrouchControl(); 
        Movement();
    }

    void LateUpdate() 
    {  
        transform.position= new Vector3(transform.position.x, transform.position.y , 0.0f); //Keep player from moving on undesired z-axis     
    }
    void Movement()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        if(!_playerController.isGrounded) //While player is in air, maintain SOME momentum but don't force full motion if input is released or lowered (more responsive).
        {       
           AerialMovement();
        }
        else
        {
            if(_isCrouching) //only apply this speed on the ground if crouching
            {
                _playerCurrentSpeed = _playerCrouchedSpeed;
            }
            else
            {
                _playerCurrentSpeed = _playerBaseSpeed;
            }

            if(_dashInProgress)
            {
                playerVelocity.x = previousMove.x;
            }
            else
            {
                playerDirection.x = _horizontalInput;
                playerVelocity = playerDirection *_playerCurrentSpeed;
            }
        } 
        Dash();
        _yVelocity = Jump(_yVelocity);
        playerVelocity.y = _yVelocity;
        DirectionCheck(playerVelocity.x);
        FaceDirectionMoved();
        _playerController.Move(playerVelocity * Time.deltaTime);
       
        if(_horizontalInput!=0 && OnSlope())
        {
            _playerController.Move(Vector3.down * _playerController.height /2 * playerSlopeForce *Time.deltaTime);
        }

        previousMove = playerVelocity;
    }

    void FaceDirectionMoved()
    {
        if(playerVelocity.x!=0)
        {
            if(playerVelocity.x<0)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3((-1*(playerVelocity.x/playerVelocity.x)), 0 , 0));
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(new Vector3((playerVelocity.x/playerVelocity.x), 0 , 0));
            }
        }
    }

    void AerialMovement()
    {
        if(_horizontalInput>0.1f && _horizontalInput<0.5f)
        {
            _horizontalInput=0.5f;
        }
        if(_horizontalInput<-0.1f && _horizontalInput> -0.5f)
        {
            _horizontalInput=-0.5f;
        }
        playerDirection.x = _horizontalInput;
        if((_wallJumpInProgress==true)
        ||(_dashInProgress==true)) 
        {
            playerVelocity = previousMove;
        }
        else 
        {
            playerVelocity = playerDirection * _playerCurrentSpeed;
        }
    }

    /*Function to toggle crouch while on the ground*/
    void CrouchControl()
    {   
        if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            if(_isCrouching)
            {
                bool canUncrouch = CheckIfCanUncrouch();
                if(canUncrouch)
                {
                    _playerController.height = 2;
                    _isCrouching = false;
                }     
            }
            else
            {
                _playerController.height = 1;
                _isCrouching = true;
            } 
        } 
    }


    /* Function used to end crouch when beginning some movement abilities like dash */
    IEnumerator EndCrouch()
    {
        yield return new WaitForSeconds(.08f); //this is a coroutine for animation purposes... maybe. No clue how to animate yet.
        if(_isCrouching)
        {
            _playerController.height = 2;
            _playerCurrentSpeed = _playerBaseSpeed;
            _isCrouching = false;
        }
    }

    //TODO: adjust movement so the player doesn't bounce going down diagonal surfaces
    float Jump(float y)
    {
        if(_playerController.isGrounded == true)
        {
            _isJumping=false;
            y = -1.0f; //Base Downward velocity to ensure collision with ground for isGrounded checks
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
            {
                if(_isCrouching)
                {   
                    bool canUncrouch = CheckIfCanUncrouch();
                    if(canUncrouch)
                    {
                        StartCoroutine(EndCrouch()); //Cancel crouch if jumping, but we'll allow crouching while airborne for now
                        y=0;
                        _isJumping=true;
                        y+=_jumpHeight;
                    }
                }
                else
                {
                    y=0;
                    _isJumping=true;
                    y+=_jumpHeight;
                }
            } 
        }
        else
        {
            if(y>_maxDownwardVelocity)
            {
                y += _playerGravity; /*if not grounded, always apply basic gravity (unless an action specifically overrides this)*/
            }
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
        return y;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {   
        WallCheck(hit);
        if(!_playerController.isGrounded)
        {
            //Debug.Log(hit.normal);
            if(hit.normal.y==-1.0f) //hits a ceiling
            {   
                _yVelocity= 3*_playerGravity;
            }
            
           
            if(hit.normal.y<0.1f)
            {    
                if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
                {
                    if(_wallJumpInProgress==false)
                    {   
                        WallJump(hit);
                    }
                }
            }
        }
        else
        {
            
        }
    }

    
    private void WallJump(ControllerColliderHit hit)
    {
        if(((hit.normal.x<0) && _horizontalInput>0) || ((hit.normal.x>0) && _horizontalInput<0))
            {   
                //Debug.DrawRay(hit.point, hit.normal, Color.green, 2.00f);
                _yVelocity = _jumpHeight - 2.0f;
                //Debug.Log(hit.normal);
                StartCoroutine(WallJumpOccurring());
                StartCoroutine(EndCrouch());
                playerVelocity= hit.normal * (_playerCurrentSpeed);
                transform.rotation = Quaternion.LookRotation(hit.normal);
            }
    }
    /* Coroutine that does continuous checks for the walljump delay period to see if another collision occurred or of the motion times out */
    IEnumerator WallJumpOccurring()
    {
        _wallJumpInProgress=true;
        _dashInProgress=false;
        _wallJumpLength = Time.time + _wallJumpDelay;
        //Debug.Log("Player:WallJump Activated");
        while(_wallJumpInProgress==true)
        {   
            yield return new WaitForSeconds(.05f);        
            if (Time.time>= _wallJumpLength)
            {
                _wallJumpInProgress=false;
                //Debug.Log("Player:WallJump Deactivated - Timed out");
            }
            
        }
        previousMove.x = _currentDirectionFaced* (0.5f * _playerCurrentSpeed);
        playerVelocity.x = _currentDirectionFaced* (0.5f * _playerCurrentSpeed);    
    }

    void Dash()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            if(_isDashOnCooldown==false)
            {   
                if(!_wallJumpInProgress)
                {   if(_isCrouching) 
                    {   
                        bool canUncrouch = CheckIfCanUncrouch();
                        if(canUncrouch)
                        {
                            StartCoroutine(EndCrouch());
                            playerVelocity.x= _currentDirectionFaced * _dashImpulse;
                            playerVelocity.y= 0;
                            previousMove = playerVelocity;
                            StartCoroutine(DashOccurring());
                        }
                    }
                    else
                    {
                        playerVelocity.x= _currentDirectionFaced * _dashImpulse;
                        playerVelocity.y= 0;
                        previousMove = playerVelocity;
                        StartCoroutine(DashOccurring());
                    }
                }
            }
        }
    }

    IEnumerator DashOccurring()
    {
        _dashInProgress=true;
        _wallJumpInProgress=false;
        _dashLength = Time.time + _dashDelay;
        _playerGravity = 0.0f;
        while(_dashInProgress==true)
        {
            _yVelocity=0;
            yield return new WaitForSeconds(.05f);
            if (Time.time>= _dashLength)
            {
                _dashInProgress=false;
                //Debug.Log("Player:Dash Deactivated - Timed out");
            }
        }
        _playerGravity = -0.5f;
        previousMove.x = _currentDirectionFaced* (0.5f * _playerCurrentSpeed);
        playerVelocity.x = _currentDirectionFaced* (0.5f * _playerCurrentSpeed);
        StartCoroutine(DashCooldown());
    }

    IEnumerator DashCooldown()
    {
        _isDashOnCooldown = true;
        _uiManager.DashAvailability(_isDashOnCooldown);
        yield return new WaitForSeconds(_dashCooldown);
        _isDashOnCooldown = false;
        _uiManager.DashAvailability(_isDashOnCooldown);
    }

    /***  START: Check Functions ***************/
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

    private void WallCheck(ControllerColliderHit potentialWall)
    {
            if(potentialWall.normal.x==-1.0f || potentialWall.normal.x == 1.0f)
            {
                _onWall=true;
                if(_dashInProgress)
                {
                    _dashInProgress=false; //Debug.Log("Player:Dash Deactivated - Wall hit during motion");
                }
                if(_wallJumpInProgress)
                {    
                        
                     _wallJumpInProgress=false; //Debug.Log("Player:Walljump Deactivated - Wall hit during motion");
                        
                }
            }
            else
            {
                _onWall=false;
            }
    }
    bool CheckIfCanUncrouch()
    {
        RaycastHit hit;
        if(Physics.Raycast(_playerController.bounds.center, Vector3.up , out hit, _playerController.bounds.extents.y + 1.0f))
        {
            //Debug.DrawRay(_playerController.bounds.center, Vector3.up * hit.distance, Color.yellow);
            return false;
        }
        else
        {
            return true;
        }
    }

    /* Credit for this function's general structure goes to Youtube user: Acacia Developer. Video used is https://www.youtube.com/watch?v=b7bmNDdYPzU */
    bool OnSlope()
    {
        if(_isJumping)
        {
            return false;
        }

        RaycastHit hit;
        if(Physics.Raycast(_playerController.bounds.center, Vector3.down, out hit, _playerController.bounds.extents.y + slopeCheckLength))
        {
            if(hit.normal != Vector3.up)
            {
                //Debug.Log("OnSlope");
                return true;
            }
        }
        
        return false;
        
    }

    /*****  END: Check Functions ***************/

    /* *** Deprecated code: Character Controller's inherently can check if grounded
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
    } */
   
    /* *** Deprecated code: Not stable to use raycasts for these collision checks.
    private void CheckIfOnWall()
    {
        float distBuffer = .08f;
        if(!_playerController.isGrounded)
        {
            Debug.Log("In Air: Checking walls");
            if ((Physics.Raycast(_playerController.bounds.center, Vector3.left, _playerController.bounds.extents.x + distBuffer))
            || (Physics.Raycast(_playerController.bounds.center, Vector3.right, _playerController.bounds.extents.x + distBuffer)))
            {
                _onWall=true;
                Debug.Log("On Wall");
            }
            else
            {
                _onWall=false;
            }
        }
        else
        {
            _onWall=false;
        }
        
    }*/

    

}

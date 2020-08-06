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
    private bool _inRangeOfPole = false;
    private bool _onPole = false;
    private Collider _poleDimensions;
    private Vector3 _posToGrabPole;
    private RaycastHit _poleContact;
    private Vector3[] _poleBounds;
    private Vector3 _poleDirectionVector;
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
    - Consolidate related boolean checks into their own functions, e.g. stuff that makes crouch checks or aerial checks etc
    */

    
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
    
    void Update()
    {  
        if(_onPole==false)
        {   
            CrouchControl(); 
            Movement();
        }
        else
        {
            LadderAndPoleMovement();
        }
    }

    void LateUpdate() 
    {  
        if(_onPole==false)
        {
            transform.position= new Vector3(transform.position.x, transform.position.y , 0.0f); //**Keep player from moving on undesired z-axis     
        }
    }

    void Movement()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        if(_playerController.isGrounded) //**While player is in air, maintain SOME momentum but don't force full motion if input is released or lowered (more responsive).
        {       
            GroundSpeedControl();
            Jump();
        }
        else
        {
            AerialMovement();
            Falling();
        } 
        Dash();
        playerVelocity.y = _yVelocity;
        DirectionCheck(playerVelocity.x);
        FaceDirectionMoved();
        _playerController.Move(playerVelocity * Time.deltaTime);
        if(_horizontalInput!=0 && OnSlope()) //**slope behavior control
        {
            _playerController.Move(Vector3.down * _playerController.height /2 * playerSlopeForce *Time.deltaTime);
        }

        previousMove = playerVelocity;
        LadderAndPoleGrab(); //**This is at the end because it splits actions off. If initiated, player can no longer fall/jump/move the same way.

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

    void GroundSpeedControl()
    {
        if(_isCrouching) //**only apply this speed on the ground if crouching
        {
            _playerCurrentSpeed = _playerCrouchedSpeed;
        }
        else
        {
            _playerCurrentSpeed = _playerBaseSpeed;
        }

        if(_dashInProgress)//**This is here since the dash moves based on a coroutine. Needs to keep updating the velocity for a set time.
        {
            playerVelocity.x = previousMove.x;
        }
        else
        {
            playerDirection.x = _horizontalInput;
            playerVelocity = playerDirection *_playerCurrentSpeed;
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
                if(CheckIfCanUncrouch()==true)
                {
                    _playerController.height = 1.5f;
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
        yield return new WaitForSeconds(.08f); //**this is a coroutine for animation purposes... maybe. No clue how to animate yet.
        if(_isCrouching)
        {
            _playerController.height = 2;
            _playerCurrentSpeed = _playerBaseSpeed;
            _isCrouching = false;
        }
    }

    void Jump()
    {
        _isJumping=false;
        _yVelocity = -1.0f; //**Base Downward velocity to ensure collision with ground for isGrounded checks
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            if(_isCrouching)
            {   
                if(CheckIfCanUncrouch()==true)
                {
                    StartCoroutine(EndCrouch()); //**Cancel crouch if jumping, but we'll allow crouching while airborne for now
                    _yVelocity=0;
                    _isJumping=true;
                    _yVelocity+=_jumpHeight;
                }
            }
            else
            {
                _yVelocity=0;
                _isJumping=true;
                _yVelocity+=_jumpHeight;
            }
        }  
    }


    void Falling()//**Activate when aerial
    {
        if(_yVelocity<0)
        {
            _isJumping=false;
        }
        if(_isJumping)
        {
            if(Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Joystick1Button0))
            {
                _yVelocity=2;
                _isJumping=false;
                //StartCoroutine(EndJumpBuffer());
            }
        }
        if(_yVelocity>_maxDownwardVelocity)
        {
            _yVelocity += _playerGravity; /**if not grounded, always apply basic gravity (unless an action specifically overrides this)*/
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
        }       
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {   
        WallCheck(hit);
        if(!_playerController.isGrounded)
        {
            //Debug.Log(hit.normal);
            if(hit.normal.y==-1.0f) //**hits a ceiling
            {   
                _yVelocity= 3*_playerGravity;
            }
            
           
            if(hit.normal.y<0.1f) //**hits a potential wall
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

    /** Coroutine that does continuous checks for the walljump delay period to see if another collision occurred or of the motion times out */
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
                        if(CheckIfCanUncrouch()==true)
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
        if(Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.Joystick1Button5))
        {
            _dashInProgress=false;
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
            return false;
        }
        return true;
    }

    /** Credit for this function's general structure goes to Youtube user: Acacia Developer. Video used is https://www.youtube.com/watch?v=b7bmNDdYPzU */
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
                return true;
            }
        }
        return false;    
    }

    /*****  END: Check Functions ***************/


    /***** START: Enemy Interactions ************/
    public void PlayerTakeDamage(float damage)
    {

    }
    /*****   END: Enemy Interactions ************/

    /***** START: Poles/Pipes/Environment specific movement *****/
    public void InPoleRange(Vector3 poleTransformPos , Vector3 poleDirectionVector, Vector3[] poleBounds)
    {
        Vector3 directionToPole = (Vector3.forward);
        RaycastHit hit;
        if(Physics.Raycast(_playerController.bounds.center, directionToPole, out hit, 2.0f))
        {
            _inRangeOfPole=true;
            if(hit.collider.gameObject.tag =="Pole" && hit.collider is BoxCollider)
            {
                _poleContact=hit;
                _posToGrabPole=(hit.point);
                _poleDirectionVector = poleDirectionVector;
                _poleBounds = poleBounds;
                _posToGrabPole.z -= 0.2f;
            }
            //**Get the normal and point of contact. get tangent vector for direction.
        }
    }

    public void ExitPoleRange()
    {
        Debug.Log("Exiting PoleRange");
        _inRangeOfPole=false;
        _poleDimensions = null;
        _posToGrabPole= new Vector3(0,0,0);
    }

    //TODO: Make transition from air to the pole more natural, and not a teleport.
    void LadderAndPoleGrab()
    {
        if(_inRangeOfPole&&Input.GetKey(KeyCode.E) || _inRangeOfPole&&Input.GetKey(KeyCode.Joystick1Button1))
        {
            if(_posToGrabPole!= new Vector3(0,0,0))
            {   
                transform.rotation = Quaternion.LookRotation(new Vector3(0, 0 , 1));
                _currentDirectionFaced=0;//**Opposite because movement effectively would be on the side back is facing 
                _onPole=true;
                previousMove.x = 0f; 
                transform.position = _posToGrabPole;
            }
        }
    }
    //TODO: Allow for diagonal movement on a pole. maybe get vector from bottom to top of collider and move along that unit vector?
    void LadderAndPoleMovement()
    {
       
        _horizontalInput = Input.GetAxis("Horizontal");
        float _verticalInput = Input.GetAxis("Vertical");

        if(_horizontalInput>0)
        {
            _currentDirectionFaced= -1;
        }
        else
        {
            _currentDirectionFaced= 1;
        }
        float poleClimbSpeed=4.0f;
        playerVelocity = _verticalInput * (poleClimbSpeed*_poleDirectionVector);

        _playerController.Move(playerVelocity * Time.deltaTime);
        if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            JumpOffPole();
        }
        BoundsOnPole();  
    }

    void BoundsOnPole()
    {
        if(_poleDirectionVector.x==0)
        {
            if(transform.position.y<_poleBounds[0].y)
            {
                Vector3 tempLower = _poleBounds[0];
                tempLower.z -= 0.5f;
                transform.position = tempLower;
            }
            if(transform.position.y>_poleBounds[1].y)
            {
                Vector3 tempUpper = _poleBounds[1];
                tempUpper.z -= 0.5f;
                transform.position = tempUpper;
            }
        }
        else if(_poleDirectionVector.x>0)
        {
            if(transform.position.x<_poleBounds[0].x ||transform.position.y<_poleBounds[0].y)
            {
                Vector3 tempLower = _poleBounds[0];
                tempLower.z -= 0.5f;
                transform.position = tempLower;
            }
            if(transform.position.x>_poleBounds[1].x ||transform.position.y>_poleBounds[1].y)
            {
                Vector3 tempUpper = _poleBounds[1];
                tempUpper.z -= 0.5f;
                transform.position = tempUpper;
            }
        }
        else if(_poleDirectionVector.x<0)
        {
             if(transform.position.x>_poleBounds[0].x ||transform.position.y<_poleBounds[0].y)
            {
                Vector3 tempLower = _poleBounds[0];
                tempLower.z -= 0.5f;
                transform.position = tempLower;
            }
            if(transform.position.x<_poleBounds[1].x ||transform.position.y>_poleBounds[1].y)
            {
                Vector3 tempUpper = _poleBounds[1];
                tempUpper.z -= 0.5f;
                transform.position = tempUpper;
            }
        }
    }


    void JumpOffPole()
    {
        if(_playerController.isGrounded)
        {
            _onPole=false;
            ExitPoleRange();
        }
        else
        {
            _yVelocity=(_jumpHeight/1.1f);
            previousMove.x = _currentDirectionFaced * -4.5f; //**if the player has no input on exiting pole, previous move dictates x-motion (for maintaining momentum)
            _onPole=false;
            ExitPoleRange();
        }
    }


    /*****   END: Poles/Pipes/Environment specific movement *****/

}

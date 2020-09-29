using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
*Simple platform manager that can be attached to a platform object and used in unity editor to designate a specific direction using degrees to move in
*Allows a speed for the platform to also be specified in the unity editor
*Accounts for the playercontroller being on the platform and makes it move with itself smoothly.
*This requires the collision box of this manager to rise about a quarter to one half the player colliders height above its surface
*/
public class Platform_Manager : MonoBehaviour
{
    private GameObject _player;
    private bool _playerOnPlatform = false;
    [SerializeField]
    private GameObject _parentObject;
    [SerializeField]
    private float _platDirectionDegrees=0; //This is a float value that is converted to radians and then cartesian coordinates for movement.
    private Vector3 _platBaseCartesianDirection;
    private Vector3 _currentCartesianDirection;
    [SerializeField]
    private float _platSpeed = 1.0f;
    
    [SerializeField]
    private float _alternationRate = 5.0f;
    private float _alternationSwitchTime;
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag("Player");
        if(_player==null)
        {
            Debug.LogError("Platform:PlayerController NOT FOUND");
        }
        if(_parentObject==null)
        {
            Debug.LogError("Platform:ParentPlatform NOT FOUND");
        }

        /*Setting the degree direction specified to cartesian coordinates for platform movement.*/
        float directionInRadians = _platDirectionDegrees * ( Mathf.PI / 180.0f);
        float cartX_direction = Mathf.Cos(directionInRadians);
        float cartY_direction = Mathf.Sin(directionInRadians);
        _platBaseCartesianDirection.x = cartX_direction; _platBaseCartesianDirection.y = cartY_direction; _platBaseCartesianDirection.z = 0;

        _currentCartesianDirection = _platBaseCartesianDirection;   
        _alternationSwitchTime = Time.time + _alternationRate; //set first direction switch
    }

    // Update is called once per frame
    void Update()
    {
        AlternateDirections();
       _parentObject.transform.Translate(_currentCartesianDirection * _platSpeed * Time.deltaTime);
    }

    // LateUpdate called at the end of each frame.
    void LateUpdate() 
    {
        MovePlayerIfOnPlatform();
    }

    /*
    * Simple function that switches the platform to the opposite direction based on its set alternation rate
    */
    private void AlternateDirections()
    {
        if(Time.time>_alternationSwitchTime)
        {
            _alternationSwitchTime = Time.time + _alternationRate;
            _currentCartesianDirection *= -1;
        }
    }

    /*
    *Function that handles moving the player alongside the platform when they are registered as on board.
    */
    private void MovePlayerIfOnPlatform()
    {
        if(_playerOnPlatform)
        {
            Vector3 playerPlatMovement = new Vector3(0,0,0);
            if(_currentCartesianDirection.y>0)
            {
                playerPlatMovement+=_currentCartesianDirection*_platSpeed;
                playerPlatMovement.y = -0.5f; //While the player is moving up, needs slight downward movement to register as "grounded"
                _player.GetComponent<CharacterController>().Move(playerPlatMovement*Time.deltaTime);
            }
            else
            {
                playerPlatMovement+=_currentCartesianDirection*_platSpeed;
                playerPlatMovement.y = -2*(_platSpeed);// while moving down on plat, needs some extra downward motion to stay grounded
                _player.GetComponent<CharacterController>().Move(playerPlatMovement*Time.deltaTime);
            }
            
        }
    }

    
    private void OnTriggerStay(Collider other) 
    {
        if(other.gameObject.tag=="Player")
        {
            //Debug.Log("Player on Platform");
            _playerOnPlatform = true;
        }
    }


    private void OnTriggerExit(Collider other) 
    {
        if(other.gameObject.tag=="Player")
        {
            //Debug.Log("Player off Platform")
            _playerOnPlatform = false;
            
        }
    }
    
}

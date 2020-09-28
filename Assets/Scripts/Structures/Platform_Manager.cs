using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
*Simple platform manager that can be attached to a platform object and used in unity editor to designate a specific direction using degrees to move in
*Allows a speed for the platform to also be specified in the unity editor
*TODO:Accounts for the playercontroller being on the platform and makes it move with itself smoothly.
*/
public class Platform_Manager : MonoBehaviour
{
    private GameObject _player;
    private bool _playerOnPlatform = false;
    private Vector3 _playerMovementOffset = new Vector3(0,0,0);
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
        float directionInRadians = _platDirectionDegrees * ( Mathf.PI / ((float)180.0));
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

    void LateUpdate() 
    {
        //movePlayerIfOnPlatform();
    }

    private void AlternateDirections()
    {
        if(Time.time>_alternationSwitchTime)
        {
            _alternationSwitchTime = Time.time + _alternationRate;
            _currentCartesianDirection *= -1;
        }
    }

    private void movePlayerIfOnPlatform()
    {
        if(_playerOnPlatform)
        {
            _player.transform.position = _parentObject.transform.position+_playerMovementOffset;
        }
    }

    private void OnTriggerStay(Collider other) 
    {
        if(other.gameObject.tag=="Player")
        {
            //Debug.Log("Player on Platform");
            _playerMovementOffset = _player.transform.position - _parentObject.transform.position;
            _playerOnPlatform = true;
            _player.GetComponent<PlayerController>().SetVerticalWhileOnPlatform(_platSpeed);
        }
    }


    private void OnTriggerExit(Collider other) 
    {
        if(other.gameObject.tag=="Player")
        {
            //Debug.Log("Player off Platform")
            _playerOnPlatform = false;
            _player.GetComponent<PlayerController>().ResetVertical();
        }
    }
    
}

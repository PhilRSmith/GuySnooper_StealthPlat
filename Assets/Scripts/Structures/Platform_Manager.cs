using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_Manager : MonoBehaviour
{
    private GameObject _player;
    [SerializeField]
    private GameObject _parentObject;
    private Vector3 _platDirection;
    private float _platSpeed = 1.0f;
    /* Directions for Switch
            1
        4       2
            3

    */
    private int _directionSwitch;
    private bool _goingUp = false;
    private bool _goingRight = false;
    private bool _goingDown = false;
    private bool _goingLeft = false;
    [SerializeField]
    private float _alternationRate = 5.0f;
    private float _alternationSwitchTime;
    // Start is called before the first frame update
    void Start()
    {
        _directionSwitch = 1; //Initial direction
        _player = GameObject.FindWithTag("Player");
        if(_player==null)
        {
            Debug.LogError("Platform:PlayerController NOT FOUND");
        }
        if(_parentObject==null)
        {
            Debug.LogError("Platform:ParentPlatform NOT FOUND");
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(_directionSwitch)
        {
            case 1:
                if(_goingUp==false)
                {
                    _platDirection = new Vector3 (0, 1, 0);
                    _goingUp = true;
                    _alternationSwitchTime = Time.time + _alternationRate;
                    //Debug.Log("Current Time: " + Time.time + " || Time Until switch: "+ _alternationSwitchTime);
                    
                }
                break;
            case 2:
                if(_goingRight==false)
                {
                    _platDirection = new Vector3 (1, 0, 0);
                    _goingRight = true;
                    _alternationSwitchTime = Time.time + _alternationRate;
                    
                }
                break;
            case 3:
                if(_goingDown==false)
                {
                    _platDirection = new Vector3 (0, -1, 0);
                    _goingDown = true;
                    _alternationSwitchTime = Time.time + _alternationRate;
                    //Debug.Log("Current Time: " + Time.time + " || Time Until switch: "+ _alternationSwitchTime);
                    
                }
                break;
            case 4:
                if(_goingLeft==false)
                {
                    _platDirection = new Vector3 (-1, 0, 0);
                    _goingLeft = true;
                    _alternationSwitchTime = Time.time + _alternationRate;
                    
                }
                break;
        }
        /*Simple Platform Movement for testing*/
        AlternateUpAndDown();
        _parentObject.transform.Translate(_platDirection*_platSpeed*Time.deltaTime);
    }

    private void AlternateUpAndDown()
    {
        if(Time.time>_alternationSwitchTime)
        {
            if(_goingUp)
            {
                _directionSwitch = 3;
                _goingUp = false;
            }
            if(_goingDown)
            {
                _directionSwitch = 1;
                _goingDown = false;
            }
        }
    }

    private void AlternateRightAndLeft()
    {
        if(Time.time>_alternationSwitchTime)
        {
            if(_goingRight)
            {
                _directionSwitch = 4;
                _goingRight = false;
            }
            if(_goingLeft)
            {
                _directionSwitch = 2;
                _goingLeft= false;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag=="Player")
        {
            //Debug.Log("Player on Platform");
            _player.transform.parent = _parentObject.transform;
            _player.GetComponent<PlayerController>().SetVerticalWhileOnPlatform(_platSpeed);
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if(other.gameObject.tag=="Player")
        {
            //Debug.Log("Player off Platform");
            _player.transform.parent = null;
            _player.GetComponent<PlayerController>().ResetVertical();
        }
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformA : MonoBehaviour
{
    private PlayerController _player;
    private Vector3 _platDirection;
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
        _directionSwitch = 1; //going up first
        _player = GameObject.Find("ProtoCat_Controller").GetComponent<PlayerController>();
         if(_player==null)
        {
            Debug.LogError("Platform:PlayerController NOT FOUND");
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
        transform.Translate(_platDirection*Time.deltaTime);
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
    
    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag=="Player")
        {
            Debug.Log("Player on Platform");
            _player.transform.parent = transform;
        }
    }
    
}

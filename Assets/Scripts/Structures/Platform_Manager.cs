using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_Manager : MonoBehaviour
{
    private GameObject _player;
    [SerializeField]
    private GameObject _parentObject;
    [SerializeField]
    private Vector3 _platDirection;
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
    }

    // Update is called once per frame
    void Update()
    {

       
    }


    private void AlternateDirections(string stringForDirection)
    {
        if(stringForDirection.Equals("upAndDown", System.StringComparison.OrdinalIgnoreCase))
        {
            AlternateUpAndDown();
        }

        if(stringForDirection.Equals("rightAndLeft", System.StringComparison.OrdinalIgnoreCase))
        {
            AlternateUpAndDown();
        }
    }
    private void AlternateUpAndDown()
    {
        if(Time.time>_alternationSwitchTime)
        {
           
        }
    }

    private void AlternateRightAndLeft()
    {
        if(Time.time>_alternationSwitchTime)
        {
           
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

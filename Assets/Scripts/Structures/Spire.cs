using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spire : MonoBehaviour
{
     private PlayerController _player;
    private SphereCollider _spireSphereCollider;
    private Transform _spireTip;
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.Find("ProtoCat_Controller").GetComponent<PlayerController>();
        if(_player==null)
        {
            Debug.LogError("Spire:PlayerController NOT FOUND");
        }
        _spireSphereCollider = GetComponent<SphereCollider>();
        if(_spireSphereCollider==null)
        {
            Debug.LogError("Spire:SphereCollider NOT FOUND");
        }
        _spireTip = this.gameObject.transform.Find("SpireTip");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            if(_player)
            {
                //Debug.Log("Spire:Collider - Player entered collider");
                _player.InSpireRange(_spireTip.position);
            }
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            if(_player)
            {
                _player.InSpireRange(_spireTip.position);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            if(_player)
            {
                //Debug.Log("Spire:Collider - Player exited collider");
                _player.ExitSpireRange();
            }
        }
    }
}

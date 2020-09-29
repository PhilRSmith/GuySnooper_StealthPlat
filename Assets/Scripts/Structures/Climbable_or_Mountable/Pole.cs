using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    private PlayerController _player;
    private CapsuleCollider _poleCapCollider;
    private BoxCollider _poleBoxCollider;
    private Vector3 _unitDirectionVector;
    private Vector3[] _poleBounds;
    
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        if(_player==null)
        {
            Debug.LogError("Pole:PlayerController NOT FOUND");
        }
        _poleCapCollider = GetComponent<CapsuleCollider>();
        if(_poleCapCollider==null)
        {
            Debug.LogError("Pole:CapsuleCollider NOT FOUND");
        }
        _poleBoxCollider = GetComponent<BoxCollider>();
        if(_poleBoxCollider==null)
        {
            Debug.LogError("Pole:BoxCollider NOT FOUND");
        }

        _unitDirectionVector = DirectionOfPole();
        _poleBounds = PoleBounds();
    }

    Vector3 DirectionOfPole()
    {
        Vector3 directionVector, poleBase, poleTop;
        poleBase = this.gameObject.transform.Find("Base").position;
        poleTop = this.gameObject.transform.Find("Top").position;
        directionVector = poleTop - poleBase;
        float denominatorScalar = Mathf.Sqrt((directionVector.x*directionVector.x) + (directionVector.y*directionVector.y) + (directionVector.z*directionVector.z));
        directionVector = directionVector/denominatorScalar;
        return directionVector;
    }

    Vector3[] PoleBounds()
    {
        Vector3[] bounds = {this.gameObject.transform.Find("Base").position, this.gameObject.transform.Find("Top").position};
        return bounds;
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
                //Debug.Log("Pole:Collider - Player entered collider");
                _player.InPoleRange(transform.position , _unitDirectionVector, _poleBounds);
            }
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            if(_player)
            {
                _player.InPoleRange(transform.position, _unitDirectionVector, _poleBounds);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            if(_player)
            {
                //Debug.Log("Pole:Collider - Player exited collider");
                _player.ExitPoleRange();
            }
        }
    }

}

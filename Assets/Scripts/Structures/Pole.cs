using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    private PlayerController _player;
    private CapsuleCollider _poleCapCollider;
    private BoxCollider _poleBoxCollider;
    /*
    NOTES: Need a way to have a player enter the collider as a trigger, and by being aerial and pressing B or O or whatever the key is,
    Player will lock position to the pole and be able to traverse vertically up and down it. 
    When locking to position, the Z-Transform.position lock needs to be temporarily released.
    Need to establish foreground, background, and default layers on the field.
    If the Player turns (left or right movement), the player moves to that side of the pole.
    If the player jumps off the pole (the pole must be in one of the 3 layers), they stay in the layer, and move in the direction they held or previously moved in.
    */
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.Find("ProtoCat_Controller").GetComponent<PlayerController>();
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
                Debug.Log("Pole:Collider - Player entered collider");
                _player.InPoleRange(_poleBoxCollider.bounds.size, transform.position);
                //Need to have some sort of public function on player that allows them to be locked onto the pole until they jump.
                //Maybe flip a boolean.
                //Need a way to send the pole's dimensions/position to the player so that they can move on it?
                
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            if(_player)
            {
                Debug.Log("Pole:Collider - Player exited collider");
                _player.ExitPoleRange();
                //Need to have some sort of public function on player that disengages them from the pole.
                //Maybe flip a boolean.
            }
        }
    }
}

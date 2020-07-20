using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /*** START: Player Movement/Gravity Variables ***/
    [SerializeField]
    private float _playerSpeed=3.0f;
    [SerializeField]
    private float _playerGravity=1.0f;
    [SerializeField]
    private float _jumpHeight = 15.0f;
    [SerializeField]
    private float _yVelocity=0.0f;
    private float _horizontalInput;
    private CharacterController _playerController;
    /***   END: Player Movement/Gravity Variables ***/

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<CharacterController>();
        if (_playerController == null)
        {
            Debug.LogError("Player:PlayerController DOES NOT EXIST");
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        Vector3 playerDirection = new Vector3(_horizontalInput, _yVelocity , 0.0f);
        Vector3 playerVelocity = playerDirection * _playerSpeed;
        
        /* Jumping behavior START */
        if(_playerController.isGrounded == true)
        {
            _yVelocity= -.5f;
            if(Input.GetKeyDown(KeyCode.Space))
            {
                _yVelocity+=_jumpHeight;
            }
        }
        else
        {
            _yVelocity -= _playerGravity;
        }
        /* Jumping behavior END */
        playerVelocity.y = _yVelocity;

        _playerController.Move(playerVelocity * Time.deltaTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    [SerializeField]
    private float _worldGravity = -10.0f;
    // Start is called before the first frame update
    void Start()
    {
        Physics.gravity = new Vector3 (0.0f, _worldGravity, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

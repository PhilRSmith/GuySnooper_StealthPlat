using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformA : MonoBehaviour
{
    private GameObject target = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*Simple Platform Movement for testing*/
        Vector3 platDirection = new Vector3 (-1, 0, 0);
        transform.Translate(platDirection*Time.deltaTime);
    }
}

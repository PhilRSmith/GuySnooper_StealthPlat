using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField]
    private Transform _targetPlayer;
    [SerializeField]
    private Vector3 _cameraDistance = new Vector3(0, 2, -8);
    public float smoothingSpeed=0.2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        CameraSnap();
    }

    void CameraSnap()
    {
        Vector3 desiredPosition = _targetPlayer.position + _cameraDistance;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothingSpeed);
        transform.position = smoothedPosition;
        
    }

    /* Note to self, remove y tracking later, then create callable methods to adjust camera height based on level or player action*/
}

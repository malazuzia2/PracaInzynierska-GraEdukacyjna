using UnityEngine;
using UnityEngine.EventSystems; // Potrzebne do sprawdzania, czy kursor jest nad UI

public class SplitScreenCameraController : MonoBehaviour
{ 
    public Camera playerCamera;
    public Camera referenceCamera;
    public Transform playerTarget;
    public Transform referenceTarget;
     
    public float distance = 10.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = 5f;
    public float distanceMax = 20f;
    public float zoomSpeed = 5f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = playerCamera.transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (playerCamera && referenceCamera && playerTarget && referenceTarget)
        { 
            if (Input.GetMouseButton(0))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            } 
            if (!EventSystem.current.IsPointerOverGameObject())
            { 
                distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
                distance = Mathf.Clamp(distance, distanceMin, distanceMax);
            }
             
            Quaternion rotation = Quaternion.Euler(y, x, 0);
             
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 playerPosition = rotation * negDistance + playerTarget.position;
            playerCamera.transform.rotation = rotation;
            playerCamera.transform.position = playerPosition;
             
            Vector3 referencePosition = rotation * negDistance + referenceTarget.position;
            referenceCamera.transform.rotation = rotation;
            referenceCamera.transform.position = referencePosition;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
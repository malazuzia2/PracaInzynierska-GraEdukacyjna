using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    // The target object we want to orbit around.
    public Transform target;

    // Initial distance from the target.
    public float distance = 10.0f;

    // Mouse sensitivity for rotation.
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    // Limits for vertical rotation (to prevent flipping over).
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    // Limits for zooming in and out.
    public float distanceMin = 5f;
    public float distanceMax = 20f;
    public float zoomSpeed = 5f;

    // Internal variables to store the current rotation angles.
    private float x = 0.0f;
    private float y = 0.0f;

    // This is called once at the start.
    void Start()
    {
        // Get the initial Euler angles of the camera.
        Vector3 angles = transform.eulerAngles;
        x = angles.y; // Horizontal angle
        y = angles.x; // Vertical angle
    }

    // LateUpdate is called after all Update functions have been called.
    // This is the best place for camera logic to avoid jittery movement.
    void LateUpdate()
    {
        // Ensure we have a target to orbit.
        if (target)
        {
            // --- Handle Rotation Input ---
            // We only rotate if the left mouse button is held down (button 0).
            if (Input.GetMouseButton(0)) // <--- THIS IS THE ONLY CHANGE
            {
                // Get mouse movement from Unity's Input Manager.
                x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime; // Note the subtraction for y

                // Clamp the vertical angle to the defined limits.
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            // --- Handle Zoom Input ---
            // Get scroll wheel input and adjust the distance.
            distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            distance = Mathf.Clamp(distance, distanceMin, distanceMax);

            // --- Calculate Camera Position and Rotation ---
            // Convert our angles into a rotation.
            Quaternion rotation = Quaternion.Euler(y, x, 0);

            // Calculate the desired position of the camera.
            // Start with a vector pointing backwards from the origin, then rotate it.
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            // --- Apply the new Position and Rotation ---
            transform.rotation = rotation;
            transform.position = position;
        }
    }

    // Helper function to clamp an angle between a min and max value.
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
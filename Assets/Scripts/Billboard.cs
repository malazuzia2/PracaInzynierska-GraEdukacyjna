using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    { 
        mainCamera = Camera.main;
    }
     
    void LateUpdate()
    { 
        if (mainCamera == null)
        {
             mainCamera = Camera.main;
            if (mainCamera == null) return; 
        }
         
        transform.rotation = mainCamera.transform.rotation;
    }
}
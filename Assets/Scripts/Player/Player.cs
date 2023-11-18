using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float minX = -1.5f;
    private float maxX = 1.5f;
    public static Vector3 startPosition;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FollowingMouse();
    }

    public void FollowingMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        float distanceFromCamera = 10f;
        mousePosition.z = distanceFromCamera;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        float clampX = Mathf.Clamp(worldPosition.x, minX, maxX);
        transform.position = new Vector3(clampX, transform.position.y, 0);
        startPosition = transform.position;
    }
   
}

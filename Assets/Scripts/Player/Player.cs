using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFollowingMouse = true;
    [SerializeField] private Transform player;
    [SerializeField] private float minX = -1.5f;
    [SerializeField] private float maxX = 1.5f;
    private Vector3 startPosition = new Vector3(0, 2, 0);
    //[SerializeField] private GameObject Object2;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        player.position = startPosition;
    }

    // Update is called once per frame
    void Update()
    {
        FollowingMouse();
        if (Input.GetMouseButtonDown(0))
        {
            DropOnClicked();
        }
    }

    public void FollowingMouse()
    {
        if (isFollowingMouse)
        {
            Vector3 mousePosition = Input.mousePosition;

            float distanceFromCamera = 10f;

            mousePosition.z = distanceFromCamera;

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            float clampX = Mathf.Clamp(worldPosition.x, minX, maxX);
            transform.position = new Vector3(clampX, transform.position.y, 0);
        }
    }
    public void DropOnClicked()
    {
        isFollowingMouse = false;
        rb.gravityScale = 1;
    }


}

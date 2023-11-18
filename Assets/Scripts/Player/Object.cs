using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool inTheAir = true;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (transform.position.y < 3f)
        {
            inTheAir = false;
            rb.gravityScale = 1;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        Drop();
    }

    public void Drop()
    {
        if (inTheAir == true)
        {
            GetComponent<Transform>().position = Player.startPosition;
        }

        if (Input.GetMouseButtonDown(0))
        {
            rb.gravityScale = 1;
            inTheAir = false;
            SpawnObjects.spawnedYet = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == gameObject.tag)
        {
            SpawnObjects.spawnPosition = transform.position;
            SpawnObjects.newObjectSpawned = true;
            SpawnObjects.whichObject = int.Parse(gameObject.tag);
            Destroy(gameObject);
        }
    }
}

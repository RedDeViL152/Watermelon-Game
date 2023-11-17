using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    private Vector3 startPosition = new Vector3(0, 2, 0);
    [SerializeField] private GameObject[] players;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateNewObject();
        }
    }

    public void CreateNewObject()
    {
        Instantiate(players[Random.Range(0, 3)], startPosition, Quaternion.identity);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    public static Vector3 spawnPosition;
    public static bool newObjectSpawned = false;
    public static bool spawnedYet = false;
    public static int whichObject = 0;
    
    [SerializeField] private Transform[] objects;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateNewObjects();
        }
        ReplaceFruit();
    }

    public void CreateNewObjects()
    {
        if (spawnedYet == false)
        {
            StartCoroutine(SpawnTimer());
            spawnedYet = true;
        }
    }

    public void ReplaceFruit()
    {
        
        if(newObjectSpawned == true)
        {
            newObjectSpawned = false;
            Instantiate(objects[whichObject + 1], spawnPosition, Quaternion.identity);
        }
    }

    IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(.75f);
        Instantiate(objects[Random.Range(0, 2)], transform.position, Quaternion.identity);
        
    }
}

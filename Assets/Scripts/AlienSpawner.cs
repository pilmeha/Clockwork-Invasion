using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject alien;

    private void Start()
    {
        InvokeRepeating("SpawnAlien", 2.0f, 5.0f);
    }

    void SpawnAlien()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        GameObject _Alien = Instantiate(alien, spawnPoints[spawnIndex].position, Quaternion.identity);
    }
}

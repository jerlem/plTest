using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerManager : MonoBehaviour, IDisposable
{
    private const float PassengerMinSpeed = 1.0f;
    private const float PassengerMaxSpeed = 2.0f;
    private const float SpawnRate = 2.0f;

    public bool Spawning = false;

    private float spawnDelay = 2f;
    private float nextSpeed = 0f;

    private GameObject passengerPrefab;
    private GameObject passengerContainer;

    public List<GameObject> Passengers = new();
    public List<Passenger> PassengersBehaviour = new();

    private void Awake()
    {
        passengerContainer = GameObject.Find("PassengerSpawner");

        if (passengerContainer == null)
            Debug.LogError("No Passenger spawner GameObject");

        //cached game object
        passengerPrefab = GameObject.Find("PR_Passenger");
    }

    private void Update()
    {
        if (!Spawning)
            return;

        Debug.Log("spawn in " + spawnDelay);

        spawnDelay -= Time.deltaTime;

        if (spawnDelay <= 0)
        {
            nextSpeed = UnityEngine.Random.Range(PassengerMinSpeed, PassengerMaxSpeed);
            spawnDelay = SpawnRate;
        }
    }

    void OnDestroy() => Dispose();

    // TODO: create a pool
    private void Spawn()
    {
        Debug.Log("Spawn passenger");

        string name = "Passenger_" + Passengers.Count;
        var go = Instantiate(passengerPrefab, passengerPrefab.transform.position, Quaternion.identity, passengerContainer.transform);
        go.SetActive(true);
        Passengers.Add(go);

        var newPassenger = go.GetComponent<Passenger>();
        PassengersBehaviour.Add(newPassenger);

        newPassenger.SetTasks();
        
    }

    public void Dispose()
    {
        foreach (var e in Passengers)
            DestroyImmediate(e);
    }
}

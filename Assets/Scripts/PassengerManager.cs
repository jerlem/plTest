using System;
using System.Collections.Generic;
using UnityEngine;

public class PassengerManager : MonoBehaviour, IDisposable
{
    private const float PassengerMinSpeed = 1.0f;
    private const float PassengerMaxSpeed = 2.0f;
    private const float SpawnRate = 2.0f;

    public bool Spawning = true;

    private float spawnDelay = 2f;
    private float nextSpeed = 0f;

    private GameObject passengerPrefab;
    private GameObject passengerContainer;

    public List<PassengerHandler> Passengers = new();

    private void Awake()
    {
        passengerContainer = GameObject.Find("PassengerSpawner");

        if (passengerContainer == null)
            Debug.LogError("No Passenger spawner GameObject");

        
        //cached game object
        passengerPrefab = GameObject.Find("PR_Passenger");

        if (passengerContainer == null)
            Debug.LogError("PR_Passenger not found");

        passengerPrefab.SetActive(false);
    }

    private void Update()
    {
        if (!Spawning)
            return;


        spawnDelay -= Time.deltaTime;

        if (spawnDelay <= 0)
        {
            nextSpeed = UnityEngine.Random.Range(PassengerMinSpeed, PassengerMaxSpeed);
            spawnDelay = SpawnRate;
            Spawn();
        }
    }

    private void UpdatePassengers()
    {
        foreach (var e in Passengers)
            e.UpdatePassenger();
    }

    void OnDestroy() => Dispose();

    private void Spawn()
    {
        Debug.Log("Spawn passenger");

        string name = "Passenger_" + Passengers.Count;
        var go = Instantiate(passengerPrefab, passengerPrefab.transform.position, Quaternion.identity, passengerContainer.transform);
        go.SetActive(true);

        var newPassenger = new PassengerHandler(go);
        Passengers.Add(newPassenger);
        
        newPassenger.SetTasks();
        
    }

    public void Dispose()
    {
        for(int i= 0; i < Passengers.Count; i++)
        {
            DestroyImmediate(Passengers[i].Parent);
            Passengers[i] = null;
        }
    }
}

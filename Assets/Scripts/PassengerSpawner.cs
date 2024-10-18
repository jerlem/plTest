using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns passengers at a given rate
/// </summary>
public class PassengerSpawner : MonoBehaviour
{
    private const string RandomSeed = "SpeedSeed";
    private const float PassengerMinSpeed = 1.0f;
    private const float PassengerMaxSpeed = 2.0f;
    private const float SpawnRate = 2.0f; // Spawn a new passenger every 2 seconds
    
    private IPassengerInstantiator _passengerInstantiator;
    private bool _spawning;

    private void Awake()
    {
        Random.InitState(RandomSeed.GetHashCode());
        _passengerInstantiator = GetComponent<IPassengerInstantiator>();
    }
    
    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        _spawning = true;
        while (_spawning)
        {
            SpawnNewPassenger(GetPassengerSpeed());
            yield return new WaitForSeconds(SpawnRate);
        }
    }
    
    /// <summary>
    /// Generate a random passenger speed
    /// </summary>
    /// <returns>The generated passenger speed</returns>
    private static float GetPassengerSpeed()
    {
        return Random.Range(PassengerMinSpeed, PassengerMaxSpeed);
    }
    
    private void SpawnNewPassenger(float passengerSpeed)
    {
        _passengerInstantiator.Instantiate(passengerSpeed);
    }
    
    private void OnDestroy()
    {
        StopSpawning();
    }
    
    private void StopSpawning()
    {
        _spawning = false;
    }
}
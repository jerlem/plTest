using UnityEngine;

/// <summary>
/// In-game vending machine
/// </summary>
public class VendingMachine : MonoBehaviour
{
    [SerializeField, Tooltip("Treatment speed is in tickets delivered / minute")] private float _speed;
    public float Speed => _speed;
}
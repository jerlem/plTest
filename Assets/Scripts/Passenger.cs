using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Passenger state
/// TODO: link to animation controller
/// </summary>
public enum PassengerState
{
    Idle,
    Walk,
    Action,
    Failed,
}

/// <summary>
/// Data for passenger tasks,
/// - destination 
/// - state change
/// - and wait time (coroutine should be replaced by an event)
/// </summary>
public class PassengerTask
{
    public float WaitTime;
    public PassengerState Action;
    public Vector3 Destination;

    public PassengerTask(Vector3 destination, PassengerState action, float waitTime = 0f)
    {
        WaitTime = waitTime;
        Destination = destination;
        Action = action;
    }
}

/// <summary>
/// Passenger behaviour
/// </summary>
public class Passenger : MonoBehaviour
{ 
    public const float reachDistance = 1f; // triggering distance

    private PassengerState state;

    private Seeker seeker;

    /// <summary>
    /// property linked to animator
    /// </summary>
    public PassengerState State
    {
        get => state;
        set
        {
            state = value;
            SetAnimation(state);
        }
    }

    private Animator animator;

    public  Vector3 CurrentDestination { get; private set; } = Vector3.zero;

    public readonly float PassengerSpeed = 1f; // TODO: get the IPassengerInstantiator value

    public Queue<PassengerTask> Tasks = new();

    private void Awake()
    {
        state = PassengerState.Idle;

        seeker = GetComponent<Seeker>();

        if (seeker == null)
            Debug.LogError("[Passenger] Seeker not found");


        animator = GetComponent<Animator>();
        animator = GameObject.Find("RM_MalePassenger").GetComponent<Animator>(); // TODO : get in a better way

        SetTasks();
    }

    /// <summary>
    /// Next task in queue
    /// <see cref="PassengerTask"/>
    /// </summary>
    public void NextTask()
    {
        PassengerTask next = Tasks.Dequeue();

        if (next != null)
        {
            MoveTo(next.Destination);
            state = next.Action;
        }
    }

    /// <summary>
    /// Get the current task
    /// </summary>
    public PassengerTask CurrentTask => Tasks.Peek();

    private void Update()
    {
        if (CurrentDestination != Vector3.zero)
        {
            if (Vector3.Distance(transform.position, CurrentDestination) < reachDistance)
            {                
                if (CurrentTask.WaitTime > 0f)
                    DelayedTask();
                else
                    NextTask();
                
            }
        }
    }

    /// <summary>
    /// Wait for next task
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedTask()
    {
        // TODO: replace by events
        NextTask();
        Debug.Log("now waiting for " + CurrentTask.WaitTime);

        yield return new WaitForSeconds(CurrentTask.WaitTime);
    }

    /// <summary>
    /// Update animation as state change
    /// TODO: To complete
    /// </summary>
    /// <param name="state"></param>
    private void SetAnimation(PassengerState state) { }

    /// <summary>
    /// Setting up task
    /// going to a vending machine, wait for ticket, go to door then to train
    /// </summary>
    public void SetTasks()
    {
        Debug.Log("SetTasks");

        // Get the fastest available vendor 
        var machine = GameManager.GetVendorPosition();
        if (machine == null)
            return;

        Tasks.Enqueue(new PassengerTask(machine.Position, PassengerState.Walk, machine.Speed));
        
        MoveTo(machine.Position); // temp
    }

    public void MoveTo(Vector3 destination)
    {
        CurrentDestination = destination;
        seeker.StartPath(gameObject.transform.position, destination);
    }
}

using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Passenger state
/// TODO: link to animation controller
/// </summary>
public enum PassengerState
{
    None,
    Idle,
    TakeTicket,
    TicketTaken,
    Walk,
    Action,
    Failed,
    Finished,
}

/// <summary>
/// Data for passenger tasks,
/// - state change
/// - destination 
/// - and wait time (coroutine should be replaced by an event)
/// - a target object
/// </summary>
public class PassengerTask<T>
{
    public PassengerState Action;
    public float WaitTime;
    public Vector3 Destination;
    public T Target;

    public PassengerTask(PassengerState action, Vector3 destination, T target, float waitTime = 0f)
    {
        Action = action;
        WaitTime = waitTime;
        Destination = destination;
        Target = target;
    }
}

/// <summary>
/// Passenger behaviour
/// </summary>
public class Passenger : MonoBehaviour
{ 
    public const float reachDistance = 1.5f; // triggering distance

    private PassengerState state = PassengerState.None;

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
            Debug.Log($"{gameObject.name.ToString()} -> {state}");
            SetAnimation(state);
        }
    }


    private Animator animator;

    public Vector3 CurrentDestination { get; private set; } = Vector3.zero;

    public bool IsWaiting { get; private set; } = false;

    public float TaskWaitTime { get; private set; } = 0f;

    public readonly float PassengerSpeed = 1f; // TODO: get the IPassengerInstantiator value

    public Queue<PassengerTask<TicketMachine>> Tasks = new();

    private void Awake()
    {

        seeker = GetComponent<Seeker>();

        if (seeker == null)
            Debug.LogError("[Passenger] Seeker not found");

        animator = GetComponent<Animator>();
        animator = GameObject.Find("RM_MalePassenger").GetComponent<Animator>(); // TODO : get in a better way

    }

    private void Update()
    {
        // init Tasks
        if (State == PassengerState.None)
        {
            State = PassengerState.Idle;
            SetTasks();

            return;
        }

        // Wait for next task?
        if (IsWaiting)
        {
            if (TaskWaitTime > 0f)
            {
                TaskWaitTime -= Time.deltaTime;
                Debug.Log("waiting : " + TaskWaitTime);
            }
            else
            {
                NextTask();
                TaskWaitTime = 0f;
                IsWaiting = false;
            }
        }
        
        // get distance to target
        // and trigger a wait timer
        var dist = Vector3.Distance(transform.position, CurrentDestination);
        if (dist < reachDistance)
        {
            MoveStop();
            IsWaiting = true;
        }
    }

    private bool IsInTrain() => Vector3.Distance(GameManager.TrainDoor.transform.position, CurrentDestination) < 3.5f;

    /// <summary>
    /// Next task in queue
    /// <see cref="PassengerTask"/>
    /// </summary>
    private void NextTask()
    {
        PassengerTask<TicketMachine> next;

        if (Tasks.Count > 0)
        {
            next = Tasks.Dequeue();
        }
        else
        {
            if (IsInTrain())
            {
                GameManager.PassengerOnBoard = true;
                gameObject.SetActive(false);
            }

            State = PassengerState.Idle;
            return;
        }

        if (CurrentTask == null)
            return;

        State = next.Action;

        // looking for next target
        MoveTo(next.Destination);

        // lock / free vending machine
        if (CurrentTask.Action == PassengerState.TakeTicket)
        {
            Debug.Log("Taking machine " + next.Target.Name.ToString());
            next.Target.Available = false;
        }

        if (CurrentTask.Action == PassengerState.TicketTaken)
        {
            Debug.Log("Freeing machine " + next.Target.Name.ToString());
            next.Target.Available = true;
        }

        // add required timer
        TaskWaitTime = next.WaitTime;
    }

    /// <summary>
    /// Get the current task
    /// </summary>
    public PassengerTask<TicketMachine> CurrentTask
    {
        get
        {
            if (Tasks.Count == 0)
                return null;

            return Tasks.Peek();
        }
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

        // Going to machine 
        Tasks.Enqueue(new PassengerTask<TicketMachine>(
            PassengerState.TakeTicket,
            machine.Position,
            machine,
            machine.Speed)
        );

        // tiket taken 
        Tasks.Enqueue(new PassengerTask<TicketMachine>(
            PassengerState.TicketTaken,
            machine.Position,
            machine,
            0.5f)
        );

        // going to out door
        Tasks.Enqueue(new PassengerTask<TicketMachine>(
            PassengerState.Walk, 
            GameManager.StationDoor.transform.position, 
            machine,
            0f)
        );

        // going in the train
        Tasks.Enqueue(new PassengerTask<TicketMachine>(
            PassengerState.Walk,
            GameManager.TrainDoor.transform.position,
            machine,
            1f)
        );

        // finished
        Tasks.Enqueue(new PassengerTask<TicketMachine>(
            PassengerState.Idle,
            GameManager.TrainDoor.transform.position,
            machine,
            0f)
        );

        NextTask();
    }

    public void MoveTo(Vector3 destination)
    {
        CurrentDestination = destination;
        seeker.StartPath(gameObject.transform.position, CurrentDestination);
    }

    public void MoveStop()
    {
        CurrentDestination = gameObject.transform.position;
        seeker.StartPath(gameObject.transform.position, CurrentDestination);
    }
}

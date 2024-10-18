using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Helper for Vending machine, adding an avaiability
/// TODO: add a stack
/// </summary>
public class TicketMachine
{
    public string Name;
    public Vector3 Position;
    public float Speed;
    public bool Available;

    public TicketMachine(VendingMachine machine)
    {
        Name = machine.name;
        Position = machine.transform.position;
        Speed = machine.Speed;
        Available = true;

    }

    public override string ToString() => $"Name: {Name} Pos: {Position} Speed: {Speed} Available {Available}";
}

public class GameManager : GameSingleton
{
    private const float TrainSpeed = 10f;


    //public static int TrainDepartureTime = 54070; // 15:00:05
    private static float elapsedTime;

    private static bool Trainstarted = false;

    public static bool PassengerOnBoard = false;

    public static int StartTime = 54060; // 15:00:01
    public static int TrainDepartureTime = 54300; // 15:00:05

    public static GameObject Passenger;
    public static Passenger PassengerBehaviour;

    public static GameObject Train;
    public static GameObject TrainDoor;
    public static GameObject StationDoor;

    public static TextMeshProUGUI UIGameTimer;
    public static TextMeshProUGUI UITrainStatus;

    public static List<TicketMachine> TicketMachines = new ();


    public void Awake()
    {
        // get Vending machine Scripts components
        Passenger = GameObject.Find("PR_Passenger");

        if (Passenger == null)
        {
            Debug.LogError("passenger game object not found");
        }
        else
        {
            PassengerBehaviour = Passenger.GetComponent<Passenger>();

            if (PassengerBehaviour == null)
                Debug.LogError("passenger has no script on it");
        }

        // then set tickets machines
        var machines = FindObjectsOfType<VendingMachine>();

        foreach (var e in machines)
            TicketMachines.Add(new TicketMachine(e));

        // UI Components

        UIGameTimer = GameObject.Find("GameTime").GetComponent<TextMeshProUGUI>();
        if (UIGameTimer == null)
            Debug.LogError("GameTime not found");

        UITrainStatus = GameObject.Find("TrainStatus").GetComponent<TextMeshProUGUI>();
        if (UIGameTimer == null)
            Debug.LogError("TrainStatus not found");

        // GameObjects needed

        Train = GameObject.Find("PR_Train");
        if (Train == null)
            Debug.LogError("PR_Train not found");

        StationDoor = GameObject.Find("PR_Door");
        if (StationDoor == null)
            Debug.LogError("PR_Door not found");

        TrainDoor = GameObject.Find("PR_TrainDoor");
        if (StationDoor == null)
            Debug.LogError("PR_TrainDoor not found");
    }

    public void Update()
    {
        UIGameTimer.text = UpdateTimer();

        if (Trainstarted)
        {
            PassengerBehaviour.State = PassengerState.Failed; 
            MoveTrain();
        }

        if (elapsedTime >= TrainDepartureTime)
        {
            Trainstarted = true;

            string passengerIn = PassengerOnBoard == true ?
                "with passenger" :
                "without passenger";

            UITrainStatus.text = "Train is leaving " + passengerIn;
        }
    }

    /// <summary>
    /// Train moving
    /// </summary>
    private static void MoveTrain()
    {
        Vector3 pos = Vector3.zero;
        pos.x = TrainSpeed * Time.deltaTime;
        Train.transform.Translate(pos);
    }

    /// <summary>
    /// Update the timer and the UI value
    /// </summary>
    /// <returns></returns>
    private static string UpdateTimer()
    {
        elapsedTime = Time.realtimeSinceStartup + StartTime;

        int h = Mathf.FloorToInt(elapsedTime / 3600);
        int m = Mathf.FloorToInt((elapsedTime % 3600) / 60);
        int s = Mathf.FloorToInt(elapsedTime % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
    }

    /// <summary>
    /// Get the fastest available vendor position
    /// </summary>
    /// <returns></returns>
    public static TicketMachine GetVendorPosition()
    {
        float min = 100f;
        TicketMachine result = null;

        foreach (var p in TicketMachines)
        {
            if (!p.Available)
                continue;

            if (min > p.Speed)
            {
                min = p.Speed;
                result = p;
            }
        }

        //Debug.Log("GetVendorPosition :" + result.ToString());
        return result;
    }

}
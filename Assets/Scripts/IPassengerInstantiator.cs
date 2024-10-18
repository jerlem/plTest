/// <summary>
/// Interface to implement in order to spawn passengers
/// </summary>
public interface IPassengerInstantiator
{
    void Instantiate(float passengerSpeed);
}
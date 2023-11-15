namespace RobotController;

public interface IRobotController
{
    void Connect();
    void Disconnect();
    void SendAction(RobotAction action);
}
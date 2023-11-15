namespace RobotController;

public interface IRobotController
{
    void Connect();
    void Disconnect();
    void SendCommand(RobotAction action);
}
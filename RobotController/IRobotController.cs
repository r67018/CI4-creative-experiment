namespace RobotController;

public interface IRobotController
{
    void Connect();
    void Disconnect();
    void SendAction(IEnumerable<RobotAction> actions);
}
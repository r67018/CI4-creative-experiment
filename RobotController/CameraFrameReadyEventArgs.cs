using System.Drawing;

namespace RobotController;

public class CameraFrameReadyEventArgs
{
    public required Bitmap Bitmap { get; init; }
}
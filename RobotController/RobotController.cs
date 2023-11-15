using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MjpegProcessor;

namespace RobotController;

public class RobotController : IRobotController
{
    private IPAddress _ipAddress;
    private int _port;
    private int _cameraPort;
    private Socket? _socket;
    private readonly MjpegDecoder _mjpegDecoder = new();
    
    public event EventHandler<FrameReadyEventArgs>? CameraImageReady;
    
    private static readonly Dictionary<RobotAction, string> ActionToCommand = new()
    {
        {RobotAction.MoveForward, "a"},
        {RobotAction.MoveBackward, "b"},
        {RobotAction.TurnLeft, "c"},
        {RobotAction.TurnRight, "d"},
    };
    
    public RobotController(string ipAddress, int port, int cameraPort)
    {
        _ipAddress = IPAddress.Parse(ipAddress);
        _port = port;
        _cameraPort = cameraPort;
        _mjpegDecoder.FrameReady += _mjpegDecoderOnFrameReady;
    }
    
    public void Connect()
    {
        // Create socket
        IPEndPoint remoteEndPoint = new(_ipAddress, _port);
        _socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(remoteEndPoint);
        // Connect to the robot's camera stream
        var uri = new Uri($"http://{_ipAddress}:{_cameraPort}/?action=stream");
        _mjpegDecoder.ParseStream(uri);
    }

    public void Disconnect()
    {
        // Close socket
        _socket?.Shutdown(SocketShutdown.Both);
        _socket?.Close();
        // Stop camera stream
        _mjpegDecoder.StopStream();
    }

    public void SendCommand(RobotAction action)
    {
        var command = ActionToCommand[action];
        var bytes = Encoding.ASCII.GetBytes(command);
        _socket?.Send(bytes);
    }
    
    private void _mjpegDecoderOnFrameReady(object? sender, FrameReadyEventArgs e)
    {
        CameraImageReady?.Invoke(this, e);
    }
}
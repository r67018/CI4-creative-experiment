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
    private readonly MjpegDecoder _mjpegDecoder = new();
    
    public event EventHandler<FrameReadyEventArgs>? CameraImageReady;
    
    private static readonly Dictionary<RobotAction, string> ActionToCommand = new()
    {
        {RobotAction.MoveForward, "w"},
        {RobotAction.MoveBackward, "s"},
        {RobotAction.TurnLeft, "a"},
        {RobotAction.TurnRight, "d"},
        {RobotAction.SpecialAction, " "},
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
        // Connect to the robot's camera stream
        var uri = new Uri($"http://{_ipAddress}:{_cameraPort}/?action=stream");
        _mjpegDecoder.ParseStream(uri);
    }

    public void Disconnect()
    {
        // Stop camera stream
        _mjpegDecoder.StopStream();
    }

    public void SendAction(RobotAction action)
    {
        // Create socket
        var socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint remoteEndPoint = new(_ipAddress, _port);
        socket.Connect(remoteEndPoint);
        // Send command
        var command = ActionToCommand[action];
        var bytes = Encoding.ASCII.GetBytes(command);
        socket.Send(bytes);
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    
    private void _mjpegDecoderOnFrameReady(object? sender, FrameReadyEventArgs e)
    {
        CameraImageReady?.Invoke(this, e);
    }
}
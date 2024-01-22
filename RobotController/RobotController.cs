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
    
    private static class CommandMask
    {
        public const byte MoveForward = 0b0000_0001;
        public const byte MoveBackward = 0b0000_0010;
        public const byte TurnLeft = 0b0000_0100;
        public const byte TurnRight = 0b0000_1000;
        public const byte SpecialAction1 = 0b0001_0000;
        public const byte SpecialAction2 = 0b0010_0000;
    }
    
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
        var socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint remoteEndPoint = new(_ipAddress, _port);
        // Set timeout
        var result = socket.BeginConnect(remoteEndPoint, null, null);
        var timeout = TimeSpan.FromSeconds(5);
        var success = result.AsyncWaitHandle.WaitOne(timeout);
        if (!success)
        {
            throw new SocketException(10060); // Connection timed out.
        }
        // Send null byte to test connection
        socket.Send(new byte[] {0});
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        
        // Connect to the robot's camera stream
        var uri = new Uri($"http://{_ipAddress}:{_cameraPort}/?action=stream");
        _mjpegDecoder.ParseStream(uri);
    }

    public void Disconnect()
    {
        // Stop camera stream
        _mjpegDecoder.StopStream();
    }

    public void SendAction(IEnumerable<RobotAction> actions)
    {
        // Create socket
        var socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint remoteEndPoint = new(_ipAddress, _port);
        socket.Connect(remoteEndPoint);
        // Convert actions to bit flags
        byte command = 0;
        foreach (var action in actions)
        {
            switch (action)
            {
                case RobotAction.MoveForward:
                    command |= CommandMask.MoveForward;
                    break;
                case RobotAction.MoveBackward:
                    command |= CommandMask.MoveBackward;
                    break;
                case RobotAction.TurnLeft:
                    command |= CommandMask.TurnLeft;
                    break;
                case RobotAction.TurnRight:
                    command |= CommandMask.TurnRight;
                    break;
                case RobotAction.SpecialAction1:
                    command |= CommandMask.SpecialAction1;
                    break;
                case RobotAction.SpecialAction2:
                    command |= CommandMask.SpecialAction2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
        // Send command
        socket.Send(new[] {command});
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    
    private void _mjpegDecoderOnFrameReady(object? sender, FrameReadyEventArgs e)
    {
        CameraImageReady?.Invoke(this, e);
    }
}
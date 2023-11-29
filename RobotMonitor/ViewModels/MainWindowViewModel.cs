using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Reactive.Bindings;
using Reactive.Bindings.TinyLinq;
using RobotController;
using RobotMonitor.Commands;

namespace RobotMonitor.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ReactiveCommand ConnectRobotCommand { get; }
    public ReactiveCommand DisconnectRobotCommand { get; }
    public ICommand SendRobotActionCommand => new SendRobotActionCommand(this);
    
    public ReactivePropertySlim<bool> IsConnected { get; } = new(false);
    public ReactivePropertySlim<string> IpAddress { get; } = new();
    public ReactivePropertySlim<int> Port { get; } = new();
    public ReactivePropertySlim<int> CameraPort { get; } = new();
    
    public RobotController.RobotController? RobotController { get; set; }
    public BitmapImage? CameraImage { get; private set; }
    public MainWindowViewModel()
    {
        ConnectRobotCommand = IsConnected.Select(x => !x).ToReactiveCommand();
        ConnectRobotCommand.Subscribe(_ => ConnectRobot());
        DisconnectRobotCommand = IsConnected.Select(x => x).ToReactiveCommand();
        DisconnectRobotCommand.Subscribe(_ => DisconnectRobot());
        
        IpAddress.Value = "192.168.10.4";
        Port.Value = 1234;
        CameraPort.Value = 8080;
    }

    public void RobotCameraFrameOnReady(object? sender, MjpegProcessor.FrameReadyEventArgs e)
    {
        CameraImage = e.BitmapImage;
        OnPropertyChanged(nameof(CameraImage));
        // Convert the bitmap to a BitmapImage
        // var bitmap = e.Bitmap;
        // using var memory = new MemoryStream();
        // bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        // memory.Position = 0;
        // var bitmapImage = new BitmapImage();
        // bitmapImage.BeginInit();
        // bitmapImage.StreamSource = memory;
        // bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        // bitmapImage.EndInit();
        // bitmapImage.Freeze();
        // CameraImage = bitmapImage;
    }
    
    private void ConnectRobot()
    {
        var robotController = new RobotController.RobotController(IpAddress.Value, Port.Value, CameraPort.Value);
        robotController.CameraImageReady += RobotCameraFrameOnReady;
        try
        {
            robotController.Connect();
            RobotController = robotController;
            IsConnected.Value = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            DisconnectRobot();
        }
    }
    
    private void DisconnectRobot()
    {
        RobotController?.Disconnect();
        RobotController = null;
        IsConnected.Value = false;
    }
}
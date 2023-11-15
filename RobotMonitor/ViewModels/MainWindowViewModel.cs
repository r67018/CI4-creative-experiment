using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Reactive.Bindings;
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

    public ICommand ConnectRobotCommand => new ConnectRobotCommand(this);
    public ICommand DisconnectRobotCommand => new DisconnectRobotCommand(this);
    public ICommand SendRobotActionCommand => new SendRobotActionCommand(this);
    
    public ReactivePropertySlim<bool> IsConnected { get; } = new(false);
    public ReactivePropertySlim<string> IpAddress { get; } = new();
    public ReactivePropertySlim<int> Port { get; } = new();
    public ReactivePropertySlim<int> CameraPort { get; } = new();
    
    public RobotController.RobotController? RobotController { get; set; }
    public BitmapImage? CameraImage { get; private set; }
    public MainWindowViewModel()
    {
        IpAddress.Value = "192.168.10.4";
        Port.Value = 1234;
        CameraPort.Value = 8080;
    }

    public void RobotCameraFrameOnReady(object? sender, MjpegProcessor.FrameReadyEventArgs e)
    {
        Console.WriteLine(e.BitmapImage);
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
}
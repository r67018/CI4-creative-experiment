using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accessibility;
using MaterialDesignThemes.Wpf;
using Reactive.Bindings;
using Reactive.Bindings.TinyLinq;
using RobotController;
using RobotMonitor.Commands;
using RobotMonitor.Helper;
using RobotMonitor.Models.Robot;
using Image = System.Windows.Controls.Image;

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
    public BitmapImage FieldMapImage { get; } = ImageHelper.Bitmap2BitmapImage(Resource.FieldMap);

    public SnackbarMessageQueue SnackbarMessageQueue { get; } = new();
    
    public Dispatcher Dispatcher { get; set; } = Dispatcher.CurrentDispatcher;
    
    public MainWindowViewModel()
    {
        ConnectRobotCommand = IsConnected.Select(x => !x).ToReactiveCommand();
        ConnectRobotCommand.Subscribe(_ =>
        {
            try
            {
                ConnectRobot();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DisconnectRobot();
                SnackbarMessageQueue.Enqueue("ロボットへの接続に失敗しました。");
                return;
            }
            SnackbarMessageQueue.Enqueue("ロボットへの接続に成功しました。");
        });
        DisconnectRobotCommand = IsConnected.Select(x => x).ToReactiveCommand();
        DisconnectRobotCommand.Subscribe(_ =>
        {
            DisconnectRobot();
            SnackbarMessageQueue.Enqueue("ロボットとの接続を切断しました。");
        });
        
        // ロボットの構成を復元
        if (File.Exists(Constants.Path.RobotConfig))
        {
            var json = File.ReadAllText(Constants.Path.RobotConfig);
            var robotConfig = JsonSerializer.Deserialize<RobotConfig>(json);
            IpAddress.Value = robotConfig.IpAddress;
            Port.Value = robotConfig.Port;
            CameraPort.Value = robotConfig.CameraPort;
        }
        else
        {
            IpAddress.Value = "192.168.10.4";
            Port.Value = 1234;
            CameraPort.Value = 8080;
        }

        Dispatcher.ShutdownStarted += Dispatcher_ShutDownStarted;
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
        robotController.Connect();
        RobotController = robotController;
        IsConnected.Value = true;
    }
    
    public void DisconnectRobot()
    {
        RobotController?.Disconnect();
        RobotController = null;
        IsConnected.Value = false;
        CameraImage = null;
    }
    
    private void Dispatcher_ShutDownStarted(object? sender, EventArgs e)
    {
        // ロボットの構成を保存
        var robotConfig = new RobotConfig
        {
            IpAddress = IpAddress.Value,
            Port = Port.Value,
            CameraPort = CameraPort.Value
        };
        var json = JsonSerializer.Serialize(robotConfig);
        File.WriteAllText(Constants.Path.RobotConfig, json);
    }

}
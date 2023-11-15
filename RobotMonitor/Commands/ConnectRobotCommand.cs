using System;
using System.Diagnostics;
using System.Windows.Input;
using RobotMonitor.ViewModels;

namespace RobotMonitor.Commands;

public class ConnectRobotCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    
    private MainWindowViewModel _vm;

    public ConnectRobotCommand(MainWindowViewModel vm)
    {
        _vm = vm;
    }
    
    public bool CanExecute(object? parameter)
    {
        return !_vm.IsConnected.Value;
    }

    public void Execute(object? parameter)
    {
        var robotController = new RobotController.RobotController(_vm.IpAddress.Value, _vm.Port.Value, _vm.CameraPort.Value);
        robotController.CameraImageReady += _vm.RobotCameraFrameOnReady;
        robotController.Connect();
        _vm.RobotController = robotController;
        _vm.IsConnected.Value = true;
    }
}
using System;
using System.Diagnostics;
using System.Windows.Input;
using RobotMonitor.ViewModels;

namespace RobotMonitor.Commands;

public class DisconnectRobotCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    
    private MainWindowViewModel _vm;

    public DisconnectRobotCommand(MainWindowViewModel vm)
    {
        _vm = vm;
    }
    
    public bool CanExecute(object? parameter)
    {
        return _vm.IsConnected.Value;
    }

    public void Execute(object? parameter)
    {
        _vm.RobotController?.Disconnect();
        _vm.RobotController = null;
        _vm.IsConnected.Value = false;
    }
}
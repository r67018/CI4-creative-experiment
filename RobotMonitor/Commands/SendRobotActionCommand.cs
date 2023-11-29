using System;
using System.Windows.Input;
using RobotController;
using RobotMonitor.ViewModels;

namespace RobotMonitor.Commands;

public class SendRobotActionCommand : ICommand
{
    private MainWindowViewModel _vm;
    
    public SendRobotActionCommand(MainWindowViewModel vm)
    {
        _vm = vm;
    }
    
    public bool CanExecute(object? parameter)
    {
        return _vm.IsConnected.Value;
        // return true;
    }

    public void Execute(object? parameter)
    {
        if (parameter is null) return;
        var key = (Key)parameter;
        try
        {
            switch (key)
            {
                case Key.W:
                    _vm.RobotController?.SendAction(RobotAction.MoveForward);
                    break;
                case Key.S:
                    _vm.RobotController?.SendAction(RobotAction.MoveBackward);
                    break;
                case Key.A:
                    _vm.RobotController?.SendAction(RobotAction.TurnLeft);
                    break;
                case Key.D:
                    _vm.RobotController?.SendAction(RobotAction.TurnRight);
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _vm.DisconnectRobotCommand.Execute();
        }
    }

    public event EventHandler? CanExecuteChanged;
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        List<RobotAction> actions = new();
        if (Keyboard.IsKeyDown(Key.W))
        {
            actions.Add(RobotAction.MoveForward);
        }
        if (Keyboard.IsKeyDown(Key.S))
        {
            actions.Add(RobotAction.MoveBackward);
        }
        if (Keyboard.IsKeyDown(Key.A))
        {
            actions.Add(Keyboard.IsKeyDown(Key.LeftShift) ? RobotAction.UrgentAvoidanceLeft : RobotAction.TurnLeft);
        }
        if (Keyboard.IsKeyDown(Key.D))
        {
            actions.Add(Keyboard.IsKeyDown(Key.LeftShift) ? RobotAction.UrgentAvoidanceRight : RobotAction.TurnRight);
        }
        if (Keyboard.IsKeyDown(Key.Space))
        {
            actions.Add(RobotAction.SpecialAction1);
        }
        if (Keyboard.IsKeyDown(Key.M))
        {
            actions.Add(RobotAction.SpecialAction2);
        }

        try
        {
            _vm.RobotController?.SendAction(actions);
        }
        catch
        {
            _vm.DisconnectRobot();
            _vm.SnackbarMessageQueue.Enqueue("操作中にエラーが発生しました。ロボットとの接続を切断します。");
        }
    }

    public event EventHandler? CanExecuteChanged;
}
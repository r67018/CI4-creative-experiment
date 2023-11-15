using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace RobotMonitor.Behaviors;

public class KeyDownBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty KeyDownCommandProperty = DependencyProperty.Register(
        nameof(KeyDownCommand), typeof(ICommand), typeof(KeyDownBehavior), new PropertyMetadata());
    
    public ICommand KeyDownCommand
    {
        get => (ICommand)GetValue(KeyDownCommandProperty);
        set => SetValue(KeyDownCommandProperty, value);
    }
    
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.KeyDown += AssociatedObjectOnKeyDown;
    }
    
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.KeyDown -= AssociatedObjectOnKeyDown;
    }
    
    private void AssociatedObjectOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyDownCommand.CanExecute(null))
        {
            KeyDownCommand.Execute(e.Key);
        }
    }
}
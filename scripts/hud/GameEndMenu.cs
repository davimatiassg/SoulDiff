using Godot;
using System;

public partial class GameEndMenu : Control
{

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey or InputEventJoypadButton && @event.IsPressed())
        {
            SceneManager.ChangeScene("res://scenes/Main_menu.tscn");
        }
    }
}

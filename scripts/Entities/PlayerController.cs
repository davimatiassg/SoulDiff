using System;
using System.Reflection.Metadata.Ecma335;
using Godot;

public partial class PlayerController : BodyController
{
    [Export] public ABody currentBody;
    public override void _Process(double delta)
    {
        base._Process(delta);

        Vector2 leftAxis = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        LeftAxisAction(leftAxis);

        Vector2 rightAxis = Input.GetVector("game_aim_left", "game_aim_right", "game_aim_up", "game_aim_down");
        if (rightAxis != Vector2.Zero) LeftAxisAction(rightAxis);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event.IsAction("game_btn_1")) { Button1Action( @event.IsPressed() ); return; }

        if (@event.IsAction("game_btn_2")) { Button2Action( @event.IsPressed() ); return; }

        if (@event.IsAction("game_btn_3")) { Button3Action( @event.IsPressed() ); return; }
        
        if (@event is InputEventMouseMotion)
        {
            RightAxisAction((GetGlobalMousePosition() - this.GlobalPosition).Normalized());
        }
    }
}
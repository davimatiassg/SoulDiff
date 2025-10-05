using System;
using System.Reflection.Metadata.Ecma335;
using Godot;

public partial class PlayerController : AnyController
{
    [Export] public AnyBody currentBody;
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (currentBody == null) return;
        
        Vector2 leftAxis = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        LeftAxisAction(leftAxis);

        RightAxisAction((GetGlobalMousePosition() - currentBody.GlobalPosition).Normalized());
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event.IsAction("game_btn_1")) { Button1Action( @event.IsPressed() ); return; }

        if (@event.IsAction("game_btn_2")) { Button2Action( @event.IsPressed() ); return; }

        if (@event.IsAction("game_btn_3")) { Button3Action( @event.IsPressed() ); return; }
        
    }
}
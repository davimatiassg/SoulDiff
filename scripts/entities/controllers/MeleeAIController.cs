using System;
using Godot;

public partial class MeleeAIController : AnyController
{


    public override void _Ready()
    {
        base._Ready();
        
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        RightAxisAction(Vector2.Right);
        Button1Action(true);

    }
}
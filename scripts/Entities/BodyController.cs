using System;
using Godot;

public abstract partial class BodyController : Node2D
{
    public Action<Vector2> LeftAxisAction = (Vector2 v) => {};
    public Action<Vector2> RightAxisAction = (Vector2 v) => {};

    public Action Button1Action = () => {};
    public Action Button2Action = () => {};
    public Action Button3Action = () => {};

}
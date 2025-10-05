using System;
using Godot;

public abstract partial class AnyController : Node2D
{
    public Action<Vector2> LeftAxisAction = (Vector2 v) => {};
    public Action<Vector2> RightAxisAction = (Vector2 v) => {};

    public Action<bool> Button1Action = (bool pressed) => {};
    public Action<bool> Button2Action = (bool pressed) => {};
    public Action<bool> Button3Action = (bool pressed) => {};

}
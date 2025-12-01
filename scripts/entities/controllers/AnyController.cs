using System;
using System.Diagnostics;
using Godot;

public partial class AnyController : Node2D
{
    public Action<Vector2> LeftAxisAction = (Vector2 v) => { };
    public Action<Vector2> RightAxisAction = (Vector2 v) => { };

    public Action<bool> Button1Action = (bool pressed) => { };
    public Action<bool> Button2Action = (bool pressed) => { };
    public Action<bool> Button3Action = (bool pressed) => { };

    [Export] public AnyBody currentBody;

    public void Connect(AnyBody body)
    {
        if (GetParent() == null) body.AddChild(this);

        body.controller = this;
        currentBody = body;

        Button1Action = body.Button1;
        Button2Action = body.Button2;
        Button3Action = body.Button3;

        LeftAxisAction = body.Move;
        RightAxisAction = body.Aim;
    }
    
    public void Disconnect(AnyBody body)
	{
        Debug.Assert(body != null);
		if (GetParent() == body) body.RemoveChild(this);

		body.controller = null;
        currentBody = null;

		Button1Action = (bool pressed) => { };
		Button2Action = (bool pressed) => { };
		Button3Action = (bool pressed) => { };

		LeftAxisAction = (Vector2 v) => { };
		RightAxisAction = (Vector2 v) => { };
	}

}
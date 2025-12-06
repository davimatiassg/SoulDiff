using System;
using System.Diagnostics;
using Godot;

public partial class PlayerController : AnyController
{
    public static PlayerController Instance;
    
    public GhostBody ghost;
    [Export] PackedScene ghostPrefab;

    Tween autoDamageCountdown;
    private static void StartCountDown()
    {
        if (Instance.autoDamageCountdown != null)
        {
            Instance.autoDamageCountdown.Kill();
        }

        Instance.autoDamageCountdown = Instance.CreateTween();
        Instance.autoDamageCountdown.TweenMethod(Callable.From((float value) => HudManager.SetAutodamageCountdownValue(value)), 0, 100, 3);
        Instance.autoDamageCountdown.TweenCallback(Callable.From(() =>
            Instance.currentBody.TakePassiveDamage((int)(Instance.currentBody.MaxHP / 10f))));
        Instance.autoDamageCountdown.SetLoops();
    }

    public static void Embody(AnyBody body)
    {
        Debug.Assert(Instance != null);
        Debug.Assert(Instance.ghost != null);

        var ghostParent = Instance.ghost.GetParent();
        if (ghostParent != null) ghostParent.RemoveChild(Instance.ghost);
        if (body.controller != null) body.controller.Disconnect(body);

        HudManager.SetBodyPortrait(body);

        Instance.Connect(body);
        body.PossessStart(Instance);
    }


    public static void Disembody(AnyBody body)
    {
        if (body != null) body.PossessEnd();
        Instance.Disconnect(body);
        Instance.Connect(Instance.ghost);

        HudManager.SetBodyPortrait(Instance.ghost);

        Instance.GetParent().CallDeferred("add_child", Instance.ghost);
        Instance.ghost.PossessStart(Instance);
        Instance.ghost.GlobalPosition = body.GlobalPosition;

        StartCountDown();

    }

    public static void Disembody()
	{
        Instance.Connect(Instance.ghost);
        Instance.GetParent().CallDeferred("add_child", Instance.ghost);

        HudManager.SetBodyPortrait(Instance.ghost);

        Instance.ghost.PossessStart(Instance);
        Instance.ghost.GlobalPosition = Vector2.Zero;
	}



  
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

        if (@event.IsAction("game_btn_1")) { Button1Action(@event.IsPressed()); return; }

        if (@event.IsAction("game_btn_2")) { Button2Action(@event.IsPressed()); return; }

        if (@event.IsAction("game_btn_3")) { Button3Action(@event.IsPressed()); return; }

    }

    public override void _Ready()
    {
        base._Ready();
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Instance.QueueFree();
            Instance = this;
        }

        ghost = (GhostBody)ghostPrefab.Instantiate();

        if (currentBody == null) Disembody();
        else Embody(currentBody);
    }
}
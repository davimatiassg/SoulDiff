using Godot;
using System;

public partial class TransitionCurtain : TextureRect
{
    private static TransitionCurtain Instance;

    [Export]
    public float sizeMax;

    public static bool isTransitioning = false;

    public static void FadeIn()
    {
        Instance.Visible = true;
        isTransitioning = true;
        Instance.Scale = Vector2.One * Instance.sizeMax;
        Tween tween = Instance.CreateTween();
        tween.SetParallel();
        tween.TweenProperty(Instance, "scale", Vector2.Zero, 0.8).
        SetTrans(Tween.TransitionType.Expo);
        tween.TweenCallback(Callable.From(() => isTransitioning = false));
        tween.TweenCallback(Callable.From(() => Instance.Visible = false));
    }

    public static void FadeOut()
    {
        Instance.Visible = true;
        Instance.Scale = Vector2.Zero;
        Tween tween = Instance.CreateTween();
        tween.TweenProperty(Instance, "scale", Vector2.One * Instance.sizeMax, 0.8).
        SetTrans(Tween.TransitionType.Expo);
        tween.TweenCallback(Callable.From(() => isTransitioning = false));

    }

    public override void _Ready()
    {
        base._Ready();
        if (Instance == null) Instance = this;
        else if (Instance != this) { QueueFree(); return; }

        // Instance.Anchor = GetViewportRect().Size / 2;

        FadeIn();
    }
}

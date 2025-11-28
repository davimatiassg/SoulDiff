using Godot;
using System;

public partial class TransitionVignette : TextureRect
{

    [Export]
    public float sizeMax;

    public bool isTransitioning = false;

    public void FadeIn()
    {
        Visible = true;
        isTransitioning = true;
        Scale = Vector2.One * sizeMax;
        Tween tween = CreateTween();
        tween.TweenProperty(this, "scale", Vector2.Zero, 0.8).SetTrans(Tween.TransitionType.Expo);
        tween.TweenCallback(Callable.From(() => isTransitioning = false));
        tween.TweenCallback(Callable.From(() => Visible = false));
        Tween tween2 = CreateTween();
        tween2.TweenProperty(this, "pivot_offset", new Vector2(320, 180), 0.8).SetTrans(Tween.TransitionType.Expo);
        tween2.SetParallel();
    }

    public void FadeOut()
    {
        Visible = true;
        Scale = Vector2.Zero;
        Tween tween = CreateTween();
        tween.TweenProperty(this, "scale", Vector2.One * sizeMax, 0.8).SetTrans(Tween.TransitionType.Expo);
        tween.TweenCallback(Callable.From(() => isTransitioning = false));
        Tween tween2 = CreateTween();
        tween2.TweenProperty(this, "pivot_offset", new Vector2(320, 280), 0.8).SetTrans(Tween.TransitionType.Expo);

    }

    public override void _Ready()
    {
        FadeIn();
    }
}

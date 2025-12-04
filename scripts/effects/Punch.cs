using Godot;
using System;

public partial class Punch : DamageEffect
{

    [Export] public Vector2 direction = Vector2.Right;

    [Export] public float distance = 32f;

    public float knockback = 320;

    public float punchDuration = 0.8f;

    public override void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        base.OnCollisionEnter(bodyRid, body, bodyShapeIndex, localShapeIndex);


        if (body is not Hitable hit) return;

        if (CheckHitability(hit))
        {
            hit.TakeDamage(damage, direction * knockback);
        }

    }

    public override void _Ready()
    {
        base._Ready();

    }

    public void LaunchPunch()
    {
        LookAt(direction);
        Tween tween = CreateTween();
        tween.TweenProperty(this, "position", direction * distance, punchDuration / 3f).SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(this, "position", Vector2.Zero, punchDuration * 2f / 3f);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    
}

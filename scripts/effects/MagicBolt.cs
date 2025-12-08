using Godot;
using System;

public partial class MagicBolt : DamageEffect
{
    public Vector2 velocity;

    [Export] public float duration = 2.0f;

    public override void _Ready()
    {
        base._Ready();
        Tween durationTween = CreateTween();
        durationTween.TweenInterval(duration);
        durationTween.TweenCallback(Callable.From(Dispawn));
    }

    public override void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        base.OnCollisionEnter(bodyRid, body, bodyShapeIndex, localShapeIndex);

        if (body is Hitable hit && CheckHitability(hit))
        {
            hit.TakeDamage(damage, Vector2.Zero);
            Dispawn();
        } else if (body is not AnyBody) { Dispawn(); }
        
        
    }

    public override void _Process(double delta)
    {
        base._PhysicsProcess(delta);
        Translate((float)delta * velocity);
        LookAt(GlobalPosition + velocity);
    }

}

using Godot;
using System;
using System.Collections.Generic;

public partial class Fireball : DamageEffect
{
    Vector2 velocity = Vector2.Zero;

    public float chargeTime = 2f;
    Action<float> processAction;

    public void StartOrbit(AnyBody target)
    {
        processAction = (float delta) =>
        {
            velocity *= 0.80f;
            velocity += ((target.GlobalPosition + target.aimDirection * 32f) - GlobalPosition) * delta * 150f;
        };

        collider.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;

        animation.Scale = 0.1f * Vector2.One;

        Tween tween = CreateTween();
        tween.TweenProperty(animation, "scale", Vector2.One * 0.5f, chargeTime);

        
    }
    public void Fling(Vector2 dir)
    {
        this.velocity = dir * Mathf.Max(350f, velocity.Length());

        LookAt(GlobalPosition - dir);

        processAction = null;

        animation.Play("fling");

        SetExitTime(5.0);

        collider.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
    }


    public override void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        base.OnCollisionEnter(bodyRid, body, bodyShapeIndex, localShapeIndex);

        if (body is Hitable hit && CheckHitability(hit))
        {
            hit.TakeDamage(damage, Vector2.Zero);
            Blast();
        }
        else if (body is not AnyBody) { Blast(); }
        
    }

    public void Blast()
    {
        animation.Play("blast");

        velocity = Vector2.Zero;
        animation.Scale = Vector2.One;
        ((CircleShape2D)collider.GetChild<CollisionShape2D>(0).Shape).Radius = 32f;


        animation.AnimationFinished += () =>
        { 
            Dispawn();
        };
        
    }

    public override void _Process(double delta)
    {
        base._PhysicsProcess(delta);
        Translate((float)delta * velocity);
        processAction?.Invoke((float)delta);
    }
    
}
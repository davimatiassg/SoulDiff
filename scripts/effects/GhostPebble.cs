using Godot;
using System;
using System.Collections.Generic;

public partial class GhostPebble : DamageEffect
{

    
    Vector2 velocity = Vector2.Zero;
    public override void _Ready()
    {
        base._Ready();
    }

    Action<float> processAction;

    public void StartOrbit(GhostBody target)
    {
        processAction = (float delta) =>
        {
            velocity *= 0.80f;
            velocity += ((target.GlobalPosition - target.aimDirection*16f + target.moveDirection*16f)- GlobalPosition) * delta * 150f;
        };
    }
    public void Fling(Vector2 dir)
    {
        this.velocity = dir * Mathf.Max(350f, velocity.Length());

        processAction = null;

        SetExitTime(5.0);
    }


    public override void OnCollisionEnter(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        base.OnCollisionEnter(bodyRid, body, bodyShapeIndex, localShapeIndex);

        if (body is Hitable hit)
        {
            if (hit is AnyBody creature) { if (creature.isPlayer) return; }

            hit.TakeDamage(damage, velocity);
        }

        Dispawn();
    }

    public override void _Process(double delta)
    {
        base._PhysicsProcess(delta);
        Translate( (float)delta * velocity);
        processAction?.Invoke( (float) delta);        
    }
    
}